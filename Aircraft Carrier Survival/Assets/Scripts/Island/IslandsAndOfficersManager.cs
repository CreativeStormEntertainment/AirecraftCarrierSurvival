using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using UnityRandom = UnityEngine.Random;

public class IslandsAndOfficersManager : ParameterEventBase<EIslandUIState>, ITickable, IEnableable, IPopupPanel
{
    public event Action<EDepartments, int> DepartmentBoostUpdated = delegate { };
    public event Action PointsChanged = delegate { };
    public event Action BuffExpired = delegate { };
    public event Action<EIslandRoomType> RoomReached = delegate { };
    public event Action SwitchUsed = delegate { };
    public event Action BuffSetupStarted = delegate { };
    public event Action BuffSetupReady = delegate { };
    public event Action BuffConfirmed = delegate { };

    public static IslandsAndOfficersManager Instance;

    public Officer SelectedOfficer
    {
        get => selectedOfficer;
        set
        {
            if (selectedOfficer != null)
            {
                selectedOfficer.Button.SetSelected(false);
                selectedOfficer.Portrait.SetSelected(false);
                selectedOfficer.CurrentIslandRoom.IslandUI.SetVisible(false);
                selectedOfficer.CurrentIslandRoom.IslandUI.SetEnabledColliders(false);
                selectedOfficer.LastIslandRoom?.UpdateOutline(false);
                selectedOfficer.CurrentIslandRoom?.UpdateOutline(false);
            }
            if (BuffsPanelOpen)
            {
                selectedOfficer = null;
                return;
            }
            selectedOfficer = value;
            if (selectedOfficer != null)
            {
                SetShowPath(selectedOfficer, true);
                lastSelectedOfficer = selectedOfficer;
                selectedOfficer.CurrentIslandRoom.IslandUI.SetVisible(true);
                selectedOfficer.CurrentIslandRoom.IslandUI.SetEnabledColliders(true);

                selectedOfficer.Button.SetSelected(true);
                selectedOfficer.Portrait.SetSelected(true);
                selectedOfficer.CurrentIslandRoom.UpdateOutline(true);

                //var hudMan = HudManager.Instance;
                //if (hudMan.HasNo(ETutorialMode.DisableFreeOfficerAssign))
                //{
                //    for (int i = 0; i <= (int)EIslandRoomType.OperationsRoom; i++)
                //    {
                //        if (i != (int)EIslandRoomType.CIC || hudMan.HasNo(ETutorialMode.DisableAssignOfficerToCIC))
                //        {
                //            /*IslandRooms[(EIslandRoomType)i].PossibleObject.SetActive(selectedOfficer.CanBeAssignedToCategory(IslandRooms[(EIslandRoomType)i].RoomCategory));*/
                //        }
                //    }
                //}
                //else
                //{
                //    EIslandRoomType room = DemoMissionGame.Instance.RoomToChoose;
                //    if (room >= EIslandRoomType.FlagPlottingRoom)
                //    {
                //        //IslandRooms[room].PossibleObject.SetActive(selectedOfficer.CanBeAssignedToCategory(IslandRooms[room].RoomCategory));
                //    }
                //}
            }
        }
    }

    public int ShipPoints
    {
        get => navyPoints;
        set
        {
            navyPoints = value;
            navySkillLvl.text = navyPoints.ToString();
            PointsChanged();
        }
    }

    public int AirPoints
    {
        get => airPoints;
        set
        {
            airPoints = value;
            airSkillLvl.text = airPoints.ToString();
            PointsChanged();
        }
    }

    public int OfficersEnabled
    {
        get => officersEnabled;
        set
        {
            officersEnabled = value;
            PortraitManager.Instance.OfficersEnabled = value;
            for (int i = 0; i < currentOfficersList.Count; i++)
            {
                buttons[i].SetSelectedEnable((value & (1 << i)) != 0);
            }
        }
    }

    public EIslandRoomFlag IslandsEnabled
    {
        get => islandsEnabled;
        set
        {
            islandsEnabled = value;
            foreach (var pair in IslandRooms)
            {
                pair.Value.SetSelectedEnable(((int)value & (1 << (int)pair.Key)) != 0);
            }
        }
    }

    public int SwitchesEnabled
    {
        get => switchesEnabled;
        set
        {
            switchesEnabled = value;
            foreach (var room in IslandRooms.Values)
            {
                room.IslandUI.SetEnableSwitches(value);
            }
        }
    }

    public bool AssignOfficersMode
    {
        get;
        private set;
    }

    public float BuffsTimeModifier
    {
        get;
        set;
    } = 1f;

    public bool CountTicksSinceLastAttack
    {
        get;
        set;
    }

    public int TicksSinceLastAttack
    {
        get;
        set;
    }

    public bool BuffsPanelOpen
    {
        get;
        set;
    }

    public OrderPanelCall CurrentBuffButton
    {
        get;
        set;
    }

    public bool DisableBuffClose
    {
        get => disableBuffClose;
        set
        {
            disableBuffClose = value;
            foreach (var button in uiManager.SelectedBuffButtons)
            {
                button.SetInteractable(false);
            }
        }
    }

    public bool DisableBuffDeallocation
    {
        get;
        set;
    }

    public bool DisableOfficerDeallocation
    {
        get;
        set;
    }

    public List<IslandBuff> UnlockedBuffs
    {
        get;
        private set;
    } = new List<IslandBuff>();

    public EWindowType Type => EWindowType.Other;

    public IslandBuff CurrentBuff => currentBuff;
    public OfficerList OfficerList => officerList;

    public HashSet<Officer>.Enumerator OfficersEnumerator => officers.GetEnumerator();

    public List<RenownReward> RenownRewards => renownRewards;
    public List<int> AdmiralOrderChoiseRewards => admiralOrderChoiseRewards;
    public List<IslandBuff> IslandBuffsList => islandBuffsList;
    public List<Officer> CurrentOfficersList => currentOfficersList;

    public Sprite NavyIcon;
    public Sprite AirIcon;
    public Sprite NavyIconBackground;
    public Sprite AirIconBackground;

    public Sprite NavyPortraitBg;
    public Sprite AirPortraitBg;
    public Sprite BothPortraitBg;

    public Sprite NavyPortraitIcon;
    public Sprite AirPortraitIcon;
    public Sprite BothPortraitIcon;

    public List<Sprite> NavyOfficerBtn = null;
    public List<Sprite> AirOfficerBtn = null;
    public List<Sprite> BothOfficerBtn = null;

    public Sprite QuatersIcon;

    public Color NavyColor = Color.yellow;
    public Color AirColor = Color.blue;
    public Color MixColor = Color.white;

    [NonSerialized]
    public Dictionary<EIslandBuff, IslandBuff> IslandBuffs;
    [NonSerialized]
    public Dictionary<EIslandRoomType, IslandRoom> IslandRooms;

    [SerializeField]
    private Transform islandParentTransform = null;

    [SerializeField]
    private List<IslandRoomSetup> islandRoomSetupsList = null;

    [SerializeField]
    private List<IslandBuff> islandBuffsList = null;

    [SerializeField]
    private OfficerList officerList = null;
    [SerializeField]
    private List<EIslandRoomType> tutorialStartOfficersRooms = new List<EIslandRoomType>();
    [SerializeField]
    private string admiralDesc = "AdmiralDesc";

    [SerializeField]
    private GameObject officerPrefab = null;
    [SerializeField]
    private GameObject admiralPrefab = null;

    [SerializeField]
    private Transform nodesGameObject = null;

    [SerializeField]
    private List<GameObject> officersModels = null;

    [SerializeField]
    private GameObject officerButton = null;

    [SerializeField]
    private Image curentBuffIcon = null;

    [SerializeField]
    private Text airSkillLvl = null;

    [SerializeField]
    private Text navySkillLvl = null;

    [SerializeField]
    private GameObject InGameUICamera = null;

    [SerializeField]
    private SpriteRenderer pathDotPrefab = null;
    [SerializeField]
    private SpriteRenderer pathEnd = null;
    [SerializeField]
    private float pathSectionSize = .3f;
    [SerializeField]
    private float pathDotSize = .1f;
    [SerializeField]
    private float maxResizePerc = .2f;
    [SerializeField]
    private float spacePerc = .33f;
    [SerializeField]
    private float overlapCap = .04f;

    [SerializeField]
    private Transform dotPathOffset = null;

    [SerializeField]
    private List<RenownReward> renownRewards = null;
    [SerializeField]
    private List<int> admiralOrderChoiseRewards = null;

    [SerializeField]
    private IslandBuffUIElement islandBuffUiPrefab = null;

    [SerializeField]
    private int floorCount = 5;

    private Officer selectedOfficer = null;
    private Officer lastOfficerPath = null;

    private HashSet<Officer> officers;
    private List<Officer> currentOfficersList;

    private Dictionary<EIslandRoomType, IslandRoomSetup> islandRoomsSetup;

    private IslandNodesGenerator islandNodes = null;

    private Dictionary<EDepartments, int> departmentsBoost;

    private int navyPoints;
    private int airPoints;

    private List<IslandBuff> activeBuffs = new List<IslandBuff>();
    private HashSet<IslandBuff> toRemove = new HashSet<IslandBuff>();
    private IslandBuff currentBuff;

    private TimeManager timeManager;

    private List<EIslandRoomType> startingRooms;

    private Officer lastSelectedOfficer;

    private List<SpriteRenderer> usedPath;
    private HashSet<SpriteRenderer> pathPool;

    private bool disabled;
    private List<OfficerButton> buttons;
    private int officersEnabled;
    private EIslandRoomFlag islandsEnabled;
    private int switchesEnabled;

    private HashSet<IslandRoom> availableIslandRooms = new HashSet<IslandRoom>();

    private UIManager uiManager;
    private int availableActiveBuffsCount;

    private bool disableBuffClose;

    protected override void Awake()
    {
        base.Awake();
        Assert.IsNull(Instance);
        Instance = this;
        buttons = new List<OfficerButton>();
        officersEnabled = -1;
        islandsEnabled = (EIslandRoomFlag)(-1);
        switchesEnabled = -1;
    }

    private void Start()
    {
        usedPath = new List<SpriteRenderer>();
        pathPool = new HashSet<SpriteRenderer>();
        WorldMap.Instance.Toggled += OnWorldMapToggled;
        CameraManager.Instance.ViewChanged += OnViewChanged;
        HudManager.Instance.TacticMapOpened += OnTacticMapOpened;
        CrewManager.Instance.CrewPanelOpened += OnCrewPanelOpened;
        uiManager = UIManager.Instance;
        uiManager.ConfirmOrderButton.onClick.AddListener(() => StartBuff(currentBuff));
        uiManager.ConfirmOrderButton.gameObject.SetActive(false);
        availableActiveBuffsCount = (int)SaveManager.Instance.Data.SelectedAircraftCarrier + 1;
        uiManager.BuffsListTip.SetTipText(0);
        for (int i = 0; i < uiManager.SelectedBuffButtons.Count; i++)
        {
            uiManager.SelectedBuffButtons[i].gameObject.SetActive(i < availableActiveBuffsCount);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Hide();
        }
        foreach (var buff in uiManager.SelectedBuffButtons)
        {
            if (buff.IslandBuff != null)
            {
                buff.UpdateCooldown();
            }
        }
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
        foreach (var enableable in buttons)
        {
            enableable.SetEnable(enable);
        }
        PortraitManager.Instance.SetEnable(enable);
        if (!enable)
        {
            SelectedOfficer = null;
        }
    }

    public void Hide()
    {
        //HideOrderPanel();
    }

    public void HideOrderPanel()
    {
        CancelBuffSetup();
        foreach (var button in uiManager.SelectedBuffButtons)
        {
            button.SetSelected(false);
        }
    }

    public void Setup(SOTacticMap map)
    {
        Assert.IsFalse(disabled);

        foreach (var officer in officerList.Officers)
        {
            officer.Init();
        }

        HudManager.Instance.RightButtonClick += CheckCancelBuffSetup;

        timeManager = TimeManager.Instance;
        timeManager.AddTickable(this);
        timeManager.MinutePassed += OnMinutePassed;

        if (islandParentTransform == null)
        {
            Debug.LogError("In IslandsAndOfficersManager: islandParentTransform must be assigned");
        }

        islandRoomsSetup = new Dictionary<EIslandRoomType, IslandRoomSetup>();
        IslandRooms = new Dictionary<EIslandRoomType, IslandRoom>();
        officers = new HashSet<Officer>();
        currentOfficersList = new List<Officer>();
        departmentsBoost = new Dictionary<EDepartments, int>();

        for (int i = 0; i < (int)EDepartments.Count; i++)
        {
            departmentsBoost.Add((EDepartments)i, 0);
        }

        foreach (IslandRoomSetup setup in islandRoomSetupsList)
        {
            if (islandRoomsSetup.ContainsKey(setup.roomType))
            {
                Debug.LogError("In IslandsAndOfficersManager: there is two setup for the same room");
            }
            islandRoomsSetup.Add(setup.roomType, setup);
        }
        var data = SaveManager.Instance.Data;
        var lastSwitches = map.Overrides.DefaultSwitchesValues;
        if (lastSwitches == null || lastSwitches.Count == 0)
        {
            lastSwitches = data.LastSwitches;
            if (lastSwitches != null && lastSwitches.Count == 0)
            {
                lastSwitches = null;
            }
        }

        foreach (Transform t in islandParentTransform)
        {
            if (t.TryGetComponent(out IslandRoom room))
            {
                IslandRooms.Add(room.RoomType, room);

                room.Setup(islandRoomsSetup[room.RoomType], lastSwitches == null ? -1 : lastSwitches[(int)room.RoomType]);
            }
        }

        ShipPoints = 0;
        AirPoints = 0;

        //testSwitchesUI.Setup(this);

        islandNodes = new IslandNodesGenerator(nodesGameObject, IslandRooms, floorCount);

        if (IslandRooms[EIslandRoomType.Bridge].AlternativeNodes != null)
        {
            Officer admiral = Instantiate(officerPrefab, transform).GetComponent<Officer>();
            admiral.IsAdmiral = true;
            this.officers.Add(admiral);
            currentOfficersList.Add(admiral);
            GameObject admiralModel = Instantiate(admiralPrefab);

            if (data.AdmiralVisual.Count > 0)
            {
                Transform admiralModelTransform = admiralModel.transform;
                foreach (Transform t in admiralModelTransform)
                {
                    t.gameObject.SetActive(false);
                }
                foreach (int childIndex in data.AdmiralVisual)
                {
                    if (admiralModelTransform.childCount > childIndex)
                    {
                        admiralModelTransform.GetChild(childIndex).gameObject.SetActive(true);
                    }
                }
            }
            var officersIndices = SaveManager.Instance.Data.IntermissionData.OfficerData.Selected;
            var overridenOfficersData = map.Overrides.OfficersData;
            if (overridenOfficersData != null)
            {
                startingRooms = overridenOfficersData;
            }
            else if (data.GameMode == EGameMode.Tutorial)
            {
                startingRooms = tutorialStartOfficersRooms;
            }
            else
            {
                startingRooms = new List<EIslandRoomType>(data.OfficersLastRooms);
                int officersCount = officersIndices.Count + 1;
                if (officersCount > startingRooms.Count)
                {
                    var set = new HashSet<EIslandRoomType>();
                    foreach (var setup in islandRoomSetupsList)
                    {
                        set.Add(setup.roomType);
                    }
                    set.Remove(EIslandRoomType.OrdersRoom);
                    foreach (var room in startingRooms)
                    {
                        set.Remove(room);
                    }
                    while (officersCount > startingRooms.Count)
                    {
                        var value = RandomUtils.GetRandom(set);
                        set.Remove(value);
                        startingRooms.Add(value);
                    }
                }
            }
            int prevRoomIndex = 0;
            int prevRoomIndex2 = 10000;
            var roomType = startingRooms[0];
            if (roomType == EIslandRoomType.OrdersRoom)
            {
                prevRoomIndex2 = prevRoomIndex++;
            }

            var admiralSkills = new List<OfficerSkill>() { new OfficerSkill(EOfficerSkills.CommandingAirForce, -1), new OfficerSkill(EOfficerSkills.CommandingNavy, -1) };
            admiral.SetupOfficer(data.AdmiralName, admiralDesc, IslandRooms[roomType], admiralSkills, admiralModel, data.AdmiralPortrait, 0, data.AdmiralVoice, "PP041.3", null,
                -1, (data.OfficersPrevRooms.Count > prevRoomIndex2 ? IslandRooms[data.OfficersPrevRooms[prevRoomIndex2]] : null), activeBuffs.Count > 0);

            var button = Instantiate(officerButton).GetComponent<OfficerButton>();
            button.Setup(admiral);
            buttons.Add(button);

            var locMan = LocalizationManager.Instance;
            int count = overridenOfficersData == null ? officersIndices.Count : overridenOfficersData.Count;
            count = Mathf.Min(count, startingRooms.Count - 1);

            for (int i = 0; i < count; ++i)
            {
                var officerSetup = officerList.Officers[i];
                PlayerManeuverData maneuver = null;
                var maneuverLevel = officerSetup.ManeuverLevel;
                int upgradeIndex = -1;
                if (overridenOfficersData == null)
                {
                    upgradeIndex = officersIndices[i];
                    if (upgradeIndex == -1)
                    {
                        continue;
                    }
                    officerSetup = officerList.Officers[upgradeIndex];
                    var officerUpgrades = SaveManager.Instance.Data.MissionRewards.OfficersUpgrades[upgradeIndex];
                    maneuver = TacticManager.Instance.AllPlayerManeuvers[officerSetup.ManeuverIndex];
                    maneuverLevel = officerUpgrades.ManeuverLevel;
                }
                Officer o = Instantiate(officerPrefab, transform).GetComponent<Officer>();
                this.officers.Add(o);
                currentOfficersList.Add(o);

                roomType = startingRooms[i + 1];
                if (roomType == EIslandRoomType.OrdersRoom)
                {
                    prevRoomIndex2 = prevRoomIndex++;
                }
                else
                {
                    prevRoomIndex2 = 10000;
                }

                o.SetupOfficer(locMan.GetText(officerSetup.Name), officerSetup.Description, IslandRooms[roomType], officerSetup.OfficerSkills, officersModels[officerSetup.ModelNumber],
                    officerSetup.PortraitNumber, maneuverLevel, officerSetup.Voice, officerSetup.Title, maneuver, upgradeIndex,
                    (data.OfficersPrevRooms.Count > prevRoomIndex2 ? IslandRooms[data.OfficersPrevRooms[prevRoomIndex2]] : null), activeBuffs.Count > 0);
                button = Instantiate(officerButton).GetComponent<OfficerButton>();
                button.Setup(o);
                buttons.Add(button);
            }

            lastSelectedOfficer = admiral;
        }

        IslandBuffs = new Dictionary<EIslandBuff, IslandBuff>();
        var overridenBuffs = map.Overrides.Buffs;
        foreach (IslandBuff buff in islandBuffsList)
        {
            IslandBuffs.Add(buff.IslandBuffType, buff);
            buff.Setup(Instantiate(islandBuffUiPrefab, uiManager.IslandBuffsParent));
            if (buff.Unlocked)
            {
                UnlockedBuffs.Add(buff);
            }
            if (overridenBuffs != null)
            {
                var overridenData = overridenBuffs.Find((x) => x.Buff == buff.IslandBuffType);
                if (overridenData == null)
                {
                    buff.IslandBuffUIElement.gameObject.SetActive(false);
                }
                else
                {
                    buff.IslandBuffUIElement.gameObject.SetActive(true);
                    if (overridenData.OverrideDurationHours >= 0)
                    {
                        buff.CooldownInTicks = timeManager.TicksForHour * overridenData.OverrideDurationHours;
                    }
                }
            }
        }

        uiManager.BuffsListTip.Init(UnlockedBuffs.Count);
        //if (SaveManager.Instance.Data.GameMode == EGameMode.Sandbox)
        //{
        //    uiManager.SandboxBuffsPanel.Setup();
        //}

        curentBuffIcon.gameObject.SetActive(false);

        CameraManager.Instance.ViewChanged += OnCameraViewChanged;

        LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
        OnLanguageChanged();
        //this.StartCoroutineActionAfterFrames(() => SelectedOfficer = admiral, 2);
    }

    public void LoadData(List<IslandBuffSaveData> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (IslandBuffs.TryGetValue(data[i].CurrentBuff, out var buffObj))
            {
                foreach (var index in data[i].AssignedOfficers)
                {
                    currentOfficersList[index].Assigned = true;
                    currentOfficersList[index].Cooldown = data[i].OfficersCooldown;
                    currentOfficersList[index].Portrait.CooldownTime = currentOfficersList[index].Cooldown;
                    buffObj.AssignedOfficers.Add(currentOfficersList[index]);
                }
                StartBuff(buffObj);
                uiManager.SelectedBuffButtons[i].Load(data[i]);
            }
        }
    }

    public void SaveData(List<IslandBuffSaveData> data)
    {
        data.Clear();
        foreach (var buff in uiManager.SelectedBuffButtons)
        {
            if (buff.IslandBuff != null)
            {
                data.Add(buff.Save());
            }
        }
    }

    public void BlockOfficer(int hours)
    {
        foreach (var officer in officers)
        {
            if (!officer.IsAdmiral)
            {
                officer.BlockForHours(hours);
                return;
            }
        }
    }

    public int GetCurrentDepartmentBoost(EDepartments department)
    {
        return departmentsBoost[department];
    }

    public void UpdateBoost(EDepartments department, int boost)
    {
        departmentsBoost[department] = boost;
        DepartmentBoostUpdated(department, departmentsBoost[department]);
    }

    public void SetEnabledBuffs()
    {
        bool enable = activeBuffs.Count - toRemove.Count < availableActiveBuffsCount;
        foreach (var buff in islandBuffsList)
        {
            buff.IslandBuffUIElement.SetEnabled(enable);
        }
        if (IslandRooms.ContainsKey(EIslandRoomType.ExperimentalTactics))
        {
            bool enableSwitch = activeBuffs.Count - toRemove.Count == 0;
            IslandRooms[EIslandRoomType.ExperimentalTactics].IslandUI.SetBlockedSwitch(1, !enableSwitch);
        }
    }

    public void SetOfficerPortraitsHidden(bool hidden)
    {
        using (var enumer = OfficersEnumerator)
        {
            while (enumer.MoveNext())
            {
                enumer.Current.SetHidden(hidden);
            }
        }
    }

    public void SetOfficerPortraitsHighlighted(bool highlighted)
    {
        using (var enumer = OfficersEnumerator)
        {
            while (enumer.MoveNext())
            {
                enumer.Current.Portrait.SetHighlighted(highlighted);
            }
        }
    }

    public bool StartBuffSetup(EIslandBuff buff, bool callOrderPanel)
    {
        CancelBuffSetup();
        if (IslandBuffs.TryGetValue(buff, out var buffObj) && currentBuff != buffObj)
        {
            currentBuff = buffObj;
            AirPoints = 0;
            ShipPoints = 0;
            AssignOfficersMode = true;
            SelectedOfficer = null;
            navySkillLvl.transform.parent.gameObject.SetActive(true);
            SetOfficerPortraitsHidden(false);
            uiManager.BuffsListTip.SetTipText(1);
            uiManager.ConfirmOrderButton.gameObject.SetActive(true);
            uiManager.ConfirmOrderButton.interactable = false;
            BuffSetupStarted();
        }
        return true;
    }

    public void CheckCancelBuffSetup()
    {
        if (HudManager.Instance.IsLastPopup(this))
        {
            CancelBuffSetup();
        }
    }

    public void CancelBuffSetup()
    {
        if (DisableBuffDeallocation)
        {
            return;
        }
        if (currentBuff != null)
        {
            currentBuff.Cancel();
            currentBuff = null;
            AssignOfficersMode = false;
            ShipPoints = 0;
            SetOfficerPortraitsHighlighted(false);
            SetOfficerPortraitsHidden(true);
            SetShowBuffPanel(true);
            navySkillLvl.transform.parent.gameObject.SetActive(false);
            uiManager.BuffsListTip.SetTipText(0);
            uiManager.ConfirmOrderButton.interactable = false;
            uiManager.ConfirmOrderButton.gameObject.SetActive(false);
        }
    }

    public void StartBuff(IslandBuff currentBuff)
    {
        AirPoints = 0;
        ShipPoints = 0;
        currentBuff.StartBuff();
        var buffButton = CurrentBuffButton != null ? CurrentBuffButton : uiManager.SelectedBuffButtons[activeBuffs.Count];
        buffButton.StartBuff(currentBuff);
        navySkillLvl.transform.parent.gameObject.SetActive(false);
        activeBuffs.Add(currentBuff);
        AssignOfficersMode = false;
        uiManager.ConfirmOrderButton.gameObject.SetActive(false);
        SetShowBuffPanel(true);
        currentBuff = null;
        SetEnabledBuffs();
        BuffConfirmed();
    }

    public void FinishBuff(IslandBuff buff)
    {
        Assert.IsNotNull(buff);
        toRemove.Add(buff);
        buff.FinishBuff();
        SetEnabledBuffs();
        BuffExpired();
    }

    public void AssignOfficer(Officer officer)
    {
        Assert.IsNotNull(currentBuff);
        var button = uiManager.ConfirmOrderButton;
        if (currentBuff.AssignOfficer(officer))
        {
            button.interactable = true;
            uiManager.BuffsListTip.SetTipText(2);
            BuffSetupReady();
        }
        else
        {
            button.interactable = false;
            uiManager.BuffsListTip.SetTipText(1);
        }
    }

    public void RemoveOfficer(Officer officer)
    {
        Assert.IsNotNull(currentBuff);
        officer.Assigned = false;
        uiManager.ConfirmOrderButton.interactable = currentBuff.RemoveOfficer(officer, true);
        currentBuff.AssignedOfficers.Remove(officer);
    }

    public void SetShowBuffPanel(bool show)
    {
        uiManager.CurrentBuffUi.gameObject.SetActive(!show);
        uiManager.BuffsPanel.gameObject.SetActive(show);
        OrderDetailsTooltipCall.Instance.HideOrderDetailsTooltip();
    }

    public void Tick()
    {
        foreach (var officer in officers)
        {
            officer.Tick();
        }
        for (int i = 0; i < uiManager.SelectedBuffButtons.Count; i++)
        {
            if (uiManager.SelectedBuffButtons[i].IslandBuff != null)
            {
                uiManager.SelectedBuffButtons[i].CooldownTick();
            }
        }
        foreach (var buff in toRemove)
        {
            activeBuffs.Remove(buff);
        }
        toRemove.Clear();
        if (CountTicksSinceLastAttack)
        {
            TicksSinceLastAttack++;
            if (TicksSinceLastAttack > timeManager.TicksForHour * 2)
            {
                CountTicksSinceLastAttack = false;
                IslandBuffs[EIslandBuff.CounterAttack].SetInteractable(false);
            }
        }
    }

    public IslandRoom GetRandomAvailableIslandRoom(Officer officer)
    {
        availableIslandRooms.Clear();
        foreach (var room in IslandRooms.Values)
        {
            if (room.RoomType != EIslandRoomType.OrdersRoom && room.CanAddOfficer(officer))
            {
                availableIslandRooms.Add(room);
            }
        }
        Assert.IsTrue(availableIslandRooms.Count > 0);
        return RandomUtils.GetRandom(availableIslandRooms);
    }

    public void OnMinutePassed()
    {
        //if (currentBuff != EIslandBuff.None)
        //{
        //    minutes = minuteCounter.Count(timeManager, SetCooldownText, minutes);
        //    //int savedMinute = this.savedMinute;
        //    //int currentMinute = timeManager.CurrentMinute;
        //    //if (savedMinute < 30)
        //    //{
        //    //    savedMinute += 60;
        //    //}
        //    //if (currentMinute < 30)
        //    //{
        //    //    currentMinute += 60;
        //    //}
        //    //currentMinute -= savedMinute;
        //    //if (Mathf.Abs(currentMinute) < 5)
        //    //{
        //    //    this.savedMinute = timeManager.CurrentMinute;
        //    //    minutes -= currentMinute;
        //    //    minutes = Mathf.Max(minutes, 0);
        //    //    SetCooldownText();
        //    //}
        //}
        //else
        //{
        //    cooldownText.text = "-00";
        //    minutesText.text = "00";
        //}
    }

    public void SetShowPath(Officer officer, bool show)
    {
        if (show)
        {
            lastOfficerPath = officer;
        }
        if (lastOfficerPath == officer)
        {
            ReturnToPool(pathPool, usedPath);
            if (selectedOfficer == null || officer.Path.Count == 0)
            {

            }
            else
            {
                int pathPosCount = officer.Path.Count;
                int start = 0;
                Vector3 scale = pathDotPrefab.transform.localScale;
                var prevPos = officer.Path[0].Position;
                SpriteRenderer last = null;
                for (int i = start + 1; i < pathPosCount; i++)
                {
                    Vector3 nextPos = officer.Path[i].Position;

                    var diff = nextPos - prevPos;
                    diff.x = 0;

                    float dist = diff.magnitude;
                    diff /= dist;
                    dist -= pathSectionSize;


                    float currentSize = pathDotSize * scale.x;
                    int count = Mathf.RoundToInt((((int)(dist / (currentSize * (1f - maxResizePerc)))) + ((int)(dist / (currentSize * (1f + maxResizePerc))))) / 2f);
                    float newDotSize = (dist / count--);
                    float newDotScale = (newDotSize / pathDotSize) / (1f + spacePerc);
                    var dotPos = prevPos + diff * pathSectionSize / 2f;
                    diff *= newDotSize;
                    dotPos += diff;

                    for (int j = 0; j < count; j++)
                    {
                        bool spawnNew = true;
                        Vector3 newPos = new Vector3(pathDotPrefab.transform.position.x, dotPos.y, dotPos.z) + dotPathOffset.transform.localPosition;
                        for (int k = usedPath.Count - 1; k >= 0; --k)
                        {
                            if ((newPos - usedPath[k].transform.position).magnitude < overlapCap)
                            {
                                spawnNew = false;
                                break;
                            }
                        }
                        if (spawnNew)
                        {
                            SpriteRenderer part = GetPathSegment(pathPool, usedPath, pathDotPrefab);
                            part.transform.position = newPos;
                            part.transform.localScale = new Vector3(newDotScale, newDotScale, newDotScale);
                            last = part;
                        }
                        dotPos += diff;
                    }
                    prevPos = nextPos;
                }
                if (last != null)
                {
                    last.enabled = false;
                    pathEnd.transform.position = last.transform.position;
                    pathEnd.transform.localScale = last.transform.localScale;
                    pathEnd.enabled = true;
                    usedPath.Add(pathEnd);
                }
            }
        }
    }

    public void SaveOfficersAndSwitches()
    {
        var data = SaveManager.Instance.Data;
        data.OfficersLastRooms.Clear();
        data.OfficersPrevRooms.Clear();
        using (var enumer = OfficersEnumerator)
        {
            while (enumer.MoveNext())
            {
                data.OfficersLastRooms.Add(enumer.Current.CurrentIslandRoom.RoomType);
                if (enumer.Current.CurrentIslandRoom.RoomType == EIslandRoomType.OrdersRoom)
                {
                    data.OfficersPrevRooms.Add(enumer.Current.LastIslandRoom.RoomType);
                }
            }
        }

        data.LastSwitches.Clear();
        foreach (var room in IslandRooms.Values)
        {
            int index = (int)room.RoomType;
            while (index >= data.LastSwitches.Count)
            {
                data.LastSwitches.Add(0);
            }
            data.LastSwitches[index] = Mathf.Clamp(room.CurrentSwitch, 0, 2);
        }
    }

    public void FireRoomReached(EIslandRoomType type)
    {
        RoomReached(type);
    }

    public void FireSwitchUsed()
    {
        SwitchUsed();
    }

    private void OnLanguageChanged()
    {
        foreach (var pair in IslandRooms)
        {
            if (pair.Key != EIslandRoomType.OrdersRoom)
            {
                pair.Value.OnLanguageChanged();
            }
        }
        /*
         IslandRooms:
            - name
         */
    }

    private void ReturnToPool(HashSet<SpriteRenderer> pool, List<SpriteRenderer> used)
    {
        foreach (var part in used)
        {
            part.enabled = false;
            pool.Add(part);
        }
        used.Clear();
    }

    private SpriteRenderer GetPathSegment(HashSet<SpriteRenderer> pool, List<SpriteRenderer> used, SpriteRenderer prefab)
    {
        if (pool.Count == 0)
        {
            var part = Instantiate(prefab, transform);
            part.enabled = false;
            pool.Add(part);
        }
        using (var enumer = pool.GetEnumerator())
        {
            enumer.MoveNext();
            var result = enumer.Current;
            used.Add(result);
            result.enabled = true;
            pool.Remove(result);
            return result;
        }
    }

    private void OnCameraViewChanged(ECameraView view)
    {
        if (view == ECameraView.Island && !disabled)
        {
            SelectedOfficer = lastSelectedOfficer;
        }
        else
        {
            SelectedOfficer = null;
        }
        InGameUICamera.SetActive(view == ECameraView.Island || view == ECameraView.Sections);
    }

    private void OnWorldMapToggled(bool state)
    {
        if (state)
        {
            foreach (var buff in uiManager.SelectedBuffButtons)
            {
                if (buff.IslandBuff != null)
                {
                    FinishBuff(buff.IslandBuff);
                    buff.FinishBuff();
                }
            }
            foreach (var b in islandBuffsList)
            {
                b.SetInteractable(true);
            }
            UnlockedBuffs.Clear();
            foreach (var buff in islandBuffsList)
            {
                buff.UpdateSandboxSelectedBuffs();
                if (buff.Unlocked)
                {
                    UnlockedBuffs.Add(buff);
                }
            }
            uiManager.BuffsListTip.UpdateTabs(UnlockedBuffs.Count);
            foreach (var officer in officers)
            {
                officer.UpdateValues();
            }
        }
    }

    private void OnTacticMapOpened(bool opened)
    {
        foreach (var button in uiManager.SelectedBuffButtons)
        {
            button.SetInteractable(!opened);
            if (opened)
            {
                button.HideOrderPanel();
            }
        }
    }

    private void OnCrewPanelOpened(bool opened)
    {
        foreach (var button in uiManager.SelectedBuffButtons)
        {
            button.SetInteractable(!opened);
            if (opened)
            {
                button.HideOrderPanel();
            }
        }
    }

    private void OnViewChanged(ECameraView view)
    {
        bool active = view == ECameraView.Island;
        foreach (var path in usedPath)
        {
            path.enabled = active;
        }
    }
}

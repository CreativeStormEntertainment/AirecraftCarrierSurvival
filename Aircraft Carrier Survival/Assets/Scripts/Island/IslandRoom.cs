using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandRoom : MonoBehaviour, IInteractive, IEnableable
{
    public event System.Action<bool> RoomIsReady = delegate { };

    public EIslandRoomType RoomType
    {
        get => roomType;
        set => roomType = value;
    }

    public int CurrentSwitch
    {
        get;
        private set;
    } = -1;

    public int SwitchCount
    {
        get;
        private set;
    }

    public IslandNode MainNode
    {
        get
        {
            if (AlternativeNodes.Count == 0)
            {
                return mainNode;
            }
            foreach (IslandNode node in AlternativeNodes)
            {
                if (node.Occupied)
                {
                    continue;
                }
                return node;
            }
            return mainNode;
        }
        set => mainNode = value;
    }

    public HashSet<IslandNode> AlternativeNodes
    {
        get;
        set;
    }
    public IslandUI IslandUI => islandRoomUI;

    public Sprite RoomIcon => setup.roomIcon;

    [SerializeField]
    private EIslandRoomType roomType;

    [SerializeField]
    private IslandUI islandRoomUI = null;

    [SerializeField]
    private GameObject hover = null;
    [SerializeField]
    private GameObject outline = null;
    [SerializeField]
    private int boostPower = 10;

    private bool showOutline;
    private bool inIslandView;

    private IslandRoomSetup setup;
    private HashSet<Officer> officers;
    private IslandNode mainNode;
    private int currentBoost = 0;

    private Coroutine outlineBlinkCoroutine = null;

    private HashSet<Officer> allOfficers;
    private bool disabled;
    private bool originalDisabled;
    private bool selectedDisabled;

    private void Awake()
    {
        allOfficers = new HashSet<Officer>();
        officers = new HashSet<Officer>();
        AlternativeNodes = new HashSet<IslandNode>();
        hover.SetActive(false);
        outline.SetActive(false);
        if (roomType == EIslandRoomType.OrdersRoom)
        {
            gameObject.SetActive(false);
        }
    }

    //private void Start()
    //{
    //    //hoverRenderer.material.SetColor("Color", RoomCategory == EIslandRoomCategory.AIR ? IslandsAndOfficersManager.Instance.AirColor : IslandsAndOfficersManager.Instance.NavyColor);
    //}

    public void SetEnable(bool enable)
    {
        originalDisabled = !enable;
        enable = enable && !selectedDisabled;
        disabled = !enable;
        islandRoomUI.SetEnable(enable);
        if (!enable)
        {
            hover.SetActive(false);
        }
    }

    public void SetSelectedEnable(bool enable)
    {
        selectedDisabled = !enable;
        SetEnable(!originalDisabled);
    }

    public void Setup(IslandRoomSetup setup, int lastSwitch)
    {
        CameraManager.Instance.ViewChanged += OnCameraViewChanged;

        this.setup = setup;
        SwitchCount = setup.switchCount;
        islandRoomUI.Setup(this, setup.name);
        SetSwitch(lastSwitch == -1 ? setup.defaultSwitch - 1 : lastSwitch);
    }

    public bool CanAddOfficer(Officer officer)
    {
        return !officer.Occupied && allOfficers.Count < setup.maxOfficersAsigned && officer.CurrentIslandRoom != this;
    }

    public void AddOfficer(Officer officer, bool isStart = false, bool showPath = true)
    {
        var islandMan = IslandsAndOfficersManager.Instance;
        if (!officer.Occupied)
        {
            if (isStart || (allOfficers.Count < setup.maxOfficersAsigned && officer.CurrentIslandRoom != this))
            {
                allOfficers.Add(officer);
                if (!isStart)
                {
                    islandMan.PlayEvent(EIslandUIState.Click);
                    VoiceSoundsManager.Instance.PlayPositive(officer.Voice);
                    officer.CurrentIslandRoom.OfficerLeaveRoom(officer);
                    officer.SetIslandRoom(this);
                    officer.StartCooldown();
                    if (showPath)
                    {
                        islandMan.SetShowPath(officer, true);
                    }
                }

                showOutline = true;
                //UpdateOutline();

                //islandRoomUI.SetVisible(true);
                //islandMan.SelectedOfficer = null;
                islandMan.SelectedOfficer = islandMan.SelectedOfficer;
            }
            else if (!isStart)
            {
                VoiceSoundsManager.Instance.PlayPositive(officer.Voice);
                //islandMan.PlayEvent(EIslandUIState.Negative);
                //VoiceSoundsManager.Instance.PlayNegative(officer.Voice);
            }
        }
    }

    public void OnHoverEnter()
    {
        if (disabled)
        {
            return;
        }
        hover.SetActive(true);
        islandRoomUI.SetVisible(true);
        var islandMan = IslandsAndOfficersManager.Instance;
        islandMan.PlayEvent(EIslandUIState.HoverRoom);
        if (islandMan.SelectedOfficer != null)
        {
            if (allOfficers.Count == 0)
            {
                islandRoomUI.SetName(setup.name);
            }
        }

        //int lvl = 0;
        //using (var enumer = officers.GetEnumerator())
        //{
        //    if (enumer.MoveNext())
        //    {
        //        lvl = enumer.Current.GetOfficerLevel(RoomCategory);
        //    }
        //}
    }

    public void OnHoverExit()
    {
        hover.SetActive(false);
        var selected = IslandsAndOfficersManager.Instance.SelectedOfficer;
        islandRoomUI.SetVisible(selected != null && selected.CurrentIslandRoom == this);
        //hoverRenderer.material.color = Color.white;
        if (allOfficers.Count == 0)
        {
            islandRoomUI.SetName(setup.name);
        }
    }

    public void OnHoverStay()
    {
        //TODO
        //Show tooltip
    }

    public float GetHoverStayTime()
    {
        return 5.0f;
    }

    public void OnClickStart()
    {
        if (disabled)
        {
            return;
        }
        /*if(officers.Count > 0 && !officers.Contains(IslandsAndOfficersManager.Instance.SelectedOfficer) && RoomType == EIslandRoomType.OfficersQuarters)
        {
            using (var enumer = officers.GetEnumerator();
            enumer.MoveNext();
            IslandsAndOfficersManager.Instance.SelectedOfficer = enumer.Current;
            return;
        }*/

        var islandMan = IslandsAndOfficersManager.Instance;
        if (islandMan.SelectedOfficer == null)
        {
            islandMan.PlayEvent(EIslandUIState.NoOfficerClick);
        }
        else if (islandMan.SelectedOfficer.Cooldown == 0)
        {
            AddOfficer(islandMan.SelectedOfficer);
        }
    }

    public void OnClickHold()
    {

    }

    public void OnClickEnd(bool success)
    {

    }

    public float GetClickHoldTime()
    {
        return 0f;
    }

    public void OfficerStartedWorking(Officer officer)
    {
        officers.Add(officer);
        RoomIsReady(true);
    }

    public void OfficerLeaveRoom(Officer officer)
    {
        allOfficers.Remove(officer);
        officers.Remove(officer);

        showOutline = officers.Count > 0;
        //UpdateOutline();

        islandRoomUI.SetVisible(hover.activeSelf);

        RoomIsReady(false);
    }

    public int GetOfficerAnimationNumber()
    {
        switch (roomType)
        {
            case EIslandRoomType.Bridge:
                return 1;
            case EIslandRoomType.CIC:
                return 4;
            case EIslandRoomType.CodingRoom:
                return 11;
            case EIslandRoomType.FlagPlottingRoom:
                return 10;
            case EIslandRoomType.MeteorologyRoom:
                return 12;
            case EIslandRoomType.NavigationRoom:
                return 7;
            case EIslandRoomType.OperationsRoom:
                return 6;
            case EIslandRoomType.PilotDebriefingRoom:
                return 9;
            case EIslandRoomType.RadioRoom:
                return 13;
            case EIslandRoomType.ExperimentalTactics:
                return 14;
            case EIslandRoomType.ActiveDefenceRoom:
                return 15;
        }
        return 0;
    }

    public bool SetSwitch(int switchNr)
    {
        int oldSwitch = CurrentSwitch;
        if (CurrentSwitch == switchNr)
        {
            return false;
        }

        switch (roomType)
        {
            case EIslandRoomType.FlagPlottingRoom:
            case EIslandRoomType.PilotDebriefingRoom:
            case EIslandRoomType.CIC:
                break;
            default:
                CurrentSwitch = switchNr;
                break;
        }

        bool canBeSwitched = true;

        EDepartments chosenDepartment = EDepartments.Air;

        var tacMan = TacticManager.Instance;
        var cloudMan = TacticalMapClouds.Instance;
        var deckMan = AircraftCarrierDeckManager.Instance;
        var dcMan = DamageControlManager.Instance;
        var islandMan = IslandsAndOfficersManager.Instance;
        var enemyAttacksMan = EnemyAttacksManager.Instance;
        switch (roomType)
        {
            case EIslandRoomType.FlagPlottingRoom:
                switch (oldSwitch)
                {
                    case 0:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.Airstrike, 0, this);
                        break;
                    case 1:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.Recon, 0, this);
                        break;
                    case 2:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.IdentifyTargets, 0, this);
                        break;
                }
                if (canBeSwitched)
                {
                    CurrentSwitch = switchNr;
                    switch (CurrentSwitch)
                    {
                        case 0:
                            tacMan.UpdateMissionCount(EMissionOrderType.Airstrike, 2, this);
                            break;
                        case 1:
                            tacMan.UpdateMissionCount(EMissionOrderType.Recon, 2, this);
                            break;
                        case 2:
                            tacMan.UpdateMissionCount(EMissionOrderType.IdentifyTargets, 2, this);
                            break;
                    }
                }
                break;

            case EIslandRoomType.PilotDebriefingRoom:
                switch (oldSwitch)
                {
                    case 0:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.Airstrike, 0, this);
                        break;
                    case 1:
                        enemyAttacksMan.SetPilotsDefencePoints(0);
                        break;
                    case 2:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.CarriersCAP, 0, this);
                        break;
                }
                if (canBeSwitched)
                {
                    CurrentSwitch = switchNr;
                    switch (CurrentSwitch)
                    {
                        case 0:
                            tacMan.UpdateMissionCount(EMissionOrderType.Airstrike, 1, this);
                            break;
                        case 1:
                            enemyAttacksMan.SetPilotsDefencePoints(1);
                            break;
                        case 2:
                            tacMan.UpdateMissionCount(EMissionOrderType.CarriersCAP, 1, this);
                            break;
                    }
                }
                break;

            case EIslandRoomType.CIC:
                switch (oldSwitch)
                {
                    case 0:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.CarriersCAP, 0, this);
                        break;
                    case 1:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.Recon, 0, this);
                        break;
                    case 2:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.IdentifyTargets, 0, this);
                        break;
                }
                if (canBeSwitched)
                {
                    CurrentSwitch = switchNr;
                    switch (CurrentSwitch)
                    {
                        case 0:
                            tacMan.UpdateMissionCount(EMissionOrderType.CarriersCAP, 1, this);
                            break;
                        case 1:
                            tacMan.UpdateMissionCount(EMissionOrderType.Recon, 1, this);
                            break;
                        case 2:
                            tacMan.UpdateMissionCount(EMissionOrderType.IdentifyTargets, 1, this);
                            break;
                    }
                }
                break;

            case EIslandRoomType.MeteorologyRoom:
                switch (oldSwitch)
                {
                    case 0:
                        cloudMan.SetShowClouds(false);
                        break;
                    case 1:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.Airstrike, 0, this);
                        break;
                    case 2:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.Recon, 0, this);
                        break;
                }
                switch (CurrentSwitch)
                {
                    case 0:
                        cloudMan.SetShowClouds(true);
                        break;
                    case 1:
                        tacMan.UpdateMissionCount(EMissionOrderType.Airstrike, 1, this);
                        break;
                    case 2:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.Recon, 1, this);
                        break;
                }
                break;

            case EIslandRoomType.Bridge:
                switch (CurrentSwitch)
                {
                    case 0:
                        deckMan.SetRepairFocus(EPlaneType.Fighter);
                        break;
                    case 1:
                        deckMan.SetRepairFocus(EPlaneType.Bomber);
                        break;
                    case 2:
                        deckMan.SetRepairFocus(EPlaneType.TorpedoBomber);
                        break;
                }
                break;

            case EIslandRoomType.NavigationRoom:
                dcMan.AutoDC = true;
                switch (oldSwitch)
                {
                    case 0:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.CarriersCAP, 0, this);
                        break;
                    case 1:
                        break;
                    case 2:
                        dcMan.DespawnDC();
                        break;
                }
                switch (CurrentSwitch)
                {
                    case 0:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.CarriersCAP, 1, this);
                        break;
                    case 1:
                        dcMan.AutoDC = false;
                        break;
                    case 2:
                        dcMan.SpawnDC();
                        break;
                }
                break;

            case EIslandRoomType.RadioRoom:
                switch (CurrentSwitch)
                {
                    case 0:
                        canBeSwitched = dcMan.SetDcButtons(EDcButtons.Floods);
                        break;
                    case 1:
                        canBeSwitched = dcMan.SetDcButtons(EDcButtons.Faults);
                        break;
                    case 2:
                        canBeSwitched = dcMan.SetDcButtons(EDcButtons.Both);
                        break;
                }
                if (!canBeSwitched)
                {
                    CurrentSwitch = oldSwitch;
                }
                break;

            case EIslandRoomType.CodingRoom:
                islandMan.UpdateBoost(EDepartments.Air, 0);
                islandMan.UpdateBoost(EDepartments.Medical, 0);
                islandMan.UpdateBoost(EDepartments.AA, 0);
                currentBoost = boostPower;
                switch (CurrentSwitch)
                {
                    case 0:
                        chosenDepartment = EDepartments.Air;
                        break;
                    case 1:
                        chosenDepartment = EDepartments.Medical;
                        break;
                    case 2:
                        chosenDepartment = EDepartments.AA;
                        break;
                }
                //if (officers.Count > 0)
                //{
                //    using (var enumer = officers.GetEnumerator())
                //    {
                //        enumer.MoveNext();
                //        currentBoost = enumer.Current.GetSkillLevel(RoomCategory);
                //    }
                //}
                islandRoomUI.SetBoost(currentBoost);
                islandRoomUI.SetName(setup.name);
                //islandRoomUI.SetName(setup.name + " + " + currentBoost + "%");
                islandMan.UpdateBoost(chosenDepartment, currentBoost);
                break;

            case EIslandRoomType.OperationsRoom:
                islandMan.UpdateBoost(EDepartments.Deck, 0);
                islandMan.UpdateBoost(EDepartments.Navigation, 0);
                islandMan.UpdateBoost(EDepartments.Engineering, 0);
                currentBoost = boostPower;
                switch (CurrentSwitch)
                {
                    case 0:
                        chosenDepartment = EDepartments.Deck;
                        break;
                    case 1:
                        chosenDepartment = EDepartments.Navigation;
                        break;
                    case 2:
                        chosenDepartment = EDepartments.Engineering;
                        break;
                }

                //if (officers.Count > 0)
                //{
                //    using (var enumer = officers.GetEnumerator())
                //    {
                //        enumer.MoveNext();
                //        currentBoost = enumer.Current.GetSkillLevel(RoomCategory);
                //    }
                //}
                islandRoomUI.SetBoost(currentBoost);
                islandRoomUI.SetName(setup.name + " + " + currentBoost + "%");
                islandRoomUI.SetName(setup.name);
                islandMan.UpdateBoost(chosenDepartment, currentBoost);
                break;

            case EIslandRoomType.ExperimentalTactics:
                switch (oldSwitch)
                {
                    case 0:
                        TimeManager.Instance.AdditionalDayTimeHours = 0;
                        break;
                    case 1:
                        islandMan.BuffsTimeModifier = 1f;
                        break;
                    case 2:
                        StrikeGroupManager.Instance.UpdateCooldowns(1f);
                        break;
                }
                switch (CurrentSwitch)
                {
                    case 0:
                        TimeManager.Instance.AdditionalDayTimeHours = 2;
                        break;
                    case 1:
                        islandMan.BuffsTimeModifier = 1.5f;
                        break;
                    case 2:
                        StrikeGroupManager.Instance.UpdateCooldowns(0.75f);
                        break;
                }
                break;

            case EIslandRoomType.ActiveDefenceRoom:
                switch (oldSwitch)
                {
                    case 0:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.CarriersCAP, 0, this);
                        enemyAttacksMan.SetDefenceRoomDefencePoints(0);
                        break;
                    case 1:
                        canBeSwitched = true;
                        tacMan.RetrievalRangeModifier = 1f;
                        break;
                    case 2:
                        canBeSwitched = tacMan.UpdateMissionCount(EMissionOrderType.NightScouts, 0, this);
                        break;
                }
                if (canBeSwitched)
                {
                    CurrentSwitch = switchNr;
                    switch (CurrentSwitch)
                    {
                        case 0:
                            tacMan.UpdateMissionCount(EMissionOrderType.CarriersCAP, 1, this);
                            enemyAttacksMan.SetDefenceRoomDefencePoints(1);
                            break;
                        case 1:
                            tacMan.RetrievalRangeModifier = 1.2f;
                            break;
                        case 2:
                            tacMan.UpdateMissionCount(EMissionOrderType.NightScouts, 2, this);
                            break;
                    }
                }
                break;
        }
        if (canBeSwitched)
        {
            islandRoomUI.SelectSwitch(switchNr);
            islandMan.FireSwitchUsed();
        }
        return canBeSwitched;
    }

    public void OutlineBlink(bool on)
    {
        if (roomType != EIslandRoomType.OrdersRoom)
        {
            if (on)
            {
                outlineBlinkCoroutine = StartCoroutine(OutlineBlinkCoroutine(0.5f));
            }
            else
            {
                if (outlineBlinkCoroutine != null)
                {
                    StopCoroutine(outlineBlinkCoroutine);
                    outlineBlinkCoroutine = null;
                }

                showOutline = true;
                //UpdateOutline();
            }
        }
    }

    public void OnLanguageChanged()
    {
        var locMan = LocalizationManager.Instance;
        //fullRoomString = locMan.GetText("Occupied");
        //emptyRoomString = locMan.GetText("Empty");
        //description = "<b><size=14><color=#55778cff>" + locMan.GetText("StateTitle") + ":</color></size></b> <b>{0}</b>\n<b><size=14><color=#55778cff>" + locMan.GetText("TypeTitle") + ":</color></size></b> <b>";
        //if (setup.roomCategory == EIslandRoomCategory.AIR)
        //{
        //    description += locMan.GetText("Air");
        //}
        //else if (setup.roomCategory == EIslandRoomCategory.SHIP)
        //{
        //    description += locMan.GetText("Navy");
        //}
        //description += "</b>\n<b><size=14><color=#55778cff>" + locMan.GetText("OfficerSkillTitle") + ":</color></size></b> <b>{1}</b>\n<b><size=14><color=#55778cff>" + locMan.GetText("EffectTitle") + ":</color></size></b>\n";
        //if (roomType != EIslandRoomType.FlagPlottingRoom && roomType != EIslandRoomType.CIC)
        //{
        //    description += locMan.GetText(roomType.ToString() + "Effect");
        //}
        //else
        //{
        //    description += "{2}";
        //    customEffectText = locMan.GetText(roomType.ToString() + "Effect");
        //}
        //description += "\n" + locMan.GetText(roomType.ToString() + "Desc");

        setup.name = locMan.GetText(roomType.ToString());
        islandRoomUI.SetName(setup.name);
    }

    public void UpdateOutline(bool setOn)
    {
        outline.SetActive(setOn && inIslandView);
    }

    private IEnumerator OutlineBlinkCoroutine(float time)
    {
        while (true)
        {
            showOutline = !showOutline;
            //UpdateOutline();
            yield return new WaitForSecondsRealtime(time);
        }
    }

    private void OnCameraViewChanged(ECameraView view)
    {
        inIslandView = view == ECameraView.Island;
        if (officers != null && officers.Count != 0 && officers.Contains(IslandsAndOfficersManager.Instance.SelectedOfficer))
        {
            UpdateOutline(islandRoomUI);
        }
        //UpdateOutline();
    }
}

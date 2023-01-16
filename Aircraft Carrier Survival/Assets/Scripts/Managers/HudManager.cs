using FMODUnity;
using GambitUtils;
using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HudManager : MonoBehaviour, IPopupPanel, IEnableable
{
    public event Action RightButtonClick = delegate { };
    public event Action<bool> TacticMapOpened = delegate { };
    public event Action EscortPanelOpened = delegate { };
    public event Action BuffPanelOpened = delegate { };
    public event Action<EWindowType, bool> WindowStateChanged = delegate { };
    public event Action<int> ShipSpeedChanged = delegate { };

    public static event Action QuickSaved = delegate { };

    public static HudManager Instance;

    public EWindowType Type => EWindowType.Other;

    public float ShipSpeedup => speedModifier * shipSpeeds[currentSpeedIndex];

    public IInteractive CurrentHovered
    {
        get => currentHoveredObject;
        set
        {
            if (CurrentHovered != null)
            {
                CurrentHovered.OnHoverExit();
                if (buttonDown)
                {
                    CurrentHovered.OnClickEnd(false);
                }
            }
            currentHoveredObject = value;
            hoverTimer = 0f;
            holdTimer = 0f;
            if (CurrentHovered != null)
            {
                CurrentHovered.OnHoverEnter();
                hoverStayTime = CurrentHovered.GetHoverStayTime();
                buttonHoldTime = CurrentHovered.GetClickHoldTime();
                if (buttonDown)
                {
                    CurrentHovered.OnClickStart();
                }
            }
        }
    }

    public int TimeIndex
    {
        get => timeIndex;
        private set
        {
            prevTimeIndex = timeIndex;
            timeIndex = value;
        }
    }
    public bool IsSettingsOpened => settingsPanel.gameObject.activeSelf;
    public bool IsTacticMapOpened => tacticMap.activeSelf;
    public SmokeController SmokeController => smokeController;
    public bool CanSetSpeed => isSpeedOn;
    public Settings Settings => settingsPanel;
    public bool BlockChangeShipSpeed
    {
        get;
        private set;
    }

    public TimePanel TimePanel => WorldMap.Instance.gameObject.activeInHierarchy ? UIManager.Instance.WorldMapTimePanel : timePanel;

#if ALLOW_CHEATS
    public bool Invincibility
    {
        get;
        private set;
    }

    public bool ObjectiveLogs
    {
        get;
        private set;
    }
#endif

    public bool OngoingReport => ongoingReport;
    public float EscortSpeedMultiplier => escortSpeedMultiplier;

    public Sprite FighterIcon;
    public Sprite BomberIcon;
    public Sprite TorpedoIcon;

    [NonSerialized]
    public bool AcceptInput = true;
    [NonSerialized]
    public bool BlockTooltips = true;
    [NonSerialized]
    public bool CinematicPlay = false;
    [NonSerialized]
    public ETutorialMode TutorialDisableMode;

    public GameObject Canvas;

    public RadialMenu RadialMenu;

    [SerializeField]
    private SmokeController smokeController = null;
    [SerializeField]
    private Text infoText = null;
    [SerializeField]
    private Text durationText = null;
    [SerializeField]
    private Text effectStatText = null;
    [SerializeField]
    private Text costNavyText = null;
    [SerializeField]
    private Text costAirText = null;
    [SerializeField]
    private Image OrderTooltipIcon = null;

    [SerializeField]
    private Text Title = null;
    private int currentSpeed = 0;
    private int currentMaxSpeed = 2;
    private int objectiveMinSpeed;
    private int objectiveMaxSpeed;
    private bool isSpeedOn = true;
    private int prevShipSpeed;

    [SerializeField]
    private GameObject tacticMap = null;
    [SerializeField]
    private WorldMap worldMap = null;

    private List<string> currentViewsStrings = null;

    [SerializeField]
    private List<float> shipSpeeds = new List<float>() { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f };
    [SerializeField]
    private float escortSpeedMultiplier = 10f;
    [SerializeField]
    private List<Button> shipSpeedButtons = null;
    [SerializeField]
    private RectTransform shipSpeedIndicator = null;
    private int currentSpeedIndex = 0;

    [SerializeField]
    private CurrentViewAmbient ambient = null;

    [SerializeField]
    private SpeedControlSound speedControlSound = null;

    [SerializeField]
    private TimePanel timePanel = null;

    [SerializeField]
    private Image blocker = null;

    [SerializeField]
    private StudioEventEmitter music = null;
    [SerializeField]
    private StudioEventEmitter worldMapMusic = null;
    [SerializeField]
    private StudioEventEmitter musicMap = null;

    [SerializeField]
    private StudioEventEmitter fastForwardMusic = null;

    [SerializeField]
    private Bus soundsBus = null;
    [SerializeField]
    private Bus animsBus = null;

    [SerializeField]
    private EPlaneType wreckTypeCheat = EPlaneType.Bomber;
    [SerializeField]
    private int wreckLevelCheat = 0;
    [SerializeField]
    private int injureCountCheat = 1;

    //[SerializeField]
    //private float firstTimeSpeedMultiplier = 2f;
    //[SerializeField]
    //private float secondTimeSpeedMultiplier = 4f;
    //[SerializeField]
    //private float thirdTimeSpeedMultiplier = 8f;

    [SerializeField]
    private List<TimeSpeeds> timeSpeeds = null;

    [SerializeField]
    private Button objectivesButton = null;

    [SerializeField]
    private Button escortButton = null;

    [SerializeField]
    private OrderPanelCall buffButton = null;

    [SerializeField]
    private TopRightPanel topRightPanel = null;

    [Header("Settings")]
    [SerializeField]
    private KeyCode settingsToggle = KeyCode.Escape;
    [SerializeField]
    private Settings settingsPanel = null;
    [SerializeField]
    private Button settingsBtn = null;
    [SerializeField]
    private ToggleObject deckPanel = null;

    [SerializeField]
    private int cheatEnemyAttackStrength = 1;
    [SerializeField]
    private int cheatEnemyAttackAnim = 0;

    private bool buttonDown = false;

    private float buttonHoldTime = 0f;
    private float hoverStayTime = 0f;

    private float hoverTimer = 0f;
    private float holdTimer = 0;

    private int sectionRoomLayerMask;
    private int officerLayerMask;
    private int islandRoomLayerMask;
    private int freecamLayerMask;
    private int inGameUiMask;

    private IInteractive currentHoveredObject;
    private GameObject currentHoveredGameObject;
    private AudioSource backgroundAudio;

    private ECameraView cameraView;
    private ECameraView cameraViewNoBlend;

    private LocalizationManager locMan;

    private List<RectTransform> shipSpeedButtonsRect = null;
    private Quaternion lastQuat;
    private Quaternion currentQuat;
    private float timeCount;
    private float delta;
    private float deltaDegrees;
    private bool isStopped = true;

    private int prevBlockTimeIndex;
    private int blockTimeIndex;
    private int prevTimeIndex;
    private int timeIndex;

    private float speedModifier = 1f;

    private bool playingMapMusic;
    private bool playingTacticMapMusic;

    private bool silent;
    private bool setuped;

    private bool ongoingReport;
    private bool blockedSpeed;

    private bool blockedSettings;

    private List<IPopupPanel> openedPopups;

    private bool blockTimeSpeedChange = false;

    private List<float> oldSpeeds = null;

    private int blockedSpeedValue;
#if ALLOW_CHEATS
    private int movie;
#endif

    private void Awake()
    {
        Instance = this;
        smokeController.Init();
        WorldMap.Instance = worldMap;
        blocker.gameObject.SetActive(true);
        blocker.enabled = false;
        RadialMenu = Canvas.GetComponentInChildren<RadialMenu>(true);
        sectionRoomLayerMask = LayerMask.GetMask("SectionRoom");
        officerLayerMask = LayerMask.GetMask("Officers");
        islandRoomLayerMask = LayerMask.GetMask("IslandRoom");
        freecamLayerMask = LayerMask.GetMask("CameraSwitchPoint");
        inGameUiMask = LayerMask.GetMask("InGameUI");
        currentViewsStrings = new List<string>();
        locMan = LocalizationManager.Instance;
        shipSpeedButtonsRect = new List<RectTransform>();
        for (int i = 0; i < shipSpeedButtons.Count; i++)
        {
            int index = i;
            shipSpeedButtonsRect.Add(shipSpeedButtons[index].GetComponent<RectTransform>());
            shipSpeedButtons[index].onClick.AddListener(() =>
            {
                ChangeSpeed(index);
                if (!silent)
                {
                    speedControlSound.PlayEvent(ESpeedControlSound.Play);
                }
                isStopped = false;
            });
        }

        escortButton.onClick.AddListener(() => EscortPanelOpened());

        prevTimeIndex = timeIndex = 1;
        objectiveMinSpeed = 0;
        objectiveMaxSpeed = 10;
        SetTooltips();

        animsBus.SetPause(true);

        openedPopups = new List<IPopupPanel>();
    }

    private void Start()
    {
        //backgroundAudio = BackgroundAudio.Instance.SFXSource;
        CameraManager.Instance.ViewChanged += OnCameraViewChanged;
        //SectionRoomManager.Instance.Engines.SectionWorkingChanged += state => { ChangeMaxSpeed(1, true); };
        //ToggleSquadronsPanel();

        //playButton.OnClick();

        SwitchSpeed(true);

        isStopped = true;
    }

    private void Update()
    {
        if (cameraView == ECameraView.Free || cameraView == ECameraView.Deck)
        {
            ambient.SetDistanceToSeaParameter(1f - CameraManager.Instance.DistanceToSea);
        }
        else
        {
            ambient.SetDistanceToSeaParameter(0f);
        }
        //ped.position = Input.mousePosition;
        //raycaster.Raycast(ped, results);
        //foreach (var res in results)
        //{
        //    Debug.Log(res, res.gameObject);
        //    if (res.gameObject.name == "TooltipCaller")
        //    {
        //        string t = "";
        //        var trans = res.gameObject.transform;
        //        while (trans != null)
        //        {
        //            t += trans.name;
        //            trans = trans.parent;
        //        }
        //        Debug.LogWarning(t, res.gameObject.GetComponent<RectTransform>());
        //    }
        //}
        Shader.SetGlobalFloat("_UnscaledTime", Time.unscaledTime * 3f);
        if (shipSpeedIndicator.localRotation != currentQuat)
        {
            timeCount += Time.unscaledDeltaTime;
            delta = (180f / deltaDegrees) * timeCount;
            shipSpeedIndicator.localRotation = Quaternion.Slerp(lastQuat, currentQuat, delta);
        }
        else if (!isStopped)
        {
            speedControlSound.PlayEvent(ESpeedControlSound.Stop);
            isStopped = true;
        }

        if (AcceptInput)
        {
            if (HasNo(ETutorialMode.DisableOfficers) && Input.GetMouseButtonDown(1))
            {
                RightButtonClick();
            }

            if (!HasNo(ETutorialMode.DisableHovers) || EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())// && !RadialMenu.IsPointerOverRadialMenu())
            {
                if (CurrentHovered != null)
                {
                    CurrentHovered = null;
                    currentHoveredGameObject = null;
                }
            }
            else
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //if (Physics.Raycast(ray, out RaycastHit hit1, 1000f, freecamLayerMask))
                //{
                //    SetHovered(hit1.collider);
                //}
                //else
                //{
                //    CurrentHovered = null;
                //    currentHoveredGameObject = null;
                //}
                switch (cameraView)
                {
                    case ECameraView.Island:
                        if (HasNo(ETutorialMode.DisableOfficers) && !(Physics.Raycast(ray, out RaycastHit hit, 1000f, inGameUiMask)) &&
                            (Physics.Raycast(ray, out hit, 1000f, officerLayerMask) ||
                            ((Physics.Raycast(ray, out hit, 1000f, islandRoomLayerMask)) &&
                                (HasNo(ETutorialMode.DisableFreeOfficerAssign) ||
                                    (DemoMissionGame.Instance.RoomToChoose >= EIslandRoomType.FlagPlottingRoom &&
                                    ((object)hit.collider.GetComponent<IInteractive>() == IslandsAndOfficersManager.Instance.IslandRooms[DemoMissionGame.Instance.RoomToChoose]))) &&
                                 (HasNo(ETutorialMode.DisableAssignOfficerToCIC) ||
                                    ((object)hit.collider.GetComponent<IInteractive>() != IslandsAndOfficersManager.Instance.IslandRooms[EIslandRoomType.CIC])))))
                        {
                            SetHovered(hit.collider);
                        }
                        else
                        {
                            CurrentHovered = null;
                            currentHoveredGameObject = null;
                        }
                        break;
                    case ECameraView.Sections:
                        if (HasNo(ETutorialMode.DisableDC) &&
                            (Physics.Raycast(ray, out hit, 1000f, officerLayerMask) ||
                            (Physics.Raycast(ray, out hit, 1000f, sectionRoomLayerMask) &&
                                (HasNo(ETutorialMode.DisableFreeSectionSelection) || ((object)hit.collider.GetComponent<IInteractive>() == DemoMissionGame.Instance.SegmentToSelect)))))
                        {
                            SetHovered(hit.collider);
                        }
                        else
                        {
                            CurrentHovered = null;
                            currentHoveredGameObject = null;
                        }
                        break;
                    case ECameraView.Free:
                        if (Physics.Raycast(ray, out hit, 1000f, freecamLayerMask))
                        {
                            SetHovered(hit.collider);
                        }
                        else
                        {
                            CurrentHovered = null;
                            currentHoveredGameObject = null;
                        }
                        break;
                }

                if (CurrentHovered != null)
                {
                    hoverTimer += Time.unscaledDeltaTime;
                    if (hoverTimer > hoverStayTime)
                    {
                        CurrentHovered.OnHoverStay();
                        if (CurrentHovered == null)
                        {
                            Debug.LogError("NULL1");
                        }
                        hoverTimer = float.NegativeInfinity;
                    }

#if ALLOW_CHEATS
                    if (BasicInput.Instance.Enabled && !worldMap.Container.activeSelf && CurrentHovered is SectionSegment segment)
                    {
                        //fire
                        if (Input.GetKeyDown(KeyCode.Z))
                        {
                            segment.MakeFire();
                        }
                        //water
                        else if (Input.GetKeyDown(KeyCode.X))
                        {
                            segment.MakeFlood(false);
                        }
                        //injured
                        else if (Input.GetKeyDown(KeyCode.RightControl))
                        {
                            if (injureCountCheat == 1)
                            {
                                segment.MakeInjured(EWaypointTaskType.Rescue);
                            }
                            else if (injureCountCheat == 2)
                            {
                                segment.MakeInjured(EWaypointTaskType.Rescue2);
                            }
                            else if (injureCountCheat == 3)
                            {
                                segment.MakeInjured(EWaypointTaskType.Rescue3);
                            }
                            else
                            {
                                UnityEngine.Assertions.Assert.IsTrue(false, "Bad injure value");
                            }
                        }
                        //damage
                        else if (Input.GetKeyDown(KeyCode.V))
                        {
                            segment.MakeDamage();
                        }
                        //door damage
                        //else if (Input.GetKeyDown(KeyCode.B))
                        //{
                        //    foreach (var data in segment.Neighbours)
                        //    {
                        //        if (data.HasDoor() && !data.Door.HasLeak(segment))
                        //        {
                        //            data.Door.MakeLeak();
                        //            break;
                        //        }
                        //    }
                        //}
                        else if (Input.GetKeyDown(KeyCode.N))
                        {
                            if (!segment.Untouchable)
                            {
                                segment.Parent.IsBroken = true;
                            }
                        }
                        if (CurrentHovered == null)
                        {
                            Debug.LogError("NULL2");
                        }
                    }
#endif

                    if (Input.GetMouseButtonDown(0))
                    {
                        var prev = CurrentHovered;
                        CurrentHovered.OnClickStart();
                        //todo click sound
                        //backgroundAudio.PlayOneShot(ClickSFX);
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        holdTimer += Time.unscaledDeltaTime;
                        if (holdTimer > buttonHoldTime)
                        {
                            CurrentHovered.OnClickHold();
                            holdTimer = float.NegativeInfinity;
                            if (CurrentHovered == null)
                            {
                                Debug.LogError("NULL4");
                            }
                        }
                    }

                    if (CurrentHovered != null && Input.GetMouseButtonUp(0))
                    {
                        holdTimer = 0f;
                        CurrentHovered.OnClickEnd(true);
                    }
                }
                //if (Input.GetKeyDown(KeyCode.Return))
                //{
                //    AircraftCarrierDeckManager.Instance.CancelFirstOrder();
                //}
            }

            buttonDown = Input.GetMouseButton(0);
        }
        if (BasicInput.Instance.Enabled && Input.GetKeyDown(settingsToggle))
        {
            if (openedPopups.Count > 0 && !settingsPanel.ClosePopupsDisabled)
            {
                openedPopups[openedPopups.Count - 1].Hide();
            }
            else if (!GameStateManager.Instance.AlreadyShown)
            {
                OnSetShowSettings(!IsSettingsOpened);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            DamageControlManager.Instance.OnDCButtonClicked(null);
        }

#if ALLOW_CHEATS
        if (!BasicInput.Instance.Enabled || worldMap.Container.activeSelf)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            GameStateManager.Instance.ShowMissionSummary(true, EMissionLoseCause.None, "CHEATER");
        }
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            Invincibility = !Invincibility;
            Debug.LogWarning($"Setting invincibility to " + Invincibility);
        }
        else if (Input.GetKeyDown(KeyCode.Equals))
        {
            if (oldSpeeds == null)
            {
                oldSpeeds = new List<float>() { 0f, 200f, 200f, 200f, 200f, 200f, 200f, 200f };
            }
            (shipSpeeds, oldSpeeds) = (oldSpeeds, shipSpeeds);
            Debug.Log(shipSpeeds[1] > 1f ? "Super carrier speed" : "Normal carrier speed");
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            var x = timeSpeeds[3];
            x.speed = x.speed == 32 ? 8 : 32;
            timeSpeeds[3] = x;
            Debug.Log(x.speed == 32 ? "Super speed" : "Normal speed");
        }
        else if (Input.GetKeyDown(KeyCode.Quote))
        {
            TacticManager.Instance.Carrier.Cheat();
        }
        else if (Input.GetKeyDown(KeyCode.Backslash))
        {
            ManeuverCalculator.Cheat = !ManeuverCalculator.Cheat;
            Debug.Log(ManeuverCalculator.Cheat ? "Easy fight" : "Normal fight");
        }
        else if (Input.GetKeyDown(KeyCode.Home))
        {
            AircraftCarrierDeckManager.Instance.SetForceWreck();
        }
        else if (Input.GetKeyDown(KeyCode.End))
        {
            TacticManager.Instance.ToggleFriendImmune();
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            EnemyAttacksManager.Instance.ToggleInvisible();
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            TacticManager.Instance.ToggleFastFriends();
        }
        else if (Input.GetKeyDown(KeyCode.Comma))
        {
            PlaneMovementManager.Instance.CreateKamikaze(true);
        }
        else if (Input.GetKeyDown(KeyCode.Slash) && !AircraftCarrierDeckManager.Instance.HasWreck)
        {
            PlaneMovementManager.Instance.CreateWreck(wreckTypeCheat, Mathf.Clamp(wreckLevelCheat, 0, 2));
        }
        else if (Input.GetKeyDown(KeyCode.RightCommand))
        {
            Debug.LogWarning("Set show enemies");
            foreach (var enemy in GambitUtils.SceneUtils.FindObjectsOfType<TacticalEnemyShip>())
            {
                var go = enemy.RectTransform.gameObject;
                go.SetActive(!go.activeSelf);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            ObjectiveLogs = !ObjectiveLogs;
            Debug.LogWarning($"{(ObjectiveLogs ? "Showing" : "Hiding")} objective logs");
        }
        else if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            GameStateManager.Instance.ShowMissionSummary(false, EMissionLoseCause.SectionsDestroyed, "CHEATER");
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            Time.timeScale = Time.timeScale > .1f ? .1f : 1f;
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            StrikeGroupManager.Instance.DamageRandom(1);
        }
        else if (Input.GetKeyDown(KeyCode.RightShift))
        {
            EnemyAttacksManager.Instance.StartSubmarineAttack(new EnemyAttackData { CurrentTarget = EEnemyAttackTarget.Carrier });
        }
        else if (Input.GetKeyDown(KeyCode.Pause))
        {
            var friendAttack = new EnemyAttackFriendData();
            friendAttack.FriendID = 0;
            EnemyAttacksManager.Instance.StartFriendlyAttack(friendAttack, 0);
        }
        else if (Input.GetKeyDown(KeyCode.PageUp))
        {
            ObjectivesManager.Instance.WriteFinishedObjectives();
        }
        else if (Input.GetKeyDown(KeyCode.F12))
        {
            var movieMan = MovieManager.Instance;
            movieMan.Play(movie);
            movie = (movie + 1) % movieMan.MoviesCount;
        }
        else if (Input.GetKeyDown(KeyCode.F11))
        {
            EnemyAttacksManager.Instance.MakeAttack(cheatEnemyAttackStrength, false, false, 1, EEnemyAttackPower.Submarine, cheatEnemyAttackAnim);
        }
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }
#endif

        var basicInput = BasicInput.Instance;
        var ui = basicInput.UI;
        ui.Map.performed -= MapPressed;
        ui.CrewPanel.performed -= CrewPressed;
        ui.Orders.performed -= OrdersPressed;
        ui.ChangeDeck.performed -= SetDeckMode;
        ui.TimeSpeedDown.performed -= TimeSpeedupCallback2;
        ui.TimeSpeedUp.performed -= TimeSpeedupCallback1;

        var carrierSpeeds = basicInput.CarrierSpeed;
        carrierSpeeds.Stop.performed -= SetInputShipSpeed0;
        carrierSpeeds.DeadSlow.performed -= SetInputShipSpeed1;
        carrierSpeeds.Slow.performed -= SetInputShipSpeed2;
        carrierSpeeds.Half.performed -= SetInputShipSpeed3;
        carrierSpeeds.Full.performed -= SetInputShipSpeed4;
    }

    public void SetEnable(bool enable)
    {
        DamageControlManager.Instance.SetEnableDCButtons(enable);
    }

    public void Setup(EGameMode mode)
    {
        FMODStudio.SetPause(musicMap.EventInstance, true);
        FMODStudio.SetPause(fastForwardMusic.EventInstance, true);
        ChangeSpeed(1);
        setuped = true;
        OnPlayPressed();

        var basicInput = BasicInput.Instance;
        var ui = basicInput.UI;
        ui.Map.performed += MapPressed;
        ui.CrewPanel.performed += CrewPressed;
        ui.Orders.performed += OrdersPressed;
        ui.ChangeDeck.performed += SetDeckMode;
        ui.TimeSpeedDown.performed += TimeSpeedupCallback2;
        ui.TimeSpeedUp.performed += TimeSpeedupCallback1;

        var carrierSpeeds = basicInput.CarrierSpeed;
        carrierSpeeds.Stop.performed += SetInputShipSpeed0;
        carrierSpeeds.DeadSlow.performed += SetInputShipSpeed1;
        carrierSpeeds.Slow.performed += SetInputShipSpeed2;
        carrierSpeeds.Half.performed += SetInputShipSpeed3;
        carrierSpeeds.Full.performed += SetInputShipSpeed4;

        var save = basicInput.QuickSave;
        save.QuickSave.performed += QuickSaveCallback1;
        save.QuickLoad.performed += QuickSaveCallback2;

        settingsPanel.Setup(mode);
    }

    public void OpenObjectives()
    {
        objectivesButton.onClick.Invoke();
    }

    public void OpenBuffPanel()
    {
        buffButton.OpenOrderPanel();
    }

    public void OpenEscortPanel()
    {
        escortButton.onClick.Invoke();
    }

    public void FireBuffPanelChanged(bool opened)
    {
        BuffPanelOpened();
        WindowStateChanged(EWindowType.BuffOrder, opened);
    }

    public void FireWindowStateChanged(EWindowType type, bool opened)
    {
        WindowStateChanged(type, opened);
    }

    public void ToggleBlockTimeButtons(bool value)
    {
        blockTimeSpeedChange = value;
    }

    public void OnPausePressed(bool muteSounds = true)
    {
        TimeIndex = 0;
        OnSpeedButtonPressed(muteSounds);
    }

    public void OnPlayPressed()
    {
        TimeIndex = 1;
        OnSpeedButtonPressed();
    }

    public void OnFastPressed()
    {
        TimeIndex = 2;
        OnSpeedButtonPressed();
    }

    public void OnFastestPressed()
    {
        TimeIndex = 3;
        OnSpeedButtonPressed();
    }

    public void ChangeTimeSpeed(int index)
    {
        TimeIndex = index;
        OnSpeedButtonPressed();
    }

    public void PlayLastSpeed(bool muteSounds = true)
    {
        var prev = prevTimeIndex;
        TimeIndex = prev;
        OnSpeedButtonPressed(muteSounds);
    }

    public void OnSpeedButtonPressed(bool muteSounds = true)
    {
        if (blockTimeSpeedChange)
        {
            return;
        }

        if (blockedSpeed && timeIndex != 0)
        {
            timeIndex = blockTimeIndex;
            if (prevTimeIndex == blockTimeIndex)
            {
                return;
            }
        }
        var timeMan = TimeManager.Instance;
        var narrator = NarratorManager.Instance;
        if (TimeIndex == 0)
        {
            if (narrator != null)
            {
                narrator.SetPause(true);
            }
            TimePanel.SelectButton(timeIndex);
            timeMan.BlockTime();
            soundsBus.SetPause(true);

            FMODStudio.SetPause(fastForwardMusic.EventInstance, true);
            if (muteSounds)
            {
                animsBus.SetPause(true);

                FMODStudio.SetPause(music.EventInstance, true);
                FMODStudio.SetPause(musicMap.EventInstance, true);
                FMODStudio.SetPause(worldMapMusic.EventInstance, true);
                ambient.SetPause(true);
            }
        }
        else
        {
            if (narrator != null)
            {
                narrator.SetPause(false);
            }
            TimePanel.SelectButton(timeIndex);
            if (prevTimeIndex == 0)
            {
                timeMan.UnblockTime();
                soundsBus.SetPause(false);

                if (muteSounds)
                {
                    if (blockedSpeed)
                    {
                        animsBus.SetPause(false);
                    }

                    ambient.SetPause(false);
                }
                SetView();
            }
            else if (prevTimeIndex == (timeSpeeds.Count - 1))
            {
                FMODStudio.SetPause(fastForwardMusic.EventInstance, true);
                soundsBus.SetMute(playingTacticMapMusic);
            }
            timeMan.TimeSpeed = timeSpeeds[TimeIndex].speed;
            if (TimeIndex == timeSpeeds.Count - 1)
            {
                FMODStudio.SetPause(fastForwardMusic.EventInstance, false);
                soundsBus.SetMute(true);
            }
        }
    }

    public void SetBlockSpeed(int speed, bool muteSounds = true)
    {
        if (!blockedSpeed)
        {
            animsBus.SetPause(false);

            prevBlockTimeIndex = TimeIndex;
            blockTimeIndex = speed;
            if (muteSounds)
            {
                ambient.Stop(true);
            }
            TimeIndex = speed;
            OnSpeedButtonPressed(muteSounds);
            blockedSpeed = true;
            TimePanel.SetBlockSpeeds(true);
        }
    }

    public void UnsetBlockSpeed(bool muteSounds = true)
    {
        if (blockedSpeed)
        {
            blockedSpeed = false;
            SetView();

            TimeIndex = prevBlockTimeIndex;
            OnSpeedButtonPressed(muteSounds);
            TimePanel.SetBlockSpeeds(false);
        }
    }

    public void ChangeSpeed(int i = 0)
    {
        if (BlockChangeShipSpeed)
        {
            return;
        }
        int speed = i;

        timeCount = 0;
        currentSpeed = speed;
        lastQuat = shipSpeedIndicator.localRotation;
        currentQuat = shipSpeedButtonsRect[speed].localRotation;
        deltaDegrees = Mathf.Abs(lastQuat.eulerAngles.z - currentQuat.eulerAngles.z);
        UIManager.Instance.SetShipSpeedImage(speed);
        //currentSpeed = (currentSpeed + 1) % shipSpeeds.Count;
        RefreshCurrentSpeed();
        SetTooltips();
    }

    public void SetBlockCarrierSpeed(int speed)
    {
        blockedSpeedValue = speed;
        BlockChangeShipSpeed = false;
        ChangeMaxSpeed(0, false);
        shipSpeedButtons[speed].onClick.Invoke();
        BlockChangeShipSpeed = true;
    }

    public void SetObjectiveMinCarrierSpeed(int speed)
    {
        objectiveMinSpeed = speed;

        silent = true;
        if (isSpeedOn)
        {
            if (currentSpeed < objectiveMinSpeed)
            {
                shipSpeedButtons[objectiveMinSpeed - 1].onClick.Invoke();
            }
        }
        silent = false;
        SetObjectiveMaxCarrierSpeed(objectiveMaxSpeed);
    }

    public void SetObjectiveMaxCarrierSpeed(int speed)
    {
        objectiveMaxSpeed = speed;
        ChangeMaxSpeed(currentMaxSpeed, true);
    }

    public void ChangeMaxSpeed(int max, bool invokeClick)
    {
        if (BlockChangeShipSpeed)
        {
            return;
        }
        silent = true;
        if (isSpeedOn)
        {
            int m = Mathf.Min(max, objectiveMaxSpeed);
            if (currentSpeed >= m && invokeClick)
            {
                shipSpeedButtons[m - 1].onClick.Invoke();
            }
            for (int i = 1; i < shipSpeedButtons.Count; ++i)
            {
                shipSpeedButtons[i].enabled = i >= objectiveMinSpeed && i < m;
            }
        }
        currentMaxSpeed = max;
        silent = false;
        SetTooltips();
    }

    public void SwitchSpeed(bool on)
    {
        isSpeedOn = on;
        bool oldBlockChangeShipSpeed = BlockChangeShipSpeed;
        BlockChangeShipSpeed = false;
        if (isSpeedOn)
        {
            int m = Mathf.Min(currentMaxSpeed, objectiveMaxSpeed);
            for (int i = 1; i < shipSpeedButtons.Count; ++i)
            {
                shipSpeedButtons[i].enabled = i >= objectiveMinSpeed && i < m;
            }

            int value = oldBlockChangeShipSpeed ? prevShipSpeed : Mathf.Clamp(prevShipSpeed, objectiveMinSpeed - 1, m - 1);
            if (value > 0)
            {
                silent = true;
                shipSpeedButtons[value].onClick.Invoke();
                silent = false;
            }
        }
        else
        {
            prevShipSpeed = currentSpeed;
            silent = true;
            ChangeSpeed(0);
            silent = false;
            for (int i = 1; i < shipSpeedButtons.Count; ++i)
            {
                shipSpeedButtons[i].enabled = false;
            }
        }
        BlockChangeShipSpeed = oldBlockChangeShipSpeed;
    }

    public void SetButtonLock(bool locked, bool resetSpeed = true)
    {
        foreach (var button in shipSpeedButtons)
        {
            button.enabled = !locked;
        }
        if (resetSpeed)
        {
            currentSpeed = 0;
            ChangeSpeed(0);
        }
    }

    #region World Map
    public void ShowWorldMap()
    {
        ChangeSpeed(shipSpeedButtons.Count - 1);
        HideTacticMap();
        SetWorldMap(true);
    }

    public void HideWorldMap()
    {
        ChangeSpeed(1);
        HideTacticMap();
        SetWorldMap(false);
    }

    public bool InWorldMap
    {
        get => worldMap.Container.activeSelf;
    }
    #endregion

    #region Tactic Map
    public void HideTacticMap()
    {
        if (tacticMap.activeSelf)
        {
            TacticManager.Instance.Hide();
        }
    }

    public void ToggleTacticMap()
    {
        if (tacticMap.activeSelf)
        {
            ForceSetTacticMap(false);
        }
        else
        {
            ForceSetTacticMap(true);
        }
    }
    #endregion

    public void ForceSetTacticMap(bool set)
    {
        SetWorldMap(false);
        SetTacticMap(set);
        if (set)
        {
            TacticMapOpened(true);
            PopupShown(TacticManager.Instance);
        }
        else
        {
            TacticMapOpened(false);
            PopupHidden(TacticManager.Instance);
        }
    }

    public bool HasNo(ETutorialMode mode)
    {
        return (TutorialDisableMode & mode) != mode;
    }

    public void SetShipSpeedIndex(int index)
    {
        currentSpeedIndex = index;
        ShipSpeedChanged(index);
    }

    public void ShowBuffTooltip(string title, string description, string duration, string effect, int costAir, int costNavy, Image orderImage)
    {
        Title.text = title;
        infoText.text = description;
        durationText.text = duration;
        effectStatText.text = effect;
        costNavyText.text = costNavy.ToString();
        costAirText.text = costAir.ToString();
        OrderTooltipIcon.sprite = orderImage.sprite;
    }

    public void SetCinematic(bool cinematic)
    {
        SetAcceptInput(!cinematic);
        CinematicPlay = cinematic;
        if (cinematic)
        {
            ambient.PlayEvent(ECurrentView.None);
#if UNITY_EDITOR
            ambient.Lock = true;
#endif
        }
        else
        {
#if UNITY_EDITOR
            ambient.Lock = false;
#endif
            SetView();
        }
    }

    public void SetReportPlaying(bool playing)
    {
        ongoingReport = playing;
    }

    public void DisableDragDrop()
    {
        if (DragDrop.CurrentDrag != null)
        {
            DragDrop.CurrentDrag.ForceEndDrag();
        }

        if (DeckOrder.CurrentDrag != null)
        {
            DeckOrder.CurrentDrag.ForceEndDrag();
        }

        DragPlanesManager.Instance.ForceEndDrag();

        if (TacticalMap.Instance != null && TacticalMap.Instance.CourseSettingMode)
        {
            TacticalMap.Instance.EndWaypoint();
        }
    }

    public void OnSetShowSettings(bool show)
    {
        if (!CinematicPlay && !ongoingReport && !blockedSettings && !GameSceneManager.Instance.IsLoading)
        {
            settingsBtn.interactable = !show;

            blocker.enabled = show;

            DisableDragDrop();

            if (show)
            {
                settingsPanel.Show();
                OnPausePressed();
                PopupShown(this);
            }
            else if (settingsPanel.Hide())
            {
                PlayLastSpeed();
                PopupHidden(this);
            }
        }
    }

    public void SetSpeedModifier(float modifier)
    {
        speedModifier = modifier;
    }

    public void SetAcceptInput(bool accept)
    {
        AcceptInput = accept && !tacticMap.activeSelf && !worldMap.Container.activeSelf && !CrewManager.Instance.Shown();
        BlockTooltips = accept;
        if (!AcceptInput)
        {
            DisableDragDrop();
        }
    }

    public void KillAmbient()
    {
        ambient.Stop(false);
    }

    public void UnpauseMapMusic()
    {
        FMODStudio.SetPause(musicMap.EventInstance, false);
    }

    public void SetBlockSettings(bool block)
    {
        blockedSettings = block;
    }

    public void PopupShown(IPopupPanel popup)
    {
        //Assert.IsFalse(openedPopups.Contains(popup));
        openedPopups.Add(popup);
        WindowStateChanged(popup.Type, true);
    }

    public void PopupHidden(IPopupPanel popup)
    {
        openedPopups.Remove(popup);
        WindowStateChanged(popup.Type, false);
    }

    public void Hide()
    {
        OnSetShowSettings(false);
    }

    public void LoadTimeSpeed(int speed)
    {
        TimeIndex = speed;
        OnSpeedButtonPressed();
    }

    public void LoadShip(ref ShipSaveData data)
    {
        ChangeSpeed(data.ShipSpeed);

        if (data.HasAny)
        {
            if (data.ForcedSpeed > -1 || BlockChangeShipSpeed)
            {
                if (data.ForcedSpeed < 0)
                {
                    BlockChangeShipSpeed = false;
                }
                else
                {
                    SetBlockCarrierSpeed(data.ForcedSpeed);
                }
            }

            objectiveMinSpeed = data.MinSpeed;
            objectiveMaxSpeed = data.MaxSpeed;
        }
    }

    public int SaveTimeSpeed()
    {
        return Mathf.Max((timeIndex == 0 ? prevTimeIndex : timeIndex), 1);
    }

    public void SaveShip(ref ShipSaveData data)
    {
        data.ShipSpeed = currentSpeed;

        data.HasAny = true;
        data.ForcedSpeed = BlockChangeShipSpeed ? blockedSpeedValue : -1;
        data.MinSpeed = objectiveMinSpeed;
        data.MaxSpeed = objectiveMaxSpeed;
    }

    public bool IsLastPopup(IPopupPanel popup)
    {
        return openedPopups.Count > 0 && openedPopups.IndexOf(popup) == (openedPopups.Count - 1);
    }

    public void SetSuperTimeSpeed()
    {
        var timeData = timeSpeeds[3];
        timeData.speed = 24;
        timeSpeeds[3] = timeData;
    }

    private void RefreshCurrentSpeed()
    {
        SetShipSpeedIndex(currentSpeed);
    }

    private void SetTacticMap(bool set)
    {
        tacticMap.SetActive(set);
        SetAcceptInput(true);
        //tacticMapButton.interactable = !set;

        SetView();
    }

    private void SetWorldMap(bool set)
    {
        if (worldMap.Container.activeSelf == set)
        {
            return;
        }
        worldMap.Toggle(set);
        SetAcceptInput(true);
        SetView();
        //if (set)
        //{
        //    worldMap.Prepare();
        //}
    }

    private void OnCameraViewChanged(ECameraView cameraView)
    {
        if (cameraView >= ECameraView.Blend)
        {
            if (setuped)
            {
                BackgroundAudio.Instance.PlayEvent(EMainSceneUI.ChangeView);
            }
        }
        else
        {
            cameraViewNoBlend = cameraView;
        }
        this.cameraView = cameraView;
        CurrentHovered = null;
        currentHoveredGameObject = null;
        SetView();
    }

    private void SetHovered(Collider collider)
    {
        if (currentHoveredGameObject != collider.gameObject)// && !RadialMenu.IsPointerOverRadialMenu())
        {
            //todo hover sound
            //backgroundAudio.PlayOneShot(HoverSFX);
            CurrentHovered = collider.GetComponent<IInteractive>();
            currentHoveredGameObject = collider.gameObject;
        }
    }

    private ECurrentView GetCurrentView()
    {
        return tacticMap.activeSelf ? ECurrentView.TacticMap : (worldMap.Container.activeSelf ? ECurrentView.WorldMap : (ECurrentView)cameraView);
    }

    private void SetView()
    {
        var view = GetCurrentView();
        playingMapMusic = view == ECurrentView.WorldMap || view == ECurrentView.TacticMap;
        playingTacticMapMusic = view == ECurrentView.TacticMap;
        FMODStudio.SetPause(worldMapMusic.EventInstance, view != ECurrentView.WorldMap);
        FMODStudio.SetPause(music.EventInstance, playingMapMusic);
        FMODStudio.SetPause(musicMap.EventInstance, view != ECurrentView.TacticMap);

        animsBus.SetMute(view == ECurrentView.TacticMap);
        soundsBus.SetMute(view == ECurrentView.TacticMap || (TimeIndex == timeSpeeds.Count - 1));

        if (view == ECurrentView.Pause)
        {
            ambient.Stop(true);
        }
        else
        {
            ambient.PlayEvent(view);
        }
        //currentViewText.text = currentViewsStrings[(int)cameraViewNoBlend];
    }

    private void SetTooltips()
    {
        for (int i = 0; i < shipSpeedButtons.Count; i++)
        {
            var button = shipSpeedButtons[i].GetComponent<StateTooltip>();
            switch (i)
            {
                case 0:
                    button.ChangeState(ETooltipSpeed.Stop);
                    break;

                case 1:
                    button.ChangeState(ETooltipSpeed.DeadSlow);
                    break;

                case 2:
                    button.ChangeState(ETooltipSpeed.Slow);
                    break;

                case 3:
                    button.ChangeState(ETooltipSpeed.Half);
                    break;

                case 4:
                    button.ChangeState(ETooltipSpeed.Full);
                    break;
            }
        }
        SetNotAvailableTooltip();
        SetCurrentTooltip();
    }

    private void SetNotAvailableTooltip()
    {
        if (currentMaxSpeed != -1)
        {
            for (int i = currentMaxSpeed; i < shipSpeedButtons.Count; i++)
            {
                var button = shipSpeedButtons[i].GetComponent<StateTooltip>();
                switch (i)
                {
                    case 2:
                        button.ChangeState(ETooltipSpeed.NSlow);
                        break;

                    case 3:
                        button.ChangeState(ETooltipSpeed.NHalf);
                        break;

                    case 4:
                        button.ChangeState(ETooltipSpeed.NFull);
                        break;
                }
            }
        }
    }

    private void SetCurrentTooltip()
    {
        var button = shipSpeedButtons[currentSpeed].GetComponent<StateTooltip>();
        switch (currentSpeed)
        {
            case 0:
                button.ChangeState(ETooltipSpeed.CStop);
                break;

            case 1:
                button.ChangeState(ETooltipSpeed.CDeadSlow);
                break;

            case 2:
                button.ChangeState(ETooltipSpeed.CSlow);
                break;

            case 3:
                button.ChangeState(ETooltipSpeed.CHalf);
                break;

            case 4:
                button.ChangeState(ETooltipSpeed.CFull);
                break;
        }
    }

    private void MapPressed(InputAction.CallbackContext _)
    {
        if (IsSettingsOpened || GameStateManager.Instance.AlreadyShown || CinematicPlay || ongoingReport || WorldMap.Instance.gameObject.activeInHierarchy)
        {
            return;
        }
        if ((UIManager.Instance.EnabledCategories & (tacticMap.activeSelf ? EUICategory.CloseWindow : EUICategory.TacticalMap)) != 0)
        {
            if (!tacticMap.activeSelf)
            {
                topRightPanel.HideOthers();

                var crewMan = CrewManager.Instance;
                if (crewMan.Shown())
                {
                    crewMan.ToggleShow();
                }
            }
            ToggleTacticMap();
        }
    }

    private void CrewPressed(InputAction.CallbackContext _)
    {
        if (IsSettingsOpened || GameStateManager.Instance.AlreadyShown || CinematicPlay || ongoingReport)
        {
            return;
        }
        var crewMan = CrewManager.Instance;
        if ((UIManager.Instance.EnabledCategories & (crewMan.Shown() ? EUICategory.CloseWindow : EUICategory.CrewManagement)) != 0)
        {
            if (!crewMan.Shown())
            {
                topRightPanel.HideOthers();
                if (tacticMap.activeSelf)
                {
                    ToggleTacticMap();
                }
            }
            crewMan.ToggleShow();
        }
    }

    private void OrdersPressed(InputAction.CallbackContext _)
    {
        if (IsSettingsOpened || GameStateManager.Instance.AlreadyShown || !AcceptInput)
        {
            return;
        }
        if ((UIManager.Instance.EnabledCategories & EUICategory.DeckOrdersWindow) != 0)
        {
            deckPanel.Toggle();
        }
    }

    private void SetDeckMode(InputAction.CallbackContext _)
    {
        if (IsSettingsOpened || GameStateManager.Instance.AlreadyShown || !AcceptInput)
        {
            return;
        }
        if ((UIManager.Instance.EnabledCategories & EUICategory.DeckOrdersWindow) == 0)
        {
            return;
        }

        var deckOrder = DeckOrderPanelManager.Instance;
        if (AircraftCarrierDeckManager.Instance.DeckMode == EDeckMode.Landing)
        {
            deckOrder.OnLaunchingBtn();
        }
        else
        {
            deckOrder.OnRecoveryBtn();
        }
    }

    private void TimeSpeedupCallback1(InputAction.CallbackContext _)
    {
        TimeSpeedup(true);
    }

    private void TimeSpeedupCallback2(InputAction.CallbackContext _)
    {
        TimeSpeedup(false);
    }

    private void TimeSpeedup(bool up)
    {
        if (IsSettingsOpened || GameStateManager.Instance.AlreadyShown || CinematicPlay || ongoingReport)
        {
            return;
        }
        if ((UIManager.Instance.EnabledCategories & EUICategory.TimeSpeed) == 0)
        {
            return;
        }

        bool isWorldMap = worldMap.gameObject.activeInHierarchy;
        if ((TimeIndex == 0 && !isWorldMap) || blockedSpeed || (up ? (TimeIndex == 3) : (TimeIndex == (isWorldMap ? 0 : 1))))
        {
            return;
        }
        ChangeTimeSpeed(TimeIndex + (up ? 1 : -1));
    }

    private void SetInputShipSpeed0(InputAction.CallbackContext _)
    {
        SetInputShipSpeed(0);
    }

    private void SetInputShipSpeed1(InputAction.CallbackContext _)
    {
        SetInputShipSpeed(1);
    }

    private void SetInputShipSpeed2(InputAction.CallbackContext _)
    {
        SetInputShipSpeed(2);
    }

    private void SetInputShipSpeed3(InputAction.CallbackContext _)
    {
        SetInputShipSpeed(3);
    }

    private void SetInputShipSpeed4(InputAction.CallbackContext _)
    {
        SetInputShipSpeed(4);
    }

    private void SetInputShipSpeed(int i)
    {
        if (IsSettingsOpened || GameStateManager.Instance.AlreadyShown || !AcceptInput)
        {
            return;
        }
        if ((UIManager.Instance.EnabledCategories & EUICategory.CarrierSpeed) == 0)
        {
            return;
        }
        if (shipSpeedButtons[i].enabled)
        {
            ChangeSpeed(i);
        }
    }

    private void QuickSaveCallback1(InputAction.CallbackContext _)
    {
        QuickSaved();
        this.StartCoroutineActionAfterFrames(() =>
        {
            if (GameStateManager.Instance.Tutorial)
            {
                return;
            }
            GameSceneManager.Instance.UpdateSave();
            SaveManager.Instance.SaveData("Quicksave");
        }, 2);
    }

    private void QuickSaveCallback2(InputAction.CallbackContext _)
    {
        SaveManager.Instance.LoadLastSave();
    }
}

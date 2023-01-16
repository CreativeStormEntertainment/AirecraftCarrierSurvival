#if UNITY_EDITOR
////#define DEBUG_TUTORIAL
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

public class DemoMissionGame : TutorialPart
{
    public static DemoMissionGame Instance;

    [NonSerialized]
    public SectionSegment SegmentToSelect;
    [NonSerialized]
    public RectTransform EnableWaypointArea;
    [NonSerialized]
    public EIslandRoomType RoomToChoose = (EIslandRoomType)(-1);
    [NonSerialized]
    public DepartmentItem EnabledDepartment;
    [NonSerialized]
    public int EnabledShipIdentificationID = -1;
    [NonSerialized]
    public HashSet<StrategyItem> PickableStrategies;

    [SerializeField]
    private List<Canvas> canvases = null;

    [SerializeField]
    private GameSceneManager gameManager = null;

    [SerializeField]
    private List<Button> settingsButtons = null;

    [SerializeField]
    private SOTacticMap demoMap = null;

    [Header("Stage 3.5")]
    [SerializeField]
    private Button navigationButton = null;
    [SerializeField]
    private List<Button> cameraViewsButtons = null;
    [SerializeField]
    private Button worldMapButton = null;
    [Header("Stage 4")]
    [SerializeField]
    private Button shipSpeedButton = null;

    [Header("Stage 5")]
    [SerializeField]
    private Button islandOrderButton = null;

    [SerializeField]
    private Button resupplyButton = null;

    [Header("DC")]
    [SerializeField]
    private List<SectionSegment> segmentForDCs;
    [SerializeField]
    private SectionSegment segmentToBreak = null;

    [Header("Missions/Deck")]
    [SerializeField]
    private RectTransform reconRetrievalPos = null;
    [SerializeField]
    private Button mapShip = null;
    [SerializeField]
    private Button tacticalMapButton = null;
    [SerializeField]
    private Button mapCloseButton;
    [SerializeField]
    private Button startingDeckButton = null;
    [SerializeField]
    private Button deckModeButton = null;
    [SerializeField]
    private Button landingDeckButton = null;
    [SerializeField]
    private Button missionIButton = null;
    public float MaxDistWaypointSqr = 6400f;

    [Header("Attack")]
    [SerializeField]
    private Button crewManageButton = null;
    [SerializeField]
    private DepartmentItem aaDepartment = null;
    [SerializeField]
    private Button closeCrewManageButton = null;
    [SerializeField]
    private Button enemyNewPositionEvent = null;

    [Header("Tactical Map")]
    [SerializeField]
    private Button enemyButton = null;
    [SerializeField]
    private Button identifyButton = null;
    [SerializeField]
    private Button prevShipButton = null;
    [SerializeField]
    private Button nextShipButton = null;
    [SerializeField]
    private Button confirmMissionButton = null;
    [SerializeField]
    public List<StrategyItem> unlockedStrategies = null;
    [SerializeField]
    private StrategyPanel strategyPanel = null;

    [Header("Attack Mission")]
    [SerializeField]
    private Button briefMissionPopupButton = null;
    [SerializeField]
    private Button aircraftAttackButton = null;
    [SerializeField]
    private RectTransform attackPos = null;
    [SerializeField]
    private RectTransform attackRetrievalPos = null;
    [SerializeField]
    private Button landingDeckModeButton = null;
    [SerializeField]
    private Button startingMissionIIButton = null;
    [SerializeField]
    private Button dynamicEventButton = null;

    [Header("Fire/Flood")]
    [SerializeField]
    private SectionSegment segmentToFire = null;
    [SerializeField]
    private SectionSegment segmentToFlood = null;

    [Header("Win popup")]
    [SerializeField]
    private Button mainMenuButton = null;

    private List<Button> allButtons;

    private EMainSceneStep step = EMainSceneStep.Start;

    private bool stepOpen;

    private ECameraView desiredView;

    private TacticalMission mission;
    private TacticalMission mission2;
    private PlaneSquadron fighterSquadron;

    private bool checkDC;
    private bool checkRecovery;
    private bool checkMissionReady;
    private bool checkMissionLaunched;
    private bool checkDCAssigned;
    private bool forceZoomToSegment;
    private bool waitForMapClose;
    private bool waitForDepartmentClose;
    private bool waitForAttack;
    private bool waitForDCTimeup;
    private bool waitToChangeDemo;
    private float timer;
    
    private bool teleported;
    private bool skipStep;

    private List<TutorialBlinker> currentHighlights;

    private void Awake()
    {
        Instance = this;

        PickableStrategies = new HashSet<StrategyItem>();

        //SetEnableButton(worldMapButton, false);

        currentHighlights = new List<TutorialBlinker>();
    }

    private void Update()
    {
        if (checkDC)
        {
#warning todo
            //todo
            //var dcGroup = DamageControlManager.Instance.FirstGroup;
            //if (dcGroup.Path.Count == 0 && dcGroup.CurrentSegment == segmentToBreak)
            //{
            //    OnSegmentEntered();
            //}
        }

        timer -= Time.deltaTime;
        if (waitForAttack && timer < 0f)
        {
            waitForAttack = false;
            ActionManager.Instance.DelayAttack(0f, 0, 0);
            NextStep();
        }

        if (waitForMapClose && timer < 0f)
        {
            waitForMapClose = false;
            HudManager.Instance.HideTacticMap();
            NextStep();
        }

        if (waitForDepartmentClose && timer < 0f)
        {
            waitForDepartmentClose = false;

            closeCrewManageButton.onClick.Invoke();

            Enable(ETutorialMode.DisableAttack);
            TimeManager.Instance.TimeSpeed += 8f;
            EnemyAttacksManager.Instance.AttackFinished += OnAttackFinished;
        }

        if (waitForDCTimeup && timer < 0f)
        {
            waitForDCTimeup = false;
            NextStep();
        }

        if (waitToChangeDemo && timer < 0f)
        {
            waitToChangeDemo = false;
            NextStep();
        }

        if (checkRecovery && AircraftCarrierDeckManager.Instance.DeckMode == EDeckMode.Landing)
        {
            var tacMan = TacticManager.Instance;
            foreach (var missionLists in tacMan.Missions.Values)
            {
                if (missionLists[0].MissionStage != EMissionStage.ReadyToRetrieve)
                {
                    if (!teleported)
                    {
                        tacMan.Carrier.TeleportToDestination();
                        teleported = true;
                    }
                }
                else
                {
                    checkRecovery = false;
                    NextStep();
                }
                break;
            }
        }

        if (checkMissionReady && AircraftCarrierDeckManager.Instance.DeckMode == EDeckMode.Starting)
        {
            checkMissionReady = false;
            NextStep();
        }

        if (checkMissionLaunched && mission2.MissionStage >= EMissionStage.Deployed)
        {
            checkMissionLaunched = false;
            NextStep();
        }

        if (checkDCAssigned)
        {
            bool foundPumping = !segmentToFlood.IsFlooding();
            bool foundFirefighting = !segmentToFire.Fire.Exists;

            foreach (var group in DamageControlManager.Instance.CurrentGroups)
            {
                //if (group.InWaterPumps() && group.Job == EWaypointTaskType.Waterpump && group.PumpedSegmentGroup == segmentToFlood.Group)
                {
                    foundPumping = true;
                }
                if ((group.Path.Count > 0 && group.FinalSegment == segmentToFire) || (group.Path.Count == 0 && group.CurrentSegment == segmentToFire))
                {
                    foundFirefighting = true;
                }
            }
            if (foundPumping && foundFirefighting)
            {
                checkDCAssigned = false;
                if (skipStep)
                {
                    ++step;
                    skipStep = false;
                }
                NextStep();
            }
        }

        if (stepOpen && steps[(int)step].IsPressAnyMode && Input.anyKeyDown)
        {
            NextStep();
        }
    }

    public void PreSetup()
    {
        HudManager.Instance.TutorialDisableMode = ETutorialMode.DisableAll;
        //var placement = DamageControlManager.Instance.DCPlacement;
        //placement.AddRange(segmentForDCs);
    }

    public void Setup()
    {
        //AircraftCarrierDeckManager.Instance.SquadronButtonsChanged += OnSquadronButtonsChanged;

        allButtons = new List<Button>();
        foreach (var canvas in canvases)
        {
            foreach (var button in canvas.gameObject.GetComponentsInChildren<Button>(true))
            {
                allButtons.Add(button);
            }
        }
        foreach (var button in settingsButtons)
        {
            allButtons.Remove(button);
        }

        //foreach (var button in squadronsButtons)
        //{
        //    allButtons.Remove(button);
        //}

        TacticManager.Instance.ChangeMapLayout(demoMap, false, false);
        strategyPanel.LockStrategiesForTutorial(unlockedStrategies);

#if DEBUG_TUTORIAL
        CameraManager.Instance.SwitchMode(ECameraView.Island);
        HudManager.Instance.ChangeSpeed();
        step = EMainSceneStep.T18_DCToRepair - 1;
        //tacticalMapButton.onClick.Invoke();
        //waitForAttack = true;
        //timer = 0f;
        NextStep();
        return;
#else
        var movMan = MovieManager.Instance;
        movMan.VideoFinished += OnVideoFinished;
        movMan.Play(0);
#endif
    }

    private void NextStep()
    {
        tutorialPopup.HideTutorialArrow();
        waitForDCTimeup = false;

        HideHighlights();

        ++step;
        stepOpen = true;
        bool pressAny = steps[(int)step].IsPressAnyMode;
        var tacMan = TacticManager.Instance;
        switch (step)
        {
            case EMainSceneStep.T39_3_SectionView:
                Assert.IsFalse(pressAny);
                NavigationStep(ECameraView.Sections);
                break;
            case EMainSceneStep.T39_5_IslandView:
                Assert.IsFalse(pressAny);
                NavigationStep(ECameraView.Island);
                break;
            case EMainSceneStep.T39_7_DeckView:
                Assert.IsFalse(pressAny);
                NavigationStep(ECameraView.Deck);
                break;
            case EMainSceneStep.T9_WorldMap:
                step = EMainSceneStep.T12_IslandOrder - 1;
                NextStep();
                return;
                Assert.IsFalse(pressAny);
                ListenForButton(worldMapButton, OnWorldMapButtonClicked);
                break;
            case EMainSceneStep.T10_2_Waypoint:
                Assert.IsFalse(pressAny);
                var worldMap = WorldMap.Instance;
                worldMap.WaypointAdded += OnWorldWaypointAdded;
                break;
            case EMainSceneStep.T11_1_ShipSpeed:
                Assert.IsFalse(pressAny);
                ListenForButton(shipSpeedButton, OnShipSpeedButtonClicked);
                break;
            case EMainSceneStep.T12_IslandOrder:
                Assert.IsFalse(pressAny);
                ListenForButton(islandOrderButton, OnIslandOrderButtonClicked);
                break;
            case EMainSceneStep.T14_IslandView:
                Assert.IsFalse(pressAny);
                NavigationStep(ECameraView.Island);
                break;
            case EMainSceneStep.T15_3_AssignOfficers:
                Assert.IsFalse(pressAny);
                EnableOfficersPortrait();
                Enable(ETutorialMode.DisableOfficers | ETutorialMode.DisableFreeOfficerAssign);
                IslandsAndOfficersManager.Instance.PointsChanged += OnPointsChanged;
                break;
            case EMainSceneStep.T16_1_Resupply:
                Assert.IsFalse(pressAny);
                ListenForButton(resupplyButton, OnResupplyButtonClicked);
                break;
            case EMainSceneStep.T17_BrokenSegment:
                ObjectivesManager.Instance.SetStepState(0, 0, true);
                Assert.IsFalse(pressAny);
                SetEnableButtons(false);
                foreach (var dc in DamageControlManager.Instance.CurrentGroups)
                {
                    JustListenForButton(dc.Portrait.GetComponent<Button>(), OnDCIdleButtonClicked);
                }
                segmentToBreak.MakeDamage();
                break;
            case EMainSceneStep.T18_DCToRepair:
                Assert.IsFalse(pressAny);
                Enable(ETutorialMode.DisableDC);
                SegmentToSelect = segmentToBreak;
                segmentToBreak.SegmentClicked += OnSegmentClicked;
                break;
            case EMainSceneStep.T19_1_ReconTactical:
                Assert.IsFalse(pressAny);
                ActionManager.Instance.DelayMission(0f, "AircraftRecon", EMissionOrderType.Recon, new List<EPlaneType>() { EPlaneType.Fighter, EPlaneType.Fighter }, 
                    EMissionStage.AwaitingRetrieval, Vector2.zero, reconRetrievalPos.anchoredPosition);
                ListenForButton(tacticalMapButton, OnTacticalMapButtonClicked);
                break;
            case EMainSceneStep.T19_2_RetrievalPosition:
                Assert.IsFalse(pressAny);
                foreach (var missionLists in tacMan.Missions.Values)
                {
                    mission = missionLists[0];
                    break;
                }
                //mission.ButtonMission.Disabled = true;
                //SetEnableButton(mission.MissionButton.Button, false);
                mission.SentSquadrons[1].IsDamaged = true;
                fighterSquadron = mission.SentSquadrons[0];

                EnableWaypointArea = reconRetrievalPos;
                mapShip.onClick.Invoke();
                TacticalMap.Instance.WaypointAdded += OnTacticalWaypointAdded;
                break;
            case EMainSceneStep.T20_1_DeckView:
                Assert.IsFalse(pressAny);
                NavigationStep(ECameraView.Deck);
                break;
            case EMainSceneStep.T20_2_DeckChangeMode:
                Assert.IsFalse(pressAny);
                ListenForButton(deckModeButton, OnDeckModeButtonClicked);
                SetEnableButton(startingDeckButton, true);
                break;
            case EMainSceneStep.T21_QueueMission:
                Assert.IsFalse(pressAny);
                ListenForButton(missionIButton, OnMissionIButtonClicked);
                SetEnableButton(landingDeckButton, true);
                break;
            case EMainSceneStep.T22_Attack_IslandView:
                Assert.IsFalse(pressAny);
                ObjectivesManager.Instance.SetStepState(0, 1, true);

                NavigationStep(ECameraView.Island);
                break;
            case EMainSceneStep.T23_AssignToCIC:
                Assert.IsFalse(pressAny);
                EnableOfficersPortrait();
                Enable(ETutorialMode.DisableOfficers | ETutorialMode.DisableAssignOfficerToCIC);
                RoomToChoose = EIslandRoomType.CIC;
                IslandsAndOfficersManager.Instance.PointsChanged += OnCICChoosed;
                break;
            case EMainSceneStep.T24_CrewManage:
                Assert.IsFalse(pressAny);
                ListenForButton(crewManageButton, OnCrewManageButtonClicked);
                break;
            case EMainSceneStep.T25_2_AssignToAA:
                Assert.IsFalse(pressAny);
                Enable(ETutorialMode.DisableCrewDrag);
                EnabledDepartment = aaDepartment;
                CrewManager.Instance.DepartmentsUnitsChanged += OnDepartmentsUnitsChanged;
                break;
            case EMainSceneStep.T28_NewReport:
                Assert.IsFalse(pressAny);
                ActionManager.Instance.DelayRevealFleet(0f, 0);
                ListenForButton(enemyNewPositionEvent, OnEnemyNewPositionEventClicked);
                break;
            case EMainSceneStep.T29_1_ClickEnemy:
                Assert.IsFalse(pressAny);
                ObjectivesManager.Instance.HideObjectiveStep(0, 1);
                ObjectivesManager.Instance.UnlockObjectiveStep(0, 2, false);
                ListenForButton(enemyButton, OnEnemyButtonClicked);
                break;
            case EMainSceneStep.Dummy_PreT30_Identification:
                Assert.IsFalse(pressAny);

                CloseTutorial();
                Enable(ETutorialMode.DisableFreeFleetIdentification);
                ListenForButton(identifyButton, OnIdentifyButtonClicked);
                SetEnableButton(nextShipButton, true);
                SetEnableButton(prevShipButton, true);

                return;
            case EMainSceneStep.T29_3_ChooseAttackMission:
                Assert.IsFalse(pressAny);
                TacticManager.Instance.HideIdentificationPanel();
                AdvisorPopup.Instance.Hide();
                Enable(ETutorialMode.DisableMissionSet);
                ListenForButton(aircraftAttackButton, OnAircraftAttackButtonClicked);
                break;
            case EMainSceneStep.T31_1_SetAttackWaypoint:
                Assert.IsFalse(pressAny);
                EnableWaypointArea = attackPos;
                TacticalMap.Instance.WaypointAdded += OnAttackWaypointAdded;
                break;
            case EMainSceneStep.Dummy_PostT31_1_SetRetrievalWaypoint:
                Assert.IsFalse(pressAny);
                EnableWaypointArea = attackRetrievalPos;
                TacticalMap.Instance.WaypointAdded += OnRetrievalWaypointAdded;
                AddHighlights();
                return;
            case EMainSceneStep.Dummy_PreT32_SetStrategies:
                Assert.IsFalse(pressAny);

                foreach (var strategy in unlockedStrategies)
                {
                    PickableStrategies.Add(strategy);
                }

                CloseTutorial();
                ListenForButton(confirmMissionButton, OnConfirmMissionButtonClicked);
                return;
            case EMainSceneStep.T32_BriefMission:
                Assert.IsFalse(pressAny);
                PickableStrategies.Clear();
                strategyPanel.UnlockAllStrategiesAfterTutorial();
                foreach (var missionLists in tacMan.Missions.Values)
                {
                    mission2 = missionLists[1];
                    break;
                }
                mission2.JustDestroy = true;
                ListenForButton(briefMissionPopupButton, OnMissionButtonClicked);
                break;
            case EMainSceneStep.T33_DeckState_Rearm:
                Assert.IsFalse(pressAny);
                EnableNavigation(ECameraView.Deck);
                SetEnableButton(landingDeckButton, true);
                SetEnableButton(landingDeckModeButton, true);

                //var fighterButton = fighterSquadron.SquadronUIElement.SquadronButton;
                //SetEnableButton(fighterButton, true);
                //SetEnableButton(fighterButton.SendToHangarButton, false);
                //fighterButton.Clickable = false;
                checkMissionReady = true;
                break;
            case EMainSceneStep.T34_LaunchMission:
                Assert.IsFalse(pressAny);

                var deck = AircraftCarrierDeckManager.Instance;
                if (deck.OrderQueue.Count > 1)
                {
                    deck.CancelOrder(1);
                }
                //deck.CancelFirstOrder();


                SetEnableButtons(false);
                SetEnableButton(startingDeckButton, true);
                SetEnableButton(startingMissionIIButton, true);

                checkMissionLaunched = true;
                TacticManager.Instance.EnemyRouted += OnEnemyRouted;
                break;
            case EMainSceneStep.T35_1_DynamicEvents_SectionView:
                Assert.IsFalse(pressAny);

                CloseTutorial();
                SetEnableButton(mainMenuButton, true);
                GameStateManager.Instance.ShowMissionSummary(true, EMissionLoseCause.None);
                return;
                NavigationStep(ECameraView.Sections);

                JustListenForButton(dynamicEventButton, OnDynamicEventButtonClicked);

                forceZoomToSegment = true;

                segmentToFlood.MakeFlood(false);
                segmentToFlood.Group.Flood.Unfillable = true;

                segmentToFire.MakeFire(false);
                //segmentToFire.Fire.SpreadData.Max = 1e9f;
                break;
            case EMainSceneStep.Dummy_PreT36_AssignDC_Timer:
                Assert.IsFalse(pressAny);

                dynamicEventButton.onClick.RemoveListener(OnDynamicEventButtonClicked);

                CloseTutorial();
                Enable(ETutorialMode.DisableDC | ETutorialMode.DisableFreeSectionSelection | ETutorialMode.DisableDCEvents);
                checkDCAssigned = true;
                waitForDCTimeup = true;
                timer = 30f;
                skipStep = true;
                return;
            case EMainSceneStep.T36_AssignDCOrElse:
                Assert.IsFalse(pressAny);
                skipStep = false;
                break;
            case EMainSceneStep.Dummy_PostT37_EnableAll:
                Assert.IsFalse(pressAny);
                segmentToFlood.Group.Flood.Unfillable = false;
                //segmentToFire.Fire.SpreadData.Max = DamageControlManager.Instance.FireSpreadTime;
                ListenForButton(worldMapButton, OnWorldMapButtonClickedPost);
                LateCloseTutorial();
                SetEnableButtons(true);
                //SetEnableButton(mission.MissionButton.Button, true);
                HudManager.Instance.TutorialDisableMode = 0;
                //AircraftCarrierDeckManager.Instance.SquadronButtonsChanged -= OnSquadronButtonsChanged;
                return;
            case EMainSceneStep.Dummy_PostT40_EnableWorldObjective:
                Assert.IsFalse(pressAny);

                var worldMap2 = WorldMap.Instance;
                worldMap2.WaypointAdded += OnWorldWaypointAddedLastStep;
                LateCloseTutorial();
                break;
            case EMainSceneStep.Dummy_LastChangeMapAndAll:
                //SetEnableButton(worldMapButton, false);
                SetEnableButton(closeCrewManageButton, true);
                HudManager.Instance.HideWorldMap();

                //tacMan.ResetAllMissions();

                LateCloseTutorial();

                gameManager.StartDemo();
                return;
        }
        AddHighlights();

        SetupStep((int)step);
    }

    private void EnableNavigation(ECameraView desired)
    {
        desiredView = desired;
        Enable((ETutorialMode)(1 << (int)desired));
        SetEnableButtons(false);
        SetEnableButton(navigationButton, true);
        SetEnableButton(cameraViewsButtons[(int)desired], true);
    }

    private void NavigationStep(ECameraView desired)
    {
        EnableNavigation(desired);

        CameraManager.Instance.ViewChanged += OnViewChanged;
    }

    private void Enable(ETutorialMode mode)
    {
        var hudMan = HudManager.Instance;
        hudMan.TutorialDisableMode = ETutorialMode.DisableAll;
        hudMan.TutorialDisableMode &= ~mode;
    }

    private void SetEnableButtons(bool set)
    {
        foreach (var button in allButtons)
        {
            if (button != null)
            {
                SetEnableButton(button, set);
            }
        }
    }

    private void SetEnableButton(Button button, bool set)
    {
        button.enabled = set;
    }

    private void ListenForButton(Button button, UnityAction callback)
    {
        SetEnableButtons(false);
        JustListenForButton(button, callback);
    }

    private void JustListenForButton(Button button, UnityAction callback)
    {
        button.onClick.AddListener(callback);
        SetEnableButton(button, true);
    }

    private void CloseTutorial()
    {
        LateCloseTutorial();

        HudManager.Instance.TutorialDisableMode = ETutorialMode.DisableAll;
        SetEnableButtons(false);
    }

    private void LateCloseTutorial()
    {
        tutorialPopup.HideTutorialArrow();
        HideHighlights();

        stepOpen = false;
        Hide();
    }

    private void OnViewChanged(ECameraView view)
    {
        if (view == ECameraView.Blend)
        {
            //CloseTutorial();
        }
        else if (view == desiredView)
        {
            if (forceZoomToSegment)
            {
                forceZoomToSegment = false;
                dynamicEventButton.onClick.Invoke();
            }
            CameraManager.Instance.ViewChanged -= OnViewChanged;
            SetEnableButtons(false);
            HudManager.Instance.TutorialDisableMode = ETutorialMode.DisableAll;
            NextStep();
        }
    }

    private void OnWorldMapButtonClicked()
    {
        SetEnableButtons(false);
        worldMapButton.onClick.RemoveListener(OnWorldMapButtonClicked);
        NextStep();
    }

    private void OnWorldWaypointAdded()
    {
        var worldMap = WorldMap.Instance;
        worldMap.WaypointAdded -= OnWorldWaypointAdded;
        NextStep();
    }

    private void OnWorldWaypointAddedLastStep()
    {
        var worldMap = WorldMap.Instance;
        worldMap.WaypointAdded -= OnWorldWaypointAddedLastStep;
        worldMap.Arrived += OnArrivedLastStep;
        waitToChangeDemo = true;
        timer = 2f;
    }

    private void OnShipSpeedButtonClicked()
    {
        SetEnableButtons(false);
        shipSpeedButton.onClick.RemoveListener(OnShipSpeedButtonClicked);
        WorldMap.Instance.Arrived += OnArrived;
        CloseTutorial();
    }

    private void OnArrived()
    {
        HudManager.Instance.HideWorldMap();
        WorldMap.Instance.Arrived -= OnArrived;

        var movMan = MovieManager.Instance;
        movMan.VideoFinished += OnVideoFinished;
        movMan.Play(1);
    }

    private void OnArrivedLastStep()
    {
        HudManager.Instance.HideWorldMap();
        WorldMap.Instance.Arrived -= OnArrivedLastStep;
    }

    private void OnIslandOrderButtonClicked()
    {
        SetEnableButtons(false);
        islandOrderButton.onClick.RemoveListener(OnIslandOrderButtonClicked);
        NextStep();
    }

    private void OnPointsChanged()
    {
        var islOffMan = IslandsAndOfficersManager.Instance;
        //if (islOffMan.IslandBuffs[EIslandBuff.Resupply].CanBeActivated())
        //{
        //    HudManager.Instance.TutorialDisableMode = ETutorialMode.DisableAll;
        //    islOffMan.PointsChanged -= OnPointsChanged;
        //    NextStep();
        //}
    }

    private void OnResupplyButtonClicked()
    {
        SetEnableButtons(false);
        resupplyButton.onClick.RemoveListener(OnResupplyButtonClicked);
        NextStep();
    }

    private void OnDCIdleButtonClicked()
    {
        SetEnableButtons(false);
        foreach (var dc in DamageControlManager.Instance.CurrentGroups)
        {
            dc.Portrait.GetComponent<Button>().onClick.RemoveListener(OnDCIdleButtonClicked);
        }
        
        desiredView = ECameraView.Sections;
        CameraManager.Instance.ZoomToSectionSegment(segmentToBreak);
        CameraManager.Instance.ViewChanged += OnViewChanged;
    }

    private void OnSegmentClicked()
    {
        HudManager.Instance.TutorialDisableMode = ETutorialMode.DisableAll;
        SegmentToSelect = null;
        segmentToBreak.SegmentClicked -= OnSegmentClicked;
        checkDC = true;
        CloseTutorial();
    }

    private void OnSegmentEntered()
    {
        checkDC = false;
        NextStep();
    }

    private void OnTacticalMapButtonClicked()
    {
        SetEnableButtons(false);
        tacticalMapButton.onClick.RemoveListener(OnTacticalMapButtonClicked);
        NextStep();
    }

    private void OnTacticalWaypointAdded()
    {
        TacticalMap.Instance.WaypointAdded -= OnTacticalWaypointAdded;
        EnableWaypointArea = null;
        CloseTutorial();
        waitForMapClose = true;
        timer = 2f;
    }

    private void OnDeckModeButtonClicked()
    {
        deckModeButton.onClick.RemoveListener(OnDeckModeButtonClicked);
        CloseTutorial();
        checkRecovery = true;
    }

    private void OnMissionIButtonClicked()
    {
        missionIButton.onClick.RemoveListener(OnMissionIButtonClicked);
        CloseTutorial();
        waitForAttack = true;
        timer = 5f;
    }

    private void OnCICChoosed()
    {
        IslandsAndOfficersManager.Instance.PointsChanged -= OnCICChoosed;
        RoomToChoose = (EIslandRoomType)(-1);
        HudManager.Instance.TutorialDisableMode = ETutorialMode.DisableAll;
        NextStep();
    }

    private void OnCrewManageButtonClicked()
    {
        SetEnableButtons(false);
        crewManageButton.onClick.RemoveListener(OnCrewManageButtonClicked);
        NextStep();
    }

    private void OnDepartmentsUnitsChanged()
    {
        var crewMan = CrewManager.Instance;
        if (crewMan.DepartmentDict[EDepartments.AA].UnitsCount == 0)
        {
            return;
        }
        crewMan.DepartmentsUnitsChanged -= OnDepartmentsUnitsChanged;
        EnabledDepartment = null;
        CloseTutorial();
        waitForDepartmentClose = true;
        timer = 5.5f;
    }

    private void OnAttackFinished(EAttackResult result)
    {
        EnemyAttacksManager.Instance.AttackFinished -= OnAttackFinished;
        HudManager.Instance.TutorialDisableMode = ETutorialMode.DisableAll;
        TimeManager.Instance.TimeSpeed -= 8f;

        var advMan = AdvisorPopup.Instance;
        advMan.PopupClosed += OnPopupClosed;
        advMan.Show(result == EAttackResult.Losses ? EAdvisorText.Losses : EAdvisorText.NoLosses);
    }

    private void OnPopupClosed()
    {
        AdvisorPopup.Instance.PopupClosed -= OnPopupClosed;
        NextStep();
    }

    private void OnEnemyNewPositionEventClicked()
    {
        SetEnableButtons(false);
        enemyNewPositionEvent.onClick.RemoveListener(OnEnemyNewPositionEventClicked);
        NextStep();
    }

    private void OnEnemyButtonClicked()
    {
        SetEnableButtons(false);
        enemyButton.onClick.RemoveListener(OnEnemyButtonClicked);
        NextStep();
    }

    private void OnIdentifyButtonClicked()
    {
        var ship = TacticManager.Instance.Fleets[0].FleetUnits[0];
        Assert.IsTrue(ship.enemyType == EEnemyTypeDemo.Unsure);
        if (ship.GuessedId == ship.ShipId)
        {
            SetEnableButtons(false);
            HudManager.Instance.TutorialDisableMode = ETutorialMode.DisableAll;
            identifyButton.onClick.RemoveListener(OnIdentifyButtonClicked);
            NextStep();
        }
        else
        {
            AdvisorPopup.Instance.Show(EAdvisorText.WrongIdentify);
        }
    }

    private void OnAircraftAttackButtonClicked()
    {
        SetEnableButtons(false);
        HudManager.Instance.TutorialDisableMode = ETutorialMode.DisableAll;
        aircraftAttackButton.onClick.RemoveListener(OnAircraftAttackButtonClicked);
        NextStep();
    }

    private void OnAttackWaypointAdded()
    {
        EnableWaypointArea = null;
        TacticalMap.Instance.WaypointAdded -= OnAttackWaypointAdded;
        NextStep();
    }

    private void OnRetrievalWaypointAdded()
    {
        EnableWaypointArea = null;
        TacticalMap.Instance.WaypointAdded -= OnRetrievalWaypointAdded;
        NextStep();
    }

    private void OnAirRaidWaypointsSet()
    {
        TacticalMap.Instance.AirRaidWaypointsSet -= OnAirRaidWaypointsSet;
        HudManager.Instance.TutorialDisableMode = ETutorialMode.DisableAll;

        NextStep();
    }

    private void OnConfirmMissionButtonClicked()
    {
        SetEnableButtons(false);
        confirmMissionButton.onClick.RemoveListener(OnConfirmMissionButtonClicked);
        
        waitForMapClose = true;
        timer = 1f;
    }

    private void OnMissionButtonClicked()
    {
        SetEnableButtons(false);
        briefMissionPopupButton.onClick.RemoveListener(OnMissionButtonClicked);
        NextStep();
    }

    private void OnEnemyRouted(MapEnemyShip _)
    {
        TacticManager.Instance.EnemyRouted -= OnEnemyRouted;
        ObjectivesManager.Instance.SetStepState(0, 2, true);
    }

    private void OnDynamicEventButtonClicked()
    {
        dynamicEventButton.onClick.RemoveListener(OnDynamicEventButtonClicked);
        forceZoomToSegment = false;
    }

    private void OnWorldMapButtonClickedPost()
    {
        worldMapButton.onClick.RemoveListener(OnWorldMapButtonClickedPost);
        //SetEnableButton(worldMapButton, false);
        //SetEnableButton(tacticalMapButton, false);
        NextStep();
    }

    private void OnVideoFinished()
    {
        MovieManager.Instance.VideoFinished -= OnVideoFinished;
        NextStep();
    }

    private void EnableOfficersPortrait()
    {
        var enumer = IslandsAndOfficersManager.Instance.OfficersEnumerator;
        while (enumer.MoveNext())
        {
            enumer.Current.Portrait.Button.enabled = true;
        }
        enumer.Dispose();
    }

    private void AddHighlights()
    {
        currentHighlights.AddRange(steps[(int)step].Highlights);

        foreach (var highglight in currentHighlights)
        {
            highglight.Setup(UnityEngine.Random.Range(0f, tutorialPopup.HighlightTime), tutorialPopup.HighlightTime, tutorialPopup.HighlightCurve);
        }
        steps[(int)step].tasks.Clear();
    }

    private void HideHighlights()
    {
        foreach (var highlight in currentHighlights)
        {
            highlight.gameObject.SetActive(false);
        }
        currentHighlights.Clear();
    }
}

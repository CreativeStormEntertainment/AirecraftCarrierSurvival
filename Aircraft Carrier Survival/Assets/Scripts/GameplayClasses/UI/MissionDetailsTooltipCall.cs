using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionDetailsTooltipCall : MonoBehaviour, IEnableable
{
    public event Action<EMissionOrderType> MissionHovered = delegate { };

    public ButtonMission Button => button;
    public Button CancelButton => cancel;

    public bool Available
    {
        get;
        private set;
    }

    [SerializeField]
    private RectTransform tooltipRect = null;
    [SerializeField]
    private RectTransform rightPanel = null;
    [SerializeField]
    private Image popupBackgroud = null;
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private Text missionTitle = null;
    [SerializeField]
    private Text missionDesc = null;
    [SerializeField]
    private Text timeText = null;
    [SerializeField]
    private Text hoursText = null;
    [SerializeField]
    private Text minutesText = null;
    [SerializeField]
    private GameObject recoveryTimer = null;
    [SerializeField]
    private Text recoveryHoursText = null;
    [SerializeField]
    private Text recoveryMinutesText = null;
    [SerializeField]
    private Text timeTitleText = null;
    [SerializeField]
    private Text fightersText = null;
    [SerializeField]
    private Text bombText = null;
    [SerializeField]
    private Text torpedoText = null;

    [SerializeField]
    private Image tab = null;
    [SerializeField]
    private Sprite lockedTab = null;
    [SerializeField]
    private Sprite normalTab = null;

    [SerializeField]
    private GameObject lockedImage = null;
    [SerializeField]
    private Color disabledColor = new Color();
    [SerializeField]
    private Color normalColor = new Color();
    [SerializeField]
    private Color redColor = new Color();

    [SerializeField]
    private Text missingSquadronsText = null;
    [SerializeField]
    private GameObject nightText = null;
    [SerializeField]
    private GameObject switchesText = null;

    [SerializeField]
    private GameObject recoveryAreaText = null;
    [SerializeField]
    private GameObject deckStateText = null;
    [SerializeField]
    private GameObject deckStateLaunchText = null;
    [SerializeField]
    private GameObject wreckText = null;
    [SerializeField]
    private GameObject suppliesRunout = null;
    [SerializeField]
    private GameObject squadronsContainer = null;

    [SerializeField]
    private GameObject availableContainer = null;

    [SerializeField]
    private string timeToObsNotLocalized = "TimeToObsolete";
    [SerializeField]
    private string timeToRetriveNotLocalized = "TimeToRetrieve";
    [SerializeField]
    private string timeToReturnNotLocalized = "TimeToReturn";

    [SerializeField]
    private GameObject retrieveDisabled = null;
    [SerializeField]
    private Button launch = null;
    [SerializeField]
    private Button cancel = null;
    [SerializeField]
    private Text launchText = null;

    [SerializeField]
    private string prepareButton = "Prepare";
    [SerializeField]
    private string launchButton = "Launch";
    [SerializeField]
    private string readyToRetrieveButton = "Retrieve";
    [SerializeField]
    private string confirm = "Confirm";

    [SerializeField]
    private GameObject squadronGrid = null;
    [SerializeField]
    private List<SquadronToggle> squadronToggles = null;

    [SerializeField]
    private RectTransform defaultRect = null;

    [SerializeField]
    private UseTorpedoToggle useTorpedoesToggle = null;

    private string timeToObs;
    private string timeToRetrieve;
    private string timeToReturn;

    private string prepareButtonText;
    private string launchButtonText;
    private string readyToRetrieveButtonText;
    private string viewResultsButtonText;
    private string confirmText;

    private TacticalMission mission;
    private ButtonMission button;

    private MissionPanel missionPanel;
    private AircraftCarrierDeckManager aircraftCarrierDeckManager;

    private TacticManager tacMan;
    private TimeManager timeMan;

    private EMissionOrderType orderType;

    private bool helperPopup = false;
    //private List<PlaneSquadron> mission.SentSquadrons;

    private bool disabled;

    private void Awake()
    {
        var locMan = LocalizationManager.Instance;

        timeToObs = locMan.GetText(timeToObsNotLocalized);
        timeToRetrieve = locMan.GetText(timeToRetriveNotLocalized);
        timeToReturn = locMan.GetText(timeToReturnNotLocalized);

        prepareButtonText = locMan.GetText(prepareButton);
        launchButtonText = locMan.GetText(launchButton);
        readyToRetrieveButtonText = locMan.GetText(readyToRetrieveButton);
        confirmText = locMan.GetText(confirm);

        //mission.SentSquadrons = new List<PlaneSquadron>();

        for (int i = 0; i < squadronToggles.Count; i++)
        {
            int index = i;
            squadronToggles[i].Clicked += () => OnToggleClicked(index);
        }
        useTorpedoesToggle.Clicked += OnTorpedoesToggleClicked;
    }

    private void Start()
    {
        tacMan = TacticManager.Instance;
        timeMan = TimeManager.Instance;
        timeMan.IsDayChanged += CheckNightTime;
        aircraftCarrierDeckManager = AircraftCarrierDeckManager.Instance;
        CameraManager.Instance.DeckCameraChanged += OnDeckCameraChanged;
        aircraftCarrierDeckManager.DeckModeChanged += CheckDeckState;
        aircraftCarrierDeckManager.WreckChanged += CheckWreckOnDeck;
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
    }

    public void ShowTooltip(RectTransform rect, bool isHelper)
    {
        aircraftCarrierDeckManager.PlaneCountChanged -= CheckSquadrons;
        aircraftCarrierDeckManager.PlaneCountChanged += CheckSquadrons;
        aircraftCarrierDeckManager.DeckSquadronsCountChanged -= CheckSquadrons;
        aircraftCarrierDeckManager.DeckSquadronsCountChanged += CheckSquadrons;
        tooltipRect.SetParent(rect);
        tooltipRect.anchoredPosition = Vector2.zero;
        tooltipRect.gameObject.SetActive(true);
        BackgroundAudio.Instance.PlayEvent(EMainSceneUI.HoverInMissionPanel);
        CheckNightTime();
        if (isHelper)
        {
            cancel.gameObject.SetActive(false);
            launch.gameObject.SetActive(false);
        }
    }

    public void HideTooltip()
    {
        aircraftCarrierDeckManager.PlaneCountChanged -= CheckSquadrons;
        aircraftCarrierDeckManager.DeckSquadronsCountChanged -= CheckSquadrons;
        if (tooltipRect != null)
        {
            tooltipRect.SetParent(null);
            tooltipRect.gameObject.SetActive(false);
        }
        if (mission != null)
        {
            mission.MissionRemoved -= Remove;
            mission.ButtonMissionTimeChanged -= TimeChanged;
            mission.StageChanged -= OnStageChanged;
        }
        BackgroundAudio.Instance.PlayEvent(EMainSceneUI.HoverOutMissionPanel);
    }

    public void SetupHelper(EMissionOrderType orderType)
    {
        helperPopup = true;
        missionTitle.text = tacMan.MissionInfo[orderType].MissionName;
        //missionDesc.text = helperText;
        this.orderType = orderType;

        if (TacticalMission.SwitchPlaneTypeMissions.Contains(orderType))
        {
            useTorpedoesToggle.Setup(false);
        }
        else
        {
            useTorpedoesToggle.gameObject.SetActive(false);
        }

        CheckSquadrons();
        mission = null;
        button = null;
        switchesText.SetActive(true);
        CheckMissionAvailable();
    }

    public void SetupTooltip(TacticalMission m, ButtonMission b)
    {
        helperPopup = false;
        mission = m;
        button = b;

        if (missionPanel == null)
        {
            missionPanel = tacMan.MissionPanel;
        }
        switchesText.SetActive(false);
        mission.MissionRemoved -= Remove;
        mission.ButtonMissionTimeChanged -= TimeChanged;
        mission.StageChanged -= OnStageChanged;
        mission.MissionRemoved += Remove;
        mission.ButtonMissionTimeChanged += TimeChanged;
        mission.StageChanged += OnStageChanged;
        launch.onClick.RemoveListener(Launch);
        cancel.onClick.RemoveListener(Cancel);
        MissionStageDiff(m);
        timeText.gameObject.SetActive(button.Timer.gameObject.activeSelf);
        recoveryTimer.gameObject.SetActive(button.RecoveryTimer.gameObject.activeSelf);
        hoursText.text = recoveryHoursText.text = b.HoursTimer.text;
        recoveryHoursText.text = b.RecoveryHoursTimer.text;
        minutesText.text = recoveryMinutesText.text = b.MinutesTimer.text;
        recoveryMinutesText.text = b.RecoveryMinutesTimer.text;
        fightersText.text = mission.Fighters.ToString();
        bombText.text = mission.Bombers.ToString();
        torpedoText.text = mission.Torpedoes.ToString();
        launch.onClick.AddListener(Launch);
        cancel.onClick.AddListener(Cancel);
        CheckSquadrons();
        CheckRecoveryArea();
        CheckDeckState();
        CheckWreckOnDeck();
        CheckSupplyRunout();

        //mission.SentSquadrons.Clear();

        if (m.MissionStage > EMissionStage.Available)
        {
            MissionHovered(m.OrderType);
        }
    }

    public void ClearIfSelected(TacticalMission m)
    {
        if (mission == m)
        {
            tooltipRect.SetParent(null);
            HideTooltip();
            mission = null;
            Debug.LogError("A3");
        }
    }


    private void OnStageChanged(TacticalMission m)
    {
        MissionStageDiff(m);
        if (mission.MissionStage < EMissionStage.Complete)
        {
            missionPanel.AddMissionButton(m);
        }
    }

    private void CheckMissionAvailable()
    {
        if (mission != null && mission.MissionSent)
        {
            missingSquadronsText.gameObject.SetActive(false);
            nightText.SetActive(false);
            switchesText.SetActive(false);
        }
        Available =
            !nightText.activeSelf &&
            !missingSquadronsText.gameObject.activeSelf &&
            !switchesText.activeSelf &&
            !recoveryAreaText.activeSelf &&
            !deckStateText.activeSelf &&
            !wreckText.activeSelf &&
            !suppliesRunout.activeSelf &&
            !deckStateLaunchText.activeSelf;

        missionDesc.gameObject.SetActive(Available);
        lockedImage.SetActive(!Available);
        tab.sprite = Available ? normalTab : lockedTab;
        popupBackgroud.color = Available ? normalColor : disabledColor;
        availableContainer.SetActive(Available);

        if (squadronGrid.activeSelf)
        {
            UpdateToggles();
        }

        if (rightPanel.gameObject.activeInHierarchy)
        {
            this.StartCoroutineActionAfterFrames(() => LayoutRebuilder.MarkLayoutForRebuild(rightPanel), 1);
        }
    }

    private void CheckSquadrons()
    {
        int bombers = 0;
        int fighters = 0;
        int torpedoes = 0;

        if (helperPopup)
        {
            useTorpedoesToggle.enabled = false;
            tacMan.GetPlanes(orderType, false, out bombers, out fighters, out torpedoes);
        }
        else if (mission != null)
        {
            useTorpedoesToggle.enabled = !disabled;
            useTorpedoesToggle.State = mission.UseTorpedoes;
            useTorpedoesToggle.gameObject.SetActive(mission.MissionStage < EMissionStage.Planned && TacticalMission.SwitchPlaneTypeMissions.Contains(mission.OrderType));
            if (mission.MissionSent)
            {
                bombText.color = Color.white;
                fightersText.color = Color.white;
                torpedoText.color = Color.white;
                CheckMissionAvailable();
                SetGrid(mission.MissionStage >= EMissionStage.AwaitingRetrieval && mission.MissionStage <= EMissionStage.Recovering);
                return;
            }
            mission.GetPlanes(out bombers, out fighters, out torpedoes);
            int planes = bombers + fighters + torpedoes;
            if (mission.MissionStage == EMissionStage.Available && TacticalMission.MissionsWithSetup.Contains(mission.OrderType) ||
                aircraftCarrierDeckManager.FreeSquadrons > 0 && planes <= aircraftCarrierDeckManager.FreeSquadrons ||
                 aircraftCarrierDeckManager.FreeMission == mission.OrderType)
            {
                fightersText.text = fighters.ToString();
                bombText.text = bombers.ToString();
                torpedoText.text = torpedoes.ToString();

                bombText.color = Color.white;
                fightersText.color = Color.white;
                torpedoText.color = Color.white;
                missingSquadronsText.gameObject.SetActive(false);
                CheckMissionAvailable();
                return;
            }
        }
        else
        {
            Debug.LogError("helperPopup == false, mission == null");
        }

        fightersText.text = fighters.ToString();
        bombText.text = bombers.ToString();
        torpedoText.text = torpedoes.ToString();
        bool missingBombers = bombers > aircraftCarrierDeckManager.GetDeckSquadronCount(EPlaneType.Bomber);
        bool missingFighters = fighters > aircraftCarrierDeckManager.GetDeckSquadronCount(EPlaneType.Fighter);
        bool missingTorpedoes = torpedoes > aircraftCarrierDeckManager.GetDeckSquadronCount(EPlaneType.TorpedoBomber);
        bombText.color = missingBombers ? redColor : Color.white;
        fightersText.color = missingFighters ? redColor : Color.white;
        torpedoText.color = missingTorpedoes ? redColor : Color.white;
        missingSquadronsText.gameObject.SetActive(missingBombers || missingFighters || missingTorpedoes);
        CheckMissionAvailable();
    }

    private void CheckNightTime()
    {
        bool allowNightTimeMission = timeMan.IsDay;
        if (mission != null)
        {
            if (mission.MissionStage == EMissionStage.Available)
            {
                allowNightTimeMission = true;
            }
            else
            {
                switch (mission.OrderType)
                {
                    case EMissionOrderType.NightAirstrike:
                    case EMissionOrderType.MagicNightScouts:
                        allowNightTimeMission = true;
                        break;
                    case EMissionOrderType.NightScouts:
                        allowNightTimeMission = true;
                        break;
                }
            }
        }
        nightText.SetActive(!allowNightTimeMission);
        CheckMissionAvailable();
    }

    private void CheckRecoveryArea()
    {
        if (mission != null)
        {
            var outOfRange = mission.MissionStage == EMissionStage.AwaitingRetrieval;
            recoveryAreaText.SetActive(outOfRange && !mission.CanBeRetrievedByEscort());
            CheckMissionAvailable();
        }
    }

    private void CheckDeckState()
    {
        if (mission != null)
        {
            var landingState = aircraftCarrierDeckManager.DeckMode == EDeckMode.Starting;
            if (mission.MissionStage == EMissionStage.ReadyToRetrieve || mission.MissionStage == EMissionStage.AwaitingRetrieval)
            {
                deckStateText.SetActive(landingState && !mission.CanBeRetrievedByEscort());
            }
            else
            {
                deckStateText.SetActive(false);
            }
            if (mission.MissionStage == EMissionStage.ReadyToLaunch)
            {
                deckStateLaunchText.SetActive(!landingState && aircraftCarrierDeckManager.FreeMission != mission.OrderType && aircraftCarrierDeckManager.FreeSquadrons < mission.GetPlanesCount());
            }
            else
            {
                deckStateLaunchText.SetActive(false);
            }
            CheckMissionAvailable();
        }
    }

    private void CheckWreckOnDeck()
    {
        if (mission != null)
        {
            if (!AircraftCarrierDeckManager.Instance.EscortRetrievingSquadrons && (mission.MissionStage == EMissionStage.ReadyToRetrieve || mission.MissionStage == EMissionStage.AwaitingRetrieval))
            {
                wreckText.SetActive(aircraftCarrierDeckManager.HasDamage);
            }
            else if (mission.MissionStage == EMissionStage.ReadyToLaunch)
            {
                wreckText.SetActive(aircraftCarrierDeckManager.HasDamage && aircraftCarrierDeckManager.FreeMission != mission.OrderType && aircraftCarrierDeckManager.FreeSquadrons < mission.GetPlanesCount());
            }
            else
            {
                wreckText.SetActive(false);
            }
            CheckMissionAvailable();
        }
    }

    private void CheckSupplyRunout()
    {
        if (mission != null && mission.MissionStage == EMissionStage.ReadyToLaunch)
        {
            suppliesRunout.SetActive(ResourceManager.Instance.Supplies <= 0f && aircraftCarrierDeckManager.FreeMission != mission.OrderType && aircraftCarrierDeckManager.FreeSquadrons < mission.GetPlanesCount());
        }
        else
        {
            suppliesRunout.SetActive(ResourceManager.Instance.Supplies <= 0f);
        }
        CheckMissionAvailable();
    }

    private void MissionStageDiff(TacticalMission m)
    {
        SetGrid(false);
        if (button != null)
        {
            icon.sprite = button.MissionSprite;
        }
        missionTitle.text = m.MissionName;
        missionDesc.text = m.MissionDescription;
        this.StartCoroutineActionAfterFrames(() => LayoutRebuilder.MarkLayoutForRebuild(rightPanel), 1);
        bool hasLaunch = false;
        bool hasCancel = false;
        bool hasDisabledRetrieve = false;
        bool showRecoveryTimer = false;
        EMissionStage stage = m.MissionStage;
        CheckRecoveryArea();
        CheckDeckState();
        CheckWreckOnDeck();
        CheckSupplyRunout();
        recoveryTimer.SetActive(false);

        if (TacticalMission.SwitchPlaneTypeMissions.Contains(m.OrderType) && stage < EMissionStage.Planned)
        {
            useTorpedoesToggle.Setup(true);
            useTorpedoesToggle.enabled = !disabled;
        }
        else
        {
            useTorpedoesToggle.gameObject.SetActive(false);
        }
        switch (stage)
        {
            case EMissionStage.Available:
                hasLaunch = !mission.ConfirmButtonShown;
                launchText.text = prepareButtonText;
                timeTitleText.text = timeToObs;
                break;
            case EMissionStage.Planned:
                hasLaunch = true;
                launchText.text = prepareButtonText;
                timeTitleText.text = timeToObs;
                break;
            case EMissionStage.ReadyToLaunch:
                timeTitleText.text = timeToObs;
                hasLaunch = true;
                launchText.text = launchButtonText;
                if (!mission.Canceled)
                {
                    hasCancel = TacticalMission.AirstrikeMissions.Contains(m.OrderType) || m.OrderType == EMissionOrderType.IdentifyTargets || m.OrderType == EMissionOrderType.MagicIdentify ||
                        m.OrderType == EMissionOrderType.Recon;
                }
                break;
            case EMissionStage.Launching:
                //if (!mission.Canceled)
                //{
                //    hasCancel = true;
                //}
                break;
            case EMissionStage.Deployed:
                //mission.SentSquadrons.Clear();
                timeTitleText.text = timeToReturn;
                if (tacMan.AllowMissionCancel &&
                    (((mission.OrderType == EMissionOrderType.Scouting || mission.OrderType == EMissionOrderType.CarriersCAP) && !mission.Canceled) || (!mission.HadAction && !mission.Canceled)) &&
                        mission.OrderType != EMissionOrderType.RescueVIP)
                {
                    hasCancel = true;
                }
                break;
            case EMissionStage.AwaitingRetrieval:
                hasLaunch = mission.CanBeRetrievedByEscort();
                timeTitleText.text = timeToRetrieve;
                SetGrid(true);
                break;
            case EMissionStage.ReadyToRetrieve:
                timeTitleText.text = timeToRetrieve;
                SetGrid(true);
                hasDisabledRetrieve = AircraftCarrierDeckManager.Instance.EscortRetrievingSquadrons ? !mission.CanBeRetrievedByEscort() : aircraftCarrierDeckManager.DeckMode == EDeckMode.Starting;
                hasLaunch = true;
                launchText.text = readyToRetrieveButtonText;
                break;
            case EMissionStage.Recovering:
                hasCancel = mission.SentSquadrons.Count > 1 && !mission.Canceled;
                showRecoveryTimer = true;
                SetGrid(true);
                //squadrons.Clear();
                break;
            case EMissionStage.ReportReady:
                break;
            case EMissionStage.AnalyzingReport:
                break;
            case EMissionStage.ViewResults:
                hasLaunch = true;
                launchText.text = confirmText;
                break;
            case EMissionStage.Complete:
                hasLaunch = true;
                launchText.text = confirmText;
                break;
            case EMissionStage.Obsolete:
                hasLaunch = true;
                launchText.text = confirmText;
                break;
            case EMissionStage.Failure:
                hasLaunch = true;
                launchText.text = confirmText;
                break;
        }

        recoveryTimer.SetActive(showRecoveryTimer);
        retrieveDisabled.SetActive(hasDisabledRetrieve);
        launch.gameObject.SetActive(hasLaunch);
        cancel.gameObject.SetActive(hasCancel);
    }

    private void Launch()
    {
        bool can = HudManager.Instance.HasNo(ETutorialMode.DisableMissionButtonStageChange);
        if (mission.MissionStage == EMissionStage.Available)
        {
            if (TacticalMission.MissionsWithSetup.Contains(mission.OrderType))
            {
                TacticManager.Instance.StartMissionSetupMode(mission);
            }
        }
        else if (mission.MissionStage == EMissionStage.Planned)
        {
            mission.StartMission();
        }
        else if (mission.MissionStage == EMissionStage.ReadyToLaunch && can)
        {
            if (aircraftCarrierDeckManager.FreeSquadrons > 0)
            {
                mission.GetPlanes(out var bombers, out var fighters, out var torpedoes);
                int planes = bombers + fighters + torpedoes;
                if (planes <= aircraftCarrierDeckManager.FreeSquadrons)
                {
                    mission.SendFreeMission();
                }
            }
            else if (aircraftCarrierDeckManager.FreeMission == mission.OrderType)
            {
                mission.SendFreeMission();
            }
            else
            {
                aircraftCarrierDeckManager.MissionOrder(mission);
            }
        }
        else if (mission.MissionStage == EMissionStage.AwaitingRetrieval && can)
        {
            if (mission.CanBeRetrievedByEscort())
            {
                mission.EscortRecover();
            }
        }
        else if (mission.MissionStage == EMissionStage.ReadyToRetrieve && can)
        {
            if (mission.CanBeRetrievedByEscort())
            {
                mission.EscortRecover();
            }
            else
            {
                aircraftCarrierDeckManager.AddSquadronLandingOrder(mission);
            }
        }
        else if (mission.MissionStage >= EMissionStage.Complete)
        {
            HideTooltip();
            mission.RemoveMission(true);
        }
        //if (mission.MissionStage < EMissionStage.Complete)
        //{
        //    missionPanel.AddMissionButton(mission, button);
        //}
        HideTooltip();
        mission.ButtonMission.PointerExit();
    }

    private void Cancel()
    {
        mission.Cancel();
    }

    private void Remove()
    {
        tooltipRect.SetParent(null);
        mission.ButtonMissionTimeChanged -= TimeChanged;
        mission.MissionRemoved -= Remove;
        mission.StageChanged -= OnStageChanged;
        this.StartCoroutineActionAfterFrames(() => missionPanel.RebuildPanel(mission.MissionList), 1);
        // missionExpand.Reset();
    }

    private void TimeChanged()
    {
        if (button != null)
        {
            hoursText.text = button.HoursTimer.text;
            minutesText.text = button.MinutesTimer.text;
            timeText.gameObject.SetActive(button.Timer.gameObject.activeSelf);
            recoveryHoursText.text = button.RecoveryHoursTimer.text;
            recoveryMinutesText.text = button.RecoveryMinutesTimer.text;
            recoveryTimer.gameObject.SetActive(button.RecoveryTimer.gameObject.activeSelf);
        }
    }

    private void OnDeckCameraChanged()
    {
        if (mission != null)
        {
            MissionStageDiff(mission);
        }
    }

    private void SetGrid(bool show)
    {
        if (show)
        {
            squadronsContainer.SetActive(false);
            squadronGrid.SetActive(true);
            //if (mission.SentSquadrons.Count == 0)
            //{
            //    mission.SentSquadrons.AddRange(mission.SentSquadrons);
            //}
            int i = 0;
            for (; i < mission.SentSquadrons.Count; i++)
            {
                bool damaged = mission.SentSquadrons[i].IsDamaged;
                while (mission.RecoveryDirections.Count <= i) ///index out of range guard
                {
                    mission.RecoveryDirections.Add(2);
                }
                int dir = damaged ? 0 : mission.RecoveryDirections[i];
                squadronToggles[i].Setup(dir, damaged, mission.SentSquadrons[i].PlaneType, !damaged);
            }
            for (; i < squadronToggles.Count; i++)
            {
                squadronToggles[i].gameObject.SetActive(false);
                squadronToggles[0].LandingHover.gameObject.SetActive(false);
            }

            tooltipRect.sizeDelta = defaultRect.sizeDelta + new Vector2(0f, 50f * Mathf.CeilToInt(mission.SentSquadrons.Count / 2f) /*+ (mission.MissionStage == EMissionStage.Recovering ? 50f : 0f)*/);
            UpdateToggles();
        }
        else
        {
            squadronsContainer.SetActive(true);
            squadronGrid.SetActive(false);
            tooltipRect.sizeDelta = defaultRect.sizeDelta;
        }
    }

    private void UpdateToggles()
    {
        int toDeck = 0;
        if (mission == null)
        {
            return;
        }
        for (int i = 0; i < mission.SentSquadrons.Count; i++)
        {
            UnityEngine.Assertions.Assert.IsNotNull(mission);
            UnityEngine.Assertions.Assert.IsNotNull(mission.RecoveryDirections);
            UnityEngine.Assertions.Assert.IsNotNull(mission.SentSquadrons[i]);
            if (mission.RecoveryDirections[i] == 2 && !mission.SentSquadrons[i].IsDamaged)
            {
                toDeck++;
            }
        }

        var deck = AircraftCarrierDeckManager.Instance;
        var deckPanelMan = DeckOrderPanelManager.Instance;
        int maxOnDeck = deck.MaxSlots - deck.DeckSquadrons.Count;
        if (toDeck > maxOnDeck)
        {
            for (int i = 0; i < mission.SentSquadrons.Count; i++)
            {
                squadronToggles[i].enabled = true;
                if (!deckPanelMan.Disable && mission.RecoveryDirections[i] == 2 && !mission.SentSquadrons[i].IsDamaged && maxOnDeck-- > 0)
                {
                }
                else if (mission.RecoveryDirections[i] == 2 && mission.MissionStage != EMissionStage.Recovering)
                {
                    squadronToggles[i].State = 0;
                    mission.RecoveryDirections[i] = squadronToggles[i].State;
                }
            }
        }
        else
        {
            for (int i = 0; i < mission.SentSquadrons.Count; i++)
            {
                squadronToggles[i].enabled = !deckPanelMan.Disable;
            }
        }
        bool disableLaunch = true;
        for (int i = 0; i < mission.SentSquadrons.Count; i++)
        {
            if (mission.RecoveryDirections[i] != 1)
            {
                disableLaunch = false;
            }
        }
        retrieveDisabled.SetActive(disableLaunch);
        if (mission.MissionStage == EMissionStage.Recovering)
        {
            squadronToggles[0].LandingHover.gameObject.SetActive(true);
            CancelButton.gameObject.SetActive(mission.SentSquadrons.Count > 1 && !mission.Canceled);
            foreach (var toggle in squadronToggles)
            {
                toggle.enabled = false;
            }
        }
    }

    private void OnToggleClicked(int i)
    {
        if (squadronToggles[i].State == 2)
        {
            var deck = AircraftCarrierDeckManager.Instance;
            int maxOnDeck = deck.MaxSlots - deck.DeckSquadrons.Count;
            int toDeck = 0;
            for (int j = 0; j < mission.SentSquadrons.Count; j++)
            {
                if (mission.RecoveryDirections[j] == 2 && !mission.SentSquadrons[j].IsDamaged)
                {
                    toDeck++;
                }
            }
            if (toDeck == maxOnDeck || mission.SentSquadrons[i].IsDamaged)
            {
                squadronToggles[i].State = 0;
            }
        }
        mission.RecoveryDirections[i] = squadronToggles[i].State;
        //mission.RecoverySquadronDirection[i] = squadronToggles[i].State;
        //mission.AllRecoverySquadronsDirection[i] = squadronToggles[i].State;
        UpdateToggles();
    }

    private void OnTorpedoesToggleClicked()
    {
        if (mission.MissionStage < EMissionStage.Launching)
        {
            mission.UseTorpedoes = useTorpedoesToggle.State;
            mission.UpdatePlanesCount();
            CheckSquadrons();
        }
    }
}

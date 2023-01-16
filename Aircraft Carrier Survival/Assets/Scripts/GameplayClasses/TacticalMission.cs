using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[Serializable]
public class TacticalMission : MonoBehaviour, ITickable
{
    public static readonly HashSet<EMissionOrderType> PersistentMissions = new HashSet<EMissionOrderType>() { EMissionOrderType.Airstrike, EMissionOrderType.AirstrikeSubmarine,
        EMissionOrderType.IdentifyTargets, EMissionOrderType.Recon, EMissionOrderType.DetectSubmarine, EMissionOrderType.CarriersCAP, EMissionOrderType.Scouting, EMissionOrderType.NightScouts};
    public static readonly HashSet<EMissionOrderType> AirstrikeMissions = new HashSet<EMissionOrderType>() { EMissionOrderType.Airstrike, EMissionOrderType.AirstrikeSubmarine,
        EMissionOrderType.MagicAirstrike, EMissionOrderType.MidwayAirstrike, EMissionOrderType.NightAirstrike };
    public static readonly HashSet<EMissionOrderType> SimpleMissions = new HashSet<EMissionOrderType>() { EMissionOrderType.CarriersCAP, EMissionOrderType.Scouting };
    public static readonly HashSet<EMissionOrderType> MissionsWithSetup = new HashSet<EMissionOrderType>() { EMissionOrderType.Airstrike, EMissionOrderType.AirstrikeSubmarine,
        EMissionOrderType.MidwayAirstrike, EMissionOrderType.AttackJapan, EMissionOrderType.Decoy, EMissionOrderType.DetectSubmarine, EMissionOrderType.FriendlyFleetCAP,
        EMissionOrderType.IdentifyTargets, EMissionOrderType.MagicIdentify, EMissionOrderType.Recon, EMissionOrderType.NightScouts, EMissionOrderType.RescueVIP, EMissionOrderType.FriendlyCAPMidway,
        EMissionOrderType.MagicAirstrike, EMissionOrderType.NightAirstrike, EMissionOrderType.MagicNightScouts };
    public static readonly HashSet<EMissionOrderType> SwitchPlaneTypeMissions = new HashSet<EMissionOrderType>() { EMissionOrderType.Recon, EMissionOrderType.IdentifyTargets,
        EMissionOrderType.NightScouts, EMissionOrderType.MagicIdentify, EMissionOrderType.MagicNightScouts };
    public static readonly HashSet<EMissionOrderType> LongerObsoleteMissions = new HashSet<EMissionOrderType>() { EMissionOrderType.RescueVIP, EMissionOrderType.AttackJapan };

    public event Action<TacticalMission> StageChanged = delegate { };
    public event Action ButtonMissionTimeChanged = delegate { };
    public event Action MissionRemoved = delegate { };

    public float TimePassed = 0;
    public string MissionName;
    public string MissionDescription;
    public Sprite MissionIcon;
    public int MissionIndex = 0;

    public EMissionOrderType OrderType = EMissionOrderType.Airstrike;
    public List<Strategy> Strategies;
    public TacticalMap Map;

    public float DistanceOnFuel;
    public float AttackDistance;
    public float ReturnDistance;
    public float TotalAttackDistance;
    public Vector2 StartPosition;
    public Vector2 AttackPosition;
    public Vector2 ReturnPosition;

    public TacticalEnemyShip EnemyShip = null;

    [HideInInspector]
    public MissionProgressVisualisation MissionProgressVisualisation;

    public TacticalObject ConfirmedTarget => confirmedTarget;
    public bool InProgress => MissionStage > EMissionStage.ReadyToLaunch && MissionStage < EMissionStage.Complete;

    public bool IsActive => MissionStage == EMissionStage.Deployed;

    public bool HadAction => hadAction;

    public List<PlayerManeuverData> Maneuvers => maneuvers;

    public EnemyAttackFriendData EnemyAttackFriend => enemyAttackFriend;

    public EnemyAttackData EnemyAttackData => enemyAttack;

    public RadarEnemy RadarObject => radarObject;

    public bool ConfirmButtonShown
    {
        get;
        set;
    }

    public TacticalMission MissionToRemoveOnConfirm
    {
        get;
        set;
    }

    public bool ExtraMission
    {
        get;
        set;
    }

    public bool HadAttack
    {
        get;
        set;
    }

    public bool MissionSent
    {
        get;
        set;
    }

    public ButtonMission ButtonMission
    {
        get;
        set;
    }

    public bool Confirmed
    {
        get;
        private set;
    }

    public bool CustomMission
    {
        get;
        private set;
    }

    public RectTransform CustomRetrievalPoint
    {
        get;
        private set;
    }

    public int SelectedObjectIndex
    {
        get;
        private set;
    }

    public List<TacticalEnemyShip> PossibleMissionTargets
    {
        get;
        set;
    } = new List<TacticalEnemyShip>();

    //public float TotalMissionTime = 0;

    //public EMissionStage MissionStage = EMissionStage.Planned;

    public EMissionStage MissionStage
    {
        get => missionStage;
        private set
        {
            missionStage = value;
            UpdateMissionText();
            StageChanged(this);
            tacMan.MissionPanel.AddMissionButton(this);
        }
    }

    public int ExpireTime
    {
        get;
        set;
    } = -1;

    public int ToObsoleteTime
    {
        get;
        private set;
    }

    public bool UseTorpedoes
    {
        get;
        set;
    }

    public bool MarkedDestroyed
    {
        get;
        private set;
    }

    public EMissionList MissionList = EMissionList.Ready;

    //public MissionButton MissionButton;

    [NonSerialized]
    public List<PlaneSquadron> SentSquadronsLeft = new List<PlaneSquadron>();
    [NonSerialized]
    public List<PlaneSquadron> SentSquadrons;
    [NonSerialized]
    public List<PlaneSquadron> SentSquadronsReportManager;

    [NonSerialized]
    public List<int> RecoveryDirections = new List<int>();

    public int SavedHour;

    public bool JustDestroy;

    public bool Canceled;

    private float toActionTime;
    private float returnTime;
    private int idlingTime;
    public int ToRecoverTime;
    private EMissionStage missionStage;

    private float carrierDistance;

    private bool action;
    private bool hadAction;

    public TacticalEnemyMapButton AttackedFleet;
    private RectTransform raid;

    public bool HadBombers;
    public bool HadFighters;
    public bool HadTorpedoes;
    public int LostFighters;
    public int LostBombers;
    public int LostTorpedoes;
    public List<EnemyManeuverData> SunkedShips;
    public List<TacticalEnemyMapButton> RevealedFleets;

    public bool ButtonHovered;

    [HideInInspector]
    public RectTransform MissionWaypoints = null;
    [HideInInspector]
    public List<Image> WaypointsImages = null;

    public bool CustomReport;

    public int Bombers;
    public int Fighters;
    public int Torpedoes;

    [SerializeField]
    public float undetectionHoursAfteScout = 2f;

    private TimeManager timeManager;
    private MinuteCounter minuteCounter;
    private LocalizationManager locMan;

    private float time;
    private int timeToFinish;
    private int minutes;
    private int recoveryMinutes;
    private int savedMinute;

    private bool flyToRecovering;

    private List<PlayerManeuverData> maneuvers;

    private TacticManager tacMan;

    private EnemyAttackData enemyAttack;
    private EnemyAttackFriendData enemyAttackFriend;

    private TacticalObject confirmedTarget;

    private Vector2 attackEulers;
    private Vector2 returnEulers;

    private EMissionDeployedStage deployedStage;

    private RadarEnemy radarObject;

    private bool hadActionSaved;
    private bool isLoaded;

    private float missionProgressTime;
    private float missionProgressEnd;
    private bool isReturning;

    private TacticalObject finalTarget;

    private bool removeOnReturn;

    private int retrievalRange;

    private bool escortRecovery;

    private bool forceAdd;
    private int bonusAttack;

    private int recoveryTimePassed;
    private int timeToRetrieve;

    private void Update()
    {
        time += Time.deltaTime;
        missionProgressTime += Time.deltaTime;
        if (ButtonMission)
        {
            ButtonMission.FillImage.fillAmount = 1f - (TimePassed + time) / timeToFinish;
            ButtonMission.RecoveryFillImage.fillAmount = 1f - (recoveryTimePassed + time) / timeToRetrieve;
        }
        if (MissionProgressVisualisation)
        {
            if ((MissionStage == EMissionStage.Deployed || MissionStage == EMissionStage.ReadyToRetrieve || MissionStage == EMissionStage.AwaitingRetrieval) && ButtonHovered)
            {
                MissionProgressVisualisation.gameObject.SetActive(true);
                switch (deployedStage)
                {
                    case EMissionDeployedStage.Attack:
                        var progress = missionProgressTime / missionProgressEnd;
                        MissionProgressVisualisation.RectTransform.anchoredPosition = new Vector2(Mathf.Lerp(StartPosition.x, AttackPosition.x, progress), Mathf.Lerp(StartPosition.y, AttackPosition.y, progress));
                        break;
                    case EMissionDeployedStage.Idle:
                        MissionProgressVisualisation.RectTransform.anchoredPosition = AttackPosition;
                        break;
                    case EMissionDeployedStage.Return:
                        var returnProgress = missionProgressTime / missionProgressEnd;
                        MissionProgressVisualisation.RectTransform.anchoredPosition = new Vector2(Mathf.Lerp(AttackPosition.x, ReturnPosition.x, returnProgress), Mathf.Lerp(AttackPosition.y, ReturnPosition.y, returnProgress));
                        break;
                }
            }
            else
            {
                MissionProgressVisualisation.gameObject.SetActive(false);
            }
        }
    }

    public MissionSaveData Save()
    {
        List<int> maneuvers = new List<int>();
        if (Maneuvers != null)
        {
            foreach (var maneuver in Maneuvers)
            {
                if (maneuver == null)
                {
                    maneuvers.Add(-1);
                    continue;
                }
                int index = tacMan.AllPlayerManeuvers.IndexOf(maneuver.MainLevel);
                if (index == -1)
                {
                    if (maneuver == tacMan.MidwayCustomManeuver)
                    {
                        index = -2;
                    }
                    else if (maneuver == tacMan.MagicCustomManeuver)
                    {
                        index = -3;
                    }
                }
                maneuvers.Add(index);
            }
        }
        List<SquadronsSaveData> sentSquadrons = new List<SquadronsSaveData>();
        foreach (var squadron in SentSquadrons)
        {
            var squadronData = new SquadronsSaveData
            {
                PlaneType = squadron.PlaneType,
                Broken = squadron.IsDamaged,
            };
            sentSquadrons.Add(squadronData);
        }
        List<SquadronsSaveData> sentSquadronsLeft = new List<SquadronsSaveData>();
        foreach (var squadron in SentSquadronsLeft)
        {
            var squadronData = new SquadronsSaveData
            {
                PlaneType = squadron.PlaneType,
                Broken = squadron.IsDamaged,
            };
            sentSquadronsLeft.Add(squadronData);
        }
        List<int> recoveryDirections = new List<int>(RecoveryDirections);

        List<int> possibleMissionTargets = new List<int>();
        foreach (var enemy in PossibleMissionTargets)
        {
            possibleMissionTargets.Add(enemy.Id);
        }

        int friendIndex = -1;
        if (EnemyAttackFriend != null)
        {
            friendIndex = EnemyAttackFriend.FriendID;
        }

        var target = EEnemyAttackTarget.Carrier;
        if (OrderType == EMissionOrderType.SubmarineHunt)
        {
            target = EnemyAttackData.CurrentTarget;
        }

        var data = new MissionSaveData
        {
            Time = TimePassed,
            Type = OrderType,
            Stage = MissionStage,
            Canceled = Canceled,
            ForceAdd = forceAdd,
            AttackPosition = AttackPosition,
            ReturnPosition = ReturnPosition,
            Strategies = maneuvers,
            SelectedObject = SelectedObjectIndex,
            ConfirmedTarget = tacMan.AllObjects.IndexOf(ConfirmedTarget),
            CustomMission = CustomMission,
            SentSquadrons = sentSquadrons,
            SentSquadronsLeft = sentSquadronsLeft,
            Bombers = Bombers,
            Fighters = Fighters,
            Torpedoes = Torpedoes,
            LostBombers = LostBombers,
            LostFighters = LostFighters,
            LostTorpedoes = LostTorpedoes,
            FriendID = friendIndex,
            HadAction = HadAction,
            Confirmed = Confirmed,
            StartPosition = StartPosition,
            MissionProgressTime = missionProgressTime,
            MissionProgressEnd = missionProgressEnd,
            IsReturning = isReturning,
            Target = target,
            RecoveryDirections = recoveryDirections,
            TargetID = tacMan.AllObjects.IndexOf(finalTarget),
            RetrievalRange = retrievalRange,
            UseTorpedoes = UseTorpedoes,
            PossibleMissionTargets = possibleMissionTargets,
            DeployedStage = deployedStage,
            BonusAttack = bonusAttack,
            EscortFreeMission = removeOnReturn
        };
        return data;
    }

    public void CreateFromSave(MissionSaveData data, TacticManager manager, TacticalMap map, MissionPanel panel)
    {
        isLoaded = true;
        Create(data.Type, manager, map);
        LoadFromSave(data, panel);
    }

    public void LoadFromSave(MissionSaveData data, MissionPanel panel)
    {
        TimePassed = data.Time;
        OrderType = data.Type;
        MissionStage = data.Stage;
        AttackPosition = data.AttackPosition;
        ReturnPosition = data.ReturnPosition;
        StartPosition = data.StartPosition;
        retrievalRange = data.RetrievalRange;
        forceAdd = data.ForceAdd;

        finalTarget = data.TargetID < 0 ? null : tacMan.AllObjects[data.TargetID];
        confirmedTarget = data.ConfirmedTarget < 0 ? null : tacMan.AllObjects[data.ConfirmedTarget];
        SelectedObjectIndex = data.SelectedObject;

        deployedStage = data.DeployedStage;

        if (data.FriendID != -1)
        {
            enemyAttackFriend = new EnemyAttackFriendData();
            enemyAttackFriend.FriendID = data.FriendID;
        }
        if (data.RecoveryDirections != null)
        {
            RecoveryDirections.Clear();
            RecoveryDirections.AddRange(data.RecoveryDirections);
        }

        hadActionSaved = data.HadAction;
        Confirmed = data.Confirmed;
        UseTorpedoes = data.UseTorpedoes;
        UpdatePlanesCount();

        foreach (var squad in data.SentSquadrons)
        {
            var newSquadron = new PlaneSquadron(squad.PlaneType);
            newSquadron.IsDamaged = squad.Broken;
            SentSquadrons.Add(newSquadron);
        }

        foreach (var squad in data.SentSquadronsLeft)
        {
            var newSquadron = new PlaneSquadron(squad.PlaneType);
            newSquadron.IsDamaged = squad.Broken;
            SentSquadronsLeft.Add(newSquadron);
        }

        foreach (var id in data.PossibleMissionTargets)
        {
            PossibleMissionTargets.Add(tacMan.GetShip(id));
        }

        if (AirstrikeMissions.Contains(OrderType))
        {
            Bombers = data.Bombers;
            Fighters = data.Fighters;
            Torpedoes = data.Torpedoes;
            LostBombers = data.LostBombers;
            LostFighters = data.LostFighters;
            LostTorpedoes = data.LostTorpedoes;
            maneuvers = new List<PlayerManeuverData>();
            foreach (var strat in data.Strategies)
            {
                if (strat == -1)
                {
                    maneuvers.Add(null);
                }
                else if (strat == -2)
                {
                    maneuvers.Add(tacMan.MidwayCustomManeuver);
                }
                else if (strat == -3)
                {
                    maneuvers.Add(tacMan.MagicCustomManeuver);
                }
                else
                {
                    maneuvers.Add(tacMan.AllPlayerManeuvers[strat]);
                }
            }
        }
        else if (OrderType == EMissionOrderType.SubmarineHunt)
        {
            enemyAttack = new EnemyAttackData();
            enemyAttack.CurrentTarget = data.Target;
        }

        if (panel != null)
        {
            panel.SpawnButton(this);
        }

        if (Confirmed)
        {
            bonusAttack = data.BonusAttack;

            if (!data.CustomMission && !SimpleMissions.Contains(OrderType))
            {
                var returnDifference = AttackPosition - ReturnPosition;
                ReturnDistance = returnDifference.magnitude;
                returnEulers = ReturnDistance == 0f ? Vector2.right : returnDifference / ReturnDistance;
                var attackDifference = StartPosition - AttackPosition;
                AttackDistance = attackDifference.magnitude;
                attackEulers = AttackDistance == 0f ? Vector2.right : attackDifference / AttackDistance;

                toActionTime = AttackDistance / tacMan.AirRaidSpeed;
                returnTime = ReturnDistance / tacMan.AirRaidSpeed;
                EnemyShip = confirmedTarget as TacticalEnemyShip;

                toActionTime += tacMan.PlanesStartingTicks * GetPlanesCount();
                AddVisualization();
                MissionProgressVisualisation = Instantiate(tacMan.MissionProgressVisualisationPrefab, Map.transform);
                MissionProgressVisualisation.gameObject.SetActive(false);

                ButtonMission.NameText.gameObject.SetActive(false);
                ToObsoleteTime = (LongerObsoleteMissions.Contains(OrderType) ? 48 : tacMan.ToObsoleteHours) * timeManager.TicksForHour;
                if (MissionStage == EMissionStage.ReadyToLaunch)
                {
                    SetTimeToFinish(ToObsoleteTime);
                }
                else if (missionStage == EMissionStage.ReadyToRetrieve)
                {
                    SetTimeToFinish(ToRecoverTime);
                }
                isReturning = data.IsReturning;
                missionProgressTime = data.MissionProgressTime;
                missionProgressEnd = data.MissionProgressEnd;
                MissionProgressVisualisation.RectTransform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, attackEulers));
            }
        }

        if (data.Canceled && missionStage <= EMissionStage.Deployed)
        {
            Cancel();
        }

        if (MissionStage >= EMissionStage.Deployed)
        {
            MissionSent = true;
            removeOnReturn = data.EscortFreeMission;
        }
        SetMinuteTimer();
        recoveryTimePassed = timeToRetrieve = 0;
        foreach (var dir in RecoveryDirections)
        {
            switch (dir)
            {
                case 0:
                    timeToRetrieve += tacMan.TicksToRetrieveSquadronToHangar;
                    break;
                case 1:
                    break;
                case 2:
                    timeToRetrieve += tacMan.TicksToRetrieveSquadronToDeck;
                    break;
            }
        }
        timeToRetrieve -= tacMan.TicksToLandSquadrons;
    }

    public void Create(EMissionOrderType orderType, TacticManager manager, TacticalMap map, EnemyAttackData enemyAttackData = null, EnemyAttackFriendData enemyAttackFriendData = null)
    {
        locMan = LocalizationManager.Instance;
        tacMan = manager;

        retrievalRange = tacMan.RetrievalRange;
        enemyAttack = enemyAttackData;
        enemyAttackFriend = enemyAttackFriendData;
        raid = new GameObject("", typeof(RectTransform)).GetComponent<RectTransform>();
        OrderType = orderType;
        Map = map;
        MissionName = tacMan.MissionInfo[orderType].MissionName;
        MissionIcon = tacMan.MissionInfo[orderType].MissionSprite;

        SentSquadrons = new List<PlaneSquadron>();
        SentSquadronsReportManager = new List<PlaneSquadron>();

        SunkedShips = new List<EnemyManeuverData>();

        timeManager = TimeManager.Instance;
        minuteCounter = new MinuteCounter();

        timeManager.MinutePassed += OnMinutePassed;

        int ticksPerHour = TimeManager.Instance.TicksForHour;

        var missionStage = EMissionStage.ReadyToLaunch;

        GetPlanes(out Bombers, out Fighters, out Torpedoes);

        UpdateMissionText();
        idlingTime = 1;
        ToObsoleteTime = 0;

        bool quicker = Parameters.Instance.DifficultyParams.QuickerAttacks;
        switch (OrderType)
        {
            case EMissionOrderType.Decoy:
            case EMissionOrderType.RescueVIP:
            case EMissionOrderType.Airstrike:
            case EMissionOrderType.AirstrikeSubmarine:
            case EMissionOrderType.MidwayAirstrike:
            case EMissionOrderType.MagicAirstrike:
            case EMissionOrderType.NightAirstrike:
                ToRecoverTime = tacMan.AttackRecoverHours * ticksPerHour;
                missionStage = EMissionStage.Available;
                idlingTime = 1 * ticksPerHour;
                break;
            case EMissionOrderType.Recon:
            case EMissionOrderType.DetectSubmarine:
            case EMissionOrderType.NightScouts:
            case EMissionOrderType.MagicNightScouts:
                ToRecoverTime = tacMan.ReconRecoverHours * ticksPerHour;
                missionStage = EMissionStage.Available;
                idlingTime = tacMan.ReconHours * ticksPerHour;
                break;
            case EMissionOrderType.IdentifyTargets:
            case EMissionOrderType.MagicIdentify:
                ToRecoverTime = tacMan.IdentifyRecoverHours * ticksPerHour;
                missionStage = EMissionStage.Available;
                idlingTime = 1 * ticksPerHour;
                break;
            case EMissionOrderType.FriendlyFleetCAP:
            case EMissionOrderType.FriendlyCAPMidway:
                toActionTime = tacMan.FriendlyCapActionHours * ticksPerHour;
                ToRecoverTime = tacMan.CounterScoutToRecoverHours * ticksPerHour;
                ToObsoleteTime = tacMan.CounterHostileScoutsObsoleteHours * ticksPerHour;
                //idlingTime = 1 * ticksPerHour;
                break;
            case EMissionOrderType.CarriersCAP:
                ToRecoverTime = tacMan.CAPRecoverHours * ticksPerHour;
                returnTime = tacMan.CAPHours * ticksPerHour;
                break;
            case EMissionOrderType.CounterHostileScouts:
                toActionTime = tacMan.CounterScoutToActionHours * ticksPerHour;
                ToRecoverTime = tacMan.CounterScoutToRecoverHours * ticksPerHour;
                ToObsoleteTime = tacMan.CounterHostileScoutsObsoleteHours * ticksPerHour;
                if (quicker)
                {
                    ToObsoleteTime /= 2;
                }
                //idlingTime = tacMan.CounterHostileScoutsHours * ticksPerHour;
                break;
            case EMissionOrderType.SubmarineHunt:
                toActionTime = tacMan.HuntToActionHours * ticksPerHour;
                ToRecoverTime = tacMan.HuntToRecoverHours * ticksPerHour;
                ToObsoleteTime = tacMan.SubmarineHuntingObsoleteHours * ticksPerHour;
                if (quicker)
                {
                    ToObsoleteTime /= 2;
                }
                //idlingTime = tacMan.SubmarineHuntingHours * ticksPerHour;
                break;
            case EMissionOrderType.Scouting:
                ToRecoverTime = tacMan.SpottingRecoverHours * ticksPerHour;
                returnTime = tacMan.SpottingHours * ticksPerHour;
                break;
            case EMissionOrderType.AttackJapan:
                missionStage = EMissionStage.Available;
                idlingTime = 1;
                break;
        }
        ToRecoverTime = (int)(ToRecoverTime * tacMan.RetrievalTimeModifier);
        ToRecoverTime += CrewManager.Instance.DepartmentDict[EDepartments.Air].EfficiencyMinutes;
        toActionTime += tacMan.PlanesStartingTicks * GetPlanesCount();
        MissionStage = missionStage;

        //special effects, add EnemyAttackData to submarine hunt
        if (!isLoaded)
        {
            switch (OrderType)
            {
                case EMissionOrderType.CounterHostileScouts:
                    radarObject = EnemyAttacksManager.Instance.SpawnScout(this);
                    EventManager.Instance.AddHostileScout(this);
                    break;
                case EMissionOrderType.SubmarineHunt:
                    EnemyAttacksManager.Instance.SpawnSubmarine(enemyAttack, this);
                    EventManager.Instance.StartSubmarineHunt(this);
                    break;
                case EMissionOrderType.FriendlyFleetCAP:
                case EMissionOrderType.FriendlyCAPMidway:
                    EventManager.Instance.AllyAttacked(this);
                    break;
            }
        }

        if (ToObsoleteTime == 0)
        {
            ToObsoleteTime = 100_000_000;
        }
        SetTimeToFinish(ToObsoleteTime);
        ////Debug.LogFormat("Order type: {0}, ToRecoverTime: {1}", OrderType, ToRecoverTime);

        TimeManager.Instance.AddTickable(this);
    }

    public void CustomCreate(EMissionOrderType orderType, TacticManager manager, TacticalMap map, Vector2 returnPosition, float retrievalHours, int bombers, int fighters, int torpedoes)
    {
        CustomMission = true;
        Confirmed = false;

        locMan = LocalizationManager.Instance;
        tacMan = manager;
        raid = new GameObject("", typeof(RectTransform)).GetComponent<RectTransform>();
        OrderType = orderType;
        Map = map;
        MissionName = tacMan.MissionInfo[orderType].MissionName;
        MissionIcon = tacMan.MissionInfo[orderType].MissionSprite;
        MissionSent = true;
        retrievalRange = tacMan.RetrievalRange;
        CustomRetrievalPoint = Instantiate(Map.AircraftAttackReturnPointPrefab, Map.transform).GetComponent<RectTransform>();
        CustomRetrievalPoint.GetComponent<ShipWaypoint>().DrawRadius(retrievalRange);
        CustomRetrievalPoint.anchoredPosition = returnPosition;
        CustomRetrievalPoint.gameObject.SetActive(false);
        ReturnPosition = returnPosition;
        Strategies = new List<Strategy>();

        SentSquadrons = new List<PlaneSquadron>();
        AddSentSquadrons(bombers, EPlaneType.Bomber);
        AddSentSquadrons(fighters, EPlaneType.Fighter);
        AddSentSquadrons(torpedoes, EPlaneType.TorpedoBomber);

        Bombers = bombers;
        Fighters = fighters;
        Torpedoes = torpedoes;

        SentSquadronsReportManager = new List<PlaneSquadron>();

        SunkedShips = new List<EnemyManeuverData>();

        timeManager = TimeManager.Instance;
        minuteCounter = new MinuteCounter();

        timeManager.MinutePassed += OnMinutePassed;

        MissionStage = EMissionStage.AwaitingRetrieval;

        UpdateMissionText();
        ToRecoverTime = (int)(retrievalHours * TimeManager.Instance.TicksForHour);
        returnTime = 1;
        SetTimeToFinish(ToRecoverTime);

        TimeManager.Instance.AddTickable(this);

        Assert.IsFalse(SentSquadrons.Count == 0);
        for (int i = 0; i < SentSquadrons.Count; i++)
        {
            //RecoverySquadronDirection.Add(true);
            //AllRecoverySquadronsDirection.Add(true);
            RecoveryDirections.Add(2);
        }
    }

    public void Confirm()
    {
        Assert.IsTrue(MissionStage == EMissionStage.Available);
        MissionStage = EMissionStage.ReadyToLaunch;
        if (tacMan.PlayerManevuers != null)
        {
            maneuvers = new List<PlayerManeuverData>(tacMan.PlayerManevuers);
        }
        if (AirstrikeMissions.Contains(OrderType) && tacMan.HasManeuversAttackBuff)
        {
            bonusAttack = tacMan.BonusManeuversAttackBuff;
            tacMan.BonusManeuversAttackBuff = 0;
        }
        Confirmed = true;
        if (MissionToRemoveOnConfirm != null)
        {
            MissionToRemoveOnConfirm.RemoveMission(true);
        }
        StartPosition = tacMan.StartPosition;
        AttackPosition = tacMan.AttackPosition;
        ReturnPosition = tacMan.ReturnPosition;

        retrievalRange = tacMan.RetrievalRange;

        var attackDifference = StartPosition - AttackPosition;
        AttackDistance = attackDifference.magnitude;
        attackEulers = AttackDistance == 0f ? Vector2.right : attackDifference / AttackDistance;

        var returnDifference = AttackPosition - ReturnPosition;
        ReturnDistance = returnDifference.magnitude;
        returnEulers = ReturnDistance == 0f ? Vector2.right : returnDifference / ReturnDistance;

        toActionTime = AttackDistance / tacMan.AirRaidSpeed;
        returnTime = ReturnDistance / tacMan.AirRaidSpeed;

        EnemyShip = tacMan.ChosenEnemyShip;
        confirmedTarget = tacMan.ConfirmedTarget;
        SelectedObjectIndex = tacMan.SelectedObjectIndex;
        if (AirstrikeMissions.Contains(OrderType))
        {
            Bombers = tacMan.Bombers;
            Fighters = tacMan.Fighters;
            Torpedoes = tacMan.Torpedoes;
        }
        MissionProgressVisualisation = Instantiate(tacMan.MissionProgressVisualisationPrefab, Map.transform);
        MissionProgressVisualisation.gameObject.SetActive(false);

        ButtonMission.NameText.gameObject.SetActive(false);
        ToObsoleteTime = (LongerObsoleteMissions.Contains(OrderType) ? 48 : tacMan.ToObsoleteHours) * timeManager.TicksForHour;
        SetTimeToFinish(ToObsoleteTime);
        TimePassed = 0f;

        MissionProgressVisualisation.RectTransform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, attackEulers));

        tacMan.FireMissionPlanned(OrderType);
    }

    public void ResetTime()
    {
        TimePassed = 0;
        time = 0f;
        ////MissionButton.obsoleteProgress.fillAmount = 0;
    }

    public void CustomStage(EMissionStage stage)
    {
        MissionStage = stage;
    }

    public void Tick()
    {
        if (MarkedDestroyed || this == null)
        {
            return;
        }
        time = 0f;
        if (MissionStage == EMissionStage.Recovering)
        {
            recoveryTimePassed++;
            SetRecoveryMinuteTimer();
            if (ButtonMission != null)
            {
                if (ButtonMission.Timer != null)
                {
                    ButtonMission.Timer.gameObject.SetActive(false);
                }
                if (ButtonMission.RecoveryTimer != null)
                {
                    ButtonMission.RecoveryTimer.gameObject.SetActive(true);
                }
            }
            return;
        }
        if (ButtonMission != null && ButtonMission.RecoveryTimer != null)
        {
            ButtonMission.RecoveryTimer.gameObject.SetActive(false);
        }
        //if (ExpireTime > 0 && MissionStage < EMissionStage.Launching)
        //{
        //    ExpireTime--;
        //    if (ExpireTime == 0)
        //    {
        //        RemoveMission();
        //    }
        //}
        TimePassed++;
        if (TimePassed < timeToFinish)
        {
            SetMinuteTimer();
        }
        switch (MissionStage)
        {
            case EMissionStage.Available:
            case EMissionStage.Confirmed:
            case EMissionStage.Planned:
            case EMissionStage.ReadyToLaunch:
                float obsolete = ToObsoleteTime - TimePassed;
                if ((!HudManager.Instance.HasNo(ETutorialMode.DisableMissionTimeout) || tacMan.ObsoleteDisabled) && obsolete < 2f)
                {
                    obsolete = 2f;
                    TimePassed = ToObsoleteTime - 2f;
                }
                ////float obsolete = TimePassed / toObsoleteTime;
                if (ButtonMission != null && ButtonMission.Timer != null)
                {
                    ButtonMission.Timer.gameObject.SetActive(ToObsoleteTime < 100000);
                }

                if (TimePassed >= timeToFinish)
                {
                    MissionStage = EMissionStage.Obsolete;
                    MissionFinished(false);
                    OnObsolete();
                    RemoveMission(true);
                }
                break;
            case EMissionStage.Launching:
                if (ButtonMission != null)
                {
                    if (ButtonMission.Timer != null)
                    {
                        ButtonMission.Timer.gameObject.SetActive(false);
                    }
                    if (ButtonMission.NameText != null)
                    {
                        ButtonMission.NameText.gameObject.SetActive(false);
                    }
                }
                break;
            case EMissionStage.Deployed:
                if (ButtonMission != null)
                {
                    if (ButtonMission.Timer != null)
                    {
                        //bool active = OrderType != EMissionOrderType.FriendlyFleetCAP && OrderType != EMissionOrderType.FriendlyCAPMidway;
                        ButtonMission.Timer.gameObject.SetActive(timeToFinish < 100000);
                    }
                    if (ButtonMission.NameText != null)
                    {
                        ButtonMission.NameText.gameObject.SetActive(false);
                    }
                }

                float time = TimePassed;
                float totalTime = toActionTime + idlingTime + returnTime;
                SetTimeToFinish((int)totalTime);
                ////Debug.LogFormat("Times: timer: {3}, total: {4} toAction: {0}, Idling: {1}, return: {2}", toActionTime, idlingTime, returnTime, time, totalTime);

                if (time > toActionTime)
                {
                    ActionStart();
                    if (deployedStage < EMissionDeployedStage.Idle && MissionProgressVisualisation != null)
                    {
                        deployedStage = EMissionDeployedStage.Idle;
                    }
                    time -= toActionTime;
                    if (time > idlingTime)
                    {
                        if (deployedStage != EMissionDeployedStage.Return && MissionProgressVisualisation != null)
                        {
                            deployedStage = EMissionDeployedStage.Return;

                            if (!isReturning)
                            {
                                missionProgressTime = 0f;
                                missionProgressEnd = totalTime - TimePassed;
                            }
                            isReturning = true;
                            MissionProgressVisualisation.RectTransform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, returnEulers));
                        }
                        time -= idlingTime;
                        ////MissionButton.Progress.fillAmount = time / returnTime;
                        ////MissionButton.UpdateTimer(totalTime - time);
                        if (time > returnTime)
                        {
                            FinishMission(true);
                        }
                        else
                        {
                            ActionEnd();
                        }
                    }
                    else
                    {
                        ////MissionButton.Progress.fillAmount = time / idlingTime;

                        //delpoyedStage = EMissionDelpoyedStage.Idle;
                        if (idlingTime > 0)
                        {
                            ////float value = TimePassed - toActionTime;
                            ////value /= idlingTime;
                            ////MissionButton.UpdateTimer(totalTime - time);
                            ////MissionButton.obsoleteProgress.fillAmount = value;
                        }
                    }
                }

                ////Debug.LogFormat("Timer update {0}, stage: {1}", totalTime - TimePassed, MissionStage);


                break;
            case EMissionStage.AwaitingRetrieval:
                ////Debug.LogFormat("AR - ToRecoverTime: {0}, TimePassed: {1}, diff: {2}", ToRecoverTime, TimePassed, ToRecoverTime - TimePassed);
                if (ButtonMission != null && ButtonMission.Timer != null)
                {
                    ButtonMission.Timer.gameObject.SetActive(true);
                }
                SetTimeToFinish(ToRecoverTime);

                //MissionButton.UpdateTimer(ToRecoverTime - TimePassed, ToRecoverTime);
                //MissionButton.missionTimer.gameObject.SetActive(true);

                bool hasReturn = false;
                if (!SimpleMissions.Contains(OrderType))
                {
                    hasReturn = true;
                }

                if (CheckRecoveryFailTime())
                {
                    //handled in function
                }
                else if (returnTime > 0)
                {
                    var tacMan = TacticManager.Instance;
                    carrierDistance = (ReturnPosition - tacMan.Carrier.Rect.anchoredPosition).sqrMagnitude;
                    //float carrierReachTime = Mathf.Approximately(Map.MapShip.ShipSpeedMod, 0f) ? 1e9f : Mathf.Sqrt(carrierDistance) / Map.MapShip.ShipSpeed * Map.MapShip.ShipSpeedMod;

                    if (!hasReturn || (carrierDistance <= retrievalRange * retrievalRange))
                    {
                        ReadyToRetrieve();
                    }
                }
                else
                {
                    ReadyToRetrieve();
                }
                SetEscortRecoverRange();
                break;

            case EMissionStage.ReadyToRetrieve:
                ////Debug.LogFormat("RTR - ToRecoverTime: {0}, TimePassed: {1}, diff: {2}", ToRecoverTime, TimePassed, ToRecoverTime - TimePassed);
                if (ButtonMission != null && ButtonMission.Timer != null)
                {
                    ButtonMission.Timer.gameObject.SetActive(true);
                }
                SetTimeToFinish(ToRecoverTime);

                //MissionButton.UpdateTimer(ToRecoverTime - TimePassed, ToRecoverTime);
                //MissionButton.missionTimer.gameObject.SetActive(true);

                bool hasReturn2 = false;
                if (!SimpleMissions.Contains(OrderType))
                {
                    hasReturn2 = true;
                }

                if (!CheckRecoveryFailTime() && hasReturn2 && returnTime > 0)
                {
                    var tacMan = TacticManager.Instance;
                    carrierDistance = (ReturnPosition - tacMan.Carrier.Rect.anchoredPosition).sqrMagnitude;
                    if (carrierDistance > retrievalRange * retrievalRange)
                    {
                        MissionStage = EMissionStage.AwaitingRetrieval;
                        ////MissionButton.UpdateTimer(returnTime, ToRecoverTime);
                        //MissionButton.SwitchInteractionState(true, EDangerLevel.MEDIUM, MissionStage);
                    }
                }
                SetEscortRecoverRange();
                break;
            case EMissionStage.Recovering:
                if (ButtonMission != null && ButtonMission.Timer != null)
                {
                    ButtonMission.Timer.gameObject.SetActive(false);
                }
                if (timeToFinish < TimePassed)
                {
                    //EscortRecovered();
                }
                //MissionButton.missionTimer.gameObject.SetActive(false);
                //MissionButton.Progress.fillAmount = 1f;
                break;
            case EMissionStage.ReportReady:
                break;
            case EMissionStage.AnalyzingReport:
                break;
            case EMissionStage.ViewResults:
                break;
            case EMissionStage.Complete:
            case EMissionStage.Obsolete:
            case EMissionStage.Failure:
                break;
        }

        if (action)
        {
            float range = tacMan.StartRange;
            switch (OrderType)
            {
                case EMissionOrderType.Recon:
                case EMissionOrderType.NightScouts:
                case EMissionOrderType.MagicNightScouts:
                    if (UseTorpedoes)
                    {
                        range *= 1.25f;
                    }
                    tacMan.NewRevealArea(AttackPosition, range * range, true, EMissionOrderType.None);
                    break;
                case EMissionOrderType.DetectSubmarine:
                    tacMan.RevealObjects(AttackPosition, Parameters.Instance.DetectSubmarineRangeSqr, true, true, EMissionOrderType.None);
                    break;
            }
        }

        if (MissionStage != EMissionStage.AwaitingRetrieval)
        {
            ////MissionButton.bottomMissionText.text = MissionStage.ToString();
            UpdateMissionText();

            //MissionButton.UpdateMissionText();
        }
    }

    public void StartMission()
    {
        Assert.IsTrue(MissionStage < EMissionStage.ReadyToLaunch);
        MissionStage = EMissionStage.ReadyToLaunch;
        //MissionButton.SwitchInteractionState(false, EDangerLevel.NONE, MissionStage);
        ////MissionButton.Button.interactable = false;
    }

    public void ResetMission()
    {
        Confirmed = true;
        RemoveMission(true);
    }

    public void RemoveMission(bool force)
    {
        if (!force && missionStage >= EMissionStage.Launching)
        {
            return;
        }

        var tacMan = TacticManager.Instance;
        if (!hadAction && tacMan.HasManeuversAttackBuff && bonusAttack > 0)
        {
            tacMan.BonusManeuversAttackBuff = bonusAttack;
        }

        MapUnitMaskManager.Instance.RemoveUnitObject(raid.gameObject);
        AircraftCarrierDeckManager.Instance.DestroyMissionOrder(this);
        if (OrderType == EMissionOrderType.FriendlyFleetCAP || OrderType == EMissionOrderType.FriendlyCAPMidway)
        {
            EventManager.Instance.RemoveAllyAttacked(this);
        }
        tacMan.MissionPanel.RemoveMissionButton(this);
        MissionRemoved();
        if (MissionProgressVisualisation != null)
        {
            Destroy(MissionProgressVisualisation.gameObject);
        }
        if (CustomRetrievalPoint != null)
        {
            Destroy(CustomRetrievalPoint.gameObject);
        }
        if (ButtonMission.Rect != null)
        {
            Destroy(ButtonMission.Rect.parent.gameObject);
        }
        if (MissionWaypoints != null)
        {
            Destroy(MissionWaypoints.gameObject);
        }
        timeManager.MinutePassed -= OnMinutePassed;
        timeManager.RemoveTickable(this);
        if (CustomMission)
        {
            tacMan.CustomMissions.Remove(this);
        }
        else
        {
            tacMan.Missions[OrderType].Remove(this);
        }
        if (!ExtraMission && (forceAdd || (Confirmed && (PersistentMissions.Contains(OrderType) || (OrderType == EMissionOrderType.MagicIdentify && !tacMan.MagicIdentifyPermanentRemove)))))
        {
            var mission = tacMan.AddNewMission(OrderType);
            mission.PossibleMissionTargets.AddRange(PossibleMissionTargets);
        }
        MarkedDestroyed = true;
        Destroy(this);
    }

    public void Deploy()
    {
        Assert.IsTrue(MissionStage < EMissionStage.Deployed, MissionStage.ToString());
        ResetTime();
        MissionStage = EMissionStage.Deployed;

        missionProgressTime = 0f;
        missionProgressEnd = toActionTime;
        if (flyToRecovering)
        {
            TimePassed = toActionTime + idlingTime + returnTime + 1f;
        }

        tacMan.FireMissionSent(OrderType);

        ////MissionButton.obsoleteProgress.gameObject.SetActive(false);
    }

    public void Recovering()
    {
        ////MissionButton.obsoleteProgress.gameObject.SetActive(false);
        Assert.IsTrue(MissionStage == EMissionStage.ReadyToRetrieve, MissionStage.ToString());
        SentSquadronsLeft.Clear();
        recoveryTimePassed = timeToRetrieve = 0;
        foreach (var dir in RecoveryDirections)
        {
            switch (dir)
            {
                case 0:
                    timeToRetrieve += tacMan.TicksToRetrieveSquadronToHangar;
                    break;
                case 1:
                    break;
                case 2:
                    timeToRetrieve += tacMan.TicksToRetrieveSquadronToDeck;
                    break;
            }
        }
        MissionStage = EMissionStage.Recovering;
        //timeToFinish = 999;
        //TimePassed = 0;
    }

    public void Recovered()
    {
        Assert.IsTrue(MissionStage == EMissionStage.Recovering);

        SentSquadrons.Clear();
        SentSquadrons.AddRange(SentSquadronsLeft);
        SentSquadronsLeft.Clear();
        RecoveryDirections.Clear();
        if (SentSquadrons.Count > 0)
        {
            Canceled = false;
            for (int i = 0; i < SentSquadrons.Count; i++)
            {
                RecoveryDirections.Add(2);
            }
            MissionStage = EMissionStage.ReadyToRetrieve;
        }
        else
        {
            MissionFinished(true);
            //CompleteMission();
            RemoveMission(true);
        }
    }

    public void MissionFinished(bool finished)
    {
        if (CustomMission)
        {
            tacMan.FireCustomMissionFinished(finished);
        }
        else
        {
            tacMan.FireMissionFinished(OrderType, finished, finalTarget);
        }
    }

    public void StopRecover()
    {
        Assert.IsTrue(MissionStage == EMissionStage.Recovering);
        MissionStage = EMissionStage.ReadyToRetrieve;
        ////MissionButton.obsoleteProgress.gameObject.SetActive(true);
    }

    public void Launching()
    {
        Assert.IsTrue(MissionStage == EMissionStage.ReadyToLaunch);
        MissionStage = EMissionStage.Launching;

        tacMan.FireOrderMissionSent(OrderType);
    }

    public void AbortLaunching()
    {
        Assert.IsTrue(MissionStage == EMissionStage.Launching);
        if (SentSquadrons.Count == 0)
        {
            MissionStage = EMissionStage.ReadyToLaunch;
        }
        else
        {
            ////MissionButton.obsoleteProgress.gameObject.SetActive(true);
            MissionStage = EMissionStage.AwaitingRetrieval;
            ResetTime();
        }
    }

    public void FinishMission(bool showReport)
    {
        switch (OrderType)
        {
            case EMissionOrderType.CarriersCAP:
                tacMan.RemoveCAP(this);
                if (showReport)
                {
                    ReportPanel.Instance.SetupCAP(HadAttack);
                }
                if (HadAttack && UnityEngine.Random.Range(0f, 1f) <= tacMan.CapDamageChance)
                {
                    foreach (var squadron in SentSquadrons)
                    {
                        squadron.IsDamaged = true;
                    }
                }
                break;
            case EMissionOrderType.Scouting:
                tacMan.Carrier.SetAircraftSpottingBonus(false);
                if (showReport)
                {
                    ReportPanel.Instance.SetupScouting();
                }
                break;
        }

        //MissionButton.SwitchInteractionState(false, EDangerLevel.NONE, MissionStage);
        ActionEnd();
        ResetTime();
        MissionStage = EMissionStage.AwaitingRetrieval;
        ButtonMission.Timer.gameObject.SetActive(false);
        if (removeOnReturn)
        {
            MissionFinished(true);
            RemoveMission(true);
        }
        ////MissionButton.obsoleteProgress.gameObject.SetActive(true);
        ////MissionButton.Progress.gameObject.SetActive(false);
    }

    public void Cancel()
    {
        //Assert.IsFalse(MissionStage > EMissionStage.Deployed);
        if (OrderType == EMissionOrderType.NightAirstrike || OrderType == EMissionOrderType.MidwayAirstrike || OrderType == EMissionOrderType.NightScouts || OrderType == EMissionOrderType.MagicNightScouts ||
            OrderType == EMissionOrderType.MagicAirstrike || OrderType == EMissionOrderType.MagicIdentify)
        {
            forceAdd = true;
        }
        Canceled = true;
        switch (MissionStage)
        {
            case EMissionStage.Available:
            case EMissionStage.Planned:
            case EMissionStage.ReadyToLaunch:
                RemoveMission(true);
                break;
            case EMissionStage.Launching:
                ReplaceSentSquadrons();
                flyToRecovering = true;

                //RecoverySquadronDirection.Clear();
                //AllRecoverySquadronsDirection.Clear();
                RecoveryDirections.Clear();
                Assert.IsFalse(SentSquadrons.Count == 0);
                for (int i = 0; i < SentSquadrons.Count; i++)
                {
                    RecoveryDirections.Add(2);
                    //RecoverySquadronDirection.Add(true);
                    //AllRecoverySquadronsDirection.Add(true);
                }
                break;
            case EMissionStage.Deployed:
                ReplaceSentSquadrons();
                Assert.IsFalse(SentSquadrons.Count == 0);
                if (RecoveryDirections.Count != SentSquadrons.Count)
                {
                    RecoveryDirections.Clear();
                    //RecoverySquadronDirection.Clear();
                    //AllRecoverySquadronsDirection.Clear();
                    for (int i = 0; i < SentSquadrons.Count; i++)
                    {
                        RecoveryDirections.Add(2);
                        //RecoverySquadronDirection.Add(true);
                        //AllRecoverySquadronsDirection.Add(true);
                    }
                }

                EnemyAttacksManager.Instance.Detect();
                if (SimpleMissions.Contains(OrderType))
                {
                    FinishMission(true);
                }
                else
                {
                    if (MissionProgressVisualisation)
                    {
                        Destroy(MissionWaypoints.gameObject);
                        var progress = missionProgressTime / missionProgressEnd;
                        MissionProgressVisualisation.RectTransform.anchoredPosition = new Vector2(Mathf.Lerp(StartPosition.x, AttackPosition.x, progress), Mathf.Lerp(StartPosition.y, AttackPosition.y, progress));
                        AttackPosition = MissionProgressVisualisation.RectTransform.anchoredPosition;
                        ReturnPosition = tacMan.Map.MapShip.Position;
                        var returnDifference = AttackPosition - ReturnPosition;
                        ReturnDistance = returnDifference.magnitude;
                        returnEulers = ReturnDistance == 0f ? Vector2.right : returnDifference / ReturnDistance;
                        deployedStage = EMissionDeployedStage.Idle;
                    }
                    TimePassed = toActionTime + idlingTime + (toActionTime - TimePassed);
                    //float recoverTime = toActionTime + idlingTime + 1;
                    //if ((!action) && TimePassed < recoverTime)
                    //{
                    //    TimePassed = recoverTime;
                    //}
                }
                break;
            case EMissionStage.Recovering:
                for (int i = 1; i < RecoveryDirections.Count; i++)
                {
                    switch (RecoveryDirections[i])
                    {
                        case 0:
                            timeToRetrieve -= tacMan.TicksToRetrieveSquadronToHangar;
                            break;
                        case 1:
                            break;
                        case 2:
                            timeToRetrieve -= tacMan.TicksToRetrieveSquadronToDeck;
                            break;
                    }
                    RecoveryDirections[i] = 1;
                }
                break;

        }
        StageChanged(this);
    }

    public void ReplaceSentSquadrons()
    {
        var newSquadrons = new List<PlaneSquadron>();
        foreach (var squadron in SentSquadrons)
        {
            var newSquadron = new PlaneSquadron(squadron.PlaneType);
            newSquadron.IsDamaged = squadron.IsDamaged;
            newSquadrons.Add(newSquadron);
        }
        SentSquadrons = newSquadrons;
    }

    public void SetRadarObject(RadarEnemy enemy)
    {
        radarObject = enemy;
    }

    private void ReadyToRetrieve()
    {
        MissionStage = EMissionStage.ReadyToRetrieve;
    }

    private void ActionStart()
    {
        if (!hadAction)
        {
            action = hadAction = true;
            var tacMan = TacticManager.Instance;
            switch (OrderType)
            {
                case EMissionOrderType.Airstrike:
                case EMissionOrderType.AirstrikeSubmarine:
                case EMissionOrderType.MidwayAirstrike:
                case EMissionOrderType.MagicAirstrike:
                case EMissionOrderType.NightAirstrike:
                    if (hadActionSaved)
                    {
                        return;
                    }

                    SavedHour = TimeManager.Instance.CurrentHour;

                    bool inRange = false;
                    if (EnemyShip == null)
                    {
                        bool submarineAttack = OrderType == EMissionOrderType.AirstrikeSubmarine;
                        foreach (var enemy in tacMan.GetAllShips())
                        {
                            if (enemy.Side == ETacticalObjectSide.Enemy && !enemy.Dead && !enemy.IsDisabled && IsEnemyInRange(enemy) && enemy.HadGreaterInvisibility == submarineAttack)
                            {
                                inRange = true;
                                EnemyShip = enemy;
                                break;
                            }
                        }
                    }
                    else
                    {
                        inRange = !EnemyShip.Dead && IsEnemyInRange(EnemyShip);
                    }

                    if (inRange)
                    {
                        finalTarget = EnemyShip;

                        var list = new List<EnemyManeuverData>();
                        foreach (var block in EnemyShip.Blocks)
                        {
                            block.Visible = true;
                            if (!block.Dead)
                            {
                                list.Add(block.Data);
                            }
                        }
                        tacMan.FireObjectIdentified(EnemyShip.Id);
                        bool attackPrevented = false;
                        if (EnemyShip.CanReceiveCAP)
                        {
                            foreach (var enemy in tacMan.GetAllShips())
                            {
                                if (EnemyShip != enemy && enemy.BaseCanAttack && !enemy.Dead && !enemy.IsDisabled && enemy.Side == ETacticalObjectSide.Enemy && tacMan.IsFriendInRange(EnemyShip, enemy))
                                {
                                    attackPrevented = true;
                                    break;
                                }
                            }
                        }
                        if (attackPrevented)
                        {
                            EnemyAttacksManager.Instance.Detect();
                            ReportPanel.Instance.Setup(EnemyShip, new CasualtiesData(), this, false);
                        }
                        else
                        {
                            int index = SelectedObjectIndex;
                            var durabilities = new List<int>();
                            for (int i = 0; i < EnemyShip.Blocks.Count; i++)
                            {
                                var block = EnemyShip.Blocks[i];
                                if (block.Dead)
                                {
                                    if (i < SelectedObjectIndex)
                                    {
                                        index--;
                                    }
                                }
                                else
                                {
                                    durabilities.Add(block.CurrentDurability);
                                }
                            }

                            var modifiers = Parameters.Instance.DifficultyParams;
                            Assert.IsFalse(EnemyShip.Dead || EnemyShip.IsDisabled);
                            ManeuverCalculator.Calculate(maneuvers, list, durabilities, index, out _, out _, out _, out _, out var casualties, ECalculateType.Real,
                                tacMan.MinDivisor, tacMan.MaxDivisor, tacMan.Divisor, tacMan.GetBonusManeuversDefence(EnemyShip), tacMan.MissionBonusManeuversAttack + bonusAttack, OrderType == EMissionOrderType.MagicAirstrike,
                                modifiers.EnemyBlocksAttackModifier, modifiers.EnemyBlocksDefenseModifier);

                            bool durabilityChanged = false;
                            for (int i = 0, j = 0; i < EnemyShip.Blocks.Count; i++)
                            {
                                var block = EnemyShip.Blocks[i];
                                if (!block.Dead)
                                {
                                    int newDurability = durabilities[j++];

                                    if (block.CurrentDurability != newDurability)
                                    {
                                        block.CurrentDurability = newDurability;
                                        durabilityChanged = true;
                                    }
                                }
                            }
                            foreach (var enemyIndex in casualties.EnemyDestroyedIndices)
                            {
                                var enemy = list[enemyIndex];
                                casualties.EnemyDestroyed.Add(enemy);
                                foreach (var block in EnemyShip.Blocks)
                                {
                                    if (!block.Dead && block.Data == enemy)
                                    {
                                        block.Dead = true;
                                        break;
                                    }
                                }
                                SunkedShips.Add(enemy);
                                tacMan.FireBlockDestroyed(enemy, true);
                            }

                            EnemyShip.CheckIsDead(false);
                            if (!EnemyShip.Dead)
                            {
                                if (GetPlanesCount() >= Parameters.Instance.AttackHinderedMinPlanes)
                                {
                                    EnemyShip.PowerfulAttack();
                                }
                                EnemyAttacksManager.Instance.Detect();
                            }

                            LostBombers = casualties.SquadronsDestroyed[EPlaneType.Bomber];
                            LostFighters = casualties.SquadronsDestroyed[EPlaneType.Fighter];
                            LostTorpedoes = casualties.SquadronsDestroyed[EPlaneType.TorpedoBomber];

                            if (!removeOnReturn)
                            {
                                tacMan.LostBombers += LostBombers;
                                tacMan.LostFighters += LostFighters;
                                tacMan.LostTorpedoes += LostTorpedoes;

                                if (Bombers == LostBombers &&
                                    Fighters == LostFighters &&
                                    Torpedoes == LostTorpedoes &&
                                    casualties.EnemyDestroyedIndices.Count == 0)
                                {
                                    tacMan.FireBlindKamikaze();
                                }
                            }
                            int damagedBombers = casualties.SquadronsBroken[EPlaneType.Bomber];
                            int damagedFighters = casualties.SquadronsBroken[EPlaneType.Fighter];
                            int damagedTorpedoes = casualties.SquadronsBroken[EPlaneType.TorpedoBomber];
                            int losses = LostBombers + LostFighters + LostTorpedoes + damagedBombers + damagedFighters + damagedTorpedoes;
                            tacMan.FireAirstrikeAttacked(losses == 0, EnemyShip);
                            tacMan.RevealObject(EnemyShip.Id);
                            if (!hadActionSaved)
                            {
                                ReportPanel.Instance.Setup(EnemyShip, casualties, this, durabilityChanged);
                            }
                            bool noSurvivors = true;
                            var destroyed = new Dictionary<EPlaneType, int>(casualties.SquadronsDestroyed);
                            var broken = new Dictionary<EPlaneType, int>(casualties.SquadronsBroken);
                            for (int i = 0; i < SentSquadrons.Count; i++)
                            {
                                var squadron = SentSquadrons[i];
                                HadBombers = HadBombers || squadron.PlaneType == EPlaneType.Bomber;
                                HadFighters = HadFighters || squadron.PlaneType == EPlaneType.Fighter;
                                HadTorpedoes = HadTorpedoes || squadron.PlaneType == EPlaneType.TorpedoBomber;

                                int destroyedLeft = destroyed[squadron.PlaneType];
                                if (destroyedLeft > 0)
                                {
                                    destroyedLeft--;
                                    destroyed[squadron.PlaneType] = destroyedLeft;
                                    SentSquadrons.RemoveAt(i--);
                                }
                                else
                                {
                                    noSurvivors = false;

                                    int brokenLeft = broken[squadron.PlaneType];
                                    if (brokenLeft > 0)
                                    {
                                        brokenLeft--;
                                        squadron.IsDamaged = true;
                                        broken[squadron.PlaneType] = brokenLeft;

                                    }
                                }
                            }
                            if (noSurvivors)
                            {
                                //todo delete mission
                            }
                        }
                        tacMan.Markers.UpdateAttackRange(EnemyShip);
                    }
                    else if (!hadActionSaved)
                    {
                        ReportPanel.Instance.AttackFoundNoEnemySetup(OrderType); ////NoEnemy
                    }
                    break;
                case EMissionOrderType.Recon:
                case EMissionOrderType.NightScouts:
                case EMissionOrderType.MagicNightScouts:
                    raid.SetParent(TacticManager.Instance.MapTransform);
                    raid.anchoredPosition = AttackPosition;
                    float range = tacMan.StartRange;
                    if (UseTorpedoes)
                    {
                        range *= 1.25f;
                    }
                    if (!hadActionSaved)
                    {
                        tacMan.NewRevealArea(AttackPosition, range * range, true, OrderType);
                    }
                    MapUnitMaskManager.Instance.AddUnitObject(raid.gameObject, (int)range);
                    break;
                case EMissionOrderType.DetectSubmarine:
                    if (!hadActionSaved)
                    {
                        tacMan.RevealObjects(AttackPosition, Parameters.Instance.DetectSubmarineRangeSqr, true, true, EMissionOrderType.DetectSubmarine);
                    }
                    MapUnitMaskManager.Instance.AddUnitObject(raid.gameObject, (int)Parameters.Instance.DetectSubmarineRange);
                    break;
                case EMissionOrderType.IdentifyTargets:
                case EMissionOrderType.MagicIdentify:
                    if (hadActionSaved)
                    {
                        return;
                    }
                    bool found = false;
                    if (confirmedTarget != null && confirmedTarget.RectTransform != null)
                    {
                        float startRange = tacMan.StartRange;
                        float mult = 1f;
                        if (UseTorpedoes)
                        {
                            mult += .25f;
                        }
                        if (!confirmedTarget.Visible)
                        {
                            mult -= Parameters.Instance.IdentifyUORangeModifier;
                        }
                        mult = Mathf.Clamp(mult, 0f, 10f);
                        startRange *= mult;
                        tacMan.RevealObject(AttackPosition, startRange * startRange, false, false, tacMan.AllObjects.IndexOf(confirmedTarget), out bool revealed, out bool removed);
                        if (revealed)
                        {
                            found = true;
                            finalTarget = confirmedTarget;
                            if (confirmedTarget is TacticalEnemyShip enemyShip && enemyShip.Side == ETacticalObjectSide.Enemy)
                            {
                                tacMan.IdentifyObject(enemyShip.Id, OrderType == EMissionOrderType.MagicIdentify);
                            }
                            if (!hadActionSaved)
                            {
                                ReportPanel.Instance.SetupIdentifyTarget(confirmedTarget, OrderType);
                            }
                        }
                    }

                    if (!found && !hadActionSaved)
                    {
                        ReportPanel.Instance.IdentifyFoundNothingSetup(OrderType);
                    }
                    break;
                case EMissionOrderType.SubmarineHunt:
                    if (hadActionSaved)
                    {
                        return;
                    }
                    EnemyAttacksManager.Instance.DespawnSubmarine(enemyAttack.CurrentTarget != EEnemyAttackTarget.Carrier);
                    ReportPanel.Instance.SubmarineDefSetup();
                    EventManager.Instance.RemoveSubmarineHunt(this);
                    break;
                case EMissionOrderType.CounterHostileScouts:
                    if (hadActionSaved)
                    {
                        return;
                    }
                    EnemyAttacksManager.Instance.DespawnScout(radarObject);
                    EnemyAttacksManager.Instance.Undetect();
                    ReportPanel.Instance.ScoutDefSetup();
                    EventManager.Instance.RemoveHostileScout(this);
                    break;
                case EMissionOrderType.FriendlyFleetCAP:
                case EMissionOrderType.FriendlyCAPMidway:
                    if (hadActionSaved)
                    {
                        return;
                    }
                    finalTarget = tacMan.GetShip(enemyAttackFriend.FriendID);
                    bool hadFight = finalTarget != null && !(finalTarget as TacticalEnemyShip).Dead;
                    EventManager.Instance.RemoveAllyAttacked(this);
                    if (hadFight)
                    {
                        EnemyAttacksManager.Instance.StartFriendlyAttack(enemyAttackFriend, tacMan.FriendlyCAP);
                        if (UnityEngine.Random.Range(0f, 1f) <= tacMan.CapDamageChance)
                        {
                            foreach (var squadron in SentSquadrons)
                            {
                                squadron.IsDamaged = true;
                            }
                        }
                    }
                    ReportPanel.Instance.SetupCAP(hadFight);
                    break;
                case EMissionOrderType.CarriersCAP:
                    tacMan.AddCAP(this, 1);
                    break;
                case EMissionOrderType.Scouting:
                    tacMan.Carrier.SetAircraftSpottingBonus(true);
                    break;
                case EMissionOrderType.Decoy:
                    foreach (var enemy in PossibleMissionTargets)
                    {
                        enemy.Distract(AttackPosition);
                    }
                    //LosePlanes();
                    //RemoveMission(true);
                    break;
                case EMissionOrderType.RescueVIP:
                case EMissionOrderType.AttackJapan:
                    break;
            }
            if (!hadActionSaved)
            {
                tacMan.FireMissionAction(this);
                RecoveryDirections.Clear();
                //RecoverySquadronDirection.Clear();
                //AllRecoverySquadronsDirection.Clear();
                Assert.IsTrue(SentSquadrons.Count > 0 || (Fighters - LostFighters == 0 && Bombers - LostBombers == 0 && Torpedoes - LostTorpedoes == 0) || removeOnReturn, MarkedDestroyed.ToString());
                for (int i = 0; i < SentSquadrons.Count; i++)
                {
                    RecoveryDirections.Add(2);
                    //RecoverySquadronDirection.Add(true);
                    //AllRecoverySquadronsDirection.Add(true);
                }
            }
        }
    }

    public void DespawnSubmarine()
    {
        EnemyAttacksManager.Instance.DespawnSubmarine(enemyAttack.CurrentTarget != EEnemyAttackTarget.Carrier);
        EventManager.Instance.RemoveSubmarineHunt(this);

        hadAction = true;
        RemoveMission(false);
    }

    public void RecalculateMissionNumber()
    {
        ////MissionButton.missionNumberImage.sprite = TacticManager.Instance.GetSpriteForMission(this);
        //MissionButton.missionNumber.text = TacticManager.Instance.GetMissionNumber(this).ToString();
    }

    public void SendMission()
    {
        if (SimpleMissions.Contains(OrderType))
        {
            Confirmed = true;
        }
        MissionSent = true;
        if (Confirmed && MissionProgressVisualisation != null)
        {
            StartPosition = tacMan.Map.MapShip.Rect.anchoredPosition;
            var attackDifference = StartPosition - AttackPosition;
            AttackDistance = attackDifference.magnitude;
            attackEulers = AttackDistance == 0f ? Vector2.right : attackDifference / AttackDistance;
            toActionTime = AttackDistance / tacMan.AirRaidSpeed;
            MissionProgressVisualisation.RectTransform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, attackEulers));
        }
    }

    public void SendFreeMission()
    {
        SendMission();
        Deploy();
        var strikeGroupManager = StrikeGroupManager.Instance;
        strikeGroupManager.ActivateSkill(EStrikeGroupActiveSkill.SendMission);
        strikeGroupManager.ActivateSkill(EStrikeGroupActiveSkill.SendAntiScoutMission);
        removeOnReturn = true;
    }

    public void EscortRecover()
    {
        StrikeGroupManager.Instance.ActivateSkill(EStrikeGroupActiveSkill.ReturnSquadrons);
        EscortRecovered();
    }

    public void SetEscortRecoverRange()
    {
        if (MissionProgressVisualisation != null)
        {
            if (ButtonHovered)
            {
                if (AircraftCarrierDeckManager.Instance.EscortRetrievingSquadrons && CanBeRetrievedByEscort())
                {
                    MissionProgressVisualisation.RangeRect.gameObject.SetActive(true);
                    var size = (Parameters.Instance.EscortCarrierRetrievalSpeed * (timeToFinish - TimePassed) + retrievalRange) * 2f;
                    MissionProgressVisualisation.RangeRect.sizeDelta = new Vector2(size, size);
                }
            }
            else
            {
                MissionProgressVisualisation.RangeRect.gameObject.SetActive(false);
            }
        }
    }

    public void SetMissionIdleTime(int ticks)
    {
        idlingTime = ticks;
    }

    public void UpdatePlanesCount()
    {
        GetPlanes(out Bombers, out Fighters, out Torpedoes);
    }

    private void ActionEnd()
    {
        if (action)
        {
            action = false;
            switch (OrderType)
            {
                case EMissionOrderType.Recon:
                case EMissionOrderType.DetectSubmarine:
                case EMissionOrderType.NightScouts:
                case EMissionOrderType.MagicNightScouts:
                    MapUnitMaskManager.Instance.RemoveUnitObject(raid.gameObject);
                    break;
            }
        }
    }

    //private void OnSubmarineMissionChanged()
    //{
    //    if (MissionStage < EMissionStage.Deployed)
    //    {
    //        ButtonMission.ChangeButtonState(StrikeGroupManager.Instance.HasSubmarine() && SectionRoomManager.Instance.GeneratorsAreWorking, EDangerLevel.NONE);

    //        //MissionButton.SwitchInteractionState(StrikeGroupManager.Instance.HasFreeSubmarine() && SectionRoomManager.Instance.Comms.IsWorking && SectionRoomManager.Instance.GeneratorsAreWorking, EDangerLevel.NONE, MissionStage);
    //        ////MissionButton.Button.interactable = StrikeGroupManager.Instance.HasFreeSubmarine() && SectionRoomManager.Instance.Comms.IsWorking;
    //    }
    //}

    //private void OnSupplyMissionChanged()
    //{
    //    if (MissionStage < EMissionStage.Deployed)
    //    {
    //        ButtonMission.ChangeButtonState(StrikeGroupManager.Instance.HasSupplyShip() && SectionRoomManager.Instance.GeneratorsAreWorking && !ResourceManager.Instance.IsRefilling, EDangerLevel.NONE);

    //        //MissionButton.SwitchInteractionState(StrikeGroupManager.Instance.HasFreeSupplyShips() && SectionRoomManager.Instance.Comms.IsWorking && SectionRoomManager.Instance.GeneratorsAreWorking && !ResourceManager.Instance.IsRefilling, EDangerLevel.NONE, MissionStage);
    //        ////MissionButton.Button.interactable = StrikeGroupManager.Instance.HasFreeSupplyShips() && SectionRoomManager.Instance.Comms.IsWorking && !ResourceManager.Instance.IsRefilling;
    //    }
    //}

    private int Rand(int percMin, int percMax, int count)
    {
        int rand = UnityEngine.Random.Range(percMin, percMax + 1);
        if (rand < 0)
        {
            return 0;
        }
        float value = (rand / 100f) * count;
        int result = (int)value;
        value -= result;
        if (UnityEngine.Random.value <= value)
        {
            result++;
        }
        return result;
    }

    private int Damage(ref int count, ref int destroyCount, ref int planes, EPlaneType type)
    {
        int result = 0;
        while (count > 0 && planes > 0)
        {
            count--;
            planes--;
            result++;
            foreach (var squadron in SentSquadrons)
            {
                if (squadron.PlaneType == type && !squadron.IsDamaged)
                {
                    if (destroyCount > 0)
                    {
                        destroyCount--;
                        SentSquadrons.Remove(squadron);
                    }
                    else
                    {
                        squadron.IsDamaged = true;
                    }
                    break;
                }
            }
        }
        return result;
    }

    private bool CheckRecoveryFailTime()
    {
        float recover = TimePassed / ToRecoverTime;

        if (!HudManager.Instance.HasNo(ETutorialMode.DisableMissionRetrieveFailure) && recover > .9f)
        {
            recover = 0f;
            TimePassed = .9f * ToRecoverTime;
        }
        if (tacMan.RecoveryTimeoutDisabled)
        {
            float diff = ToRecoverTime - TimePassed;
            float minute = timeManager.TicksForHour / 60f;
            if (diff < minute)
            {
                TimePassed = ToRecoverTime - minute;
                recover = 0f;
            }
        }
        if (recover >= 1f)
        {
            MissionFinished(false);

            LosePlanes();

            ButtonMission.Timer.gameObject.SetActive(false);
            RemoveMission(true);
            TacticManager.Instance.FireMissionLost();
            MissionStage = EMissionStage.Failure;
            return true;
        }
        return false;
    }

    private void EscortRecovered()
    {
        var deck = AircraftCarrierDeckManager.Instance;
        foreach (var plane in SentSquadrons)
        {
            deck.SendSquadronToHangar(plane);
        }
        TimePassed = 0f;

        MissionStage = EMissionStage.Recovering;
        Recovered();

        deck.FirePlaneCountChanged();
    }

    public bool GetPlanesReadyToLaunch(out int bombers, out int fighters, out int torpedoes)
    {
        bombers = 0;
        fighters = 0;
        torpedoes = 0;
        if (MissionStage == EMissionStage.ReadyToLaunch)
        {
            GetPlanes(out bombers, out fighters, out torpedoes);
            return true;
        }
        return false;
    }

    public void GetPlanes(out int bombers, out int fighters, out int torpedoes)
    {
        if (CustomMission || AirstrikeMissions.Contains(OrderType))
        {
            bombers = Bombers;
            fighters = Fighters;
            torpedoes = Torpedoes;
        }
        else
        {
            TacticManager.Instance.GetPlanes(OrderType, UseTorpedoes, out bombers, out fighters, out torpedoes);
        }
        //Assert.IsFalse(bombers == 0 && fighters == 0 && torpedoes == 0);
    }

    public int GetPlanesCount()
    {
        return Bombers + Fighters + Torpedoes;
    }

    public void CheckSquadronsLeft()
    {
        if (Fighters - LostFighters == 0 && Bombers - LostBombers == 0 && Torpedoes - LostTorpedoes == 0)
        {
            MissionFinished(true);
            RemoveMission(true);
        }
    }

    public void UpdateMissionText()
    {
        //if (tacMan.MissionDescriptions[OrderType].ContainsKey(MissionStage))
        //{
        //    MissionDescription = tacMan.MissionDescriptions[OrderType][MissionStage];
        //}

        MissionDescription = tacMan.MissionInfo[OrderType].MissionDesc;

        //if (MissionStage == EMissionStage.AwaitingRetrieval)
        //{
        //    MissionDescription = locMan.GetText(EMissionStage.ReadyToRetrieve.ToString());
        //}
        //else
        //{
        //    MissionDescription = locMan.GetText(MissionStage.ToString());
        //}
    }

    public void SetTimeToFinish(int value)
    {
        timeToFinish = value;
    }

    public List<TacticalEnemyShip> GetEnemiesInDecoyRange()
    {
        var decoyEnemies = new List<TacticalEnemyShip>();
        foreach (var enemy in tacMan.GetAllShips())
        {
            if (Vector2.SqrMagnitude(AttackPosition - enemy.RectTransform.anchoredPosition) <= enemy.AttackRangeSqr && enemy.Side == ETacticalObjectSide.Enemy)
            {
                decoyEnemies.Add(enemy);
            }
        }
        return decoyEnemies;
    }

    public bool CanBeRetrievedByEscort()
    {
        return AircraftCarrierDeckManager.Instance.EscortRetrievingSquadrons/* && (Map.MapShip.Position - ReturnPosition).magnitude * Parameters.Instance.EscortCarrierRetrievalSpeed < timeToFinish - TimePassed*/;
    }

    public void SetToObsoleteTime(int timeInHours)
    {
        ToObsoleteTime = timeInHours * timeManager.TicksForHour;
        SetTimeToFinish(ToObsoleteTime);
        ButtonMission.NameText.gameObject.SetActive(false);
    }

    private void OnMinutePassed()
    {
        minutes = minuteCounter.Count(timeManager, SetTimeText, minutes);
        recoveryMinutes = minuteCounter.Count(timeManager, SetRecoveryTimeText, recoveryMinutes);
    }

    private void SetMinuteTimer()
    {
        // minutes = Mathf.RoundToInt(60f * (timeToObsolide) / timeManager.TicksForHour) - Mathf.RoundToInt(60f * (mission.TimePassed) / timeManager.TicksForHour);
        minutes = Mathf.RoundToInt(60f * (timeToFinish - TimePassed) / timeManager.TicksForHour);
        minutes = Mathf.Max(minutes, 0);
        SetTimeText();
    }

    private void SetRecoveryMinuteTimer()
    {
        recoveryMinutes = Mathf.RoundToInt(60f * (timeToRetrieve - recoveryTimePassed) / timeManager.TicksForHour);
        recoveryMinutes = Mathf.Max(recoveryMinutes, 0);
        SetRecoveryTimeText();
    }

    private void SetRecoveryTimeText()
    {
        ButtonMission.RecoveryHoursTimer.text = (recoveryMinutes / 60).ToString("00");
        ButtonMission.RecoveryMinutesTimer.text = (recoveryMinutes % 60).ToString("00");
        ButtonMissionTimeChanged();
    }

    private void SetTimeText()
    {
        ButtonMission.HoursTimer.text = (minutes / 60).ToString("00");
        ButtonMission.MinutesTimer.text = (minutes % 60).ToString("00");
        ButtonMissionTimeChanged();
    }

    private void OnObsolete()
    {
        var enemyAttackMan = EnemyAttacksManager.Instance;
        switch (OrderType)
        {
            case EMissionOrderType.SubmarineHunt:
                enemyAttackMan.StartSubmarineAttack(enemyAttack);
                enemyAttackMan.DespawnSubmarine(enemyAttack.CurrentTarget != EEnemyAttackTarget.Carrier);
                EventManager.Instance.RemoveSubmarineHunt(this);
                break;
            case EMissionOrderType.CounterHostileScouts:
                enemyAttackMan.DespawnScout(radarObject);
                enemyAttackMan.Detect();
                EventManager.Instance.RemoveHostileScout(this);
                break;
            case EMissionOrderType.FriendlyFleetCAP:
            case EMissionOrderType.FriendlyCAPMidway:
#if ALLOW_CHEATS
                if (!TacticManager.Instance.FriendImmune)
#endif
                {
                    enemyAttackMan.StartFriendlyAttack(enemyAttackFriend, 0);
                }
                EventManager.Instance.RemoveAllyAttacked(this);
                break;
        }
    }

    private void AddSentSquadrons(int count, EPlaneType type)
    {
        for (int i = 0; i < count; i++)
        {
            SentSquadrons.Add(new PlaneSquadron(type));
        }
    }

    private void AddVisualization()
    {
        MissionWaypoints = new GameObject("", typeof(RectTransform)).GetComponent<RectTransform>();
        MissionWaypoints.SetParent(Map.transform);
        MissionWaypoints.anchoredPosition = Vector2.zero;
        MissionWaypoints.localScale = Vector3.one;
        MissionWaypoints.name = "Waypoints";
        MissionWaypoints.gameObject.SetActive(false);

        var attackPosWaypoint = Instantiate(AirstrikeMissions.Contains(OrderType) ? Map.AircraftAttackPointPrefab : Map.AircraftReconPointerPrefab, MissionWaypoints);

        var param = Parameters.Instance;
        float mult = 1f;
        if (OrderType == EMissionOrderType.IdentifyTargets && confirmedTarget != null && !confirmedTarget.Visible)
        {
            mult -= param.IdentifyUORangeModifier;
        }
        if (SwitchPlaneTypeMissions.Contains(OrderType) && UseTorpedoes)
        {
            mult += .25f;
        }
        mult = Mathf.Clamp(mult, 0f, 10f);
        attackPosWaypoint.DrawRadius(OrderType == EMissionOrderType.DetectSubmarine ? param.DetectSubmarineRange : (tacMan.StartRange * mult));

        attackPosWaypoint.enabled = false;
        attackPosWaypoint.RectTransform.anchoredPosition = AttackPosition;

        var returnPosWaypoint = Instantiate(Map.AircraftAttackReturnPointPrefab, MissionWaypoints);
        returnPosWaypoint.DrawRadius(retrievalRange);
        returnPosWaypoint.enabled = false;
        returnPosWaypoint.RectTransform.anchoredPosition = ReturnPosition;

        Map.SpawnTrack(StartPosition, AttackPosition, attackPosWaypoint, MissionWaypoints);
        Map.SpawnTrack(AttackPosition, ReturnPosition, returnPosWaypoint, MissionWaypoints);
    }

    private bool IsEnemyInRange(TacticalEnemyShip ship)
    {
        return Vector2.SqrMagnitude(ship.RectTransform.anchoredPosition - AttackPosition) < (tacMan.StartRange * tacMan.StartRange);
    }

    private void LosePlanes()
    {
        tacMan.LostBombers += Bombers - LostBombers;
        tacMan.LostFighters += Fighters - LostFighters;
        tacMan.LostTorpedoes += Torpedoes - LostTorpedoes;
    }
}

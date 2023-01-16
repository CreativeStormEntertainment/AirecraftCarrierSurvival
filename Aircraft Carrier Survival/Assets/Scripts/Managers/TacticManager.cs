using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class TacticManager : MonoBehaviour, IPopupPanel, ITickable, IEnableable
{
    public const int MOVE_STRAIGHT_COST = 10;
    public const int MOVE_DIAGONAL_COST = 14;

    public delegate void DestinationReachedDelegate(int enemy, int node);

    public event Action<TacticalEnemyMapButton> NewEnemyPosition = delegate { };
    public event Action<TacticalEnemyMapButton> EnemyDisappeared = delegate { };
    public event Action<TacticalEnemyMapButton> EnemyUnitIdentified = delegate { };
    public event Action<MapEnemyShip> EnemyRouted = delegate { };
    public event Action<int, bool> ObjectVisibilityChanged = delegate { };
    public event Action<int> ObjectIdentified = delegate { };
    public event Action<int, bool> ObjectDestroyed = delegate { };
    public event Action<EnemyManeuverData> BlockDestroyed = delegate { };
    public event Action<EnemyManeuverData> EnemyBlockDestroyed = delegate { };
    public event Action EnemyBlockDestroyedAfterWar = delegate { };
    public event DestinationReachedDelegate DestinationReached = delegate { };
    public event Action EdgeMapReached = delegate { };
    public event Action<bool> CustomMissionFinished = delegate { };
    public event Action<EMissionOrderType, bool, TacticalObject> MissionFinished = delegate { };
    public event Action<TacticalMission> MissionAction = delegate { };
    public event Action<TacticalMission> CurrentMissionChanged = delegate { };
    public event Action BlindKamikaze = delegate { };
    public event Action<TacticalEnemyShip> ObjectRevealed = delegate { };
    public event Action<int, bool> SurvivorObjectFinished = delegate { };
    public event Action<bool, TacticalEnemyShip> AirstrikeAttacked = delegate { };
    public event Action<EMissionOrderType> MissionPlanned = delegate { };
    public event Action<EMissionOrderType> MissionSent = delegate { };
    public event Action<EMissionOrderType> MissionPlanningStarted = delegate { };
    public event Action<EMissionOrderType> OrderMissionSent = delegate { };
    public event Action UORevealed = delegate { };
    public event Action SwimMile = delegate { };
    public event Action<int, bool> RandomNodeChosen = delegate { };
    public event Action SearchAndDestroyReady = delegate { };

    public event Action<int, int> PotentialRandomNodeChosen = delegate { };
    public event Action RandomNodeDenied = delegate { };
    public event Action MissionLost = delegate { };

    public event Action NodesChanged = delegate { };

    public static TacticManager Instance;

    public Markers Markers => markers;

    public float RetrievalRangeModifier
    {
        get => retrievalRangeModifier;
        set
        {
            retrievalRangeModifier = value;
            RetrievalRange = (int)(baseRetrievalRange * retrievalRangeModifier) + BonusRetrievalRange;
        }
    }

    public int BonusRetrievalRange
    {
        get => bonusRetrievalRange;
        set
        {
            bonusRetrievalRange = value;
            RetrievalRange = (int)(baseRetrievalRange * retrievalRangeModifier) + BonusRetrievalRange;
        }
    }

    public float RetrievalTimeModifier
    {
        get;
        set;
    } = 1f;

    public bool MagicIdentifyPermanentRemove
    {
        get;
        set;
    }

    public bool AllowMissionCancel
    {
        get;
        set;
    }

    public bool ObsoleteDisabled
    {
        get;
        set;
    }

    public bool RecoveryTimeoutDisabled
    {
        get;
        set;
    }

    public EMissionOrderFlag EnabledMissions
    {
        get;
        set;
    }

    public int RetrievalRange
    {
        get;
        private set;
    }

    public Vector2 MinMapPosition
    {
        get;
        private set;
    }
    public Vector2 MaxMapPosition
    {
        get;
        private set;
    }

    public SOTacticMap SOTacticMap
    {
        get;
        private set;
    }

    public int EnemyBlocksDestroyed
    {
        get;
        private set;
    }
    public ManeuversList PlayerManeuversList => playerManeuversList;

    public Dictionary<EManeuverType, List<PlayerManeuverData>> PlayerManevuersDict => maneuversDict;
    public List<PlayerManeuverData> AllPlayerManeuvers => playerManeuversList.Maneuvers;
    public PlayerManeuverData MidwayCustomManeuver => playerManeuversList.MidwayCustomManeuver;
    public PlayerManeuverData MagicCustomManeuver => playerManeuversList.MagicCustomManeuver;

    public EWindowType Type => EWindowType.TacticalMap;

    [SerializeField]
    private float airRaidSpeed = 1f;
    [SerializeField]
    private float flightTimeOnFuel = 10f;

    [SerializeField]
    private int toObsoleteHours = 4;
    [SerializeField]
    private float timeToBrief = 1;
    [SerializeField, Range(0f, 20f)]
    private float enemyHighlightTime = 7f;

    [SerializeField]
    private TacticalMap map = null;
    [SerializeField]
    private RectTransform enemyInfoPanelParent = null;
    [SerializeField]
    private Transform gridParent = null;
    [SerializeField]
    private Text airRaidButtonText;
    [SerializeField]
    private StrategyAnalysisPanel startegyAnalysisPanel = null;
    [SerializeField]
    private List<Image> identificationsIcons = null;
    [SerializeField]
    private GameObject identifyUnitItemPrefab = null;
    [SerializeField]
    private Transform identifyUnitsParent = null;
    [SerializeField]
    private GameObject attackMapButtons = null;
    [SerializeField]
    private Button confirmButton = null;
    [SerializeField]
    private GameObject missionUIButton = null;
    [SerializeField]
    private List<Strategy> strategies = new List<Strategy>();
    [SerializeField]
    private Sprite defaultMissionSprite = null;
    [SerializeField]
    private int baseRetrievalRange = 100;
    [SerializeField]
    private int ticksToRetrieveSquadronToDeck = 46;
    [SerializeField]
    private int ticksToRetrieveSquadronToHangar = 52;
    [SerializeField]
    private int ticksToLandSquadrons = 25;

    private float retrievalRangeModifier = 1f;
    private int bonusRetrievalRange;

    private static Color unactiveColor = new Color(1f, 1f, 1f, 0.3f);

    public List<EnemyUnit> availableUnits = new List<EnemyUnit>();

    public float RevealedPositionUncertaintyRange = 40f;

    public float DotPerX = 30f;

    public ChooseOrderType Radial;

    [SerializeField]
    private SOTacticMap defaultMap = null;

    [Header("RedWaters Setup")]

    public List<EnemyManeuverData> EnemiesList = new List<EnemyManeuverData>();

    public RedWaterTacticSetup RedWaterSetup = null;

    public float AirRaidSpeed => airRaidSpeed;
    public int TicksToRetrieveSquadronToHangar => ticksToRetrieveSquadronToHangar;
    public int TicksToLandSquadrons => ticksToLandSquadrons;
    public int TicksToRetrieveSquadronToDeck => ticksToRetrieveSquadronToDeck;
    public int ToObsoleteHours => toObsoleteHours;
    public float TimeToBrief => timeToBrief;
    public int StrategyBonusManeuversAttack => BonusManeuversAttackBuff + buffManeuversAttack + consequenceManeuversAttack;
    public int MissionBonusManeuversAttack => buffManeuversAttack + consequenceManeuversAttack;
    public EMissionOrderType OrderType
    {
        get;
        set;
    }
    public List<Strategy> Strategies => strategies;
    public TacticalEnemyShip ChosenEnemyShip
    {
        get;
        set;
    }

    public TacticalObject ConfirmedTarget
    {
        get;
        set;
    }
    public List<EnemyUnit> PreviewEnemyUnits
    {
        get;
        set;
    }

    public float DistanceOnFuel
    {
        get;
        private set;
    }
    public float AttackDistance
    {
        get;
        private set;
    }
    public float ReturnDistance
    {
        get;
        private set;
    }
    public float TotalAttackDistance
    {
        get;
        private set;
    }
    public int BonusManeuversDefence
    {
        get;
        set;
    }
    public int IslandBuffBonusManeuversDefence
    {
        get;
        set;
    }
    public Vector2 StartPosition
    {
        get;
        set;
    }
    public Vector2 AttackPosition
    {
        get;
        set;
    }
    public Vector2 ReturnPosition
    {
        get;
        set;
    }
    public Vector3 ShipDirection
    {
        get;
        set;
    }

    public TacticalMapGrid MapNodes => Map.MapNodes;
    public int LostSquadrons => LostBombers + LostFighters + LostTorpedoes;
    public List<TacticalMission> CustomMissions => customMissions;
    public RectTransform EnemyInfoPanelParent => enemyInfoPanelParent;
    public Button ConfirmButton => confirmButton;

    public List<TacticalEnemyMapButton> Fleets = new List<TacticalEnemyMapButton>();

    public List<EnemyLastSeen> FleetMarkers = new List<EnemyLastSeen>();

    public TacticalMapShip Carrier;

    public GameObject BuoyOutlinePrefab = null;
    public GameObject MissionWaypointOutlinePrefab = null;

    public List<RuntimeAnimatorController> BuoysAnimationControllers = null;

    private Dictionary<TacticalEnemyMapButton, EnemyLastSeen> lastSeenEnemies;
    private HashSet<EnemyLastSeen> freeMarkers;
    private List<TacticalEnemyMapButton> toRemoveList = new List<TacticalEnemyMapButton>();

    public Dictionary<EMissionOrderType, List<TacticalMission>> Missions = new Dictionary<EMissionOrderType, List<TacticalMission>>();
    private List<TacticalMission> customMissions = new List<TacticalMission>();
    public GameObject MissionUIButton => missionUIButton;
    public Transform missionButtonsParent;
    public MissionPanel MissionPanel;
    public TacticalEnemyMapButton CurrentFleetBtn
    {
        get;
        set;
    }

    public List<PlayerManeuverData> PlayerManevuers
    {
        get;
        set;
    }

    public List<TacticalObject> DefaultAirstrikeTargets
    {
        get;
        set;
    } = new List<TacticalObject>();

    public RectTransform MapTransform => mapTransform;

    private GameObject missionsObject;
    private List<RangeData> revealedRanges;

    private Dictionary<TacticalMission, CapData> capsDict;

    private string missionName;
    private string missionDesc;
    private Sprite missionIcon;

    public int CAPDefencePoints = 1;

    public float StartRange = 150f;

    public int CounterScoutToRecoverHours = 1;
    public int CounterScoutToActionHours = 1;
    public int HuntToRecoverHours = 1;
    public int HuntToActionHours = 1;

    public int SpottingHours = 8;
    public int ReconHours = 4;

    public int CAPHours = 8;
    public int CAPRecoverHours = 8;
    public int SpottingRecoverHours = 2;
    public int AttackRecoverHours = 2;
    public int ReconRecoverHours = 2;
    public int IdentifyRecoverHours = 2;
    public int SubmarineHuntingHours = 4;
    public int SubmarineHuntingObsoleteHours = 6;
    public int CounterHostileScoutsHours = 1;
    public int CounterHostileScoutsObsoleteHours = 6;
    public int FriendlyCapActionHours = 5;
    public int PlanesStartingTicks = 10;

    public int DecoyFlyHours = 2;

    public FightModifiersData CargoData;
    public FightModifiersData CarrierData;
    public FightModifiersData FightersData;
    public FightModifiersData LightShipData;
    public FightModifiersData HeavyShipData;
    public FightModifiersData OffenceBData;
    public FightModifiersData OffenceTData;
    public FightModifiersData DefenceBData;
    public FightModifiersData DefenceTData;
    public FightModifiersData DefenceHPData;

    public int SelectedObjectIndex;

    public int FriendlyCAP = 2;
    [SerializeField]
    private int CAPDefence = 1;
    [SerializeField]
    private int CAPEscort = 1;

    private int timer;

    public int BaseMaxMissions = 3;
    private int islandBoost;
    private int islandBuff;
    [NonSerialized]
    public int CurrentMaxMissions;

    public MapMovement MapMovement;

    public string TacticMissionReportTitle = "Mission ";
    [TextArea]
    public string TacticMissionLosses = "Fighter squadrons lost: {0}\nBomber squadrons lost:{1}\nTorpedo squadrons lost:{2}";
    public string EnemyShipSunkedText = "Destroyed enemy ";
    public string EnemyFleetRouted = "Routed enemy fleet!";
    public string EnemyFleetDestroyed = "Destroyed enemy fleet!";
    public string EnemyNotFound = "No enemy fleets encountered";

    public List<MissionName> missionsNames;
    public List<MissionName> missionsDescs;

    private List<MissionInfoData> missionInfoDatas;
    [NonSerialized]
    public Dictionary<EMissionOrderType, MissionInfoData> MissionInfo;

    public TacticalMap Map => map;
    [SerializeField]
    private Button tacticMapButton = null;
    [Header("New Enemy Movement")]
    public bool PatrolNodesVisible = false;
    public bool EnemyPathVisible = false;

    public GameObject MapNodePrefab = null;
    public RectTransform PatrolNodePrefab = null;

    [SerializeField]
    private Vector2Int nodeResolution = new Vector2Int(100, 200);
    [SerializeField]
    private bool mapNodesVisible = false;
    [SerializeField]
    private Transform enemyParent = null;
    [SerializeField]
    private GameObject enemyShipPrefab = null;
    [SerializeField]
    private GameObject mapEdges = null;
    [SerializeField]
    private RectTransform mapCornerHorizontal = null;
    [SerializeField]
    private RectTransform mapCornerVertical = null;

    [Header("Unidentified Objects")]
    [SerializeField]
    private Markers markers = null;
    public bool UOVisibility = false;
    [SerializeField]
    private float carrierOffset = 300;
    [SerializeField]
    private float borderOffset = 100;
    [SerializeField]
    private Transform unidentifiedObjectParent = null;
    [SerializeField]
    private UnidentifiedObject unidentifiedObjectPrefab = null;
    [SerializeField]
    private float hoursToSpawnUO = 2f;
    [SerializeField]
    private int maxToSpawnUO = 3;

    [SerializeField]
    private ManeuversList playerManeuversList = null;

    [SerializeField]
    private RectTransform mapTransform = null;

    [Header("MISSIONS")]
    [SerializeField]
    private List<StartMissionStruct> startMissionsSetup = new List<StartMissionStruct>();
    public Dictionary<EMissionOrderType, MissionCountData> missionsCount = new Dictionary<EMissionOrderType, MissionCountData>();
    public StrategySelectionPanel StrategySelectionPanel;
    public TacticalMission CurrentMission
    {
        get => currentMission;
        set
        {
            currentMission = value;
            CurrentMissionChanged(currentMission);
        }
    }

    public int Bombers
    {
        get;
        set;
    }
    public int Fighters
    {
        get;
        set;
    }
    public int Torpedoes
    {
        get;
        set;
    }

    public int LostBombers
    {
        get;
        set;
    }
    public int LostFighters
    {
        get;
        set;
    }
    public int LostTorpedoes
    {
        get;
        set;
    }

    public bool MissionsEnabled
    {
        get;
        set;
    } = true;

    public MapCornerData MapCornerData
    {
        get;
        private set;
    }

    public int BonusAllyBlocks
    {
        get;
        private set;
    }

    public bool HasManeuversAttackBuff
    {
        get;
        set;
    }

    public int BonusManeuversAttackBuff
    {
        get;
        set;
    }

#if ALLOW_CHEATS
    public bool FriendImmune
    {
        get;
        private set;
    }

    public bool FastFriends
    {
        get;
        private set;
    }
#endif

    public int MinDivisor => minDivisor;
    public int MaxDivisor => maxDivisor;
    public int Divisor => estimateLossesDivisor;

    public List<TacticalObject> AllObjects => allObjects;

    public SOTacticMap DefaultMap => defaultMap;
    public SOTacticMap EmptyMap => worldMapMap;

    public MissionProgressVisualisation MissionProgressVisualisationPrefab;

    public EnemyManeuverData BonusAllyStrikeGroupBlock => bonusAllyStrikeGroupBlock;
    public EnemyManeuverData BonusAllyOutpostBlock => bonusAllyOutpostBlock;
    public float CapDamageChance => capDamageChance;

    [SerializeField]
    private float capDamageChance = 0.35f;
    [SerializeField]
    private int minDivisor = 18;
    [SerializeField]
    private int maxDivisor = 6;
    [SerializeField]
    private int estimateLossesDivisor = 10;

    [SerializeField]
    private DamageRange damageRange = null;
    [SerializeField]
    private DamageRange damageRange2 = null;

    [SerializeField]
    private List<AttackParametersData> bonusFromBombersLevel = null;

    [SerializeField]
    private List<AttackParametersData> bonusFromFightersLevel = null;

    [SerializeField]
    private List<AttackParametersData> bonusFromTorpedoesLevel = null;

    [SerializeField]
    private SOTacticMap worldMapMap = null;

    [SerializeField]
    private SurvivorObject survivorObjectPrefab = null;
    [SerializeField]
    private Transform survivorObjectParent = null;

    [SerializeField]
    private ToggleSpriteSwap attackRangeToggle = null;
    [SerializeField]
    private ToggleSpriteSwap reconRangeToggle = null;
    [SerializeField]
    private ToggleSpriteSwap rescueRangeToggle = null;
    [SerializeField]
    private ToggleSpriteSwap cannonRangeToggle = null;
    [SerializeField]
    private ToggleSpriteSwap missionRangeToggle = null;
    [SerializeField]
    private ToggleSpriteSwap magicSpriteToggle = null;
    [SerializeField]
    private ToggleSpriteSwap pathToggle = null;

    [SerializeField]
    private EnemyManeuverData bonusAllyStrikeGroupBlock = null;
    [SerializeField]
    private EnemyManeuverData bonusAllyOutpostBlock = null;
    [SerializeField]
    private int bonusAllyBlocks = 2;

    [SerializeField]
    private List<EnemyManeuverData> allManeuvers = null;

    private List<TacticalEnemyShip> objects;
    private List<TacticalObject> allObjects;
    private Dictionary<EEnemyShipType, List<EnemyManeuverData>> enemies;

    private TacticalMission currentMission;
    private int spawnUOTimer;

    private List<TacticalMission> switchMissionsToDelete = new List<TacticalMission>();
    private EMissionOrderType orderTypeToDelete;

    private Dictionary<EManeuverType, List<PlayerManeuverData>> maneuversDict;

    //private Dictionary<EMissionStage, string> dict = new Dictionary<EMissionStage, string>();

    private AttackParametersData bomberData;
    private AttackParametersData fighterData;
    private AttackParametersData torpedoData;

    private List<SurvivorObject> allSurvivors = new List<SurvivorObject>();

    private List<ITacticalObjectHelper> helperReconList = new List<ITacticalObjectHelper>();
    private List<TacticalEnemyShip> unrevealedEnemies = new List<TacticalEnemyShip>();

    private bool showPath;
    private bool moreDefense;
    private int buffManeuversAttack;
    private int consequenceManeuversAttack;

    private ETemporaryBuff buff;

    private Dictionary<Objective, ProximityData> proximities;
    private Dictionary<Objective, bool> proximityHelper;

    private Dictionary<EMisidentifiedType, List<EnemyManeuverData>> misidentifieds;
    private Dictionary<EMisidentifiedType, HashSet<EnemyManeuverData>> misidentifiedsDict;

    private List<int> missionRangeTargets;

    private List<int> damageRangeEnemies;

    private bool lesserForm;
    private bool finalForm;

    private void Awake()
    {
        Instance = this;

        missionRangeTargets = new List<int>();

        RetrievalRange = baseRetrievalRange;
        EnabledMissions = (EMissionOrderFlag)(-1);

        missionIcon = defaultMissionSprite;
        lastSeenEnemies = new Dictionary<TacticalEnemyMapButton, EnemyLastSeen>();
        freeMarkers = new HashSet<EnemyLastSeen>();
        foreach (var marker in FleetMarkers)
        {
            freeMarkers.Add(marker);
        }

        revealedRanges = new List<RangeData>();

        DistanceOnFuel = airRaidSpeed * flightTimeOnFuel * TimeManager.Instance.TicksForHour;

        var locMan = LocalizationManager.Instance;
        missionInfoDatas = new List<MissionInfoData>();
        MissionInfo = new Dictionary<EMissionOrderType, MissionInfoData>();
        for (int i = 0; i < missionsNames.Count; i++)
        {
            missionInfoDatas.Add(new MissionInfoData());
            MissionInfo[missionsNames[i].Type] = missionInfoDatas[i];
            missionInfoDatas[i].MissionName = locMan.GetText(missionsNames[i].Title);
            missionInfoDatas[i].MissionDesc = locMan.GetText(missionsDescs[i].Descriptions[0].desc);
            missionInfoDatas[i].MissionSprite = missionsNames[i].Icon;

            //Dictionary<EMissionStage, string> dict = new Dictionary<EMissionStage, string>();
            //for (int index = 0; index < missionsNames[i].Descriptions.Count; index++)
            //{
            //    dict.Add(missionsNames[i].Descriptions[index].stage, locMan.GetText(missionsNames[i].Descriptions[index].desc));
            //}
            Missions.Add(missionsNames[i].Type, new List<TacticalMission>());
        }

        objects = new List<TacticalEnemyShip>();
        allObjects = new List<TacticalObject>();

        enemies = new Dictionary<EEnemyShipType, List<EnemyManeuverData>>();
        enemies.Add(EEnemyShipType.Cargo, EnemiesList.FindAll(x => x.ShipType == EEnemyShipType.Cargo));
        enemies.Add(EEnemyShipType.Carrier, EnemiesList.FindAll(x => x.ShipType == EEnemyShipType.Carrier));
        enemies.Add(EEnemyShipType.Warship, EnemiesList.FindAll(x => x.ShipType == EEnemyShipType.Warship));

        maneuversDict = new Dictionary<EManeuverType, List<PlayerManeuverData>>();
        maneuversDict[EManeuverType.Aggressive] = new List<PlayerManeuverData>();
        maneuversDict[EManeuverType.Supplementary] = new List<PlayerManeuverData>();
        maneuversDict[EManeuverType.Defensive] = new List<PlayerManeuverData>();

        attackRangeToggle.ToggleChanged += SetShowAttackRange;
        reconRangeToggle.ToggleChanged += SetShowReconRange;
        rescueRangeToggle.ToggleChanged += SetShowRescueRange;
        cannonRangeToggle.ToggleChanged += SetShowCannonRange;
        missionRangeToggle.ToggleChanged += SetShowMissionRange;
        magicSpriteToggle.ToggleChanged += SetShowMagicSprite;
        pathToggle.ToggleChanged += SetShowPath;

        maxToSpawnUO = Mathf.Max(1, maxToSpawnUO);

        proximities = new Dictionary<Objective, ProximityData>();
        proximityHelper = new Dictionary<Objective, bool>();

        misidentifieds = new Dictionary<EMisidentifiedType, List<EnemyManeuverData>>();
        misidentifiedsDict = new Dictionary<EMisidentifiedType, HashSet<EnemyManeuverData>>();
        foreach (var maneuver in allManeuvers)
        {
            if (!misidentifieds.TryGetValue(maneuver.MisidentifiedType, out var list))
            {
                list = new List<EnemyManeuverData>();
                misidentifieds.Add(maneuver.MisidentifiedType, list);
            }
            if (!misidentifiedsDict.TryGetValue(maneuver.MisidentifiedType, out var set))
            {
                set = new HashSet<EnemyManeuverData>();
                misidentifiedsDict.Add(maneuver.MisidentifiedType, set);
            }
            list.Add(maneuver);
            set.Add(maneuver);
        }

        capsDict = new Dictionary<TacticalMission, CapData>();

        damageRangeEnemies = new List<int>();
    }

    private void Start()
    {
        map.Setup();
        missionsObject = new GameObject("Missions");

        //foreach (var fleet in Fleets)
        //{
        //    fleet.CreateFleet();
        //}

        foreach (var marker in FleetMarkers)
        {
            marker.gameObject.SetActive(false);
        }

        for (int i = 0; i < availableUnits.Count; ++i)
        {
            availableUnits[i].GuessedId = i;
        }

        TimeManager.Instance.AddTickable(this);

        WorldMap.Instance.Toggled += OnWorldMapToggled;
        SectionRoomManager.Instance.GeneratorsStateChanged += OnGeneratorsStateChanged;
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }
#endif

        var missions = BasicInput.Instance.TacticMissions;
        missions.PrepareAirstrike.performed -= PrepareMissionCallbackAirstrike;
        missions.PrepareIdentify.performed -= PrepareMissionCallbackIdentify;
        missions.PrepareRecon.performed -= PrepareMissionCallbackRecon;
    }

    public void SetEnable(bool enable)
    {
        if (enable)
        {
            if (spawnUOTimer > 100_000)
            {
                spawnUOTimer = 0;
            }
        }
        else
        {
            spawnUOTimer = 100_000_000;
        }
    }

    public void PreSetup(ETemporaryBuff buff)
    {
        for (int i = 0; i < startMissionsSetup.Count; ++i)
        {
            var data = startMissionsSetup[i];
            switch (data.Mission)
            {
                case EMissionOrderType.Airstrike:
                    if (buff == ETemporaryBuff.AirstrikeMission)
                    {
                        data.Count++;
                    }
                    break;
                case EMissionOrderType.Recon:
                    if (buff == ETemporaryBuff.ReconMission)
                    {
                        data.Count++;
                    }
                    break;
                case EMissionOrderType.IdentifyTargets:
                    if (buff == ETemporaryBuff.IdentifyMission)
                    {
                        data.Count++;
                    }
                    break;
            }
            missionsCount.Add(data.Mission, new MissionCountData(data.Count));
            for (int j = 0; j < data.Count; j++)
            {
                AddNewMission(data.Mission);
            }
        }
        switch (buff)
        {
            case ETemporaryBuff.StrongerAirstrikeAttack:
                buffManeuversAttack = 1;
                break;
            case ETemporaryBuff.StrongerAirstrikeDefence:
                moreDefense = true;
                break;
            case ETemporaryBuff.StrongerAllies:
                BonusAllyBlocks = bonusAllyBlocks;
                break;
        }
    }

    public void Setup(SOTacticMap map, ETemporaryBuff buff)
    {
        this.buff = buff;
        var deck = AircraftCarrierDeckManager.Instance;

        bomberData = bonusFromBombersLevel[deck.BomberLv];
        fighterData = bonusFromFightersLevel[deck.FighterLv];
        torpedoData = bonusFromTorpedoesLevel[deck.TorpedoLv];

        foreach (var maneuver in playerManeuversList.Maneuvers)
        {
            AddValues(maneuver, bomberData, fighterData, torpedoData);
            AddValues(maneuver.Level2, bomberData, fighterData, torpedoData);
            AddValues(maneuver.Level3, bomberData, fighterData, torpedoData);

            maneuver.MainLevel = maneuver;
            maneuver.Level2.MainLevel = maneuver;
            maneuver.Level3.MainLevel = maneuver;
        }
        AddValues(playerManeuversList.MidwayCustomManeuver, bomberData, fighterData, torpedoData);
        AddValues(playerManeuversList.MagicCustomManeuver, bomberData, fighterData, torpedoData);

        foreach (var survivor in allSurvivors)
        {
            if (!survivor.Dead)
            {
                survivor.Die();
            }
            Destroy(survivor.gameObject);
            allObjects.Remove(survivor);
        }
        allSurvivors.Clear();
        var levels = SaveManager.Instance.Data.ManeuversLevels;
        var overrideManeuvers = map.Overrides.AvailableManeuvers;
        for (int i = 0; i < playerManeuversList.AdmiralManeuversCount; i++)
        {
            PlayerManeuverData manev;
            if (overrideManeuvers == null)
            {
                var mainManev = playerManeuversList.Maneuvers[i];
                switch (levels[i])
                {
                    case 3:
                        manev = mainManev.Level3;
                        break;
                    case 2:
                        manev = mainManev.Level2;
                        break;
                    default:
                        manev = mainManev;
                        break;
                }
                manev.MainLevel = mainManev;
                manev.Level = levels[i];
            }
            else
            {
                if (i < overrideManeuvers.Count)
                {
                    manev = overrideManeuvers[i];
                    manev.Level = 1;
                }
                else
                {
                    break;
                }
            }
            maneuversDict[manev.ManeuverType].Add(manev);
        }
        if (overrideManeuvers == null)
        {
            AddOfficerManeuvers();
        }
        StrategySelectionPanel.Setup();
        for (int i = 0; i < (int)EMissionOrderType.Count; i++)
        {
            ShowMissionPanelHelper((EMissionOrderType)i, Missions[(EMissionOrderType)i].Count <= 0);
        }

        var missions = BasicInput.Instance.TacticMissions;
        missions.PrepareAirstrike.performed += PrepareMissionCallbackAirstrike;
        missions.PrepareIdentify.performed += PrepareMissionCallbackIdentify;
        missions.PrepareRecon.performed += PrepareMissionCallbackRecon;
    }

    public void RefreshManeuvers()
    {
        var levels = SaveManager.Instance.Data.ManeuversLevels;
        maneuversDict[EManeuverType.Aggressive].Clear();
        maneuversDict[EManeuverType.Defensive].Clear();
        maneuversDict[EManeuverType.Supplementary].Clear();
        for (int i = 0; i < playerManeuversList.AdmiralManeuversCount; i++)
        {
            PlayerManeuverData manev;
            switch (levels[i])
            {
                case 3:
                    manev = playerManeuversList.Maneuvers[i].Level3;
                    break;
                case 2:
                    manev = playerManeuversList.Maneuvers[i].Level2;
                    break;
                default:
                    manev = playerManeuversList.Maneuvers[i];
                    break;
            }
            manev.Level = levels[i];
            maneuversDict[manev.ManeuverType].Add(manev);
        }
        AddOfficerManeuvers();
    }

    public void LoadData(ref TacticalMapSaveData data, List<MissionSaveData> missions)
    {
        MagicIdentifyPermanentRemove = data.MagicIdentifyPermanentRemove;
        damageRangeEnemies.Clear();
        if (data.EnemiesRanges != null)
        {
            damageRangeEnemies.AddRange(data.EnemiesRanges);
        }

        Carrier.LoadData(ref data.Ship);

        for (int i = 0; i < objects.Count; i++)
        {
            var enemyData = data.EnemyShips[i];
            objects[i].LoadData(enemyData);

            if (!objects[i].Dead && !objects[i].Invisible && !objects[i].IsDisabled && !markers.LoadData(objects[i], ref enemyData.ObjectData, false))
            {
                markers.LoadData(objects[i], ref enemyData.ObjectData, true);
            }
        }

        for (int i = 0; i < data.Survivors.Count; i++)
        {
            var obj = Instantiate(survivorObjectPrefab, survivorObjectParent);
            allSurvivors.Add(obj);
            var survivorData = data.Survivors[i];
            obj.LoadData(survivorData);
            if (!obj.Dead)
            {
                allObjects.Add(obj);
                markers.LoadData(obj, ref survivorData.ObjectData, true);
            }
        }

        DestroyNeutrals();
        int count = allObjects.Count;
        foreach (var neutral in data.NeutralShips)
        {
            var uo = CreateUO();
            var uoData = neutral;
            uo.LoadData(uoData);
            markers.LoadData(uo, ref uoData, true);
        }

        if (data.CustomShips != null)
        {
            foreach (var custom in data.CustomShips)
            {
                var customData = custom;
                SpawnUO(ETacticalObjectType.StrikeGroup, customData.EnemyID - 100);
                var ship = AllObjects[AllObjects.Count - 1] as TacticalEnemyShip;
                ship.LoadData(customData.SaveData);

                if (!ship.Dead && !ship.Invisible && !ship.IsDisabled && !markers.LoadData(ship, ref customData.SaveData.ObjectData, false))
                {
                    markers.LoadData(ship, ref customData.SaveData.ObjectData, true);
                }
                if (count > customData.CurrentIndex)
                {
                    Debug.LogError("Error custom ship");
                }
                else if (customData.CurrentIndex >= AllObjects.Count)
                {
                    Debug.LogError("Error custom ship count");
                }
                else
                {
                    AllObjects.RemoveAt(AllObjects.Count - 1);
                    AllObjects.Insert(customData.CurrentIndex, ship);
                }
            }
        }

        BonusManeuversAttackBuff = data.CurrentBonusBuff;

        LostBombers = data.LostBombers;
        LostFighters = data.LostFighters;
        LostTorpedoes = data.LostTorpedoes;
        consequenceManeuversAttack = data.ConsequenceManeuversAttack;
        EnemyBlocksDestroyed = data.DestroyedBlocks;

        LoadMissions(missions);

        for (int i = 0; i < (int)EMissionOrderType.Count; i++)
        {
            ShowMissionPanelHelper((EMissionOrderType)i, Missions[(EMissionOrderType)i].Count <= 0);
        }
    }

    public void LoadRanges(ref VisualsSaveData data)
    {
        if (data.RescueRange || rescueRangeToggle.gameObject.activeSelf)
        {
            SetShowRescueRangeToggle(data.RescueRange);
        }
        if (data.CannonRange || cannonRangeToggle.gameObject.activeSelf)
        {
            SetShowCannonRangeToggle(data.CannonRange);
        }
        if (data.MissionRange || missionRangeToggle.gameObject.activeSelf)
        {
            SetShowMissionRangeToggle(data.MissionRange, data.MissionTargets);
        }
        if (data.MagicSprite || magicSpriteToggle.gameObject.activeSelf)
        {
            if (data.MagicSprite2)
            {
                SetMagicSprite(false);
            }
            else if (data.MagicSprite3)
            {
                SetMagicSprite(true);
            }
            SetShowMagicSpriteToggle(data.MagicSprite);
        }
        if (data.PathRange || !showPath)
        {
            SetShowPath(data.PathRange);
        }
        StrategySelectionPanel.LoadStrategySprites(ref data.StrategyVisuals);
    }

    public void SaveData(ref TacticalMapSaveData data, List<MissionSaveData> missions)
    {
        data.CurrentBonusBuff = BonusManeuversAttackBuff;

        data.MagicIdentifyPermanentRemove = MagicIdentifyPermanentRemove;
        if (data.EnemiesRanges == null)
        {
            data.EnemiesRanges = new List<int>();
        }
        data.EnemiesRanges.AddRange(damageRangeEnemies);

        Carrier.SaveData(ref data.Ship);

        data.EnemyShips.Clear();
        foreach (var obj in objects)
        {
            var enemyData = obj.SaveData();
            if (!obj.Dead && !obj.Invisible && !obj.IsDisabled)
            {
                markers.SaveData(obj, ref enemyData.ObjectData, false);
            }
            data.EnemyShips.Add(enemyData);
        }
        data.Survivors.Clear();
        foreach (var survivor in allSurvivors)
        {
            var survivorData = survivor.SaveData();
            if (!survivor.Dead)
            {
                markers.SaveData(survivor, ref survivorData.ObjectData, true);
            }
            data.Survivors.Add(survivorData);
        }

        data.NeutralShips.Clear();
        if (data.CustomShips == null)
        {
            data.CustomShips = new List<CustomTacticalObjectSaveData>();
        }
        data.CustomShips.Clear();
        for (int i = 0; i < AllObjects.Count; i++)
        {
            var obj = AllObjects[i];
            if (obj.Side == ETacticalObjectSide.Neutral && obj is TacticalEnemyShip ship)
            {
                var customData = new CustomTacticalObjectSaveData(i, ship);
                if (!ship.Dead && !obj.Invisible && !ship.IsDisabled)
                {
                    markers.SaveData(obj, ref customData.SaveData.ObjectData, false);
                }
                data.CustomShips.Add(customData);
            }
            else if (obj is UnidentifiedObject uo && !uo.Dead && (markers.IsUO(obj) || markers.IsStrikeGroups(obj)))
            {
                var uoData = uo.SaveData();
                markers.SaveData(uo, ref uoData, false);
                data.NeutralShips.Add(uoData);
            }
        }

        data.LostBombers = LostBombers;
        data.LostFighters = LostFighters;
        data.LostTorpedoes = LostTorpedoes;
        data.ConsequenceManeuversAttack = consequenceManeuversAttack;
        data.DestroyedBlocks = EnemyBlocksDestroyed;

        SaveMissions(missions);
    }

    public void SaveRanges(ref VisualsSaveData data)
    {
        data.RescueRange = rescueRangeToggle.gameObject.activeSelf;
        data.CannonRange = cannonRangeToggle.gameObject.activeSelf;
        data.MissionRange = missionRangeToggle.gameObject.activeSelf;

        data.MissionTargets.Clear();
        data.MissionTargets.AddRange(missionRangeTargets);

        data.MagicSprite = magicSpriteToggle.gameObject.activeSelf;
        data.MagicSprite2 = lesserForm;
        data.MagicSprite3 = finalForm;
        data.PathRange = showPath;

        StrategySelectionPanel.SaveStrategySprites(ref data.StrategyVisuals);
    }

    public void SaveLosses(ref IntermissionData data)
    {
        data.SetCurrent(Mathf.Max(0, data.GetCurrent(EPlaneType.Bomber)) - LostBombers, Mathf.Max(0, data.GetCurrent(EPlaneType.Fighter)) - LostFighters, Mathf.Max(0, data.GetCurrent(EPlaneType.TorpedoBomber)) - LostTorpedoes);
    }

    public void HighlightEnemy(TacticalEnemyMapButton enemy)
    {
        OnMapClosed();
        HudManager.Instance.ForceSetTacticMap(true);

        lastSeenEnemies[enemy].HighlightPosition(enemyHighlightTime);
    }

    public void OnMapClosed()
    {
#warning delight enemy fleets

        if (map.CourseSettingMode)
        {
            map.ToggleCourseSetup(false);
        }
        if (map.AirRaidMode)
        {
            map.ClearRaidWaypoints();
            map.ToggleAirRaidMode();
            strategies.Clear();
            attackMapButtons.gameObject.SetActive(false);
        }
        //map.ChooseOrderType.CloseConfirmPanel();
        map.TurnOffMap();
    }

    public void CloseStrategyPanels()
    {
        attackMapButtons.gameObject.SetActive(false);
    }

    public void SetStrategy(Strategy strategy)
    {
        if (strategies.Count < 3)
        {
            strategies.Add(strategy);
        }
        else
        {
            strategies[0] = strategy;
        }
    }

    public void GetPlanesFromStrategies(List<Strategy> strategies, out int bombers, out int fighters, out int torpedoes)
    {
        bombers = 0;
        fighters = 0;
        torpedoes = 0;
        foreach (var strategy in strategies)
        {
            bombers = Mathf.Max(bombers, strategy.bombersCount);
            fighters = Mathf.Max(fighters, strategy.fightersCount);
            torpedoes = Mathf.Max(torpedoes, strategy.torpedoCount);
        }
    }
    public void UnsetStrategy(Strategy strategy)
    {
        if (strategies.Count > 0)
        {
            strategies.Remove(strategy);
        }
    }

    public void ShowIdentificationPanel(TacticalEnemyMapButton btn)
    {
        HideIdentificationPanel(CurrentFleetBtn != btn);
        EnemyUnitIdentified += OnIdentifyingFleetShipIdentified;
        CurrentFleetBtn = btn;
        List<Transform> childrenList = new List<Transform>();
        foreach (Transform child in identifyUnitsParent)
        {
            childrenList.Add(child);
        }
        transform.DetachChildren();
        childrenList.ForEach(child => GameObject.Destroy(child.gameObject));
        foreach (Image i in identificationsIcons)
        {
            i.color = unactiveColor;
        }
        int idx = 0;
        for (int i = 0; i < btn.FleetUnits.Count; i++)
        {
            //if (btn.FleetUnits[i].IsDead)
            //{
            //    continue;
            //}
            idx++;
            GameObject g = Instantiate(identifyUnitItemPrefab, identifyUnitsParent, false);
            TacticalUnitItem u = g.GetComponent<TacticalUnitItem>();
            u.SetUnit(btn.FleetUnits[i], btn, identifyUnitsParent, idx);

            EEnemyTypeDemo type = btn.FleetUnits[i].enemyType;
            if (btn.FleetUnits[i].isHidden || type == EEnemyTypeDemo.Unsure)
            {
                type = btn.FleetUnits[i].guessedEnemyType;
            }

            switch (type)
            {
                case (EEnemyTypeDemo.DefenceHP):
                    identificationsIcons[0].color = Color.white;
                    break;
                case (EEnemyTypeDemo.DefenceT):
                    identificationsIcons[1].color = Color.white;
                    break;
                case (EEnemyTypeDemo.DefenceB):
                    identificationsIcons[2].color = Color.white;
                    break;
                case (EEnemyTypeDemo.OffenceT):
                    identificationsIcons[4].color = Color.white;
                    break;
                case (EEnemyTypeDemo.OffenceB):
                    identificationsIcons[5].color = Color.white;
                    break;
                case (EEnemyTypeDemo.Fighters):
                    identificationsIcons[7].color = Color.white;
                    break;


            }
        }

        if (Strategies.Count > 0)
        {
            startegyAnalysisPanel.SetPredictedDataTexts(CalculateFightData(btn, Strategies, out _, true));
        }
        startegyAnalysisPanel.gameObject.SetActive(Strategies.Count > 0);
    }

    public void HideIdentificationPanel()
    {
        HideIdentificationPanel(true);
    }

    public EnemyUnit CreateEnemyShip(int id, int index)
    {
        EnemyUnit unit = new EnemyUnit();
        EnemyUnit unitTemplate = availableUnits[id];

        unit.Index = index;
        unit.ShipId = id;
        unit.UnitName = unitTemplate.UnitName;
        unit.MastsLength = Random.Range(unitTemplate.MastsMinLength, unitTemplate.MastsMaxLength);
        unit.MastsMinLength = unitTemplate.MastsMinLength;
        unit.MastsMaxLength = unitTemplate.MastsMaxLength;
        unit.FunnelCount = unitTemplate.FunnelCount;
        unit.ArmamentCount = Random.Range(unitTemplate.ArmamentMinCount, unitTemplate.ArmamentMaxCount);
        unit.ArmamentMinCount = unitTemplate.ArmamentMinCount;
        unit.ArmamentMaxCount = unitTemplate.ArmamentMaxCount;
        unit.UnitLength = Random.Range(unitTemplate.UnitMinLength, unitTemplate.UnitMaxLength);
        unit.UnitMinLength = unitTemplate.UnitMinLength;
        unit.UnitMaxLength = unitTemplate.UnitMaxLength;
        unit.enemyType = unitTemplate.enemyType;
        unit.trueEnemyType = unitTemplate.enemyType;
        unit.guessedEnemyType = EEnemyTypeDemo.Unsure;
        unit.IsLight = unitTemplate.IsLight;
        unit.IsHeavy = unitTemplate.IsHeavy;
        unit.IsCarrier = unitTemplate.IsCarrier;
        unit.IsLightGuessed = false;
        unit.IsHeavyGuessed = false;
        unit.IsCarrierGuessed = false;
        unit.isHidden = true;
        return unit;
    }

    public void RevealFleet(int fleet)
    {
        RevealFleet(Fleets[fleet]);
    }

    public void RevealFleet(TacticalEnemyMapButton fleet)
    {
        FleetSeen(fleet.ShipScript.ObjectTransform.anchoredPosition, fleet);
    }

    public void Tick()
    {
        foreach (var obj in allObjects)
        {
            if (obj.InstantUpdate)
            {
                markers.SetInstantUpdateTimer(obj);
                obj.InstantUpdate = false;
            }
        }

        foreach (var pair in proximities)
        {
            proximityHelper.Add(pair.Key, Vector2.SqrMagnitude(Carrier.Rect.anchoredPosition - pair.Value.Enemy.RectTransform.anchoredPosition) <= pair.Value.Proximity);
        }
        foreach (var pair in proximityHelper)
        {
            if (proximities.TryGetValue(pair.Key, out var data))
            {
                data.Callback(pair.Value);
            }
        }
        proximityHelper.Clear();

        //int ticksPerHour = TimeManager.Instance.TicksForHour;
        //if (--timer <= 0)
        //{

        RevealObjects(Carrier.Rect.anchoredPosition, Carrier.CurrentRangeSqr, false, false, EMissionOrderType.None);
        //    //CheckFleetsInRange(Carrier.Rect.anchoredPosition, Carrier.CurrentRangeSqr, false);
        //    timer = ticksPerHour;
        //}
        //for (int i = 0; i < revealedRanges.Count;)
        //{
        //    var data = revealedRanges[i];
        //    ++data.Timer;
        //    if (data.Timer % ticksPerHour == 0)
        //    {
        //        CheckFleetsInRange(data.Position, data.RangeSqr, true);
        //    }
        //    if (data.Timer == data.MaxTime)
        //    {
        //        revealedRanges.RemoveAt(i);
        //        //FogOfWarManager.Instance.RemoveUnmaskPosition(revealedRanges[i].Position);
        //    }
        //    else
        //    {
        //        i++;
        //    }
        //}

        //bool canHide = HudManager.Instance.HasNo(ETutorialMode.DisableFleetDisappearance);
        //foreach (var pair in lastSeenEnemies)
        //{
        //    if (pair.Key.ShipScript.IsDead || (++pair.Value.SeenTimer > ticksPerHour && canHide))
        //    {
        //        toRemoveList.Add(pair.Key);
        //        pair.Value.gameObject.SetActive(false);
        //    }
        //}

        //foreach (var enemy in toRemoveList)
        //{
        //    EnemyDisappeared(enemy);
        //    freeMarkers.Add(lastSeenEnemies[enemy]);
        //    lastSeenEnemies.Remove(enemy);
        //}
        //toRemoveList.Clear();

        if (--spawnUOTimer <= 0)
        {
            SpawnUnidentifiedObjects(Random.Range(1, maxToSpawnUO));
            spawnUOTimer = Mathf.RoundToInt(TimeManager.Instance.TicksForHour * hoursToSpawnUO);
        }
    }

    public void AddCAP(TacticalMission mission, int planes)
    {
        capsDict.Add(mission, new CapData(mission, planes * CAPDefence, planes * CAPEscort));
        //if (capsInProgress.Count == 1)
        {
            foreach (var value in capsDict.Values)
            {
                SetCAP(value);
                return;
            }
        }
    }

    public void RemoveCAP(TacticalMission mission)
    {
        capsDict.Remove(mission);
        foreach (var value in capsDict.Values)
        {
            SetCAP(value);
            return;
        }
        SetCAP(null);
    }

    //public TacticalEnemyMapButton GetFleetInRange(Vector2 pos)
    //{
    //    foreach (var fleet in Fleets)
    //    {
    //        var diff = fleet.ShipScript.ObjectTransform.anchoredPosition - pos;
    //        if (diff.sqrMagnitude < (AttackRange * AttackRange))
    //        {
    //            return fleet;
    //        }
    //    }
    //    return null;
    //}

    public bool IsCarrierInRange(TacticalEnemyShip ship)
    {
        return Vector2.SqrMagnitude(ship.RectTransform.anchoredPosition - Carrier.Rect.anchoredPosition) < ship.AttackRangeSqr;
    }

    public bool IsFriendInRange(TacticalEnemyShip friend, TacticalEnemyShip enemy)
    {
        return Vector2.SqrMagnitude(friend.RectTransform.anchoredPosition - enemy.RectTransform.anchoredPosition) < enemy.AttackRangeSqr;
    }

    public void FireUnitIdentified(TacticalEnemyMapButton fleet)
    {
        EnemyUnitIdentified(fleet);
    }

    public void SetMissionSlotCountIslandBoost(int bonus)
    {
        islandBoost = bonus;
        RefreshMaxMissions();
    }

    public void SetMissionSlotCountIslandBuff(int bonus)
    {
        islandBuff = bonus;
        RefreshMaxMissions();
    }

    public void FireEnemyRouted(MapEnemyShip fleet)
    {
        EnemyRouted(fleet);
    }

    public void ChangeMapLayout(SOTacticMap tMap, bool isSandbox, bool setDate)
    {
        if (tMap == null)
        {
            tMap = defaultMap;
            SaveManager.Instance.Data.CurrentMission = 1;
        }

        if (Fleets == null)
        {
            Fleets = new List<TacticalEnemyMapButton>();
        }
        SOTacticMap = tMap;
        Fleets.Clear();
        var timeMan = TimeManager.Instance;
        if (isSandbox)
        {
            Map.MapNodes = null;
            timeMan.SetTime(timeMan.GetCurrentTime());
        }
        map.SetMap(tMap, isSandbox);
        CreateMovementGrid();
        Carrier.SetPosition(tMap.PlayerPosition);

        MovieManager.Instance.Setup(tMap);

        MinMapPosition = tMap.MinMapPosition;
        MaxMapPosition = tMap.MaxMapPosition;

        TacticalMapClouds.Instance.Setup(tMap);

        for (int i = 0; i < objects.Count; i++)
        {
            var ship = objects[i];
            DestroyObject(i, true, true);
            allObjects.Remove(ship);

            this.StartCoroutineActionAfterFrames(() => Destroy(ship.gameObject), 1);
        }
        objects.Clear();
        for (int i = 0; i < allObjects.Count; i++)
        {
            var obj = allObjects[i];
            if (obj is UnidentifiedObject uo)
            {
                if (markers.IsUO(uo))
                {
                    markers.HideUO(obj, out _, out _, out _);
                }
                this.StartCoroutineActionAfterFrames(() => uo.Destroy(), 1);
            }
            else if (obj is TacticalEnemyShip ship)
            {
                Destroy(ship);
            }
            else if (obj is SurvivorObject survivor)
            {
                DestroySurvivor(survivor);
                i--;
            }
            else
            {
                Assert.IsTrue(false, obj.GetType() + " " + obj.name);
            }
        }
        allObjects.Clear();

        SpawnEnemy();
        EnemyAttacksManager.Instance.SetupEnemyAttacks(tMap);

        spawnUOTimer = Mathf.RoundToInt(TimeManager.Instance.TicksForHour * hoursToSpawnUO);

        ObjectivesManager.Instance.UpdateObjectives(tMap.Objectives);
        SpawnUnidentifiedObjects(Random.Range(tMap.AdditionalObjectsToSpawnMin, tMap.AdditionalObjectsToSpawnMax));

        var saveMan = SaveManager.Instance;
        ref var inprogress = ref SaveManager.Instance.Data.MissionInProgress;
        inprogress.HasMissionStartTime = setDate;
        if (setDate)
        {
            var date = saveMan.TransientData.ForcedDate;
            if (date.HasValue)
            {
                var date2 = date.Value;
                date2.Hour = tMap.Date.Hour;
                date2.Minute = tMap.Date.Minute;
                inprogress.MissionStartTimeB = date2;
            }
            else
            {
                inprogress.MissionStartTimeB = tMap.Date;
            }
            timeMan.SetTime(inprogress.MissionStartTimeB);
        }
    }

    public FightModifiersData CalculateFightData(TacticalEnemyMapButton enemy, List<Strategy> strategies, out FightSquadronData helpData, bool useGuessed = false)
    {
        var result = new FightModifiersData(100);

        helpData = new FightSquadronData();

        helpData.PrioritizeBombers = false;
        helpData.PrioritizeTorpedoes = false;

        var strategyBonus = new FightModifiersData();
        foreach (var strategy in strategies)
        {
            strategyBonus.Add(strategy.Modifiers);
        }
        string text = "Stategies: " + strategyBonus.ToString();
        result.Add(strategyBonus);

        GetPlanesFromStrategies(Strategies, out helpData.Bombers, out helpData.Fighters, out helpData.Torpedoes);
        var deck = AircraftCarrierDeckManager.Instance;
        var bomberBonus = new FightModifiersData().Multiply(helpData.Bombers);
        var fighterBonus = new FightModifiersData().Multiply(helpData.Fighters);
        var torpedoBonus = new FightModifiersData().Multiply(helpData.Torpedoes);

        bomberBonus.Add(fighterBonus).Add(torpedoBonus);
        text += "; planes: " + bomberBonus.ToString();
        result.Add(bomberBonus);

        var malusUsed = new HashSet<EEnemyTypeDemo>();
        var enemySpecialMalus = new FightModifiersData();
        var enemyFleetMalus = new FightModifiersData();
        foreach (var unit in enemy.FleetUnits)
        {
            if (unit.IsDead)
            {
                continue;
            }
            EEnemyTypeDemo type = useGuessed ? unit.guessedEnemyType : unit.trueEnemyType;
            if (malusUsed.Add(type))
            {
                switch (type)
                {
                    case EEnemyTypeDemo.OffenceT:
                        helpData.PrioritizeTorpedoes = true;
                        var offenceTData = new FightModifiersData().Add(OffenceTData).Multiply(helpData.Torpedoes);
                        enemySpecialMalus.Add(offenceTData);
                        break;
                    case EEnemyTypeDemo.OffenceB:
                        helpData.PrioritizeBombers = true;
                        var offenceBData = new FightModifiersData().Add(OffenceBData).Multiply(helpData.Bombers);
                        enemySpecialMalus.Add(offenceBData);
                        break;
                    case EEnemyTypeDemo.Fighters:
                        if (helpData.Fighters == 0)
                        {
                            enemySpecialMalus.Add(FightersData);
                        }
                        break;
                    case EEnemyTypeDemo.DefenceT:
                        helpData.PrioritizeTorpedoes = true;
                        var defenceTData = new FightModifiersData().Add(DefenceTData).Multiply(helpData.Torpedoes);
                        enemySpecialMalus.Add(defenceTData);
                        break;
                    case EEnemyTypeDemo.DefenceB:
                        helpData.PrioritizeBombers = true;
                        var defenceBData = new FightModifiersData().Add(DefenceBData).Multiply(helpData.Bombers);
                        enemySpecialMalus.Add(defenceBData);
                        break;
                    case EEnemyTypeDemo.DefenceHP:
                        helpData.PrioritizeTorpedoes = true;
                        helpData.PrioritizeBombers = true;
                        enemySpecialMalus.Add(DefenceHPData);
                        break;
                }
            }
            if (type == EEnemyTypeDemo.Cargo)
            {
                enemyFleetMalus.Add(CargoData);
            }
            else if (useGuessed ? unit.IsCarrierGuessed : unit.IsCarrier)
            {
                enemyFleetMalus.Add(CarrierData);
            }
            else if (useGuessed ? unit.IsLightGuessed : unit.IsLight)
            {
                enemyFleetMalus.Add(LightShipData);
            }
            else if (useGuessed ? unit.IsHeavyGuessed : unit.IsHeavy)
            {
                enemyFleetMalus.Add(HeavyShipData);
            }
        }
        text += "; enemySpecial: " + enemySpecialMalus.ToString();
        text += "; enemyFleet: " + enemyFleetMalus.ToString();
        result.Add(enemySpecialMalus);
        result.Add(enemyFleetMalus);
        text += "; total: " + result.ToString();
        //Debug.Log(text);
        return result;
    }

    public void FireBlockDestroyed(EnemyManeuverData block, bool destroyedByPlayer)
    {
        BlockDestroyed(block);
        if (destroyedByPlayer)
        {
            EnemyBlocksDestroyed++;
            EnemyBlockDestroyed(block);

            var timeMan = TimeManager.Instance;
            if (timeMan.CurrentYear > 1945 ||
                (timeMan.CurrentYear == 1945 &&
                 (timeMan.CurrentMonth > 9 ||
                  (timeMan.CurrentMonth == 9 &&
                   timeMan.CurrentDay > 2))))
            {
                EnemyBlockDestroyedAfterWar();
            }
        }
    }

    public void FireDestinationReached(int id, int node)
    {
        DestinationReached(id, node);
    }

    public void FireEdgeMapReached()
    {
        EdgeMapReached();
    }

    public void FireMapCornerReached()
    {
        MapCornerData.Callback();
    }

    public void FireCustomMissionFinished(bool success)
    {
        CustomMissionFinished(success);
    }

    public void FireMissionFinished(EMissionOrderType type, bool success, TacticalObject target)
    {
        MissionFinished(type, success, target);
    }

    public void FireMissionAction(TacticalMission mission)
    {
        MissionAction(mission);
    }

    public void FireObjectVisibilityChanged(int id, bool visible)
    {
        ObjectVisibilityChanged(id, visible);
    }

    public void FireObjectIdentified(int id)
    {
        ObjectIdentified(id);
    }

    public void FireSwimMile()
    {
        SwimMile();
    }

    public void FireRandomNodeChosen(int nodeID, bool taken)
    {
        RandomNodeChosen(nodeID, taken);
    }

    public void FirePotentialRandomNodeChosen(int nodeID, int prevNodeID)
    {
        PotentialRandomNodeChosen(nodeID, prevNodeID);
    }

    public void FireRandomNodeDenied()
    {
        RandomNodeDenied();
    }

    public void FireMissionLost()
    {
        MissionLost();
    }

    public void FireBlindKamikaze()
    {
        BlindKamikaze();
    }

    public float GetMilesTravelled()
    {
        return (Map.MapShip as TacticalMapShip).MilesTravelled;
    }

    public bool HasReachedDestination(int id, int patrolIndexNode, out int node)
    {
        var obj = objects[id];
        node = -1;
        if (obj.FinishedPatrol || patrolIndexNode < obj.PatrolNodeIndex)
        {
            node = obj.CurrentPatrol.Poses[patrolIndexNode];
            return true;
        }
        return false;
    }

    public TacticalEnemyShip GetShip(int id)
    {
        return objects[id];
    }

    public IEnumerable<TacticalEnemyShip> GetAllShips()
    {
        foreach (var obj in objects)
        {
            yield return obj;
        }
    }

    public int GetRandomEnemyId()
    {
        unrevealedEnemies.Clear();
        foreach (var enemy in objects)
        {
            if (!enemy.IsDisabled && !enemy.Dead && !enemy.HadGreaterInvisibility && !enemy.NotTargetable && enemy.Side == ETacticalObjectSide.Enemy)
            {
                unrevealedEnemies.Add(enemy);
            }
        }
        return unrevealedEnemies.Count > 0 ? RandomUtils.GetRandom(unrevealedEnemies).Id : -1;
    }

    public void NewRevealArea(Vector2 pos, float rangeSqr, bool recon, EMissionOrderType type)
    {
        //var data = new RangeData(pos, rangeSqr, maxTime);
        //revealedRanges.Add(data);
        RevealObjects(pos, rangeSqr, recon, false, type);
        //FogOfWarManager.Instance.AddUnmaskPosition(pos, (int)rangeSqr);
    }

    public void RevealObjects(Vector2 pos, float rangeSqr, bool recon, bool detectSubmarines, EMissionOrderType type)
    {
        helperReconList.Clear();
        for (int i = 0; i < allObjects.Count; i++)
        {
            RevealObject(pos, rangeSqr, recon, detectSubmarines, i, out bool revealed, out bool removed);
            if (removed)
            {
                i--;
            }
            if (revealed)
            {
                if (removed)
                {
                    helperReconList.Add(new DummyWhale());
                }
                else
                {
                    helperReconList.Add(allObjects[i]);
                }
            }
        }
        if (type != EMissionOrderType.None)
        {
            ReportPanel.Instance.SetupRecon(type, helperReconList);
        }
        if (!detectSubmarines)
        {
            foreach (var pair in markers.GetUOs())
            {
                if (!pair.Key.Invisible && Vector2.SqrMagnitude(pos - pair.Value.Transform.anchoredPosition) < rangeSqr)
                {
                    pair.Key.Invisible = true;
                    pair.Value.MarkerObj.gameObject.SetActive(false);
                }
            }
        }
    }

    public void RevealObject(Vector2 pos, float rangeSqr, bool recon, bool detectSubmarines, int index, out bool revealed, out bool removed)
    {
        revealed = false;
        removed = false;

        var obj = allObjects[index];

        if (obj.RectTransform == null || Vector2.SqrMagnitude(pos - obj.RectTransform.anchoredPosition) > rangeSqr)
        {
            return;
        }
        if (!recon)
        {
            //obj.InstantUpdate = true;
            //markers.SetInstantUpdateTimer(obj);
        }
        revealed = true;
        if (obj is TacticalEnemyShip obje && obje.Side != ETacticalObjectSide.Neutral)
        {
            if (obje.IsDisabled || obje.Dead || (detectSubmarines && !obje.HadGreaterInvisibility))
            {
                revealed = false;
                return;
            }
            if (!obj.Visible)
            {
                obj.Visible = true;
                UORevealed();
            }
            Assert.IsFalse(obje.Side == ETacticalObjectSide.Neutral);
            if (detectSubmarines)
            {
                obje.GreaterInvisibility = false;
            }
            RevealObject(obje.Id);
            bool can;
            if (recon)
            {
                can = !obje.Reconed;
                obje.Reconed = true;
            }
            else
            {
                can = !obje.Spotted;
                obje.Spotted = true;
            }
            if (can)
            {
                foreach (var block in obje.Blocks)
                {
                    block.WasDetected = block.Visible;
                    float chance = obje.OverrideChanceToReveal >= 0f ? obje.OverrideChanceToReveal : block.Data.ChanceToReveal;
                    if (!block.Visible && (block.Data.MisidentifiedType == EMisidentifiedType.Unique || Random.value <= chance) && !block.Data.NeedMagicIdentify)
                    {
                        block.Visible = true;
                    }
                }
                ObjectIdentified(obje.Id);
                markers.UpdateAttackRange(obje);
            }
        }
        else if (!detectSubmarines)
        {
            if (!obj.Visible)
            {
                obj.Visible = true;
                UORevealed();
            }
            Assert.IsTrue(obj.Side == ETacticalObjectSide.Neutral);
            if (!markers.IsUO(obj))
            {
                markers.UpdateNeutralFleet(obj);
                return;
            }
            switch (obj.Type)
            {
                case ETacticalObjectType.Nothing:
                case ETacticalObjectType.Whales:
                    removed = true;
                    allObjects.RemoveAt(index);
                    (obj as UnidentifiedObject).Destroy();
                    break;
                case ETacticalObjectType.Outpost:
                    markers.Show(obj, ETacticalObjectType.Outpost, obj.Side);
                    break;
                case ETacticalObjectType.StrikeGroup:
                    markers.Show(obj, ETacticalObjectType.StrikeGroup, obj.Side);
                    break;
                case ETacticalObjectType.Survivors:
                    if ((obj as SurvivorObject).Dead)
                    {
                        break;
                    }
                    obj.Visible = true;
                    obj.Invisible = false;
                    markers.ShowSurvivor(obj);
                    break;
            }
        }
    }

    public void IdentifyObject(int id, bool magicIdentify)
    {
        foreach (var block in objects[id].Blocks)
        {
            if (!block.Data.NeedMagicIdentify || magicIdentify)
            {
                block.Visible = true;
            }
        }
        markers.UpdateAttackRange(objects[id]);
        ObjectIdentified(id);
    }

    public void RevealObject(int id)
    {
        var obj = objects[id];
        if (!obj.IsDisabled && !obj.GreaterInvisibility)
        {
            obj.Invisible = false;
            obj.Visible = true;
            Assert.IsTrue(obj.Type == ETacticalObjectType.Outpost || obj.Type == ETacticalObjectType.StrikeGroup);
            if (markers.IsUO(obj))
            {
                markers.Show(obj, obj.Type, obj.Side);
                if (obj.Side == ETacticalObjectSide.Enemy)
                {
                    ObjectRevealed(obj);
                    ObjectVisibilityChanged(obj.Id, true);
                }
            }
            else if (obj.Side != ETacticalObjectSide.Friendly && !obj.UpdateRealtime)
            {
                markers.Reshow(obj);
            }
        }
    }

    public void IdentifyObject(int id)
    {
        var obj = objects[id];
        foreach (var block in obj.Blocks)
        {
            block.Visible = true;
        }
        FireObjectIdentified(id);
    }

    public void UpdateRealtimeObject(int id)
    {
        objects[id].UpdateRealtime = true;
    }

    public void ShowHiddenObject(int id)
    {
        var obj = objects[id];
        obj.Invisible = false;
        obj.GreaterInvisibility = false;
        markers.MakeVisible(obj);
    }

    public void HideObject(int id)
    {
        var obj = objects[id];
        Assert.IsTrue(obj.Type == ETacticalObjectType.Outpost || obj.Type == ETacticalObjectType.StrikeGroup);
        markers.Hide(obj, obj.Type);

        ObjectVisibilityChanged(obj.Id, false);
    }

    public void DestroyBlock(int id, EEnemyShipType shipType)
    {
        var obj = GetShip(id);
        if (obj.Dead || obj.IsDisabled)
        {
            return;
        }
        foreach (var block in obj.Blocks)
        {
            if (!block.Dead && block.Visible && block.Data.ShipType == shipType)
            {
                block.Dead = true;
                FireBlockDestroyed(block.Data, true);
                obj.CheckIsDead(true);
                return;
            }
        }
    }

    public void DestroyBlock(int id, int blockID)
    {
        var obj = GetShip(id);
        Assert.IsFalse(obj.Dead || obj.IsDisabled, $"{obj.Dead} {obj.IsDisabled}");
        var block = obj.Blocks[blockID];
        if (!block.Dead)
        {
            block.Dead = true;
            FireBlockDestroyed(block.Data, false);
            obj.CheckIsDead(false);
        }
    }

    public void DestroyObject(int id, bool notByPlayer, bool clear = false)
    {
        //#error smth more? avoid infinite callback
        var obj = objects[id];
        Assert.IsTrue(obj.Type == ETacticalObjectType.Outpost || obj.Type == ETacticalObjectType.StrikeGroup);
        obj.Die(notByPlayer);
        Assert.IsFalse(obj.Ignore);
        obj.Ignore = clear;
        if (markers.IsUO(obj))
        {
            markers.HideUO(obj, out _, out _, out _);
        }
        else
        {
            markers.Hide(obj, obj.Type, false);
        }
        ObjectDestroyed(id, !clear);
    }

    public void SpawnObject(int id)
    {
        var obj = objects[id];
        Assert.IsFalse(obj.Side == ETacticalObjectSide.Neutral);
        Assert.IsTrue(obj.Type == ETacticalObjectType.Outpost || obj.Type == ETacticalObjectType.StrikeGroup);

        if (obj.IsDisabled)
        {
            obj.EnableShip();
            if (obj.Side == ETacticalObjectSide.Friendly)
            {
                markers.Show(obj, obj.Type, ETacticalObjectSide.Friendly);
            }
            else
            {
                markers.ShowUO(obj);
            }
        }
    }

    public void SetShowMapEdges(bool show)
    {
        mapEdges.SetActive(show);
    }

    public void SetMapCornerTrigger(MapCornerData mapCornerData)
    {
        MapCornerData = mapCornerData;
        if (mapCornerData == null)
        {
            mapCornerHorizontal.gameObject.SetActive(false);
            mapCornerVertical.gameObject.SetActive(false);
        }
        else
        {
            mapCornerHorizontal.gameObject.SetActive(true);
            mapCornerVertical.gameObject.SetActive(true);

            float arrowHeight = 62f;
            float arrowWidth = 59f;
            var horizontalScale = Vector3.one;
            var verticalScale = Vector3.one;
            float maxX = 944f;
            float maxY = 524f;
            float maxResize = 12f;

            float posX = Mathf.Clamp(mapCornerData.CornerPositional.x, -maxX, maxX);
            float posY = Mathf.Clamp(mapCornerData.CornerPositional.y, -maxY, maxY);

            var horizontalPos = new Vector2(posX, maxY - arrowHeight);
            var verticalPos = new Vector2(maxX - arrowHeight, posY);
            var horizontalSizeDelta = new Vector2(maxX - posX - arrowHeight, arrowHeight);
            var verticalSizeDelta = new Vector2(arrowHeight, maxY - posY - arrowHeight);

            switch (mapCornerData.CornerOrientation)
            {
                case EOrientation.NE:
                    break;
                case EOrientation.SE:
                    horizontalScale.y = -1f;

                    horizontalPos.y = -horizontalPos.y - arrowHeight;
                    verticalPos.y = -maxY + arrowHeight;

                    verticalSizeDelta.y += 2f * posY;
                    break;
                case EOrientation.NW:
                    verticalScale.x = -1f;

                    horizontalPos.x = -maxX + arrowHeight;
                    verticalPos.x = -verticalPos.x - arrowHeight;

                    horizontalSizeDelta.x += 2f * posX;
                    break;
                case EOrientation.SW:
                    horizontalScale.y = -1f;
                    verticalScale.x = -1f;

                    horizontalPos.x = -maxX + arrowHeight;
                    horizontalPos.y = -horizontalPos.y - arrowHeight;
                    verticalPos.x = -verticalPos.x - arrowHeight;
                    verticalPos.y = -maxY + arrowHeight;

                    horizontalSizeDelta.x += 2f * posX;
                    verticalSizeDelta.y += 2f * posY;
                    break;
            }

            horizontalPos += horizontalSizeDelta / 2f;
            verticalPos += verticalSizeDelta / 2f;

            float count = horizontalSizeDelta.x / arrowWidth;
            float round = Mathf.Round(count);
            horizontalSizeDelta.x = arrowWidth * round;
            horizontalScale.x *= count / round;

            count = verticalSizeDelta.y / arrowWidth;
            round = Mathf.Round(count);
            verticalSizeDelta.y = arrowWidth * round;
            verticalScale.y *= count / round;

            mapCornerHorizontal.anchoredPosition = horizontalPos;
            mapCornerVertical.anchoredPosition = verticalPos;

            mapCornerHorizontal.sizeDelta = horizontalSizeDelta;
            mapCornerVertical.sizeDelta = verticalSizeDelta;

            horizontalScale.x *= (horizontalSizeDelta.x + maxResize * 2f) / horizontalSizeDelta.x;
            mapCornerHorizontal.localScale = horizontalScale;
            verticalScale.y *= (verticalSizeDelta.y + maxResize * 2f) / verticalSizeDelta.y;
            mapCornerVertical.localScale = verticalScale;
        }
    }

    public void Trigger(Objective data, int nodeToReach, Action onReached, float range)
    {
        Carrier.SetupTrigger(data, nodeToReach, onReached, range);
    }

    public void Trigger(Objective data, List<int> nodesToReach, Action<int> onReachedAny)
    {
        Carrier.SetupTrigger(data, nodesToReach, onReachedAny);
    }

    public void RemoveTriggers(Objective data)
    {
        Carrier.RemoveTriggers(data);
    }

    public void ResetAllMissions(bool losePlanes)
    {
        var deck = AircraftCarrierDeckManager.Instance;

        foreach (var pair in Missions)
        {
            int count = pair.Value.Count;
            for (int index = 0; index < count; index++)
            {
                var mission = pair.Value[0];
                if (!losePlanes && mission.MissionStage > EMissionStage.Launching && mission.MissionStage < EMissionStage.ReportReady)
                {
                    foreach (var squadron in mission.SentSquadrons)
                    {
                        deck.OnResetAddPlane(squadron.PlaneType);
                    }
                }

                mission.ResetMission();
            }
        }
    }

    public bool UpdateMissionCount(EMissionOrderType orderType, int bonus, IslandRoom room)
    {
        if (bonus > 0)
        {
            var counts = missionsCount[orderType];
            counts.SetBonus(room, bonus);
        }
        else
        {
            switchMissionsToDelete.Clear();
            int currentValue = 0;
            orderTypeToDelete = orderType;
            missionsCount[orderType].Bonuses.TryGetValue(room, out currentValue);
            int newCount = missionsCount[orderType].Count - (currentValue - bonus);
            if (newCount < Missions[orderType].Count)
            {
                int diff = Missions[orderType].Count - newCount;
                bool launchedAdded = false;
                List<TacticalMission> list = new List<TacticalMission>(Missions[orderType]);
                list.Sort((x, y) => x.MissionStage.CompareTo(y.MissionStage));
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].ExtraMission)
                    {
                        list.RemoveAt(i);
                        i--;
                        continue;
                    }
                    switchMissionsToDelete.Add(list[i]);
                    if (list[i].InProgress)
                    {
                        launchedAdded = true;
                    }
                    --diff;
                    if (diff <= 0)
                    {
                        break;
                    }
                }
                if (launchedAdded)
                {
                    EventManager.Instance.SwitchesPopup();
                    //Debug.LogWarning("SHOULD SHOW POPUP!");
                    //FindObjectOfType<TestIslandsSwitchUI>().SetPopup(true);
                    return false;
                }
            }
            DeleteMissionSwitchMissions();
            missionsCount[orderType].SetBonus(room, bonus);
        }
        CreateMissions(orderType, missionsCount[orderType].Count);
        ShowMissionPanelHelper(orderType, Missions[orderType].Count <= 0);
        return true;
    }

    public void AddMissionFromSave(MissionSaveData data)
    {
        var mission = missionsObject.AddComponent<TacticalMission>();
        Missions[data.Type].Add(mission);
        mission.CreateFromSave(data, this, map, MissionPanel);
    }

    public TacticalMission AddNewMission(EMissionOrderType orderType, EnemyAttackData enemyAttackData = null, EnemyAttackFriendData enemyAttackFriendData = null)
    {
        var mission = missionsObject.AddComponent<TacticalMission>();
        //AttackDistance = (AttackPosition - StartPosition).magnitude;
        //ReturnDistance = (ReturnPosition - AttackPosition).magnitude;
        Missions[orderType].Add(mission);

        mission.Create(orderType, this, map, enemyAttackData, enemyAttackFriendData);
        MissionPanel.SpawnButton(mission);
        strategies.Clear();
        if (!SectionRoomManager.Instance.GeneratorsAreWorking && (orderType == EMissionOrderType.CounterHostileScouts || orderType == EMissionOrderType.SubmarineHunt))
        {
            mission.ButtonMission.gameObject.SetActive(false);
        }
        return mission;
    }

    public void AddCustomMissionRetrieval(EMissionOrderType orderType, int nodeID, float retrievalHours, int bombers, int fighters, int torpedoes)
    {
        var mission = missionsObject.AddComponent<TacticalMission>();
        customMissions.Add(mission);
        mission.CustomCreate(orderType, this, map, Map.Nodes[nodeID], retrievalHours, bombers, fighters, torpedoes);
        MissionPanel.SpawnButton(mission);
    }

    public void AddNewMission(EMissionOrderType orderType, EnemyAttackFriendData data)
    {
        AddNewMission(orderType, null, data);
    }

    public void DeleteMissionSwitchMissions()
    {
        for (int i = 0; i < switchMissionsToDelete.Count; ++i)
        {
            AircraftCarrierDeckManager.Instance.DestroyMissionOrder(switchMissionsToDelete[i]);
            switchMissionsToDelete[i].RemoveMission(false);
        }
        switchMissionsToDelete.Clear();
    }

    public void StartMissionSetupMode(TacticalMission mission)
    {
        CurrentMission = mission;
        ConfirmedTarget = null;
        ChosenEnemyShip = null;
        SelectedObjectIndex = -1;
        if (!map.gameObject.activeInHierarchy)
        {
            tacticMapButton.onClick.Invoke();
        }
        map.ChangeAirRaidMode(true, mission);

        MissionPlanningStarted(mission.OrderType);
    }

    public void OpenTutorial()
    {
        MovieManager.Instance.Play(ETutorialType.TacticalMap);
    }

    public void RecalculateDistances()
    {
        AttackDistance = (AttackPosition - StartPosition).magnitude;
        ReturnDistance = (ReturnPosition - AttackPosition).magnitude;
    }

    public void Hide()
    {
        OnMapClosed();
    }

    public void GetPlanes(EMissionOrderType orderType, bool useTorpedoes, out int bombers, out int fighters, out int torpedoes)
    {
        bombers = 0;
        fighters = 0;
        torpedoes = 0;
        switch (orderType)
        {
            case EMissionOrderType.Airstrike:
            case EMissionOrderType.AirstrikeSubmarine:
            case EMissionOrderType.MidwayAirstrike:
            case EMissionOrderType.MagicAirstrike:
            case EMissionOrderType.NightAirstrike:
                break;
            case EMissionOrderType.Recon:
            case EMissionOrderType.NightScouts:
            case EMissionOrderType.MagicNightScouts:
                if (useTorpedoes)
                {
                    torpedoes = 2;
                }
                else
                {
                    bombers = 2;
                }
                break;
            case EMissionOrderType.DetectSubmarine:
                torpedoes = 1;
                break;
            case EMissionOrderType.IdentifyTargets:
            case EMissionOrderType.MagicIdentify:
                if (useTorpedoes)
                {
                    torpedoes = 1;
                }
                else
                {
                    bombers = 1;
                }
                break;
            case EMissionOrderType.SubmarineHunt:
                bombers = 2;
                break;
            case EMissionOrderType.CounterHostileScouts:
                fighters = 2;
                break;
            case EMissionOrderType.FriendlyFleetCAP:
            case EMissionOrderType.FriendlyCAPMidway:
                fighters = 2;
                break;
            case EMissionOrderType.CarriersCAP:
                fighters = 2;
                break;
            case EMissionOrderType.Scouting:
                fighters = 2;
                break;
            case EMissionOrderType.Decoy:
                fighters = 2;
                break;
            case EMissionOrderType.RescueVIP:
                bombers = 1;
                fighters = 1;
                torpedoes = 1;
                break;
            case EMissionOrderType.AttackJapan:
                bombers = 2;
                fighters = 2;
                break;
        }
    }

    public int IndexOf(TacticalMission mission)
    {
        int result = mission.CustomMission ? -1 - CustomMissions.IndexOf(mission) : Missions[mission.OrderType].IndexOf(mission);
        Assert.IsFalse(mission.CustomMission == (result >= 0), mission.OrderType.ToString() + Missions[mission.OrderType].Count + "; " + (mission == null ? "null" : "not null"));
        return result;
    }

    public int IndexOf(SurvivorObject survivor)
    {
        int result = allSurvivors.IndexOf(survivor);
        Assert.IsNotNull(survivor);
        Assert.IsFalse(result == -1);
        return result;
    }

    public TacticalMission GetMission(EMissionOrderType type, int index)
    {
        if (index < 0)
        {
            return CustomMissions[1 - index];
        }
        return Missions[type][index];
    }

    public SurvivorObject GetSurvivor(int index)
    {
        return allSurvivors[index];
    }

    public void ShowDamageRange(int power, int hours, float range, int enemy)
    {
        Assert.IsTrue(damageRangeEnemies.Count < 2);
        damageRangeEnemies.Add(enemy);

        var enemyShip = GetShip(enemy);
        enemyShip.Special = true;

        (damageRangeEnemies.Count == 1 ? damageRange : damageRange2).Show(power, hours, range, enemy);

        markers.Reshow(enemyShip);
    }

    public void HideDamageRange()
    {
        damageRange.Hide();
        damageRange2.Hide();

        foreach (var enemy in damageRangeEnemies)
        {
            var enemyShip = GetShip(enemy);
            enemyShip.Special = false;
            markers.Reshow(enemyShip);
        }
        damageRangeEnemies.Clear();
    }

    public void HideDamageRange(int id)
    {
        Assert.IsTrue(damageRangeEnemies.Count < 3, $"2423 {damageRangeEnemies.Count}");
        Assert.IsTrue(damageRangeEnemies.Count == 1 || damageRangeEnemies[0] == id, $"2424 {id}");
        if (damageRangeEnemies.Count == 1)
        {
            Assert.IsTrue(damageRangeEnemies[0] == id, $"2427 {damageRangeEnemies[0]} {id}");
            HideDamageRange();
        }
        else
        {
            GetShip(id).Special = false;
            if (damageRangeEnemies[0] == id)
            {
                (damageRange, damageRange2) = (damageRange2, damageRange);
                UIManager.Instance.SwapCannons();
                damageRangeEnemies.RemoveAt(0);
            }
            else
            {
                damageRangeEnemies.RemoveAt(1);
            }
            damageRange2.Hide();
        }
    }

    public void SpawnUO(ETacticalObjectType type, int enemyId)
    {
        TacticalObject obj;
        var enemy = GetShip(enemyId);
        if (type == ETacticalObjectType.StrikeGroup && enemy.Type == ETacticalObjectType.StrikeGroup)
        {
            var neutral = Instantiate(enemyShipPrefab, enemyParent, false).GetComponent<TacticalEnemyShip>();
            neutral.SetupFromEnemy(enemyId + 100, ETacticalObjectSide.Neutral, enemy);
            obj = neutral;
            allObjects.Add(obj);
        }
        else
        {
            var uo = CreateUO();
            uo.Setup(type, enemy);
            obj = uo;
        }
        var data = markers.ShowUO(obj);
        if (type != ETacticalObjectType.StrikeGroup || enemy.Type != ETacticalObjectType.StrikeGroup)
        {
            data.HideTimer = 100_000_000;
        }
    }

    public void DestroyNeutrals()
    {
        for (int objectsIndex = 0; objectsIndex < allObjects.Count; objectsIndex++)
        {
            var obj = allObjects[objectsIndex];
            if (obj.Side == ETacticalObjectSide.Neutral && obj.Type != ETacticalObjectType.Survivors)
            {
                (obj as UnidentifiedObject).Destroy();
                objectsIndex--;
            }
        }
    }

    public void SpawnSurvivor(int enemyId)
    {
        var obj = Instantiate(survivorObjectPrefab, survivorObjectParent);
        obj.Init(GetShip(enemyId).RectTransform.anchoredPosition);
        Markers.ShowUO(obj);
        allSurvivors.Add(obj);
        allObjects.Add(obj);
    }

    public void DestroySurvivor(SurvivorObject survivor)
    {
        survivor.Die();
        if (markers.IsUO(survivor))
        {
            markers.HideUO(survivor, out _, out _, out _);
        }
        else
        {
            markers.Hide(survivor, survivor.Type, false);
        }
        allObjects.Remove(survivor);
    }

    public void DestroySurvivor(int id)
    {
        var survivor = allSurvivors[id];
        if (!survivor.Dead)
        {
            DestroySurvivor(survivor);
        }
    }

    public void FireSurvivorObjectFinished(int id, bool success)
    {
        SurvivorObjectFinished(id, success);
    }

    public void FireAirstrikeAttacked(bool success, TacticalEnemyShip enemyShip)
    {
        AirstrikeAttacked(success, enemyShip);
    }

    public void FireMissionPlanned(EMissionOrderType type)
    {
        MissionPlanned(type);
    }

    public void FireMissionSent(EMissionOrderType type)
    {
        MissionSent(type);
    }

    public void FireOrderMissionSent(EMissionOrderType type)
    {
        OrderMissionSent(type);
    }

    public void FireSearchAndDestroyReady()
    {
        SearchAndDestroyReady();
    }

    public SurvivorObject GetSurvivorObjectInRange()
    {
        foreach (var survivor in allSurvivors)
        {
            if (!survivor.Dead && survivor.PlayerInRange)
            {
                return survivor;
            }
        }
        return null;
    }

    public bool IsUoInReconRange(Vector2 pos)
    {
        float rangeSqr = (StartRange * .8f);
        rangeSqr *= rangeSqr;
        foreach (var obj in Markers.Uos.Values)
        {
            if (Vector2.SqrMagnitude(obj.Object.RectTransform.anchoredPosition - pos) < rangeSqr)
            {
                return true;
            }
        }
        return false;
    }

    public void SetMissionIdleTime(EMissionOrderType orderType, int hours)
    {
        var missionsList = Missions[orderType];
        int ticks = hours * TimeManager.Instance.TicksForHour;
        foreach (var mission in missionsList)
        {
            if (mission.MissionSent)
            {
                mission.SetMissionIdleTime(ticks);
                return;
            }
        }
        Assert.IsTrue(false, "Mission not found");
    }

    public void DespawnAllSubmarines()
    {
        var list = Missions[EMissionOrderType.SubmarineHunt];
        for (int i = list.Count; i > 0; i--)
        {
            list[i - 1].DespawnSubmarine();
        }
    }

    public void SetAirstrikeDefaultTargets(List<int> targets)
    {
        var list = new List<TacticalEnemyShip>();
        if (targets != null)
        {
            foreach (var id in targets)
            {
                list.Add(GetShip(id));
            }
        }
        foreach (var mission in Missions[EMissionOrderType.Airstrike])
        {
            mission.PossibleMissionTargets.Clear();
            mission.PossibleMissionTargets.AddRange(list);
        }
    }

    public int GetBonusManeuversDefence(TacticalEnemyShip enemyShip = null)
    {
        int defence = BonusManeuversDefence;
        if (enemyShip != null)
        {
            if (enemyShip.GetDeadBlocksCount() >= 3)
            {
                defence += IslandBuffBonusManeuversDefence;
            }
        }
        if (moreDefense)
        {
            defence += 1;
        }
        return defence;
    }

    public void SetShowPath(bool show)
    {
        showPath = show;

        if (!show && pathToggle.Show)
        {
            pathToggle.Toggle();
        }
        pathToggle.gameObject.SetActive(show);

        SetShowPath();
    }

    public void Destroy(TacticalEnemyShip ship)
    {
        Assert.IsTrue(ship != null && ship.Side == ETacticalObjectSide.Neutral, ship == null ? "null ship" : (ship.ToString() + ";" + ship.Side));
        OnUODestroying(ship);
        ship.Die(true);
        this.StartCoroutineActionAfterFrames(() => Destroy(ship.gameObject), 1);
    }

    public void SetShowRescueRangeToggle(bool show)
    {
        if (!show && rescueRangeToggle.Show)
        {
            rescueRangeToggle.Toggle();
        }
        rescueRangeToggle.gameObject.SetActive(show);
    }

    public void SetShowCannonRangeToggle(bool show)
    {
        if (!show && cannonRangeToggle.Show)
        {
            cannonRangeToggle.Toggle();
        }
        cannonRangeToggle.gameObject.SetActive(show);
    }

    public void SetShowMissionRangeToggle(bool show, List<int> targets)
    {
        bool shouldShow = false;
        if (missionRangeToggle.Show)
        {
            shouldShow = show;
            missionRangeToggle.Toggle();
        }
        markers.SetMissionRangeTargets(targets);
        if (shouldShow)
        {
            missionRangeToggle.Toggle();
        }
        missionRangeToggle.gameObject.SetActive(show);
        missionRangeTargets.Clear();
        missionRangeTargets.AddRange(targets);
    }

    public void SetShowMagicSpriteToggle(bool show)
    {
        if (!show && magicSpriteToggle.Show)
        {
            magicSpriteToggle.Toggle();
        }
        magicSpriteToggle.gameObject.SetActive(show);
    }

    public void SetMagicSprite(bool finalForm)
    {
        lesserForm = !finalForm;
        this.finalForm = finalForm;

        var uiMan = UIManager.Instance;
        uiMan.MagicSprite1.SetActive(false);
        uiMan.MagicSprite2.SetActive(lesserForm);
        uiMan.MagicSprite3.SetActive(finalForm);
    }

    public void SetBonusConsequenceManeuversAttack(int value)
    {
        consequenceManeuversAttack = value;
    }

    public void ShowSurvivors()
    {
        foreach (var survivor in allSurvivors)
        {
            if (!survivor.Visible && !survivor.Dead)
            {
                survivor.Visible = true;
                survivor.Invisible = false;
                markers.ShowSurvivor(survivor);
            }
        }
    }

    public void RegisterProximity(Objective objective, TacticalEnemyShip enemy, float proximity, Action<bool> callback)
    {
        proximities.Add(objective, new ProximityData(enemy, proximity * proximity, callback));
    }

    public void UnregisterProximity(Objective objective)
    {
        proximities.Remove(objective);
    }

    public void SetShowSpriteStrategyPanel(bool show, List<int> targets, int objective)
    {
        StrategySelectionPanel.SetShowSprite(show, targets, objective);
    }

    public float GetAirstrikeMaxRange()
    {
        return airRaidSpeed * flightTimeOnFuel * 180;
    }

    public float GetMissionMaxRange(EMissionOrderType type, bool useTorpedoes)
    {
        float result = DistanceOnFuel;
        if (TacticalMission.SwitchPlaneTypeMissions.Contains(type) && !useTorpedoes)
        {
            result *= 1.25f;
        }
        return result;
    }

    public void ShowCannonRange()
    {
        if (!cannonRangeToggle.Show)
        {
            cannonRangeToggle.Toggle();
        }
    }

    public void RefreshCannons()
    {
        if (cannonRangeToggle.Show)
        {
            cannonRangeToggle.Toggle();
            ShowCannonRange();
        }
    }

    public void CancelMissions()
    {
        var deck = AircraftCarrierDeckManager.Instance;
        deck.RemoveMissionOrders();
        foreach (var list in Missions.Values)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                var mission = list[count - i - 1];
                if (mission.MissionStage > EMissionStage.Launching && mission.MissionStage < EMissionStage.Recovering)
                {
                    foreach (var squadron in mission.SentSquadrons)
                    {
                        deck.SendSquadronToHangar(squadron);
                    }
                }
                mission.RemoveMission(true);
            }
        }
        deck.FirePlaneCountChanged();
    }

    public void FireNodesChanged()
    {
        NodesChanged();
    }

#if ALLOW_CHEATS
    public void ToggleFriendImmune()
    {
        FriendImmune = !FriendImmune;
        Debug.Log(FriendImmune ? "Friend immune" : "Friend vulnerable");
    }

    public void ToggleFastFriends()
    {
        FastFriends = !FastFriends;
        Debug.Log(FastFriends ? "Friends fast speed" : "Friends normal speed");
    }
#endif

    private void ShowMissionPanelHelper(EMissionOrderType orderType, bool value)
    {
        switch (orderType)
        {
            case EMissionOrderType.Airstrike:
                MissionPanel.AirStrikeHelper.SetActive(value);
                break;
            case EMissionOrderType.Recon:
                MissionPanel.ReconHelper.SetActive(value);
                break;
            case EMissionOrderType.CarriersCAP:
                MissionPanel.CapHelper.SetActive(value);
                break;
            case EMissionOrderType.IdentifyTargets:
                MissionPanel.IdentifyTargetHelper.SetActive(value);
                break;
            case EMissionOrderType.NightScouts:
                MissionPanel.NighScoutsHelper.SetActive(value && SaveManager.Instance.Data.SelectedAircraftCarrier == ECarrierType.CV9);
                break;
        }
    }

    private void SaveMissions(List<MissionSaveData> missionsSave)
    {
        missionsSave.Clear();
        foreach (var list in Missions.Values)
        {
            foreach (var mission in list)
            {
                missionsSave.Add(mission.Save());
            }
        }
        foreach (var mission in customMissions)
        {
            missionsSave.Add(mission.Save());
        }
    }

    private void LoadMissions(List<MissionSaveData> missionsSave)
    {
        int index = 0;
        EMissionOrderType type = EMissionOrderType.Airstrike;
        foreach (var data in missionsSave)
        {
            if (!data.CustomMission)
            {
                if (data.Type != type)
                {
                    type = data.Type;
                    index = 0;
                }
                var list = Missions[data.Type];
                if (index >= list.Count)
                {
                    AddMissionFromSave(data);
                }
                else
                {
                    list[index].LoadFromSave(data, null);
                }
                index++;
            }
            else
            {
                AddMissionFromSave(data);
            }
        }
    }

    private void FleetSeen(Vector2 startPos, TacticalEnemyMapButton fleet)
    {
        if (!lastSeenEnemies.TryGetValue(fleet, out var marker))
        {
            using (var enumer = freeMarkers.GetEnumerator())
            {
                bool next = enumer.MoveNext();
                Assert.IsTrue(next);
                marker = enumer.Current;
                freeMarkers.Remove(marker);
            }
            lastSeenEnemies[fleet] = marker;
            NewEnemyPosition(fleet);
        }
        marker.SeenTimer = 0;
        marker.Fleet = fleet;
        var markerTrans = marker.Transform;
        var shipTrans = fleet.ShipScript.ObjectTransform;
        UncertainPosition(markerTrans, shipTrans.anchoredPosition, startPos);
        markerTrans.rotation = shipTrans.rotation;
        marker.gameObject.SetActive(true);
    }

    private void UncertainPosition(RectTransform marker, Vector2 fleetPos, Vector2 startPos)
    {
        fleetPos += (startPos - fleetPos).normalized * RevealedPositionUncertaintyRange / 2f;
        fleetPos += Random.insideUnitCircle * RevealedPositionUncertaintyRange / 2f;
        marker.anchoredPosition = fleetPos;
    }

    private List<TacticalEnemyMapButton> CheckFleetsInRange(Vector2 pos, float rangeSqr, bool recon)
    {
        var result = new List<TacticalEnemyMapButton>();
        foreach (var fleet in Fleets)
        {
            var diff = fleet.ShipScript.ObjectTransform.anchoredPosition - pos;
            if (diff.sqrMagnitude < rangeSqr)
            {
                bool identified = false;
                int index = fleet.FleetUnits.FindIndex((x) => x.Index == 0);
                if (index != -1)
                {
                    identified = fleet.FleetUnits[index].isHidden;
                    fleet.FleetUnits[index].Reveal();
                }
                if (recon)
                {
                    index = fleet.FleetUnits.FindIndex((x) => x.Index == 1);
                    if (index != -1)
                    {
                        identified = identified || fleet.FleetUnits[index].isHidden;
                        fleet.FleetUnits[index].Reveal();
                    }
                    index = fleet.FleetUnits.FindIndex((x) => x.Index == 2);
                    if (index != -1 && fleet.FleetUnits[index].enemyType != EEnemyTypeDemo.Unsure)
                    {
                        identified = identified || fleet.FleetUnits[index].isHidden;
                        fleet.FleetUnits[index].Reveal();
                    }
                }
                if (identified)
                {
                    EnemyUnitIdentified(fleet);
                }

                FleetSeen(pos, fleet);
                result.Add(fleet);
            }
        }
        return result;
    }

    private void HideIdentificationPanel(bool hidePreviewPanel)
    {
        EnemyUnitIdentified -= OnIdentifyingFleetShipIdentified;
        CurrentFleetBtn = null;
    }

    private void OnIdentifyingFleetShipIdentified(TacticalEnemyMapButton fleet)
    {
        if (CurrentFleetBtn == fleet)
        {
            ShowIdentificationPanel(fleet);
        }
    }

    private void CreateMovementGrid()
    {
        if (Map.MapNodes == null)
        {
            Map.MapNodes = new TacticalMapGrid(
                map,
                map.MapImage.GetComponent<RectTransform>().rect.width,
                map.MapImage.GetComponent<RectTransform>().rect.height,
                nodeResolution.x,
                nodeResolution.y);
        }

        if (mapNodesVisible)
        {
            foreach (var node in MapNodes.Nodes)
            {
                var nodeObj = Instantiate(MapNodePrefab, gridParent, false);
                ((RectTransform)nodeObj.transform).anchoredPosition = node.Position;

                if (node.IsOnLand)
                    nodeObj.GetComponent<Image>().color = Color.red;
                else
                    nodeObj.GetComponent<Image>().color = Color.green;
            }
        }
    }

    private void SpawnEnemy(bool isRedWater = false)
    {
        allObjects.Clear();
        objects.Clear();
        if (isRedWater)
        {
            Assert.IsTrue(map.EnemyUnits.Count == 2);
            int strikeGroups = 1;/*(Random.value <= RedWaterSetup.StrikeGroup2Chance ? 2 : 1);*/
            for (int i = 0; i < strikeGroups; i++)
            {
                List<EnemyManeuverData> buildingBlocks = new List<EnemyManeuverData>();
                buildingBlocks.Add(enemies[EEnemyShipType.Carrier][Random.Range(0, enemies[EEnemyShipType.Carrier].Count)]);
                buildingBlocks.Add(enemies[EEnemyShipType.Cargo][Random.Range(0, enemies[EEnemyShipType.Cargo].Count)]);
                if (Random.value <= RedWaterSetup.WarshipChance)
                {
                    buildingBlocks.Add(enemies[EEnemyShipType.Warship][Random.Range(0, enemies[EEnemyShipType.Warship].Count)]);
                    if (Random.value <= RedWaterSetup.AnotherShipChance)
                    {
                        EEnemyShipType randType = (EEnemyShipType)Random.Range(0, (int)EEnemyShipType.Count);
                        buildingBlocks.Add(enemies[randType][Random.Range(0, enemies[randType].Count)]);
                        if (Random.value <= RedWaterSetup.ExtraWarshipChance)
                        {
                            buildingBlocks.Add(enemies[EEnemyShipType.Warship][Random.Range(0, enemies[EEnemyShipType.Warship].Count)]);
                        }
                    }
                }
                var obj = Instantiate(enemyShipPrefab, enemyParent, false).GetComponent<TacticalEnemyShip>();
                obj.Setup(i, map.EnemyUnits[i], buildingBlocks, misidentifieds, misidentifiedsDict, null, false);
                objects.Add(obj);
                allObjects.Add(obj);
                markers.ShowUO(obj);
            }
        }
        else
        {
            bool weaker = buff == ETemporaryBuff.LessEnemies;
            for (int i = 0; i < map.EnemyUnits.Count; i++)
            {
                var obj = Instantiate(enemyShipPrefab, enemyParent, false).GetComponent<TacticalEnemyShip>();
                var data = map.EnemyUnits[i];
                obj.Setup(i, data, data.BuildingBlocks, misidentifieds, misidentifiedsDict, map.EnemyUnits[i].InstantDeadBlocks, !data.IsAlly && weaker);
                objects.Add(obj);
                allObjects.Add(obj);
                if (obj.Side == ETacticalObjectSide.Friendly)
                {
                    foreach (var block in obj.Blocks)
                    {
                        block.Visible = true;
                    }
                    if (!obj.IsDisabled)
                    {
                        markers.Show(obj, obj.Type, ETacticalObjectSide.Friendly);
                    }
                }
                else if (!obj.IsDisabled)
                {
                    markers.ShowUO(obj);
                }
            }
        }
    }

    private void SpawnUnidentifiedObjects(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var uo = CreateUO();
            uo.Setup(map, carrierOffset, borderOffset, UOVisibility);
            markers.ShowUO(uo);
        }
    }

    private UnidentifiedObject CreateUO()
    {
        var uo = Instantiate(unidentifiedObjectPrefab, unidentifiedObjectParent, false);
        uo.TypeChanged += () => OnUOTypeChanged(uo);
        uo.Destroying += () => OnUODestroying(uo);
        allObjects.Add(uo);
        return uo;
    }

    private void RefreshMaxMissions()
    {
        CurrentMaxMissions = BaseMaxMissions + islandBoost + islandBuff;
    }

    private void CreateMissions(EMissionOrderType missionType, int count)
    {
        var missions = Missions[missionType];
        int extraMissions = 0;
        foreach (var mission in missions)
        {
            if (mission.ExtraMission)
            {
                extraMissions++;
            }
        }
        while (missions.Count - extraMissions < count)
        {
            AddNewMission(missionType);
        }
    }

    private void SetCAP(CapData data)
    {
        var enemyAttacksMan = EnemyAttacksManager.Instance;
        if (data == null)
        {
            enemyAttacksMan.SetCAPDefencePoints(0);
            enemyAttacksMan.SetCapEscortPoints(0);
        }
        else
        {
            enemyAttacksMan.SetCAPDefencePoints(data.Defence);
            //enemyAttacksMan.SetCAPDefencePoints(data.Defence * capsInProgress.Count);

            enemyAttacksMan.SetCapEscortPoints(data.Escort);
            //enemyAttacksMan.SetCapEscortPoints(data.Escort * capsInProgress.Count);
        }
    }

    private void OnUOTypeChanged(UnidentifiedObject obj)
    {
        Assert.IsTrue(obj.Type == ETacticalObjectType.Nothing);
        if (!markers.IsUO(obj))
        {
            markers.Hide(obj, ETacticalObjectType.StrikeGroup, false);
            Destroy(obj.gameObject);
        }
    }

    private void OnUODestroying(TacticalObject obj)
    {
        if (markers.IsUO(obj))
        {
            markers.HideUO(obj, out _, out _, out _);
        }
        else if (markers.StrikeGroups.ContainsKey(obj))
        {
            markers.Hide(obj, ETacticalObjectType.StrikeGroup, false);
        }
        allObjects.Remove(obj);
    }

    private void OnWorldMapToggled(bool worldMap)
    {
        if (worldMap)
        {
            SaveManager.Instance.Data.CurrentMission = -1;
            ChangeMapLayout(worldMapMap, false, false);
            spawnUOTimer = 100_000_000;
            RefreshManeuvers();
        }
    }

    private void AddOfficerManeuvers()
    {
        using (var enumer = IslandsAndOfficersManager.Instance.OfficersEnumerator)
        {
            while (enumer.MoveNext())
            {
                int lv = enumer.Current.ManeuverLevel;
                var parentManev = enumer.Current.Maneuver;
                if (parentManev != null && lv != 0)
                {
                    PlayerManeuverData manev;
                    switch (lv)
                    {
                        case 3:
                            manev = parentManev.Level3;
                            break;
                        case 2:
                            manev = parentManev.Level2;
                            break;
                        default:
                            manev = parentManev;
                            break;
                    }
                    manev.Level = lv;
                    maneuversDict[parentManev.ManeuverType].Add(manev);
                }
            }
        }
    }

    private void AddValues(PlayerManeuverData manev, AttackParametersData bomber, AttackParametersData fighter, AttackParametersData torpedo)
    {
        switch (manev.NeededSquadrons.Type)
        {
            case EPlaneType.Bomber:
                AddValues(manev, bomber);
                break;
            case EPlaneType.Fighter:
                AddValues(manev, fighter);
                break;
            case EPlaneType.TorpedoBomber:
                AddValues(manev, torpedo);
                break;
            default:
                Assert.IsTrue(false, "bad type of needed squadrons for: " + manev.name);
                break;
        }
    }

    private void AddValues(PlayerManeuverData manev, AttackParametersData data)
    {
        manev.PlaneBonusValues.Attack = data.Attack;
        manev.PlaneBonusValues.Defense = data.Defense;
    }

    private void OnGeneratorsStateChanged(bool active)
    {
        foreach (var mission in Missions[EMissionOrderType.CounterHostileScouts])
        {
            SetShowMission(mission, active);
        }
        foreach (var mission in Missions[EMissionOrderType.SubmarineHunt])
        {
            SetShowMission(mission, active);
        }
    }

    private void SetShowMission(TacticalMission mission, bool show)
    {
        if (show || mission.MissionStage > EMissionStage.Launching)
        {
            mission.ButtonMission.gameObject.SetActive(show);
        }
    }

    private void SetShowAttackRange()
    {
        markers.SetShowAttackRange(attackRangeToggle.Show);
    }

    private void SetShowReconRange()
    {
        markers.SetShowReconRange(reconRangeToggle.Show);
    }

    private void SetShowRescueRange()
    {
        UIManager.Instance.SurvivorsRange.gameObject.SetActive(rescueRangeToggle.Show);
    }

    private void SetShowCannonRange()
    {
        UIManager.Instance.CannonRange.gameObject.SetActive(damageRange.Active() && cannonRangeToggle.Show);
        UIManager.Instance.CannonRange2.gameObject.SetActive(damageRange2.Active() && cannonRangeToggle.Show);
    }

    private void SetShowMissionRange()
    {
        markers.SetShowMissionRange(missionRangeToggle.Show);
    }

    private void SetShowMagicSprite()
    {
        UIManager.Instance.MagicSprite.gameObject.SetActive(magicSpriteToggle.Show);
    }

    private void SetShowPath()
    {
        markers.SetShowPath(showPath && pathToggle.Show);
    }

    private void PrepareMissionCallbackAirstrike(InputAction.CallbackContext _)
    {
        PrepareMission(EMissionOrderType.Airstrike);
    }

    private void PrepareMissionCallbackIdentify(InputAction.CallbackContext _)
    {
        PrepareMission(EMissionOrderType.IdentifyTargets);
    }

    private void PrepareMissionCallbackRecon(InputAction.CallbackContext _)
    {
        PrepareMission(EMissionOrderType.Recon);
    }

    private void PrepareMission(EMissionOrderType type)
    {
        var hudMan = HudManager.Instance;
        if (hudMan.IsSettingsOpened || !MissionsEnabled || GameStateManager.Instance.AlreadyShown || hudMan.CinematicPlay || hudMan.OngoingReport || WorldMap.Instance.gameObject.activeInHierarchy)
        {
            return;
        }
        foreach (var mission in Missions[type])
        {
            if (mission.MissionStage == EMissionStage.Available)
            {
                StartMissionSetupMode(mission);
                break;
            }
        }
    }
}
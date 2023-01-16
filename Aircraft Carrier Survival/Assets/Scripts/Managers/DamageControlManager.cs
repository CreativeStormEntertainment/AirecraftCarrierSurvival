using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class DamageControlManager : MonoBehaviour, ITickable, IEnableable
{
    public event Action OnDCChanged = delegate { };
    public event Action DcCategoryClicked = delegate { };
    public event Action MaintenanceActivated = delegate { };
    public event Action PumpsActivated = delegate { };

    public static DamageControlManager Instance;

    public bool MaintenanceActive
    {
        get;
        private set;
    }
    public bool PumpsActive
    {
        get;
        private set;
    }

    public bool MaintenanceDCFreeze
    {
        get;
        set;
    }
    public bool PumpsDCFreeze
    {
        get;
        set;
    }

    public EIssue? IssueDestination
    {
        get;
        set;
    }

    public float IslandInjuryChanceModifier
    {
        get;
        set;
    }

    public EDcCategory DefaultCategory
    {
        get;
        set;
    } = EDcCategory.Fire;

    public EDcCategoryFlag DcCategoryEnabled
    {
        get => dcCategoryEnabled;
        set
        {
            dcCategoryEnabled = value;
            foreach (var dcGroup in groups)
            {
                dcGroup.Portrait.RefreshButtons();
            }
        }
    }

    public float RepairSpeedModifier => 1 + nightShiftRepairSpeedModifier + escortRepairSpeedModifier + hastenedRepairsSpeedModifier;

    public bool AutoDC
    {
        get => autoDC;
        set
        {
            autoDC = value;
            foreach (var group in groups)
            {
                group.Button.SetSelectedEnable(!value);
                group.Portrait.SetSelectedEnable(!value);
            }
        }
    }

    public DcButtons SelectedButton => selectedButton;

    public SectionRoom WreckSection => wreckSection;

    public List<SectionRoom> MaintenanceRooms => maintenanceRooms;
    public List<SectionRoom> PumpRooms => pumpRooms;

    [NonSerialized]
    public List<List<Waypoint>> WreckWaypoints;

    public DCInstanceGroup SelectedGroup;

    public DCButton DcButtonPrefab;

    public BasketRandom FaultRandom = null;
    public BasketRandom FireRandom = null;
    public BasketRandom FloodRandom = null;
    public BasketRandom InjuredRandom = null;

    public int FireStaticEventTime = 20;

    public int FloodStaticEventTime = 20;

    public int RepairLeakTime = 20;
    public int InitialStartLeakTime = 3;
    public int BurstDoorTime = 60;
    public int SealedStartLeakTime = 300;

    public float DCNeighbourBonus = .2f;

    public float MinNeighbourFloodLevelToPump = .1f;

    public int DCGroupCount = 3;
    public int TempGroupsCount = 2;

    public float DisabledCrewQuartersInjureChanceModifier = 1.25f;

    public Dictionary<EWaypointTaskType, EDCType> EnumConvertFromWaypoint;
    public Dictionary<EDCType, EWaypointTaskType> EnumConvertToWaypoint;

    public List<WreckButton> WreckButtons;
    public List<Transform> FinalWreckPositions;
    public float WreckSpeed = 5f;

    public Dictionary<EWaypointTaskType, string> LocalizedJobs;

    public HashSet<DCInstanceGroup> CurrentGroups;

    public int MaxDCQueue = 5;

    [SerializeField]
    private List<DcImages> dcImages = null;

    [SerializeField]
    private Transform dcPanel = null;

    [SerializeField]
    private PortraitDC dcPortrait = null;

    [SerializeField]
    private SectionRoom crewQuarters = null;

    [SerializeField]
    private List<SectionRoom> maintenanceRooms = null;

    [SerializeField]
    private List<SectionRoom> pumpRooms = null;

    [SerializeField]
    private List<DcButtons> faultsButtons = null;
    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("flooodsButtons")]
    private List<DcButtons> floodsButtons = null;

    [SerializeField]
    private FaceToSectionCamera pathDotPrefab = null;
    [SerializeField]
    private FaceToSectionCamera pathSectionPrefab = null;
    [SerializeField]
    private Sprite dcPathDestination = null;
    [SerializeField]
    private float pathSectionSize = .3f;
    [SerializeField]
    private float pathDotSize = .1f;
    [SerializeField]
    private float maxResizePerc = .2f;
    [SerializeField]
    private float spacePerc = .33f;
    [SerializeField]
    private SectionRoom wreckSection = null;

    [SerializeField]
    private List<ExtinguisherHandler> extinguishers = null;

    private List<DCInstanceGroup> groups;
    private HashSet<DCInstanceGroup> freeGroups;

    private HashSet<SectionSegmentGroup> allGroups;
    //private bool selectFloodedSegments;

    private HashSet<SectionSegmentGroup> segmentGroupsFlooded;

    private int timer;

    private bool onWorldMap;

    private HashSet<SectionSegment> crewQuartersSegments;
    private HashSet<SectionSegment> crewQuartersSegmentsPool;
    private List<SectionSegment> toVisit;
    private HashSet<SectionSegment> backup;
    private HashSet<SectionSegment> alreadyVisited;

    private bool setup;
    private int dcs;

    private List<SectionSegment> segmentsToDamage;

    private DcButtons oldSelectedButton;
    private DcButtons selectedButton;

    private List<DCButtonCooldown> cooldownButtons;

    private List<FaceToSectionCamera> usedPath;
    private List<FaceToSectionCamera> usedSection;
    private HashSet<FaceToSectionCamera> dcPathPool;
    private HashSet<FaceToSectionCamera> dcSectionPool;

    private HashSet<SectionSegment> floodableSegments;

    private HashSet<int> portraits;
    private bool disabled;

    private Dictionary<EDcCategory, HashSet<DCInstanceGroup>> categorizedGroups;
    private Dictionary<EDcCategory, HashSet<SectionSegment>> segmentsWithIssues;
    private SortedDictionary<int, List<KeyValuePair<DCInstanceGroup, SectionSegment>>> paths;
    private List<PathCheckData> autoToVisit;

    private bool autoDC;

    private HashSet<SectionSegment> workingSegmentsSpecialSection;
    private EDcCategoryFlag dcCategoryEnabled;

    private EDcButtons buttons;

    private float nightShiftRepairSpeedModifier;
    private float hastenedRepairsSpeedModifier;
    private float escortRepairSpeedModifier;

    private Dictionary<AnimationClip, ExtinguisherHandler> poolPrefabs;
    private Dictionary<AnimationClip, List<ExtinguisherHandler>> pool;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        EnumConvertFromWaypoint = new Dictionary<EWaypointTaskType, EDCType>();
        EnumConvertFromWaypoint[EWaypointTaskType.Firefighting] = EDCType.Firefight;
        EnumConvertFromWaypoint[EWaypointTaskType.Repair] = EDCType.Repair;
        EnumConvertFromWaypoint[EWaypointTaskType.Rescue] = EDCType.Rescue;
        EnumConvertFromWaypoint[EWaypointTaskType.Rescue2] = EDCType.Rescue;
        EnumConvertFromWaypoint[EWaypointTaskType.Rescue3] = EDCType.Rescue;

        EnumConvertToWaypoint = new Dictionary<EDCType, EWaypointTaskType>();
        foreach (var pair in EnumConvertFromWaypoint)
        {
            EnumConvertToWaypoint[pair.Value] = pair.Key;
        }

        groups = new List<DCInstanceGroup>();
        CurrentGroups = new HashSet<DCInstanceGroup>();
        freeGroups = new HashSet<DCInstanceGroup>();

        allGroups = new HashSet<SectionSegmentGroup>();

        LocalizationManager locMan = LocalizationManager.Instance;
        LocalizedJobs = new Dictionary<EWaypointTaskType, string>()
        {
            {EWaypointTaskType.Firefighting, locMan.GetText("Firefighting")},
            {EWaypointTaskType.Normal, "-"},
            {EWaypointTaskType.Repair, locMan.GetText("Repair")},
            {EWaypointTaskType.RepairDoor, locMan.GetText("RepairDoor")},
            {EWaypointTaskType.Rescue, locMan.GetText("Rescue")},
            {EWaypointTaskType.Rescue2, locMan.GetText("Rescue")},
            {EWaypointTaskType.Rescue3, locMan.GetText("Rescue")},
            {EWaypointTaskType.Waterpump, locMan.GetText("Waterpump")},
        };

        toVisit = new List<SectionSegment>();
        backup = new HashSet<SectionSegment>();
        alreadyVisited = new HashSet<SectionSegment>();

        segmentsToDamage = new List<SectionSegment>();

        cooldownButtons = new List<DCButtonCooldown>();

        usedPath = new List<FaceToSectionCamera>();
        usedSection = new List<FaceToSectionCamera>();
        dcPathPool = new HashSet<FaceToSectionCamera>();
        dcSectionPool = new HashSet<FaceToSectionCamera>();

        segmentGroupsFlooded = new HashSet<SectionSegmentGroup>();

        floodableSegments = new HashSet<SectionSegment>();

        portraits = new HashSet<int>();

        segmentsWithIssues = new Dictionary<EDcCategory, HashSet<SectionSegment>>();
        segmentsWithIssues[EDcCategory.Fire] = new HashSet<SectionSegment>();
        segmentsWithIssues[EDcCategory.Water] = new HashSet<SectionSegment>();
        segmentsWithIssues[EDcCategory.Injured] = new HashSet<SectionSegment>();
        segmentsWithIssues[EDcCategory.Crash] = new HashSet<SectionSegment>();
        segmentsWithIssues[EDcCategory.Destroyed] = new HashSet<SectionSegment>();
        segmentsWithIssues[EDcCategory.Fault] = new HashSet<SectionSegment>();

        categorizedGroups = new Dictionary<EDcCategory, HashSet<DCInstanceGroup>>();
        categorizedGroups[EDcCategory.Fire] = new HashSet<DCInstanceGroup>();
        categorizedGroups[EDcCategory.Water] = new HashSet<DCInstanceGroup>();
        categorizedGroups[EDcCategory.Injured] = new HashSet<DCInstanceGroup>();
        categorizedGroups[EDcCategory.Crash] = new HashSet<DCInstanceGroup>();
        categorizedGroups[EDcCategory.Destroyed] = new HashSet<DCInstanceGroup>();
        categorizedGroups[EDcCategory.Fault] = new HashSet<DCInstanceGroup>();

        workingSegmentsSpecialSection = new HashSet<SectionSegment>();
        paths = new SortedDictionary<int, List<KeyValuePair<DCInstanceGroup, SectionSegment>>>();
        autoToVisit = new List<PathCheckData>();

        dcCategoryEnabled = (EDcCategoryFlag)(-1);

        pool = new Dictionary<AnimationClip, List<ExtinguisherHandler>>();
        poolPrefabs = new Dictionary<AnimationClip, ExtinguisherHandler>();

        foreach (var extinguisher in extinguishers)
        {
            poolPrefabs[extinguisher.Clip] = extinguisher;
            pool[extinguisher.Clip] = new List<ExtinguisherHandler>();
        }
    }

    private void Start()
    {
        CameraManager.Instance.ViewChanged += OnViewChanged;

        foreach (var section in pumpRooms)
        {
            foreach (var segment in section.GetAllSegments(true))
            {
                segment.CanPumpWater = true;
            }
        }

        TimeManager.Instance.AddTickable(this);

        foreach (var segment in wreckSection.GetAllSegments(true))
        {
            segment.Untouchable = true;
        }

        WreckWaypoints = new List<List<Waypoint>>();
        foreach (var subsection in wreckSection.SubsectionRooms)
        {
            WreckWaypoints.Add(SetupWreckWaypoints(subsection.Path));
        }
        WorldMap.Instance.Toggled += OnWorldMapToggled;

        crewQuartersSegments = new HashSet<SectionSegment>();
        crewQuartersSegmentsPool = new HashSet<SectionSegment>();
        foreach (var segment in crewQuarters.GetAllSegments(true))
        {
            crewQuartersSegments.Add(segment);
        }
    }

    private void Update()
    {
        //if (SelectedGroup != null && CameraManager.Instance.CurrentCameraView == ECameraView.Sections &&
        //    SelectedGroup.InWaterPumps())
        //{
        //    selectFloodedSegments = true;
        //    foreach (var segment in SectionRoomManager.Instance.GetAllSegments())
        //    {
        //        segment.SetShowSegmentSelection(segment.IsFlooding());
        //    }
        //}
        //else if (selectFloodedSegments)
        //{
        //    foreach (var segment in SectionRoomManager.Instance.GetAllSegments())
        //    {
        //        segment.SetShowSegmentSelection(false);
        //    }
        //}

        foreach (var group in groups)
        {
            group.Update();
        }
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
        var damageControl = basicInput.DamageControl;
        damageControl.MalfunctionButton.performed -= DCButtonCallback1;
        damageControl.DefloodButton.performed -= DCButtonCallback2;

        var setup = basicInput.SetupDC;
        setup.SetToFire.performed -= SetAllDCCallbackFire;
        setup.SetToMalfunction.performed -= SetAllDCCallbackFault;
        setup.SetToFlood.performed -= SetAllDCCallbackWater;
        setup.SetToMedic.performed -= SetAllDCCallbackInjured;
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
    }

    public void SetEnableDC(bool enable)
    {
        OnDCSelected(null);
        foreach (var dc in groups)
        {
            dc.SetEnable(enable);
            dc.Portrait.RefreshButtons();
        }
    }

    public void SetEnableDCButtons(bool enable)
    {
        foreach (var button in faultsButtons)
        {
            button.SetEnable(enable);
        }
        foreach (var button in floodsButtons)
        {
            button.SetEnable(enable);
        }
    }

    public void Setup(int maxGroupCount)
    {
        var walk = AnimationManager.Instance.DCWalkClip;
        var trans = transform;
        //bool can = HudManager.Instance.HasNo(ETutorialMode.DisableOtherDCIdle);

        var basicInput = BasicInput.Instance;
        var damageControl = basicInput.DamageControl;
        damageControl.MalfunctionButton.performed += DCButtonCallback1;
        damageControl.DefloodButton.performed += DCButtonCallback2;

        var setup = basicInput.SetupDC;
        setup.SetToFire.performed += SetAllDCCallbackFire;
        setup.SetToMalfunction.performed += SetAllDCCallbackFault;
        setup.SetToFlood.performed += SetAllDCCallbackWater;
        setup.SetToMedic.performed += SetAllDCCallbackInjured;

        for (int i = 0; i < maxGroupCount; i++)
        {
            var group = CreateGroup(walk, trans);
            groups.Add(group);
            freeGroups.Add(group);
            SetEnableVisuals(group, false);
        }
        OnViewChanged(CameraManager.Instance.CurrentCameraView);

        var paramsMan = Parameters.Instance;
        FaultRandom.Init(paramsMan.FaultSpawnRateCount);
        FireRandom.Init(paramsMan.FireSpawnRateCount);
        FloodRandom.Init(paramsMan.FloodSpawnRateCount);
        InjuredRandom.Init(paramsMan.InjuredSpawnRateCount);
        timer = Mathf.RoundToInt(paramsMan.SpontaneousDamageHours * TimeManager.Instance.TicksForHour);
    }

    public void PostSetup()
    {
        setup = true;

        int index1 = 0;
        int index2 = 0;
        var rooms1 = maintenanceRooms;
        var rooms2 = pumpRooms;
        for (int i = 0; i < dcs; i++)
        {
            if (rooms1.Count > index1)
            {
                SpawnDC(rooms1[index1].SubsectionRooms[0].Segments[0]);
                index1++;
                (index1, index2) = (index2, index1);
                (rooms1, rooms2) = (rooms2, rooms1);
            }
            else if (rooms2.Count > index2)
            {
                SpawnDC(rooms2[index2].SubsectionRooms[0].Segments[0]);
                index2++;
            }
            else
            {
                SpawnDC();
            }
        }
    }

    public void LoadData(ref DamageControlSaveData data)
    {
        int index = 0;
        if (data.DcDatas.Count != CurrentGroups.Count)
        { 
            Debug.LogError($"{data.DcDatas.Count} - {CurrentGroups.Count}");
        }
        foreach (var group in CurrentGroups)
        {
            group.LoadData(data.DcDatas[index++], dcImages);
            if (index >= data.DcDatas.Count)
            {
                break;
            }
        }

        var sectionMan = SectionRoomManager.Instance;
        if (data.Damage1ButtonUsed > -1)
        {
            selectedButton = faultsButtons[0];
            SetDCButtonAction(sectionMan.GetSegment(data.Damage1ButtonUsed));
        }
        if (data.Damage2ButtonUsed > -1)
        {
            selectedButton = faultsButtons[1];
            SetDCButtonAction(sectionMan.GetSegment(data.Damage2ButtonUsed));
        }
        if (data.Water1ButtonUsed > -1)
        {
            selectedButton = floodsButtons[0];
            SetDCButtonAction(sectionMan.GetSegment(data.Water1ButtonUsed));
        }
        if (data.Water2ButtonUsed > -1)
        {
            selectedButton = floodsButtons[1];
            SetDCButtonAction(sectionMan.GetSegment(data.Water2ButtonUsed));
        }

        FaultRandom.LoadData(ref data.Fault);
        FireRandom.LoadData(ref data.Fire);
        FloodRandom.LoadData(ref data.Flood);
        InjuredRandom.LoadData(ref data.Injured);
        timer = data.RandomTimer;
    }

    public void SaveData(ref DamageControlSaveData data)
    {
        data.DcDatas.Clear();
        foreach (var group in CurrentGroups)
        {
            data.DcDatas.Add(group.SaveData());
        }

        var sectionMan = SectionRoomManager.Instance;

        data.Damage1ButtonUsed = -1;
        data.Damage2ButtonUsed = -1;
        data.Water1ButtonUsed = -1;
        data.Water2ButtonUsed = -1;
        foreach (var button in cooldownButtons)
        {
            if (button.DcButtons.IsFlood)
            {
                if (floodsButtons[0] == button.DcButtons)
                {
                    data.Water1ButtonUsed = sectionMan.IndexOf(button.SectionSegment);
                }
                else
                {
                    data.Water2ButtonUsed = sectionMan.IndexOf(button.SectionSegment);
                }
            }
            else
            {
                if (faultsButtons[0] == button.DcButtons)
                {
                    data.Damage1ButtonUsed = sectionMan.IndexOf(button.SectionSegment);
                }
                else
                {
                    data.Damage2ButtonUsed = sectionMan.IndexOf(button.SectionSegment);
                }
            }
        }

        FaultRandom.SaveData(ref data.Fault);
        FireRandom.SaveData(ref data.Fire);
        FloodRandom.SaveData(ref data.Flood);
        InjuredRandom.SaveData(ref data.Injured);
        data.RandomTimer = timer;
    }

    public void Tick()
    {
        CheckDCPlacement();
        var sectionMan = SectionRoomManager.Instance;
        if (allGroups.Count == 0)
        {
            foreach (var segment in sectionMan.GetAllSegments())
            {
                allGroups.Add(segment.Group);
            }
        }

        foreach (var set in categorizedGroups.Values)
        {
            set.Clear();
        }

        bool crashOnly = false;
        foreach (var dc in CurrentGroups)
        {
            if (dc.Category == EDcCategory.Crash && dc.CrashOnly)
            {
                if (dc.CanAssign(false))
                {
                    crashOnly = true;
                }
            }
        }

        int maintenanceCount = 0;
        int floodCount = 0;
        foreach (var dc in CurrentGroups)
        {
            switch (dc.Category)
            {
                case EDcCategory.Crash:
                    maintenanceCount++;
                    break;
                case EDcCategory.Water:
                    floodCount++;
                    break;
            }
        }

        int foundDc = 0;
        bool maintenance = false;
        bool flood = false;
        foreach (var dc in CurrentGroups)
        {
            if (dc.CurrentSegment.HasAnyRepairableIssue() && dc.CanAssign(true))
            {
                dc.RecheckJob();
            }

            if (maintenanceCount > 1 && !maintenance && dc.Category == EDcCategory.Crash && maintenanceRooms.Contains(dc.FinalSegment.Parent.ParentSection))
            {
                maintenance = true;
                continue;
            }

            if (floodCount > 1 && !flood && dc.Category == EDcCategory.Water && pumpRooms.Contains(dc.FinalSegment.Parent.ParentSection))
            {
                flood = true;
                continue;
            }

            //noflood
            if (dc.CanAssign(false))
            {
                AddAutoDc(dc, crashOnly, ref foundDc);
            }
        }
        if (foundDc != 0)
        {
            if (!maintenance && maintenanceCount > 1)
            {
                CheckDCRooms(categorizedGroups[EDcCategory.Fault], MaintenanceRooms);
            }
            if (!flood && floodCount > 1)
            {
                CheckDCRooms(categorizedGroups[EDcCategory.Water], PumpRooms);
            }

            foreach (var set in segmentsWithIssues.Values)
            {
                set.Clear();
            }
            var wreckSegments = segmentsWithIssues[EDcCategory.Crash];
            var deckMan = AircraftCarrierDeckManager.Instance;
            if (deckMan.HasDamage)
            {
                if (deckMan.HasWreck)
                {
                    foreach (var segment in WreckSection.SubsectionRooms[0].Segments)
                    {
                        if (segment.DcCanEnter())
                        {
                            wreckSegments.Add(segment);
                        }
                    }
                }
                if (deckMan.HasKamikazeFront)
                {
                    foreach (var segment in WreckSection.SubsectionRooms[1].Segments)
                    {
                        if (segment.DcCanEnter())
                        {
                            wreckSegments.Add(segment);
                        }
                    }
                }
                if (deckMan.HasKamikazeEnd)
                {
                    foreach (var segment in WreckSection.SubsectionRooms[2].Segments)
                    {
                        if (segment.DcCanEnter())
                        {
                            wreckSegments.Add(segment);
                        }
                    }
                }
            }
            foreach (var segment in sectionMan.GetAllSegments())
            {
                if (segment.DcCanEnter())
                {
                    if (segment.Fire.Exists)
                    {
                        segmentsWithIssues[EDcCategory.Fire].Add(segment);
                    }
                    if (segment.Damage.Exists)
                    {
                        segmentsWithIssues[EDcCategory.Fault].Add(segment);
                    }
                    if (segment.IsFlooding())
                    {
                        segmentsWithIssues[EDcCategory.Water].Add(segment);
                    }
                    if (segment.Injured())
                    {
                        segmentsWithIssues[EDcCategory.Injured].Add(segment);
                    }
                    if (segment.Parent.BrokenRepairable)
                    {
                        segmentsWithIssues[EDcCategory.Destroyed].Add(segment);
                    }
                }
            }
            for (EDcCategory category = 0; category < EDcCategory.Count; category++)
            {
                var groups = categorizedGroups[category];
                if (groups.Count == 0)
                {
                    continue;
                }
                var segments = segmentsWithIssues[category];
                if (segments.Count > 0)
                {
                    paths.Clear();

                    foreach (var group in groups)
                    {
                        AutoCheckPath(group, segments);
                    }
                    AutoSetPath(groups, segments);
                }
                if (category >= EDcCategory.Crash)
                {
                    continue;
                }
                foreach (var group in groups)
                {
                    if (group.CanAssign(false) && group.CurrentSegment.HasAnyRepairableIssue())
                    {
                        group.Kickout(true);
                    }
                }
            }
            for (EDcCategory category = EDcCategory.Crash; category < EDcCategory.Count; category++)
            {
                foreach (var group in categorizedGroups[category])
                {
                    if (group.CanAssign(false) && group.CurrentSegment.HasAnyRepairableIssue())
                    {
                        group.Kickout(true);
                    }
                }
            }

            SetDcToSpecialSection(categorizedGroups[EDcCategory.Water], pumpRooms);
            SetDcToSpecialSection(categorizedGroups[EDcCategory.Fault], maintenanceRooms);
        }

        if (!disabled && !onWorldMap && --timer <= 0 && !sectionMan.DisableDangers)
        {
            var paramsMan = Parameters.Instance;
            FaultRandom.Check((MaintenanceActive ? paramsMan.FaultSpontaneousSpreadWithMaintenance : 1f), FireDamageRandom);
            FireRandom.Check(1f, FireFireRandom);
            FloodRandom.Check((PumpsActive ? paramsMan.FloodSpontaneousSpreadWithPumps : 1f), FireFloodRandom);
            float injuryChance = 1f + (sectionMan.CrewQuarters.IsWorking ? 0f : paramsMan.InjuredSpawnRateDisabledCrewQuartersMultiplier) + IslandInjuryChanceModifier;
            InjuredRandom.Check(injuryChance, FireInjureRandom);
            timer = Mathf.RoundToInt(paramsMan.SpontaneousDamageHours * TimeManager.Instance.TicksForHour);
        }
    }

    public void OnDCButtonClicked(DcButtons button)
    {
        if (selectedButton != button)
        {
            oldSelectedButton = selectedButton;
            if (selectedButton != null)
            {
                selectedButton.Stop();
                selectedButton = null;
            }
            selectedButton = button;
            if (selectedButton != null)
            {
                selectedButton.Press();
                CursorV2.Instance.SetCursor(selectedButton.IsFlood ? ECursor.Flood : ECursor.Fault);
                OnDCSelected(null);
            }
            else
            {
                CursorV2.Instance.SetCursor(ECursor.Default);
            }
        }
    }

    public void AddTempGroups(int tempCount)
    {
        for (int i = 0; i < tempCount; i++)
        {
            SpawnDC();
        }
    }

    public void RemoveTempGroups(int tempCount)
    {
        for (int i = 0; i < tempCount; i++)
        {
            DespawnDC();
        }
    }

    public void SetShowWreckButton(bool show, EWreckType type)
    {
        var deck = AircraftCarrierDeckManager.Instance;
        switch (type)
        {
            case EWreckType.Wreck:
                show = show && deck.HasWreck;
                break;
            case EWreckType.FrontKamikaze:
                show = show && deck.HasKamikazeFront;
                break;
            case EWreckType.EndKamikaze:
                show = show && deck.HasKamikazeEnd;
                break;
        }

        int index = (int)type;
        var wreck = PlaneMovementManager.Instance.CurrentWrecks[index];
        WreckButtons[index].gameObject.SetActive(wreck != null && !wreck.AnimCrash && show && CameraManager.Instance.CurrentCameraView == ECameraView.Sections);
    }

    public void CheckDCPlacement()
    {
        MaintenanceActive = false;
        foreach (var room in maintenanceRooms)
        {
            if (room.IsWorking && CheckDCPlacement(room))
            {
                MaintenanceActive = true;
                MaintenanceActivated();
                break;
            }
        }

        PumpsActive = false;
        foreach (var room in pumpRooms)
        {
            if (room.IsWorking && CheckDCPlacement(room))
            {
                PumpsActive = true;
                PumpsActivated();
                break;
            }
        }
        UpdateDcButtons();
    }

    public void FireDamageRandom()
    {
        if (!onWorldMap && !SectionRoomManager.Instance.DisableDangers)
        {
            foreach (var segment in GetAllDamagableSegments())
            {
                if (!segment.Damage.Exists && !segment.Parent.IsBroken)
                {
                    segmentsToDamage.Add(segment);
                }
            }
            if (segmentsToDamage.Count > 0)
            {
                RandomUtils.GetRandom(segmentsToDamage).MakeDamage();
            }
            segmentsToDamage.Clear();
        }
    }

    public void FireFireRandom()
    {
        if (!onWorldMap && !SectionRoomManager.Instance.DisableDangers)
        {
            foreach (var segment in GetAllDamagableSegments())
            {
                if (!segment.Fire.Exists && !segment.IsFlooding())
                {
                    segmentsToDamage.Add(segment);
                }
            }
            if (segmentsToDamage.Count > 0)
            {
                RandomUtils.GetRandom(segmentsToDamage).MakeFire();
            }
            segmentsToDamage.Clear();
        }
    }

    public void FireFloodRandom()
    {
        if (!onWorldMap && !SectionRoomManager.Instance.DisableDangers)
        {
            SectionRoomManager.Instance.GetFloodableDeckSegments(floodableSegments);
            if (floodableSegments.Count > 0)
            {
                RandomUtils.GetRandom(floodableSegments).MakeFlood(false);
            }
        }
    }

    public void FireInjureRandom()
    {
        if (!onWorldMap && !SectionRoomManager.Instance.DisableDangers)
        {
            foreach (var segment in GetAllDamagableSegments())
            {
                if (!segment.Parent.IsBroken && !segment.Fire.Exists && !segment.IsFlooding() && !segment.HasInjured)
                {
                    segmentsToDamage.Add(segment);
                }
            }
            if (segmentsToDamage.Count > 0)
            {
                RandomUtils.GetRandom(segmentsToDamage).MakeInjured(EWaypointTaskType.Rescue);
            }
            segmentsToDamage.Clear();
        }
    }

    public void SpawnDC()
    {
        if (!setup)
        {
            dcs++;
            return;
        }

        crewQuartersSegmentsPool.Clear();
        foreach (var segment in crewQuartersSegments)
        {
            crewQuartersSegmentsPool.Add(segment);
        }
        toVisit.Clear();
        alreadyVisited.Clear();
        while (crewQuartersSegmentsPool.Count > 0)
        {
            var segment = RandomUtils.GetRandom(crewQuartersSegmentsPool);
            toVisit.Add(segment);
            alreadyVisited.Add(segment);
            crewQuartersSegmentsPool.Remove(segment);
        }
        int index = 0;
        int count = toVisit.Count;
        SectionSegment freeSegment = null;
        while (toVisit.Count > 0)
        {
            freeSegment = toVisit[0];
            toVisit.RemoveAt(0);
            if (!freeSegment.IsFlooded() && freeSegment.Dc == null)
            {
                break;
            }
            foreach (var segment in freeSegment.NeighboursDirectionDictionary.Keys)
            {
                if (alreadyVisited.Add(segment))
                {
                    toVisit.Add(segment);
                }
            }
            if (++index == count)
            {
                backup.Clear();
                foreach (var segment in toVisit)
                {
                    backup.Add(segment);
                }
                toVisit.Clear();
                while (backup.Count != 0)
                {
                    var segment = RandomUtils.GetRandom(backup);
                    toVisit.Add(segment);
                    backup.Remove(segment);
                }
            }
            Assert.IsFalse(toVisit.Count == 0);
        }

        SpawnDC(freeSegment);
    }

    public void DespawnDC()
    {
        DCInstanceGroup group = null;
        foreach (var group2 in CurrentGroups)
        {
            if (group2.Job == EWaypointTaskType.Normal)
            {
                group = group2;
                break;
            }
        }
        if (group == null)
        {
            group = RandomUtils.GetRandom(CurrentGroups);
        }

        DespawnDC(group);
    }

    public void DespawnDC(DCInstanceGroup group)
    {
        CurrentGroups.Remove(group);
        freeGroups.Add(group);
        group.Hide();
        SetEnableVisuals(group, false);

        if (SelectedGroup == group || freeGroups.Contains(SelectedGroup))
        {
            OnDCSelected(null);
        }

        for (int i = 0; i < 7; i++)
        {
            portraits.Add(i);
        }
        foreach (var currentGroup in CurrentGroups)
        {
            portraits.Remove(currentGroup.PortraitIndex);
        }
        OnDCChanged();
    }

    public bool SetDcButtons(EDcButtons buttons)
    {
        if (cooldownButtons.Count > 0)
        {
            return false;
        }

        this.buttons = buttons;

        faultsButtons[0].gameObject.SetActive(false);
        faultsButtons[1].gameObject.SetActive(false);
        floodsButtons[0].gameObject.SetActive(false);
        floodsButtons[1].gameObject.SetActive(false);
        switch (buttons)
        {
            case EDcButtons.Both:
                faultsButtons[0].gameObject.SetActive(true);
                floodsButtons[0].gameObject.SetActive(true);
                break;
            case EDcButtons.Faults:
                faultsButtons[0].gameObject.SetActive(true);
                faultsButtons[1].gameObject.SetActive(true);
                break;
            case EDcButtons.Floods:
                floodsButtons[0].gameObject.SetActive(true);
                floodsButtons[1].gameObject.SetActive(true);
                break;
            default:
                Debug.LogError("EDcButtons error");
                break;
        }
        return true;
    }

    public bool SetDCButtonAction(SectionSegment segment)
    {
        bool inSectionView = CameraManager.Instance.CurrentCameraView == ECameraView.Sections;
        if (selectedButton != null)
        {
            if (selectedButton.IsFlood)
            {
                if (segment.IsFlooding() && !segment.Group.Flood.Repair)
                {
                    if (segment.Group.Flood.Exists)
                    {
                        segment.Group.ButtonRepair = true;
                        segment.Group.Flood.Repair = true;
                        cooldownButtons.Add(new DCButtonCooldown(segment, selectedButton, false));
                        UpdateDcButtons();

                        if (segment.PumpingDCIcon != null)
                        {
                            segment.PumpingDCIcon.SetActive(inSectionView);
                        }
                    }
                    else
                    {
                        segment.Group.StopFlood();
                    }
                    OnDCButtonClicked(null);
                    return true;
                }
            }
            else
            {
                RepairableDanger danger = null;
                bool destruction = false;
                if (segment.Parent.BrokenRepairable && !segment.Parent.Destruction.Repair)
                {
                    danger = segment.Parent.Destruction;
                    destruction = true;
                }
                if (segment.Damage.Exists && !segment.Damage.Repair)
                {
                    danger = segment.Damage;
                }
                if (danger != null)
                {
                    if (segment.RepairingDCIcon != null)
                    {
                        segment.RepairingDCIcon.SetActive(inSectionView);
                    }

                    danger.RepairData.Max = Parameters.Instance.DCButtonFixTickTime;
                    danger.Repair = true;
                    cooldownButtons.Add(new DCButtonCooldown(segment, selectedButton, destruction));
                    UpdateDcButtons();
                    OnDCButtonClicked(null);
                    return true;
                }
            }
        }
        return false;
    }

    public void CheckDCButton(SectionSegment segment, bool isFlood, bool destruction)
    {
        for (int i = 0; i < cooldownButtons.Count; i++)
        {
            if (cooldownButtons[i].SectionSegment == segment && isFlood == cooldownButtons[i].DcButtons.IsFlood && destruction == cooldownButtons[i].Destruction)
            {
                if (isFlood)
                {
                    if (segment.PumpingDCIcon != null)
                    {
                        segment.PumpingDCIcon.SetActive(false);
                    }
                    segment.Group.Flood.Repair = false;
                    segment.Group.ButtonRepair = false;

                    cooldownButtons.RemoveAt(i);
                    UpdateDcButtons();
                    break;
                }
                else
                {
                    if (segment.RepairingDCIcon != null)
                    {
                        segment.RepairingDCIcon.SetActive(false);
                    }
                    if (destruction)
                    {
                        segment.Parent.Destruction.Repair = false;
                    }
                    else
                    {
                        segment.Damage.Repair = false;
                    }

                    cooldownButtons.RemoveAt(i);
                    UpdateDcButtons();
                    break;
                }
            }
        }
    }

    public void SetShowPath(DCInstanceGroup group)
    {
        if (group == SelectedGroup || SelectedGroup == null)
        {
            ReturnToPool(dcPathPool, usedPath);
            ReturnToPool(dcSectionPool, usedSection);

            if (SelectedGroup == null || group.Path.Count == 0)
            {
                //dcPathVisualisation.positionCount = 0;
            }
            else
            {
                int pathPosCount = group.PathPos.Count;
                int start = pathPosCount - Mathf.Max(group.PathPos.Count - group.NextSegmentIndex + 1, 2);
                Vector3 scale = pathDotPrefab.Transform.localScale;
                bool helper = group.PathPos[start].HelperPos.HasValue;
                var prevPos = group.PathPos[start].Pos;
                for (int i = start + 1; i < pathPosCount; i++)
                {
                    Vector3 nextPos;

                    bool wasHelper = helper;
                    if (helper)
                    {
                        i--;
                        nextPos = group.PathPos[i].HelperPos.Value;
                        helper = false;
                    }
                    else
                    {
                        nextPos = group.PathPos[i].Pos;
                        helper = group.PathPos[i].HelperPos.HasValue;
                    }
                    var diff = nextPos - prevPos;
                    float dist = diff.magnitude;
                    diff /= dist;
                    dist -= pathSectionSize;

                    var pathSection = GetPathSegment(dcSectionPool, usedSection, pathSectionPrefab);
                    pathSection.Transform.position = nextPos;
                    if (wasHelper)
                    {
                        pathSection.enabled = false;
                    }
                    else
                    {
                        pathSection.Renderer.sprite = pathSectionPrefab.Renderer.sprite;
                        pathSection.SetInnerTransform(180f + Mathf.Atan2(diff.y, diff.z) * (180f / Mathf.PI));
                    }

                    float currentSize = pathDotSize * scale.x;
                    int count = Mathf.RoundToInt((((int)(dist / (currentSize * (1f - maxResizePerc)))) + ((int)(dist / (currentSize * (1f + maxResizePerc))))) / 2f);
                    float newDotSize = (dist / count--);
                    float newDotScale = (newDotSize / pathDotSize) / (1f + spacePerc);
                    var dotPos = prevPos + diff * pathSectionSize / 2f;
                    diff *= newDotSize;
                    dotPos += diff;
                    for (int j = 0; j < count; j++)
                    {
                        var pathPart = GetPathSegment(dcPathPool, usedPath, pathDotPrefab);
                        pathPart.Transform.position = dotPos;
                        pathPart.Transform.localScale = new Vector3(newDotScale, newDotScale, newDotScale);

                        dotPos += diff;
                    }

                    prevPos = nextPos;
                }
                var last = usedSection[usedSection.Count - 1];
                last.Renderer.sprite = dcPathDestination;
                last.SetInnerTransform(0f);

                foreach (var segment in group.GetQueue())
                {
                    var queueSection = GetPathSegment(dcSectionPool, usedSection, pathSectionPrefab);
                    queueSection.Transform.position = segment.Center;
                    queueSection.Renderer.sprite = dcPathDestination;
                    last.SetInnerTransform(0f);
                }
            }
        }
    }

    public void UpdateRepairTicks(PercentageData data)
    {
        data.Max = Mathf.Round(data.TemplateMax * (1f - CrewManager.Instance.DepartmentDict[EDepartments.Deck].EfficiencyBonus / 100f));
    }

    public void SetEscortRepairSpeedModifier(float percent)
    {
        escortRepairSpeedModifier = percent;
    }

    public void SetNightShiftRepairSpeedModifier(float percent)
    {
        nightShiftRepairSpeedModifier = percent;
    }

    public void SetHastenedRepairsSpeedModifier(float percent)
    {
        hastenedRepairsSpeedModifier = percent;
    }

    public void FireDcCategoryClicked()
    {
        DcCategoryClicked();
    }

    public DCInstanceGroup SpawnDcInSegment(SectionSegment segment)
    {
        return SpawnDC(segment);
    }

    public ExtinguisherHandler GetExtinguisher(AnimationClip inClip)
    {
        var list = pool[inClip];
        if (list.Count == 0)
        {
            var obj = Instantiate(poolPrefabs[inClip]);
            obj.Init();
            obj.gameObject.SetActive(false);
            list.Add(obj);
        }
        int index = list.Count - 1;
        var result = list[index];
        result.gameObject.SetActive(true);
        list.RemoveAt(index);
        return result;
    }

    public void ReturnExtinguisher(ExtinguisherHandler extinguisher)
    {
        extinguisher.gameObject.SetActive(false);
        pool[extinguisher.Clip].Add(extinguisher);
    }

    //private void UpdateDebuff()
    //{
    //    var sectionMan = SectionRoomManager.Instance;
    //    float value = sectionMan.DCWorkshop.IsWorking && sectionMan.GeneratorsAreWorking ? 1f : .5f;
    //    foreach (var segment in sectionMan.GetAllSegments())
    //    {
    //        segment.Damage.RepairPower = value;
    //        segment.Fire.RepairPower = value;
    //        segment.Parent.Destruction.RepairPower = value;
    //        segment.Group.Flood.RepairPower = value;
    //    }
    //}

    void ShowRadialMenu()
    {

    }

    void HideRadialMenu(bool success)
    {

    }

    private void DCButtonCallback1(InputAction.CallbackContext _)
    {
        SetDCButton(true);
    }

    private void DCButtonCallback2(InputAction.CallbackContext _)
    {
        SetDCButton(false);
    }

    private void SetDCButton(bool fault)
    {
        var hudMan = HudManager.Instance;
        if (hudMan.IsSettingsOpened || GameStateManager.Instance.AlreadyShown || !hudMan.AcceptInput)
        {
            return;
        }
        int count = 0;
        switch (buttons)
        {
            case EDcButtons.Both:
                count = 1;
                break;
            case EDcButtons.Faults:
                if (fault)
                {
                    count = 2;
                }
                else
                {
                    return;
                }
                break;
            case EDcButtons.Floods:
                if (fault)
                {
                    return;
                }
                else
                {
                    count = 2;
                }
                break;
        }
        for (int i = 0; i < count; i++)
        {
            DcButtons button;
            if (fault)
            {
                button = faultsButtons[i];
            }
            else
            {
                button = buttons == EDcButtons.Both ? floodsButtons[i] : floodsButtons[floodsButtons.Count - i - 1];
            }

            bool found = false;
            foreach (var cooldown in cooldownButtons)
            {
                if (cooldown.DcButtons == button)
                {
                    found = true;
                    break;
                }
            }
            if (!found && button.Button.interactable)
            {
                OnDCButtonClicked(button);
                break;
            }
        }
    }

    private void SetAllDCCallbackFire(InputAction.CallbackContext _)
    {
        SetAllDC(EDcCategory.Fire);
    }

    private void SetAllDCCallbackFault(InputAction.CallbackContext _)
    {
        SetAllDC(EDcCategory.Crash);
    }

    private void SetAllDCCallbackWater(InputAction.CallbackContext _)
    {
        SetAllDC(EDcCategory.Water);
    }

    private void SetAllDCCallbackInjured(InputAction.CallbackContext _)
    {
        SetAllDC(EDcCategory.Injured);
    }

    private void SetAllDC(EDcCategory category)
    {
        var hudMan = HudManager.Instance;
        if (hudMan.IsSettingsOpened || GameStateManager.Instance.AlreadyShown || !hudMan.AcceptInput)
        {
            return;
        }

        DefaultCategory = category;
        foreach (var group in CurrentGroups)
        {
            group.Portrait.SetButton(category, false);
        }
    }

    private DCInstanceGroup CreateGroup(AnimationClip walk, Transform parent)
    {
        var dcGroup = new DCInstanceGroup(Instantiate(DcButtonPrefab, transform), Instantiate(dcPortrait, dcPanel));
        dcGroup.Button.Selected += OnDCSelected;
        dcGroup.Portrait.Selected += OnDCSelected;
        WorkerInstancesManager.Instance.CreateWorkers(dcGroup, EWorkerType.DC, 3, EWaypointTaskType.Normal, null, walk, parent);
        dcGroup.Setup();
        return dcGroup;
    }

    private void ChooseImage(DCInstanceGroup group)
    {
        if (portraits.Count == 0)
        {
            for (int i = 0; i < dcImages.Count; i++)
            {
                portraits.Add(i);
            }
        }

        int index = RandomUtils.GetRandom(portraits);
        group.Portrait.SetButton(group.Category, true);
        group.SetPortrait(index, dcImages);
        portraits.Remove(index);
    }

    private void OnViewChanged(ECameraView view)
    {
        foreach (var group in CurrentGroups)
        {
            group.Button.gameObject.SetActive(view == ECameraView.Sections);
        }
        if (SelectedGroup != null && view != ECameraView.Blend && view != ECameraView.Sections)
        {
            SelectedGroup.Button.SetSelected(false);
            SelectedGroup.Portrait.SetSelected(false);
            SelectedGroup = null;
        }
        SetShowWreckButton(view == ECameraView.Sections, EWreckType.Wreck);
        SetShowWreckButton(view == ECameraView.Sections, EWreckType.FrontKamikaze);
        SetShowWreckButton(view == ECameraView.Sections, EWreckType.EndKamikaze);

        foreach (var pair in cooldownButtons)
        {
            if (pair.DcButtons.IsFlood)
            {
                if (pair.SectionSegment.PumpingDCIcon != null)
                {
                    pair.SectionSegment.PumpingDCIcon.SetActive(view == ECameraView.Sections);
                }
            }
            else
            {
                if (pair.SectionSegment.RepairingDCIcon != null)
                {
                    pair.SectionSegment.RepairingDCIcon.SetActive(view == ECameraView.Sections);
                }
            }
        }
    }

    private void OnDCSelected(DCInstanceGroup group)
    {
        if (SelectedGroup != group)
        {
            if (SelectedGroup != null)
            {
                SelectedGroup.Button.SetSelected(false);
                SelectedGroup.Portrait.SetSelected(false);
                if (SelectedGroup.FinalSegment != null)
                {
                    SelectedGroup.FinalSegment.RemoveOutlineBlinking();
                    SectionRoomManager.Instance.PlayEvent(ESectionUIState.DCDeselect);
                }
            }

            if (freeGroups.Contains(group))
            {
                SelectedGroup = null;
                return;
            }

            SelectedGroup = group;
            if (SelectedGroup != null)
            {
                SelectedGroup.Button.SetSelected(true);
                SelectedGroup.Portrait.SetSelected(true);
                if (SelectedGroup.FinalSegment != SelectedGroup.CurrentSegment)
                {
                    SelectedGroup.FinalSegment.AddOutlineBlinking();
                }
                OnDCButtonClicked(null);

                var camMan = CameraManager.Instance;
                if (camMan.CurrentCameraView != ECameraView.Sections)
                {
                    camMan.SwitchMode(ECameraView.Sections);
                    camMan.ZoomToSectionSegment(SelectedGroup.CurrentSegment);
                }
            }
            SetShowPath(SelectedGroup);
        }
        if (SelectedGroup != null)
        {
            VoiceSoundsManager.Instance.PlaySelect(EVoiceType.DC);
        }
    }

    private List<Waypoint> SetupWreckWaypoints(WorkerPath path)
    {
        var result = new List<Waypoint>();
        foreach (var waypoint in path.AnimWaypoints)
        {
            if (waypoint.Data.PossibleTasks == EWaypointTaskType.Repair)
            {
                result.Add(waypoint);
            }
        }
        return result;
    }

    private void StopAllFloods()
    {
        foreach (var group in allGroups)
        {
            group.StopFlood();
        }
    }

    private void SetEnableVisuals(DCInstanceGroup group, bool enable)
    {
        group.Button.gameObject.SetActive(enable && CameraManager.Instance.CurrentCameraView == ECameraView.Sections);
        group.Portrait.gameObject.SetActive(enable);
    }

    private bool CheckDCPlacement(SectionRoom room)
    {
        foreach (var segment in room.GetAllSegments(false))
        {
            if (segment.Dc != null && segment.Dc.Path.Count == 0)
            {
                return true;
            }
        }
        return false;
    }

    private DCInstanceGroup SpawnDC(SectionSegment segment)
    {
        var group = RandomUtils.GetRandom(freeGroups);

        ChooseImage(group);

        CurrentGroups.Add(group);
        freeGroups.Remove(group);

        group.Show(segment);
        SetEnableVisuals(group, true);

        group.OnSpawned();
        OnDCChanged();
        return group;
    }

    private IEnumerable<SectionSegment> GetAllDamagableSegments()
    {
        foreach (var section in SectionRoomManager.Instance.GetAllSections())
        {
            foreach (var segment in section.GetAllSegments(false))
            {
                if (!segment.Untouchable)
                {
                    yield return segment;
                }
            }
        }
    }

    private void UpdateDcButtons()
    {
        foreach (var button in faultsButtons)
        {
            button.Button.interactable = !IsButtonOnCD(button) && MaintenanceActive;
        }
        foreach (var button in floodsButtons)
        {
            button.Button.interactable = !IsButtonOnCD(button) && PumpsActive;
        }
    }

    private bool IsButtonOnCD(DcButtons button)
    {
        foreach (var btn in cooldownButtons)
        {
            if (btn.DcButtons == button)
            {
                return true;
            }
        }
        return false;
    }

    private FaceToSectionCamera GetPathSegment(HashSet<FaceToSectionCamera> pool, List<FaceToSectionCamera> used, FaceToSectionCamera prefab)
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

    private void ReturnToPool(HashSet<FaceToSectionCamera> pool, List<FaceToSectionCamera> used)
    {
        foreach (var part in used)
        {
            part.enabled = false;
            pool.Add(part);
        }
        used.Clear();
    }

    private void AddAutoDc(DCInstanceGroup dc, bool crashOnly, ref int foundDc)
    {
        if (dc.Category == EDcCategory.Crash)
        {
            if (!dc.CrashOnly)
            {
                categorizedGroups[EDcCategory.Destroyed].Add(dc);
                categorizedGroups[EDcCategory.Fault].Add(dc);
                if (crashOnly)
                {
                    return;
                }
            }
        }
        categorizedGroups[dc.Category].Add(dc);
        foundDc |= 1 << (int)dc.Category;
    }

    private void AutoCheckPath(DCInstanceGroup group, HashSet<SectionSegment> segments)
    {
        Assert.IsTrue(group.Path.Count == 0);
        Assert.IsTrue(group.CurrentSegment == group.FinalSegment);
        if (group.CurrentSegment.DcCanEnter())
        {
            Debug.LogError("Dont break the game");
        }
        Assert.IsFalse(group.CurrentSegment.DcCanEnter());
        int segmentsPaths = 0;
        autoToVisit.Clear();
        autoToVisit.Add(new PathCheckData(group.CurrentSegment));
        alreadyVisited.Clear();
        alreadyVisited.Add(group.CurrentSegment);
        if (segments.Contains(group.CurrentSegment) && ++segmentsPaths == segments.Count)
        {
            return;
        }
        while (autoToVisit.Count > 0)
        {
            var path = autoToVisit[0];
            autoToVisit.RemoveAt(0);
            foreach (var key in path.CurrentSegment.NeighboursDirectionDictionary.Keys)
            {
                if (alreadyVisited.Add(key))
                {
                    if (segments.Contains(key))
                    {
                        if (!paths.TryGetValue(path.PathLength, out var list))
                        {
                            list = new List<KeyValuePair<DCInstanceGroup, SectionSegment>>();
                            paths.Add(path.PathLength, list);
                        }
                        list.Add(new KeyValuePair<DCInstanceGroup, SectionSegment>(group, key));
                        if (++segmentsPaths == segments.Count)
                        {
                            return;
                        }
                    }
                    autoToVisit.Add(new PathCheckData(key, path.PathLength + 1));
                }
            }
        }
        Debug.LogError(group.CurrentSegment, group.CurrentSegment);
        foreach (var segment in segments)
        {
            Debug.LogError(segment, segment);
        }
        Assert.IsTrue(false);
    }

    private void AutoSetPath(HashSet<DCInstanceGroup> groups, HashSet<SectionSegment> segments)
    {
        foreach (var list in paths.Values)
        {
            foreach (var value in list)
            {
                if (groups.Contains(value.Key) && segments.Remove(value.Value))
                {
                    value.Key.SetPath(value.Value, EWaypointTaskType.Normal, false, true);
                    groups.Remove(value.Key);
                    if (value.Key.Category == EDcCategory.Crash)
                    {
                        categorizedGroups[EDcCategory.Destroyed].Remove(value.Key);
                    }
                    if (value.Key.Category == EDcCategory.Crash || value.Key.Category == EDcCategory.Destroyed)
                    {
                        categorizedGroups[EDcCategory.Fault].Remove(value.Key);
                    }
                    if (groups.Count == 0 || segments.Count == 0)
                    {
                        return;
                    }
                }
            }
        }
        Debug.LogError(groups.Count);
        foreach (var segment in segments)
        {
            Debug.LogError(segment.name, segment);
        }
        Assert.IsTrue(false);
    }

    private void SetDcToSpecialSection(HashSet<DCInstanceGroup> groups, List<SectionRoom> sections)
    {
        foreach (var group in groups)
        {
            if (sections.Contains(group.FinalSegment.Parent.ParentSection))
            {
                return;
            }
        }

        foreach (var group in groups)
        {
            if (!group.CanAssign(false))
            {
                continue;
            }

            workingSegmentsSpecialSection.Clear();
            foreach (var section in sections)
            {
                if (!section.IsWorking)
                {
                    continue;
                }
                foreach (var segment in section.GetAllSegments(false))
                {
                    if (!segment.IsFlooded())
                    {
                        workingSegmentsSpecialSection.Add(segment);
                    }
                }
            }

            if (workingSegmentsSpecialSection.Count > 0)
            {
                group.SetPath(RandomUtils.GetRandom(workingSegmentsSpecialSection), EWaypointTaskType.Normal, false, true);
            }
            break;
        }
    }

    private void OnWorldMapToggled(bool state)
    {
        onWorldMap = state;
        if (state)
        {
            StopAllFloods();
        }
    }

    private void CheckDCRooms(HashSet<DCInstanceGroup> groups, List<SectionRoom> rooms)
    {
        DCInstanceGroup group = null;
        foreach (var group2 in groups)
        {
            if (group2.FinalSegment.Parent.IsWorking && rooms.Contains(group2.FinalSegment.Parent.ParentSection))
            {
                group = group2;
                break;
            }
        }
        if (group == null)
        {
            paths.Clear();
            backup.Clear();
            foreach (var room in rooms)
            {
                if (!room.IsWorking)
                {
                    continue;
                }
                foreach (var subroom in room.SubsectionRooms)
                {
                    if (!subroom.IsWorking)
                    {
                        continue;
                    }
                    foreach (var segment in subroom.Segments)
                    {
                        backup.Add(segment);
                    }
                }
            }
            if (backup.Count == 0)
            {
                return;
            }
            foreach (var group2 in groups)
            {
                AutoCheckPath(group2, backup);
            }
            foreach (var path in paths)
            {
                if (path.Value.Count > 0)
                {
                    group = path.Value[0].Key;
                    group.SetPath(path.Value[0].Value, EWaypointTaskType.Normal, false, true);
                    break;
                }
            }
            if (group == null)
            {
                return;
            }
        }

        groups.Remove(group);
        if (rooms == maintenanceRooms)
        {
            categorizedGroups[EDcCategory.Crash].Remove(group);
            categorizedGroups[EDcCategory.Destroyed].Remove(group);
        }
    }
}
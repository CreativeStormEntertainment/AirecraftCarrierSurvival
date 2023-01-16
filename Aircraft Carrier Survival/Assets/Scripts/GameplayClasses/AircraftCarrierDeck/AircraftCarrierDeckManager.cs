using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityRandom = UnityEngine.Random;

public class AircraftCarrierDeckManager : MonoBehaviour, ITickable, IEnableable
{
    public event Action PlaneCountChanged = delegate { };

    public event Action DeckSquadronsCountChanged = delegate { };

    public event Action DeckModeChanged = delegate { };

    public event Action WreckChanged = delegate { };

    public event Action BlockOrdersChanged = delegate { };

    public event Action<bool, EPlaneType> RepairPlaneChanged = delegate { };

    public event Action<EPlaneType> SquadronCreated = delegate { };

    public static AircraftCarrierDeckManager Instance;

    public bool HasDamage => HasWreck || HasKamikazeFront || HasKamikazeEnd;

    public bool HasWreck
    {
        get => hasWreck;
        set
        {
            if (HasWreck != value)
            {
                hasWreck = value;
                if (!HasDamage && EventManager.Instance != null)
                {
                    EventManager.Instance.RemoveWreck();
                }
            }
            WreckChanged();
        }
    }

    public bool HasKamikazeFront
    {
        get => hasKamikazeFront;
        set
        {
            if (HasKamikazeFront != value)
            {
                hasKamikazeFront = value;
                if (value)
                {
                    //wreckSound.Play();
                }
                else if (!HasDamage)
                {
                    EventManager.Instance.RemoveWreck();
                }
            }
            WreckChanged();
        }
    }

    public bool HasKamikazeEnd
    {
        get => hasKamikazeEnd;
        set
        {
            if (HasKamikazeEnd != value)
            {
                hasKamikazeEnd = value;
                if (value)
                {
                    //wreckSound.Play();
                }
                else if (!HasDamage)
                {
                    EventManager.Instance.RemoveWreck();
                }
            }
            WreckChanged();
        }
    }

    public EDeckMode DeckMode
    {
        get;
        private set;
    }

    public bool IsRunwayDamaged
    {
        get;
        set;
    }

    public ACOrder CurrentOrder
    {
        get;
        private set;
    }

    public int BomberLv
    {
        get;
        private set;
    }

    public int FighterLv
    {
        get;
        private set;
    }

    public int TorpedoLv
    {
        get;
        private set;
    }

    public bool IgnoreBadWeather
    {
        get;
        set;
    }

    public int RepairTimer
    {
        get;
        private set;
    }

    public int FreeSquadrons
    {
        get;
        set;
    }

    public EMissionOrderType FreeMission
    {
        get;
        set;
    } = EMissionOrderType.None;

    public bool EscortRetrievingSquadrons
    {
        get;
        set;
    }

    public bool BlockCrashes
    {
        get;
        set;
    }

    public bool BlockOrders
    {
        get;
        set;
    }

    public TacticalMapShip MapShip => mapShip;

    public float PlaneSpeed => planeSpeed;

    public int MaxSlots => baseMaxSlots + CrewManager.Instance.DepartmentDict[EDepartments.Air].UnitsCount;
    public int MaxSlotsLimit => baseMaxSlots + CrewManager.Instance.DepartmentDict[EDepartments.Air].MaxUnitsCount;
    public int MaxAllSquadronsCount => maxAllSquadronsCount;

    public StudioEventEmitter DeckStateChangeSound => deckStateChangeSound;

    public int FinishOrderTime => finishOrderTime;

    [NonSerialized]
    public List<ACOrder> OrderQueue = new List<ACOrder>();

    [NonSerialized]
    public List<PlaneSquadron> DeckSquadrons;

    [SerializeField]
    private float planeSpeed = 8f;

    [SerializeField]
    private int startingBomberCount = 10;
    [SerializeField]
    private int startingFighterCount = 10;
    [SerializeField]
    private int startingTorpedoCount = 10;
    [SerializeField]
    private int maxAllSquadronsCount = 10;

    [SerializeField]
    private int maxOrderCount = 5;

    [SerializeField]
    private int baseMaxSlots = 1;

    [SerializeField]
    private BasketRandom randomWreckChance = null;

    [SerializeField]
    private int normalRepairTime = 10;

    [SerializeField]
    private int worldMapRepairTime = 2;

    [SerializeField]
    private TacticalMapShip mapShip = null;

    [SerializeField]
    private StudioEventEmitter deckStateChangeSound = null;

    [SerializeField]
    private float disabledHangarSquadronDamagedChanceModifier = 1.25f;
    [SerializeField]
    private BasketRandom randomSquadronDamage = null;

    [SerializeField]
    private List<HangarCapacityData> hangarUpgradesData = null;

    [SerializeField]
    private List<Lift> lifts = null;
    private int bonusMaxSquadrons;

    private Dictionary<EPlaneType, PlaneData> planeDict;
    private Dictionary<EPlaneType, int> planeDeckDict;
    private Dictionary<EPlaneType, int> planeMissionDict;

    private bool isRepairing;
    private EPlaneType currentRepairType;
    private EPlaneType focusedType;
    private HashSet<EPlaneType> toRepair;

    private int currentOrderTimer;
    private int finishOrderTime;

    private int repairTimeTemplate;

    private float elevatorState = 1f;
    private float elevatorTimer;
    private float elevatorBlendingValue;

    private bool hasWreck;
    private bool hasKamikazeFront;
    private bool hasKamikazeEnd;

    private int startDelay = 1;
    private int startTimer;
    private bool disabled;

    private List<int> maxPlanes;

    private bool kamikazeInProgress;
#if ALLOW_CHEATS
    private bool forceWreck;
#endif
    private List<float> liftDelay;

    private bool losePlanes;

    private void Awake()
    {
        Instance = this;

        DeckSquadrons = new List<PlaneSquadron>();

        DeckMode = EDeckMode.Starting;

        planeDict = new Dictionary<EPlaneType, PlaneData>
        {
            { EPlaneType.Bomber, new PlaneData(startingBomberCount)},
            { EPlaneType.Fighter, new PlaneData(startingFighterCount)},
            { EPlaneType.TorpedoBomber, new PlaneData(startingTorpedoCount)}
        };
        planeDeckDict = new Dictionary<EPlaneType, int>();
        planeMissionDict = new Dictionary<EPlaneType, int>();

        toRepair = new HashSet<EPlaneType>();

        randomWreckChance.Init();
        randomSquadronDamage.Init();

        maxPlanes = new List<int>() { -1, -1, -1 };

        repairTimeTemplate = normalRepairTime;

        liftDelay = new List<float> { -1f, -1f, -1f };
    }

    private void Start()
    {
        TimeManager.Instance.AddTickable(this);
        WorldMap.Instance.Toggled += OnWorldMapToggled;
        OnWorldMapToggled(false);

        foreach (var lift in lifts)
        {
            lift.Setup();
        }

        SectionRoomManager.Instance.GeneratorsStateChanged += OnGeneratorsStateChanged;
    }

    private void LateUpdate()
    {
        UpdateLifts();
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }
#endif

        var planes = BasicInput.Instance.PreparePlanes;
        planes.PrepareFighter.performed -= PreparePlaneCallbackFighter;
        planes.PrepareBomber.performed -= PreparePlaneCallbackBomber;
        planes.PrepareTorpedo.performed -= PreparePlaneCallbackTorpedo;
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
    }

    public void PreSetup()
    {
        var saveData = SaveManager.Instance.Data.IntermissionData;
        BomberLv = saveData.GetUpgrade(EPlaneType.Bomber);
        FighterLv = saveData.GetUpgrade(EPlaneType.Fighter);
        TorpedoLv = saveData.GetUpgrade(EPlaneType.TorpedoBomber);
    }

    public void Setup()
    {
        var saveMan = SaveManager.Instance;
        MapShip.Setup();

        var intermissionData = saveMan.Data.IntermissionData;
        for (int i = 0; i < 3; i++)
        {
            planeDict[(EPlaneType)i].Free = intermissionData.GetCurrent((EPlaneType)i);
        }
        int carrier = (int)saveMan.Data.SelectedAircraftCarrier;
        maxAllSquadronsCount = hangarUpgradesData[carrier].Capacity[BinUtils.ExtractData(saveMan.Data.IntermissionData.CarriersUpgrades, 3, carrier + ((int)ECarrierType.Count * 2))];

        DeckOrderPanelManager.Instance.UpdateOrders();
        FirePlaneCountChanged();

        var planes = BasicInput.Instance.PreparePlanes;
        planes.PrepareFighter.performed += PreparePlaneCallbackFighter;
        planes.PrepareBomber.performed += PreparePlaneCallbackBomber;
        planes.PrepareTorpedo.performed += PreparePlaneCallbackTorpedo;
    }

    public void LoadData(ref DeckSaveData data)
    {
        foreach (var planeSaveData in data.PlaneDatas)
        {
            planeDict[planeSaveData.Type].LoadData(planeSaveData);
        }
        randomSquadronDamage.LoadData(ref data.PlaneDamageRandom);
        randomWreckChance.LoadData(ref data.WreckRandom);

        ChangeDeckMode(data.LaunchingMode ? EDeckMode.Starting : EDeckMode.Landing, true);

        if (data.RepairTimeLeft > 0)
        {
            SetRepairType(false, false);
            RepairTimer = data.RepairTimeLeft;
            currentRepairType = data.CurrentRepairType;
        }

        elevatorState = data.ElevatorState;
        elevatorTimer = data.ElevatorTimer;

        var planeMovementMan = PlaneMovementManager.Instance;
        for (int i = 0; i < data.DeckSquadrons.Count; i++)
        {
            var squadron = CreateNewSquadron(data.DeckSquadrons[i], false);
            squadron.AnimationPlay = false;
            planeMovementMan.CreatePlanes(squadron, i, data.LaunchingMode);
        }

        if (data.HasWreck)
        {
            planeMovementMan.CreateWreck(data.WreckType, 0);
            planeMovementMan.CurrentWrecks[0].Load();
        }
        for (int i = 0; i < 2; i++)
        {
            if ((data.Kamikaze & 1 << i) != 0)
            {
                planeMovementMan.CreateKamikaze(i == 0);
                planeMovementMan.CurrentWrecks[i + 1].Load();
            }
        }

        if (data.KamikazeInProgress)
        {
            SetupKamikaze();
        }

        for (int i = 0; i < data.DeckQueue.Count; i++)
        {
            switch (data.DeckQueue[i].Type)
            {
                case EDeckOrderType.Mission:
                    OrderQueue.Add(new MissionOrder(this, data.DeckQueue[i], i == 0));
                    break;
                case EDeckOrderType.Landing:
                    OrderQueue.Add(new LandingOrder(this, data.DeckQueue[i], i == 0));
                    break;
                case EDeckOrderType.ToLaunching:
                    OrderQueue.Add(new ChangeToLaunchingOrder(this, i == 0));
                    break;
                case EDeckOrderType.ToRecovering:
                    OrderQueue.Add(new ChangeToRecoveringOrder(this, i == 0));
                    break;
                case EDeckOrderType.SquadronCreation:
                    OrderQueue.Add(new SquadronCreationOrder(this, data.DeckQueue[i], i == 0));
                    break;
                case EDeckOrderType.SendToHangar:
                    OrderQueue.Add(new SendToHangarOrder(this, data.DeckQueue[i], i == 0));
                    break;
                case EDeckOrderType.Swap:
                    OrderQueue.Add(new SwapOrder(this, data.DeckQueue[i], i == 0));
                    break;
                case EDeckOrderType.SwapToFront:
                    OrderQueue.Add(new SwapToFrontOrder(this, data.DeckQueue[i], i == 0));
                    break;
            }
        }
        int orderCount = OrderQueue.Count;
        ACOrder order = null;
        if (orderCount != 0)
        {
            order = OrderQueue[0];
            CurrentOrder = order;
            finishOrderTime = CurrentOrder.Timer;
            if (data.FirstOrderDelay > 0)
            {
                startTimer = data.FirstOrderDelay;
                finishOrderTime += startTimer;
            }
        }

        planeMovementMan.LoadData(ref data.PlaneMovement);
        Assert.IsTrue(orderCount == OrderQueue.Count);
        Assert.IsTrue(order == null || order == OrderQueue[0]);

        FirePlaneCountChanged();

        if (OrderQueue.Count == 0)
        {
            StartNextOrder();
            DeckOrderPanelManager.Instance.SwitchHighlights(false);
        }
    }

    public void SaveData(ref DeckSaveData data)
    {
        data.PlaneDatas.Clear();
        foreach (var pair in planeDict)
        {
            var planeSaveData = pair.Value.SaveData();
            planeSaveData.Type = pair.Key;
            data.PlaneDatas.Add(planeSaveData);
        }
        randomSquadronDamage.SaveData(ref data.PlaneDamageRandom);
        randomWreckChance.SaveData(ref data.WreckRandom);

        data.LaunchingMode = DeckMode == EDeckMode.Starting;

        if (isRepairing)
        {
            data.RepairTimeLeft = RepairTimer;
            data.CurrentRepairType = currentRepairType;
        }
        else
        {
            data.RepairTimeLeft = -1;
        }

        data.ElevatorState = elevatorState;
        data.ElevatorTimer = elevatorTimer;

        data.DeckSquadrons.Clear();
        foreach (var squadron in DeckSquadrons)
        {
            data.DeckSquadrons.Add(squadron.PlaneType);
        }

        var planeMovementMan = PlaneMovementManager.Instance;
        planeMovementMan.SaveData(ref data.PlaneMovement);

        if (HasWreck)
        {
            data.HasWreck = true;
            data.WreckType = planeMovementMan.GetWreckType();
        }
        else
        {
            data.HasWreck = false;
        }

        if (HasKamikazeFront)
        {
            data.Kamikaze = 1 << 0;
        }
        if (HasKamikazeEnd)
        {
            data.Kamikaze = 1 << 1;
        }

        data.FirstOrderDelay = startTimer;
        data.DeckQueue.Clear();
        var orderSaveData = new DeckOrderSaveData();
        for (int i = 0; i < OrderQueue.Count; i++)
        {
            orderSaveData.Type = (EDeckOrderType)OrderQueue[i].OrderType;
            if (orderSaveData.Type == EDeckOrderType.Swap && !(OrderQueue[i] is SwapOrder))
            {
                orderSaveData.Type = EDeckOrderType.SwapToFront;
            }
            orderSaveData.Params = new List<int>();
            orderSaveData.Squadrons = new List<int>();
            OrderQueue[i].SaveData(ref orderSaveData, i == 0);
            data.DeckQueue.Add(orderSaveData);
        }

        data.KamikazeInProgress = kamikazeInProgress;
    }

    public void Tick()
    {
        RepairTick();
        OrderTick();
        mapShip.Tick();
    }

    public void FirePlaneCountChanged()
    {
        for (int i = 0; i < 3; i++)
        {
            planeDeckDict[(EPlaneType)i] = GetSquadronCountOfType((EPlaneType)i);
        }

        planeMissionDict.Clear();
        planeMissionDict[EPlaneType.Bomber] = 0;
        planeMissionDict[EPlaneType.Fighter] = 0;
        planeMissionDict[EPlaneType.TorpedoBomber] = 0;
        foreach (var missionList in TacticManager.Instance.Missions.Values)
        {
            foreach (var mission in missionList)
            {
                if (mission.InProgress)
                {
                    foreach (var squadron in mission.SentSquadrons)
                    {
                        planeMissionDict[squadron.PlaneType]++;
                    }
                    foreach (var s in mission.SentSquadronsLeft)
                    {
                        planeMissionDict[s.PlaneType]++;
                    }
                }
            }
        }
        if (CurrentOrder is LandingOrder landing && landing.Mission.SentSquadrons.Count > 0)
        {
            planeMissionDict[landing.Mission.SentSquadrons[0].PlaneType]--;
        }

        PlaneCountChanged();
    }

    public void ForceGoToHangar()
    {
        var order = new ForcedDecreaseSquadronsOrder(this, DeckSquadrons[DeckSquadrons.Count - 1]);
        if (OrderQueue.Count == 0)
        {
            AddOrder(order);
        }
        else
        {
            OrderQueue.Insert(0, order);
        }
    }

    public PlaneData GetPlaneData(EPlaneType type)
    {
        return planeDict[type];
    }

    public int GetAllSquadronsCount()
    {
        return GetTotalSquadronCount(EPlaneType.Bomber) + GetTotalSquadronCount(EPlaneType.Fighter) + GetTotalSquadronCount(EPlaneType.TorpedoBomber);
    }

    public int GetFreeSquadronCount(EPlaneType type)
    {
        return planeDict[type].Free;
    }

    public int GetDeckSquadronCount(EPlaneType type)
    {
        return planeDeckDict[type];
    }

    public int GetMissionSquadronCount(EPlaneType type)
    {
        return planeMissionDict[type];
    }

    public int GetAllSquadronsCount(EPlaneType type)
    {

        int deckCount = GetDeckSquadronCount(type);
        var data = GetPlaneData(type);
        int missionCount = GetMissionSquadronCount(type);
        int totalCount = deckCount + data.Free + data.Damaged + missionCount;
        return totalCount;
    }

    public int GetAvailableSquadronCount(EPlaneType type)
    {
        return GetFreeSquadronCount(type) + GetDeckSquadronCount(type);
    }

    public int GetBrokenSquadronCount(EPlaneType type)
    {
        return planeDict[type].Damaged;
    }

    public int GetTotalSquadronCount(EPlaneType type)
    {
        return GetAvailableSquadronCount(type) + GetBrokenSquadronCount(type);
    }

    public PlaneSquadron CreateNewSquadron(EPlaneType planeType, bool cost)
    {
        if (cost)
        {
            if (!CanGetSquadron(planeType))
            {
                return null;
            }
            --planeDict[planeType].Free;
        }
        var squadron = new PlaneSquadron(planeType);
        DeckSquadrons.Add(squadron);
        FireDeckSquadronsCountChanged();
        SquadronCreated(planeType);
        return squadron;
    }

    public bool IsMaxSlotsReached
    {
        get
        {
            return DeckSquadrons.Count >= MaxSlots;
        }
    }

    public int SquadronsCount()
    {
        int sqInQueue = 0;
        foreach (var t in OrderQueue)
        {
            if (t.OrderType == EOrderType.SquadronCreation)
            {
                sqInQueue++;
            }
        }

        int sqInDeck = DeckSquadrons.Count;
        foreach (var t in DeckSquadrons)
        {
            if (t.AnimationPlay)
            {
                sqInDeck--;
                break;
            }
        }

        return sqInQueue + sqInDeck;
    }

    public bool CanGetSquadron(EPlaneType planeType)
    {
        return !IsMaxSlotsReached && planeDict[planeType].Free > 0;
    }

    public void ChangeDeckMode(EDeckMode mode, bool force)
    {
        if (DeckMode == mode)
        {
            return;
        }

        var dpMan = DragPlanesManager.Instance;
        var toSpot = DeckMode == EDeckMode.Landing ? dpMan.PlanesLaunchingSpots : dpMan.PlanesRecoverySpots;
        var fromSpot = DeckMode == EDeckMode.Starting ? dpMan.PlanesLaunchingSpots : dpMan.PlanesRecoverySpots;
        for (int i = 0; i < toSpot.Count; i++)
        {
            toSpot[i].CopyHollow(fromSpot[i].Hollows);
        }

        DeckMode = mode;

        bool starting = mode == EDeckMode.Starting;
        if (starting || mode == EDeckMode.Landing)
        {
            CameraManager.Instance.SetDeckView(starting, force);
            DeckSquadrons.Reverse();
        }
        DeckModeChanged();
    }

    public void AddBomberSquadronCreationOrder()
    {
        AddOrder(new SquadronCreationOrder(this, EPlaneType.Bomber));
    }

    public void AddFighterSquadronCreationOrder()
    {
        AddOrder(new SquadronCreationOrder(this, EPlaneType.Fighter));
    }

    public void AddTorpedoBomberSquadronCreationOrder()
    {
        AddOrder(new SquadronCreationOrder(this, EPlaneType.TorpedoBomber));
    }

    public void AddSquadronLandingOrder(TacticalMission mission)
    {
        if (mission.MissionStage == EMissionStage.ReadyToRetrieve)
        {
            AddOrder(new LandingOrder(this, mission));
        }
    }

    public void SetOrderDelay(int delay)
    {
        startDelay = delay;
    }

    public void SetOrderTimer(int newTimer)
    {
        finishOrderTime = newTimer;
    }

    public void CancelOrder(int index)
    {
        if (index != 0)
        {
            OrderQueue.RemoveAt(index);
        }
    }

    public void AddOrder(ACOrder order)
    {
        if (BlockOrders)
        {
            return;
        }
        if (OrderQueue.Count >= maxOrderCount)
        {
            EventManager.Instance.OrderQueueFullPopup();
            return;
        }
        OrderQueue.Add(order);
        if (OrderQueue.Count == 1)
        {
            StartNextOrder();
        }
        DeckOrderPanelManager.Instance.SwitchHighlights(false);
        DeckOrderPanelManager.Instance.UpdateOrders();
    }

    public void RemoveDuplicateSwapOrders(PlaneSquadron withSquadron)
    {
        for (int i = 1; i < OrderQueue.Count; i++)
        {
            if (OrderQueue[i] is SwapToFrontOrder order && order.SquadronA == withSquadron)
            {
                CancelOrder(i--);
            }
        }
    }

    public void SendSquadronToHangar(PlaneSquadron squadron)
    {
        var data = planeDict[squadron.PlaneType];
        if (squadron.IsDamaged)
        {
            squadron.IsDamaged = false;
            data.Damaged++;
        }
        else
        {
            data.Free++;
        }
        DeckSquadrons.Remove(squadron);
        FireDeckSquadronsCountChanged();
        FirePlaneCountChanged();
    }

    public void MissionOrder(TacticalMission mission)
    {
        AddOrder(new MissionOrder(this, mission));
    }

    public List<PlaneSquadron> GetMissionSquadronList(TacticalMission mission)
    {
        if (mission.GetPlanesReadyToLaunch(out int bombers, out int fighters, out int torpedoes) &&
            HasEnough(bombers, fighters, torpedoes, out _, out _, out _))
        {
            var list = new List<PlaneSquadron>();
            AddSquadronsToMission(EPlaneType.Bomber, bombers, list);
            AddSquadronsToMission(EPlaneType.Fighter, fighters, list);
            AddSquadronsToMission(EPlaneType.TorpedoBomber, torpedoes, list);
            list.Sort((x, y) => Comparer<int>.Default.Compare(DeckSquadrons.IndexOf(y), DeckSquadrons.IndexOf(x)));
            Assert.IsTrue((bombers + fighters + torpedoes) == list.Count);
            return list;
        }
        return null;
    }

    public bool ShouldWreck()
    {
#if ALLOW_CHEATS
        if (forceWreck)
        {
            return true;
        }
#endif
        return !disabled && !BlockCrashes && randomWreckChance.Check(1f, out _);
        //if (HudManager.Instance.HasNo(ETutorialMode.DisableWrecks))
        //{
        //    missions = (missions + 1) % perMissionCount;
        //    if (missions == 0)
        //    {
        //        wrecks = 0;
        //    }

        //    if ((wreckCount > wrecks) && UnityEngine.Random.value <= (((float)(wreckCount - wrecks)) / ((float)(perMissionCount - missions))))
        //    {
        //        wrecks++;
        //        return true;
        //    }
        //}
        //return false;
    }

    //public void SetBonusMaxSlots(int bonus)
    //{
    //    bonusMaxSlots = bonus;
    //}

    public void ReplenishSquadrons(int count, EPlaneType type)
    {
        var data = planeDict[type];
        int total = data.Free + data.Damaged;
        int maxTotal = maxAllSquadronsCount + bonusMaxSquadrons;
        if (total < maxTotal)
        {
            data.Free += Mathf.Min(total + count, maxTotal) - total;
        }
        FirePlaneCountChanged();
    }

    public void SetMaxSquadronsBonus(int bonus)
    {
        bonusMaxSquadrons = bonus;
    }

    public void SetRepairFocus(EPlaneType type)
    {
        focusedType = type;
        SetRepairType(false, true);
    }

    public void FireDeckSquadronsCountChanged()
    {
        for (int i = 0; i < 3; i++)
        {
            planeDeckDict[(EPlaneType)i] = GetSquadronCountOfType((EPlaneType)i);
        }

        DeckSquadronsCountChanged();
    }

    public void RepairRandom(int count)
    {
        var prevRepaired = currentRepairType;
        for (int i = 0; i < count; i++)
        {
            SetRepairType(true, false);
            if (!isRepairing)
            {
                break;
            }
            Repair(currentRepairType);
        }
        FirePlaneCountChanged();
        currentRepairType = prevRepaired;
        SetRepairType(false, true);
        RepairPlaneChanged(isRepairing, currentRepairType);
    }

    public void DestroyMissionOrder(TacticalMission mission)
    {
        var found = OrderQueue.Find(x => x.Mission == mission);
        if (found != CurrentOrder)
        {
            OrderQueue.Remove(found);
            DeckOrderPanelManager.Instance.UpdateOrders();
        }
    }

    public void CheckSquadronDamage(float power, SectionSegment culprit)
    {
        if (!disabled)
        {
            float mult = SectionRoomManager.Instance.Hangar.IsWorking ? 1f : disabledHangarSquadronDamagedChanceModifier;
            randomSquadronDamage.Check(power * mult, () => MakeRandomSquadronDamage(culprit));
        }
    }

    public void SetMaxPlanes(EManeuverSquadronType type, int max)
    {
        if (type == EManeuverSquadronType.Any)
        {
            for (int i = 0; i < maxPlanes.Count; i++)
            {
                maxPlanes[i] = max;
            }
        }
        else
        {
            maxPlanes[(int)type] = max;
        }
    }

    public bool CanAdd(EPlaneType type)
    {
        int max = maxPlanes[(int)type];
        if (max == -1)
        {
            return true;
        }
        int value = 0;
        foreach (var squadron in DeckSquadrons)
        {
            if (squadron.PlaneType == type)
            {
                value++;
            }
        }
        foreach (var order in OrderQueue)
        {
            if (CurrentOrder != order && order is SquadronCreationOrder creationOrder && creationOrder.PlaneType == type)
            {
                value++;
            }
        }
        return max > value;
    }

    public void SetupKamikaze()
    {
        kamikazeInProgress = true;
        if (OrderQueue.Count == 0)
        {
            StartNextOrder();
        }
    }

    public PlaneSquadron GetSquadron(int index)
    {
        return DeckSquadrons[index];
    }

    public int IndexOf(PlaneSquadron squadron)
    {
        int result = DeckSquadrons.IndexOf(squadron);
        Assert.IsFalse(result == -1);
        return result;
    }

    public void SetBlockOrders(bool block)
    {
        BlockOrders = block;
        BlockOrdersChanged();
        if (block)
        {
            while (OrderQueue.Count > 1)
            {
                CancelOrder(1);
            }
        }
        else
        {
            if (DeckSquadrons.Count > MaxSlots)
            {
                ForceGoToHangar();
            }
        }
    }

    public void SetLiftState(int liftIndex, float state, bool delayed)
    {
        var lift = lifts[liftIndex];
        if (lift.ElevatorState != state)
        {
            if (delayed)
            {
                liftDelay[liftIndex] = .5f;
            }
            else
            {
                liftDelay[liftIndex] = -1f;
                lift.ElevatorState = state;
            }
        }
    }

    public void SetConditionalLift(float state)
    {
        SetLiftState(DeckMode == EDeckMode.Starting ? 0 : 2, state, false);
    }

    public void UpdateLift(int lift)
    {
        float delta = planeSpeed * Time.deltaTime * PlaneMovementManager.Instance.Speedup;
        lifts[lift].UpdateLift(delta);
    }

    public void DestroySquadrons(int count)
    {
        int max = 0;
        foreach (var data in planeDict.Values)
        {
            max += data.Free + data.Damaged;
        }

        for (int i = 0; i < count; i++)
        {
            if (max <= 0)
            {
                return;
            }

            int index = UnityRandom.Range(0, max--);
            foreach (var data in planeDict.Values)
            {
                index -= data.Free;
                index -= data.Damaged;
                if (index <= 0)
                {
                    if (data.Free > 0)
                    {
                        data.Free--;
                    }
                    else
                    {
                        data.Damaged--;
                    }
                    break;
                }
            }
        }
    }

    public void LosePlanes()
    {
        losePlanes = true;
    }

    public void OnResetAddPlane(EPlaneType type)
    {
        planeDict[type].Free++;
    }

    public void RemoveMissionOrders()
    {
        if (OrderQueue.Count <= 0)
        {
            return;
        }
        for (int i = 1; i < OrderQueue.Count; i++)
        {
            if (OrderQueue[i] is MissionOrder || OrderQueue[i] is LandingOrder)
            {
                OrderQueue.RemoveAt(i);
                i--;
            }
        }
        if (CurrentOrder is MissionOrder || CurrentOrder is LandingOrder)
        {
            CurrentOrder.ForceCancel();
            var planeMovementMan = PlaneMovementManager.Instance;
            foreach (var squadron in CurrentOrder.GetSquadrons())
            {
                planeDict[squadron.PlaneType].Free++;
                planeMovementMan.FreePlanes(squadron);
            }
            foreach (var squadron in CurrentOrder.Mission.SentSquadrons)
            {
                planeDict[squadron.PlaneType].Free++;
                planeMovementMan.FreePlanes(squadron);
            }

            if (startTimer > 0)
            {
                startTimer = -1;
            }
            CurrentOrder = null;
            currentOrderTimer = 0;
            StartNextOrder();
        }
        FirePlaneCountChanged();
        DeckOrderPanelManager.Instance.UpdateOrders();
    }

#if ALLOW_CHEATS
    public void SetForceWreck()
    {
        forceWreck = !forceWreck;
        Debug.Log(forceWreck ? "Forcing wreck on every landing" : "Normal wrecks");
    }
#endif
    private void UpdateLifts()
    {
        for (int i = 0; i < liftDelay.Count; i++)
        {
            if (liftDelay[i] > 0f)
            {
                liftDelay[i] -= Time.deltaTime;
                if (liftDelay[i] < 0f)
                {
                    var lift = lifts[i];
                    lift.ElevatorState = lift.ElevatorState == 1f ? 0f : 1f;
                }
            }
        }

        float delta = planeSpeed * Time.deltaTime * PlaneMovementManager.Instance.Speedup;
        foreach (var lift in lifts)
        {
            lift.UpdateLift(delta);
        }
    }

    private void RepairTick()
    {
        SetRepairType(false, true);
        if (!isRepairing)
        {
            return;
        }

        int repairTime = Mathf.RoundToInt(repairTimeTemplate - CrewManager.Instance.DepartmentDict[EDepartments.Engineering].EfficiencyMinutes);
        RepairTimer--;
        while (RepairTimer <= 0)
        {
            Assert.IsTrue(repairTime > 0);
            RepairTimer += repairTime;
            Repair(currentRepairType);
            FirePlaneCountChanged();

            SetRepairType(false, false);
        }
        RepairPlaneChanged(isRepairing, currentRepairType);
    }

    private void OrderTick()
    {
        if (OrderQueue.Count == 0)
        {
            StartNextOrder();
            return;
        }

        Assert.IsTrue(OrderQueue.Count > 0);
        Assert.IsTrue(CurrentOrder == OrderQueue[0]);
        if (--startTimer == 0)
        {
            CurrentOrder.OnStart();
        }

        if (++currentOrderTimer >= finishOrderTime)
        {
            CurrentOrder.Execute();
            if (OrderQueue.Count > 0 && OrderQueue[0] == CurrentOrder)
            {
                OrderQueue.RemoveAt(0);
            }
            CurrentOrder = null;
            currentOrderTimer = 0;
            StartNextOrder();
            DeckOrderPanelManager.Instance.UpdateOrders();
        }
    }

    private void SetRepairType(bool random, bool retainType)
    {
        toRepair.Clear();
        foreach (var pair in planeDict)
        {
            if (pair.Value.Damaged > 0)
            {
                toRepair.Add(pair.Key);
            }
        }

        if (toRepair.Count == 0)
        {
            isRepairing = false;
            RepairTimer = repairTimeTemplate - CrewManager.Instance.DepartmentDict[EDepartments.Engineering].EfficiencyMinutes;
            return;
        }
        isRepairing = true;

        var randomType = RandomUtils.GetRandom(toRepair);
        retainType = retainType && toRepair.Contains(currentRepairType);
        bool canFocus = SectionRoomManager.Instance.Workshop.IsWorking && toRepair.Contains(focusedType);
        if (random || (!canFocus && !retainType))
        {
            currentRepairType = randomType;
        }
        else if (canFocus)
        {
            currentRepairType = focusedType;
        }
    }

    private void StartNextOrder()
    {
        if (kamikazeInProgress)
        {
            kamikazeInProgress = false;
            if (!HasKamikazeFront || !HasKamikazeEnd)
            {
                bool front = HasKamikazeEnd;
                if (!HasKamikazeFront && !HasKamikazeEnd)
                {
                    front = UnityRandom.value >= .5f;
                }
                PlaneMovementManager.Instance.CreateKamikaze(front);
            }
        }
        if (OrderQueue.Count > 0)
        {
            if (OrderQueue[0].CanBeDone())
            {
                if (DeckSquadrons.Count > MaxSlots && OrderQueue[0].OrderType != EOrderType.ForcedDecreaseSquadrons)
                {
                    ForceGoToHangar();
                }
                CurrentOrder = OrderQueue[0];
                finishOrderTime = CurrentOrder.Timer + startDelay;
                startTimer = startDelay;
            }
            else
            {
                OrderQueue.RemoveAt(0);
                StartNextOrder();
            }
        }
        else if (!BlockOrders && !IsRunwayDamaged && DeckSquadrons.Count > MaxSlots)
        {
            ForceGoToHangar();
        }
    }

    private bool HasEnough(int bombers, int fighters, int torpedoes, out int ourBombers, out int ourFighters, out int ourTorpedoes)
    {
        ourBombers = GetSquadronCountOfType(EPlaneType.Bomber);
        ourFighters = GetSquadronCountOfType(EPlaneType.Fighter);
        ourTorpedoes = GetSquadronCountOfType(EPlaneType.TorpedoBomber);
        return ourBombers >= bombers && ourFighters >= fighters && ourTorpedoes >= torpedoes;
    }

    private int GetSquadronCountOfType(EPlaneType type)
    {
        int result = 0;
        foreach (var squadron in DeckSquadrons)
        {
            if (squadron.PlaneType == type)
            {
                result++;
            }
        }
        return result;
    }

    private void AddSquadronsToMission(EPlaneType type, int count, List<PlaneSquadron> list)
    {
        for (int i = DeckSquadrons.Count; i > 0 && count > 0; i--)
        {
            var squadron = DeckSquadrons[i - 1];
            if (squadron.PlaneType == type)
            {
                list.Add(squadron);
                --count;
            }
        }
    }

    private void OnWorldMapToggled(bool state)
    {
        if (!state)
        {
            return;
        }

        foreach (var data in planeDict.Values)
        {
            data.Free += data.Damaged;
            data.Damaged = 0;
        }

        if (CurrentOrder != null)
        {
            CurrentOrder.ForceCancel();
        }
        startTimer = -1;
        CurrentOrder = null;
        OrderQueue.Clear();
        DeckOrderPanelManager.Instance.UpdateOrders();

        var planeMovementMan = PlaneMovementManager.Instance;
        foreach (var squadron in DeckSquadrons)
        {
            planeDict[squadron.PlaneType].Free++;
            planeMovementMan.FreePlanes(squadron);
        }
        DeckSquadrons.Clear();

        TacticManager.Instance.ResetAllMissions(losePlanes);
        losePlanes = false;
        planeMovementMan.ResetMovement();

        FireDeckSquadronsCountChanged();
        FirePlaneCountChanged();
        RepairPlaneChanged(false, EPlaneType.Bomber);
        RepairPlaneChanged(false, EPlaneType.Fighter);
        RepairPlaneChanged(false, EPlaneType.TorpedoBomber);
    }

    private void MakeRandomSquadronDamage(SectionSegment culprit)
    {
        List<PlaneData> list = new List<PlaneData>();
        foreach (var planes in planeDict.Values)
        {
            if (planes.Free > 0)
            {
                list.Add(planes);
            }
        }
        if (list.Count > 0)
        {
            var data = RandomUtils.GetRandom(list);
            data.Free--;
            data.Damaged++;
            EventManager.Instance.AddSquadronDamaged(culprit);
            FirePlaneCountChanged();
        }
    }

    private void Repair(EPlaneType type)
    {
        var data = planeDict[type];
        data.Damaged--;
        data.Free++;
    }

    private void OnGeneratorsStateChanged(bool active)
    {
        if (!active)
        {
            for (int i = 0; i < OrderQueue.Count; i++)
            {
                if (OrderQueue[i] is MissionOrder missionOrder &&
                    (missionOrder.Mission.OrderType == EMissionOrderType.CounterHostileScouts || missionOrder.Mission.OrderType == EMissionOrderType.SubmarineHunt))
                {
                    CancelOrder(i--);
                }
            }
        }
    }

    private void PreparePlaneCallbackFighter(InputAction.CallbackContext _)
    {
        PreparePlane(EPlaneType.Fighter);
    }

    private void PreparePlaneCallbackBomber(InputAction.CallbackContext _)
    {
        PreparePlane(EPlaneType.Bomber);
    }

    private void PreparePlaneCallbackTorpedo(InputAction.CallbackContext _)
    {
        PreparePlane(EPlaneType.TorpedoBomber);
    }

    private void PreparePlane(EPlaneType type)
    {
        var hudMan = HudManager.Instance;
        if (hudMan.IsSettingsOpened || GameStateManager.Instance.AlreadyShown || !hudMan.AcceptInput)
        {
            return;
        }

        var planesType = DragPlanesManager.Instance.SquadronTypeEnabled;
        if (GetFreeSquadronCount(type) == 0 || (planesType != EManeuverSquadronType.Any && ((int)planesType != (int)type)) || !CanAdd(type))
        {
            return;
        }
        AddOrder(new SquadronCreationOrder(this, type));
    }
}

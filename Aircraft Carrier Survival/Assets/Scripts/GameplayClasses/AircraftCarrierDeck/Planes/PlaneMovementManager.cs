using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PlaneMovementManager : MonoBehaviour, ITickable
{
    public event Action MovementFinished = delegate { };

    public static PlaneMovementManager Instance;

    public EPlaneDirection CurrentMovement => currentMovement;

    public int ParallelLifts => parallelLifts;

    public List<PlaneObjects> BomberPrefabs;
    public List<PlaneObjects> FighterPrefabs;
    public List<PlaneObjects> TorpedoPrefabs;

    public Dictionary<EPlaneType, PlaneMovement> PlanePrefabs;
    public Dictionary<EPlaneType, Transform> PlaneHollowPrefabs;

    public Transform RootNode;

    public int PlaneCount = 3;

    public float Speedup = 16f;
    public float AnimSpeedup = 1.9f;

    public List<float> ToHangarLineDelayLaunching;
    public List<float> ToHangarLineDelayRecovering;
    public List<float> SwitchModeFromLaunchLineDelay;
    public List<float> SwitchModeFromRecoveringLineDelay;
    public List<float> SwitchModeRowDelay;

    public List<Wreck> WreckPrefabs;

    [NonSerialized]
    public List<Wreck> CurrentWrecks;
    [SerializeField]
    private List<float> launchDelay = null;

    //[SerializeField]
    //private List<float> startingDelay = null;

    [SerializeField]
    private CoroutineHelper helper = null;

    //[SerializeField]
    //private List<EPlaneRotation> deckLaunchingRotationConstraints = null;

    //[SerializeField]
    //private List<EPlaneRotation> deckRecoveringRotationConstraints = null;

    [SerializeField]
    private List<Transform> wreckSpawnPositions = null;

    [SerializeField]
    private int parallelLifts = 1;

    [SerializeField]
    private bool swapHangarOnRecovery = false;

    [SerializeField]
    private List<float> swapLastLineDelay = new List<float>() { 0f, 0f, 0f };

    private PlaneNodesGenerator generatedNodes;

    private Dictionary<EPlaneType, HashSet<PlaneMovement>> freePlanes;
    private List<PlaneMovement> planes;
    private List<PlaneMovement> eventPlanes;

    private Transform trans;

    private HashSet<PlaneMovement> allPlanes;
    private HashSet<PlaneMovement> planesInDestination;
    private HashSet<PlaneSquadron> squadrons;
    private EPlaneDirection currentMovement;
    private EPlaneMoveStage currentStageInMovement;

    private List<PlaneSquadron> movingSquadrons;
    private List<PlaneSquadron> movedSquadrons;
    private int movingSquadronSlot;
    private int currentSquadronLane;
    private int lineA;
    private int lineB;
    private int waitingBLine;
    private bool lastLane;
    private bool lastLaneB;
    private List<PlaneSquadron> squadronsInState;

    private HashSet<PlaneMovement> registered;

    private bool inCancel;
    private bool animationActive;
    private int sentCount;
    private int sent;

    private HashSet<PlaneMovement> rotatingPlanes;

    private List<int> squadronsIndices;
    private int lastWreckIndex;
    private bool hadCrash;

    private HashSet<PlaneMovement> set;
    private bool inTickable;
    private bool ignoreTick;

    private PlaneNode hangarStarting;
    private PlaneNode hangarLanding;

    private void Awake()
    {
        Instance = this;

        set = new HashSet<PlaneMovement>();
        generatedNodes = new PlaneNodesGenerator(RootNode);
        PlanePrefabs = new Dictionary<EPlaneType, PlaneMovement>();
        PlaneHollowPrefabs = new Dictionary<EPlaneType, Transform>();
        freePlanes = new Dictionary<EPlaneType, HashSet<PlaneMovement>>();
        planes = new List<PlaneMovement>();
        eventPlanes = new List<PlaneMovement>();
        trans = transform;

        allPlanes = new HashSet<PlaneMovement>();
        planesInDestination = new HashSet<PlaneMovement>();
        squadrons = new HashSet<PlaneSquadron>();

        movingSquadrons = new List<PlaneSquadron>();
        movedSquadrons = new List<PlaneSquadron>();
        squadronsInState = new List<PlaneSquadron>();

        rotatingPlanes = new HashSet<PlaneMovement>();

        registered = new HashSet<PlaneMovement>();

        squadronsIndices = new List<int>();

        if (Speedup > 0f)
        {
            SetSpeed(Speedup);
        }
        else
        {
            Speedup = 1f;
        }

        if (AnimSpeedup < 0f)
        {
            AnimSpeedup = 1f;
        }
    }

    private void LateUpdate()
    {
        foreach (var plane in planes)
        {
            if (plane.MyUpdate(Time.deltaTime))
            {
                eventPlanes.Add(plane);
            }
        }

        foreach (var plane in eventPlanes)
        {
            plane.UpdateEvent();
        }
        eventPlanes.Clear();
    }

    public void Setup()
    {
        var deck = AircraftCarrierDeckManager.Instance;
        PlanePrefabs[EPlaneType.Bomber] = BomberPrefabs[deck.BomberLv].PlaneMovement;
        PlanePrefabs[EPlaneType.Fighter] = FighterPrefabs[deck.FighterLv].PlaneMovement;
        PlanePrefabs[EPlaneType.TorpedoBomber] = TorpedoPrefabs[deck.TorpedoLv].PlaneMovement;

        PlaneHollowPrefabs[EPlaneType.Bomber] = BomberPrefabs[deck.BomberLv].PlaneHollow;
        PlaneHollowPrefabs[EPlaneType.Fighter] = FighterPrefabs[deck.FighterLv].PlaneHollow;
        PlaneHollowPrefabs[EPlaneType.TorpedoBomber] = TorpedoPrefabs[deck.TorpedoLv].PlaneHollow;

        freePlanes[EPlaneType.Bomber] = new HashSet<PlaneMovement>();
        freePlanes[EPlaneType.Fighter] = new HashSet<PlaneMovement>();
        freePlanes[EPlaneType.TorpedoBomber] = new HashSet<PlaneMovement>();

        DragPlanesManager.Instance.SetupHollows();

        CurrentWrecks = new List<Wreck>();
        CurrentWrecks.Add(null);
        CurrentWrecks.Add(null);
        CurrentWrecks.Add(null);

        if (swapHangarOnRecovery)
        {
            hangarStarting = generatedNodes.GroupNodes[EPlaneNodeGroup.Hangar].LineNodes[0][1];
            hangarLanding = generatedNodes.GroupNodes[EPlaneNodeGroup.Hangar].LineNodes[0][3];
        }

        Assert.IsTrue(parallelLifts > 0 && parallelLifts < 4);
    }

    public void LoadData(ref PlaneMovementSaveData data)
    {
        if (data.Movement)
        {
            var deck = AircraftCarrierDeckManager.Instance;
            var group = generatedNodes.GroupNodes[deck.DeckMode == EDeckMode.Starting ? EPlaneNodeGroup.DeckLaunching : EPlaneNodeGroup.DeckRecovering].LineNodes;
            Assert.IsTrue(generatedNodes.GroupNodes[EPlaneNodeGroup.Landing].LineNodes.Count > 0, "a000");
            var landingNodes = generatedNodes.GroupNodes[EPlaneNodeGroup.Landing].LineNodes[0];
            Assert.IsTrue(landingNodes.Count > 0, "a001");
            var unloadedPlanes = new List<PlaneSquadron>();
            foreach (var planePath in data.Planes)
            {
                if (planePath.Squadron < 0)
                {
                    while (unloadedPlanes.Count < -planePath.Squadron)
                    {
                        unloadedPlanes.Add(new PlaneSquadron(EPlaneType.Bomber));
                    }
                    Assert.IsTrue(unloadedPlanes.Count > (-planePath.Squadron - 1), "a002");
                    var squadron = unloadedPlanes[-planePath.Squadron - 1];
                    squadron.PlaneType = planePath.Type;
                    if (squadron.Planes.Count == 0)
                    {
                        CreatePlanes(squadron, landingNodes, false, planePath.PlaneCount);
                    }
                    Assert.IsTrue(squadron.Planes.Count > planePath.Index, "a003");
                    squadron.Planes[planePath.Index].LoadData(planePath);
                }
                else
                {
                    Assert.IsTrue(deck.DeckSquadrons.Count > planePath.Squadron, "a004");
                    Assert.IsTrue(deck.DeckSquadrons[planePath.Squadron].Planes.Count > planePath.Index, "a005");
                    deck.DeckSquadrons[planePath.Squadron].Planes[planePath.Index].LoadData(planePath);
                }
            }
            Assert.IsTrue(unloadedPlanes.Count < 2, "a006");
            if (unloadedPlanes.Count > 0)
            {
                Assert.IsTrue(deck.OrderQueue.Count > 0, "a007");
                (deck.OrderQueue[0] as LandingOrder).SetSquadron(unloadedPlanes[0]);
            }
            var lateAnimList = new List<PlaneMovement>();
            if (data.FinishMovement)
            {
                if (data.MovementType == EPlaneDirection.DeckLaunchingToAirLaunching)
                {
                    foreach (var index in data.Params)
                    {
                        FreePlanes(deck.GetSquadron(index));
                    }
                }

                var rotations = deck.DeckMode == EDeckMode.Starting ? generatedNodes.DeckLaunchingRotations : generatedNodes.DeckRecoveringRotations;
                for (int i = 0; i < deck.DeckSquadrons.Count; i++)
                {
                    var planes = deck.DeckSquadrons[i].Planes;
                    for (int j = 0; j < planes.Count; j++)
                    {
                        var plane = planes[j];
                        bool snap = planes[j].SnapRotation;
                        plane.SnapRotation = true;
                        Assert.IsTrue(rotations.Count > i, "a008");
                        Assert.IsTrue(rotations[i].Count > j, "a009");
                        plane.Rotate(rotations[i][j]);
                        plane.LoadTeleport();
                        plane.SnapRotation = snap;
                    }
                }

                helper.StopCoroutine();
                helper.StartCoroutine(() => MovementFinished());
            }
            else
            {
                switch (data.MovementType)
                {
                    case EPlaneDirection.DeckLaunchingToAirLaunching:
                        var list = new List<PlaneSquadron>();
                        foreach (var index in data.Params)
                        {
                            list.Add(deck.GetSquadron(index));
                        }
                        Launch(list, deck.DeckSquadrons, deck.PlaneSpeed);
                        break;
                    case EPlaneDirection.DeckLaunchingToDeckRecovering:
                        BetweenLaunchingRecovering(deck.DeckSquadrons, EPlaneNodeGroup.DeckLaunching, deck.PlaneSpeed);
                        break;
                    case EPlaneDirection.DeckLaunchingToHangar:
                        Assert.IsTrue(data.Params.Count > 0, "a010");
                        ToHangar(deck.GetSquadron(data.Params[0]), deck.DeckSquadrons, EPlaneNodeGroup.DeckLaunching, deck.PlaneSpeed);
                        break;
                    case EPlaneDirection.DeckRecoveringToDeckLaunching:
                        BetweenLaunchingRecovering(deck.DeckSquadrons, EPlaneNodeGroup.DeckRecovering, deck.PlaneSpeed);
                        break;
                    case EPlaneDirection.DeckRecoveringToHangar:
                        Assert.IsTrue(data.Params.Count > 0, "a011");
                        ToHangar(deck.GetSquadron(data.Params[0]), deck.DeckSquadrons, EPlaneNodeGroup.DeckRecovering, deck.PlaneSpeed);
                        break;
                    case EPlaneDirection.LandingToDeckRecovering:
                        Assert.IsTrue(data.Params.Count > 0, "a012");
                        FromAirToRecovering(deck.GetSquadron(data.Params[0]), deck.PlaneSpeed, false, true);
                        break;
                    case EPlaneDirection.LandingToHangar:
                        Assert.IsTrue(data.Params.Count > 1, "a013");
                        FromAirToHangar(unloadedPlanes[0], deck.PlaneSpeed, data.Params[1] > 0, true);
                        break;
                    case EPlaneDirection.HangarToDeckLaunching:
                        Assert.IsTrue(data.Params.Count > 0, "a014");
                        FromHangar(deck.GetSquadron(data.Params[0]), EPlaneNodeGroup.DeckLaunching, deck.PlaneSpeed, true);
                        break;
                    case EPlaneDirection.HangarToDeckRecovering:
                        Assert.IsTrue(data.Params.Count > 0, "a015");
                        FromHangar(deck.GetSquadron(data.Params[0]), EPlaneNodeGroup.DeckRecovering, deck.PlaneSpeed, true);
                        break;
                    case EPlaneDirection.SwapLaunching:
                        Assert.IsTrue(data.Params.Count > 1, "a016");
                        Swap(deck.GetSquadron(data.Params[0]), deck.GetSquadron(data.Params[1]), deck.DeckSquadrons, EPlaneNodeGroup.DeckLaunching, deck.PlaneSpeed, out _, out _, out _);
                        break;
                    case EPlaneDirection.SwapRecovering:
                        Assert.IsTrue(data.Params.Count > 1, "a017");
                        Swap(deck.GetSquadron(data.Params[0]), deck.GetSquadron(data.Params[1]), deck.DeckSquadrons, EPlaneNodeGroup.DeckRecovering, deck.PlaneSpeed, out _, out _, out _);
                        break;
                    case EPlaneDirection.SwapFrontLaunching:
                        Assert.IsTrue(data.Params.Count > 0, "a018");
                        SwapToFront(deck.GetSquadron(data.Params[0]), deck.DeckSquadrons, EPlaneNodeGroup.DeckLaunching, deck.PlaneSpeed, out _);
                        break;
                    case EPlaneDirection.SwapFrontRecovering:
                        Assert.IsTrue(data.Params.Count > 0, "a019");
                        SwapToFront(deck.GetSquadron(data.Params[0]), deck.DeckSquadrons, EPlaneNodeGroup.DeckRecovering, deck.PlaneSpeed, out _);
                        break;
                }

                int loopBreak = 0;
                var animList = new List<PlaneMovement>();
                while (data.MovementStage != currentStageInMovement)
                {
                    Assert.IsFalse(loopBreak++ > 100_000, "a020");
                    foreach (var plane in planes)
                    {
                        var result = plane.LoadUpdate();
                        switch (result)
                        {
                            case EPlaneUpdateResult.Anim:
                                animList.Add(plane);
                                break;
                            case EPlaneUpdateResult.Arrived:
                            case EPlaneUpdateResult.ArrivedLoad:
                                eventPlanes.Add(plane);
                                break;
                            case EPlaneUpdateResult.AnimLoad:
                                Assert.IsTrue(false, "a021");
                                break;
                        }
                    }

                    foreach (var plane in eventPlanes)
                    {
                        plane.UpdateEvent();
                    }
                    eventPlanes.Clear();

                    foreach (var plane in animList)
                    {
                        plane.AnimCallback();
                    }
                    animList.Clear();
                }

                var allList = new List<PlaneMovement>();
                loopBreak = 0;
                while (true)
                {
                    Assert.IsFalse(loopBreak++ > 100_000, "a022");
                    foreach (var plane in planes)
                    {
                        var result = plane.LoadUpdate();

                        switch (result)
                        {
                            case EPlaneUpdateResult.Arrived:
                                eventPlanes.Add(plane);
                                break;
                            case EPlaneUpdateResult.Anim:
                                animList.Add(plane);
                                break;
                            case EPlaneUpdateResult.AnimLoad:
                                allList.Add(plane);
                                lateAnimList.Add(plane);
                                break;
                            default:
                                allList.Add(plane);
                                break;
                        }
                    }

                    foreach (var plane in eventPlanes)
                    {
                        plane.UpdateEvent();
                    }
                    eventPlanes.Clear();

                    foreach (var plane in animList)
                    {
                        plane.AnimCallback();
                    }

                    if (allList.Count == planes.Count)
                    {
                        break;
                    }
                    animList.Clear();
                    allList.Clear();
                    lateAnimList.Clear();
                }
                Assert.IsFalse(planes.Count == 0, "a023");
                foreach (var planePath in data.Planes)
                {
                    if (planePath.Squadron < 0)
                    {
                        Assert.IsTrue(unloadedPlanes.Count > (-planePath.Squadron - 1), "a024");
                        unloadedPlanes[-planePath.Squadron - 1].Planes[planePath.Index].LoadTeleport();
                    }
                    else
                    {
                        Assert.IsTrue(deck.DeckSquadrons.Count > planePath.Squadron, "a025");
                        Assert.IsTrue(deck.DeckSquadrons[planePath.Squadron].Planes.Count > planePath.Index, "a026");
                        deck.DeckSquadrons[planePath.Squadron].Planes[planePath.Index].LoadTeleport();
                    }
                }
                foreach (var plane in lateAnimList)
                {
                    plane.AnimCallback();
                }
            }
            PlaneCrewMovementManager.Instance.Load();
            foreach (var plane in planes)
            {
                if (plane.IsRotating() && plane.Crew != null && plane.Crew.Count > 0 && plane.Crew[0].CrewPathState == PlaneCrewMovement.EPlaneCrewPathState.AT_SIDE)
                {
                    plane.SnapRotate();
                }
            }
        }
    }

    public void SaveData(ref PlaneMovementSaveData data)
    {
        if (allPlanes.Count == 0)
        {
            SaveNoMovement(ref data);
        }
        else
        {
            data.Movement = true;

            data.MovementType = currentMovement;
            data.MovementStage = currentStageInMovement;
            data.FinishMovement = rotatingPlanes.Count > 0;

            SaveActionParams(ref data);
            var deck = AircraftCarrierDeckManager.Instance;

            data.Planes.Clear();
            var set = new HashSet<PlaneSquadron>();
            SaveDeckSquadronPlanes(ref data, deck.DeckSquadrons, set);
            SaveOtherSquadronPlanes(ref data, set);
        }
    }

    public int GetWreckType()
    {
        return lastWreckIndex;
    }

    public void SetSpeed(float speed)
    {
        foreach (var plane in planes)
        {
            plane.SpeedMultiplier = speed;
        }
    }

    public void CreatePlanes(PlaneSquadron squadron, int squadronIndex, bool launching)
    {
        CreatePlanes(squadron, generatedNodes.GroupNodes[(launching ? EPlaneNodeGroup.DeckLaunching : EPlaneNodeGroup.DeckRecovering)].LineNodes[squadronIndex], true, PlaneCount, squadronIndex, launching);
    }

    public void CreatePlanes(PlaneSquadron squadron, List<PlaneNode> positions, bool spawnedOnStart)
    {
        CreatePlanes(squadron, positions, spawnedOnStart, PlaneCount);
    }

    public void CreatePlanes(PlaneSquadron squadron, List<PlaneNode> positions, bool spawnedOnStart, int planeCount, int squadronIndex = -1, bool launching = true)
    {
        try
        {
            try
            {
                Assert.IsTrue(squadron.Planes.Count == 0);
            }
            catch (Exception ex)
            {
                Debug.LogError("E01");
                throw ex;
            }
            for (int i = 0; i < planeCount; i++)
            {
                PlaneMovement plane = null;
                HashSet<PlaneMovement> set = null;

                try
                {
                    set = freePlanes[squadron.PlaneType];
                }
                catch (Exception ex)
                {
                    Debug.Log(i);
                    Debug.LogError("E02");
                    throw ex;
                }
                try
                {
                    using (var planesEnumer = set.GetEnumerator())
                    {
                        try
                        {
                            if (planesEnumer.MoveNext())
                            {
                                try
                                {
                                    plane = planesEnumer.Current;
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log(i);
                                    Debug.LogError("E03");
                                    throw ex;
                                }
                            }
                            else
                            {
                                try
                                {
                                    plane = Instantiate(PlanePrefabs[squadron.PlaneType], trans);
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log(i);
                                    if (squadron != null)
                                    {
                                        Debug.Log(squadron.PlaneType);
                                    }
                                    Debug.LogError("E04");
                                    throw ex;
                                }
                                try
                                {
                                    plane.name += "#_" + i;
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log(i);
                                    Debug.LogError("E05");
                                    throw ex;
                                }
                                try
                                {
                                    plane.Type = squadron.PlaneType;
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log(i);
                                    Debug.LogError("E06");
                                    throw ex;
                                }
                                try
                                {
                                    planes.Add(plane);
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log(i);
                                    Debug.LogError("E07");
                                    throw ex;
                                }
                                try
                                {
                                    plane.SpeedMultiplier = Speedup;
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log(i);
                                    Debug.LogError("E08");
                                    throw ex;
                                }
                                try
                                {
                                    plane.AnimSpeed = AnimSpeedup;
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log(i);
                                    Debug.LogError("E09");
                                    throw ex;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(i);
                            Debug.LogError("E10");
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(i);
                    Debug.LogError("E11");
                    throw ex;
                }
                try
                {
                    set.Remove(plane);
                }
                catch (Exception ex)
                {
                    Debug.Log(i);
                    Debug.LogError("E12");
                    throw ex;
                }
                try
                {
                    squadron.Planes.Add(plane);
                }
                catch (Exception ex)
                {
                    Debug.Log(i);
                    Debug.LogError("E13");
                    throw ex;
                }
                try
                {
                    Assert.IsTrue(positions.Count > i);
                }
                catch (Exception ex)
                {
                    if (positions != null)
                    {
                        Debug.Log(positions.Count);
                    }
                    Debug.Log(i);
                    Debug.LogError("E14");
                    throw ex;
                }
                try
                {
                    Assert.IsNull(positions[i].OccupiedBy);
                }
                catch (Exception ex)
                {
                    Debug.Log(i);
                    Debug.LogError("E15");
                    throw ex;
                }
                try
                {
                    plane.Setup(squadron, positions[i], true);
                }
                catch (Exception ex)
                {
                    Debug.Log(i);
                    Debug.LogError("E16");
                    throw ex;
                }
                if (spawnedOnStart)
                {
                    var planeCrewMan = PlaneCrewMovementManager.Instance;
                    try
                    {
                        plane.Rotate(generatedNodes.DeckLaunchingRotations[squadronIndex][i]);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(squadronIndex);
                        Debug.Log(i);
                        Debug.LogError("E17");
                        throw ex;
                    }
                    try
                    {
                        planeCrewMan.AskForCrewAtStart(plane, launching);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(i);
                        Debug.LogError("E18");
                        throw ex;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.Log("ERROR 5");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
            Application.Quit();
#endif
        }
    }

    public void FreePlanes(PlaneSquadron squadron)
    {
        var set = freePlanes[squadron.PlaneType];
        foreach (var plane in squadron.Planes)
        {
            plane.gameObject.SetActive(true);
            plane.ResetMovement();
            plane.StopCrewmates();
            plane.Remove();
            set.Add(plane);
        }
        squadron.Planes.Clear();
    }

    public void FreePlane(PlaneSquadron squadron)
    {
        var set = freePlanes[squadron.PlaneType];
        var plane = squadron.Planes[squadron.Planes.Count - 1];

        plane.StopCrewmates();
        plane.Remove();
        set.Add(plane);

        squadron.Planes.RemoveAt(squadron.Planes.Count - 1);
    }

    public void FromHangar(PlaneSquadron squadron, EPlaneNodeGroup to, float speed, bool load)
    {
        Assert.IsFalse(inCancel);
        Assert.IsTrue(allPlanes.Count == 0);
        Assert.IsTrue(planesInDestination.Count == 0);
        Assert.IsTrue(squadrons.Count == 0);
        Assert.IsTrue(movingSquadrons.Count == 0);
        Assert.IsTrue(movedSquadrons.Count == 0);

        SetLiftsState(0f);

        bool toLaunching = to == EPlaneNodeGroup.DeckLaunching;
        if (swapHangarOnRecovery)
        {
            generatedNodes.GroupNodes[EPlaneNodeGroup.Hangar].LineNodes[0][1] = toLaunching ? hangarStarting : hangarLanding;
        }

        if (load)
        {
            squadron.AnimationPlay = true;
            for (int i = 0; i < squadron.Planes.Count; i++)
            {
                squadron.Planes[i].ResetNode(generatedNodes.GroupNodes[EPlaneNodeGroup.Hangar].LineNodes[0][i]);
            }
        }
        else
        {
            CreatePlanes(squadron, generatedNodes.GroupNodes[EPlaneNodeGroup.Hangar].LineNodes[0], false);
        }
        squadron.FromHangar = true;

        squadronsIndices.Clear();
        squadronsIndices.Add(AircraftCarrierDeckManager.Instance.IndexOf(squadron));

        Assert.IsTrue(toLaunching || to == EPlaneNodeGroup.DeckRecovering);
        //List<EPlaneRotation> constraints;
        List<Quaternion> rotations;
        int slot = GetFreeSlot(generatedNodes.GroupNodes[to]);
        if (toLaunching)
        {
            currentMovement = EPlaneDirection.HangarToDeckLaunching;
            currentStageInMovement = EPlaneMoveStage.HangarToLiftLaunching;
            to = EPlaneNodeGroup.LiftLaunchingToDeck;

            //constraints = deckLaunchingRotationConstraints;
            rotations = generatedNodes.DeckLaunchingRotations[slot];
        }
        else
        {
            currentMovement = EPlaneDirection.HangarToDeckRecovering;
            currentStageInMovement = EPlaneMoveStage.HangarToLiftRecovering;
            to = EPlaneNodeGroup.LiftRecoveringToDeck;

            //constraints = deckRecoveringRotationConstraints;
            rotations = generatedNodes.DeckRecoveringRotations[slot];
        }
        for (int i = 0; i < squadron.Planes.Count; i++)
        {
            //squadron.Planes[i].LaterRotationConstraint = constraints[i];
            squadron.Planes[i].RotationAtDestination = rotations[i];
        }

        squadrons.Add(squadron);
        foreach (var plane in squadron.Planes)
        {
            AddPlaneMove(plane, speed, false);
            PrepareCrewmates(plane, false);
        }
        //added handling multiple lifts
        for (int i = 0; i < parallelLifts; i++)
        {
            StartPlaneSingleLane(squadron.Planes[i], EPlaneNodeGroup.Hangar, to, i, i);
        }
        Assert.IsFalse(allPlanes.Count == 0);
    }

    public void ToHangar(PlaneSquadron squadron, List<PlaneSquadron> allSquadrons, EPlaneNodeGroup from, float speed)
    {
        Assert.IsFalse(inCancel);
        Assert.IsTrue(allPlanes.Count == 0);
        Assert.IsTrue(planesInDestination.Count == 0);
        Assert.IsTrue(squadrons.Count == 0);
        Assert.IsTrue(movingSquadrons.Count == 0);
        Assert.IsTrue(movedSquadrons.Count == 0);

        bool fromLaunching = from == EPlaneNodeGroup.DeckLaunching;
        Assert.IsTrue(fromLaunching || from == EPlaneNodeGroup.DeckRecovering);
        if (swapHangarOnRecovery)
        {
            generatedNodes.GroupNodes[EPlaneNodeGroup.Hangar].LineNodes[0][1] = fromLaunching ? hangarStarting : hangarLanding;
        }

        if (parallelLifts == 3)
        {
            squadron.Planes[0].Dependencies2.Add(squadron.Planes[2]);
            squadron.Planes[2].Dependencies2.Add(squadron.Planes[0]);
        }

        PlaneNodeGroup fromNodes;
        EPlaneMoveStage firstStage;
        EPlaneMoveStage secondStage;
        EPlaneNodeGroup firstDestination;
        EPlaneNodeGroup secondDestination;
        List<float> delays;

        squadronsIndices.Clear();
        squadronsIndices.Add(AircraftCarrierDeckManager.Instance.IndexOf(squadron));

        //List<EPlaneRotation> constraints;
        List<List<Quaternion>> rotations;
        if (fromLaunching)
        {
            currentMovement = EPlaneDirection.DeckLaunchingToHangar;
            firstStage = EPlaneMoveStage.DeckLaunchingToLaunchingWait_Hangar;
            secondStage = EPlaneMoveStage.DeckLaunchingToLiftLaunching;
            firstDestination = EPlaneNodeGroup.WaitLaunching;
            secondDestination = EPlaneNodeGroup.LiftLaunchingToHangar;
            delays = ToHangarLineDelayLaunching;

            //constraints = deckLaunchingRotationConstraints;
            rotations = generatedNodes.DeckLaunchingRotations;
        }
        else
        {
            currentMovement = EPlaneDirection.DeckRecoveringToHangar;
            firstStage = EPlaneMoveStage.DeckRecoveringToRecoveringWait;
            secondStage = EPlaneMoveStage.DeckRecoveringToLiftRecovering;
            firstDestination = EPlaneNodeGroup.WaitRecovering;
            secondDestination = EPlaneNodeGroup.LiftRecoveringToHangar;
            delays = ToHangarLineDelayRecovering;

            //constraints = deckRecoveringRotationConstraints;
            rotations = generatedNodes.DeckRecoveringRotations;
        }
        fromNodes = generatedNodes.GroupNodes[from];

        Assert.IsTrue(movingSquadrons.Count == 0);
        movingSquadrons.Add(squadron);
        Assert.IsTrue(squadronsInState.Count == 0);
        squadronsInState.AddRange(allSquadrons);
        movingSquadronSlot = allSquadrons.IndexOf(squadron);
        Assert.IsTrue(movingSquadronSlot >= 0);
        Assert.IsTrue(movingSquadronSlot < allSquadrons.Count);
        lastLane = (movingSquadronSlot + 1) == allSquadrons.Count;

        var planeCrewMan = PlaneCrewMovementManager.Instance;
        for (int i = movingSquadronSlot; i < allSquadrons.Count; i++)
        {
            var planes = allSquadrons[i].Planes;
            for (int j = 0; j < planes.Count; j++)
            {
                planeCrewMan.MovePlaneCrewToPlane(planes[j], from);
            }
        }

        if (lastLane)
        {
            currentStageInMovement = secondStage;

            var path = generatedNodes.PathNodes[from][secondDestination];
            var to = generatedNodes.GroupNodes[secondDestination];
            for (int i = 0; i < squadron.Planes.Count; i++)
            {
                var plane = squadron.Planes[i];
                plane.RotationAtDestination = rotations[movingSquadronSlot][i];
                //plane.RotationConstraint = constraints[i];
                //plane.LaterRotationConstraint = constraints[i];
                plane.Delay = delays[i];
                StartPlaneDeckBasic(plane, movingSquadronSlot, i, i, fromNodes, to, path);
            }

            squadrons.Add(squadron);
            AddPlaneMove(squadron.Planes[0], speed, !fromLaunching);
        }
        else
        {
            currentStageInMovement = firstStage;
            currentSquadronLane = allSquadrons.Count - 1;
            var plane = allSquadrons[currentSquadronLane].Planes[0];
            //plane.MoveBackwards = !fromLaunching;
            var path = generatedNodes.PathNodes[from][firstDestination];
            StartPlaneDeckToWaitBasic(plane, currentSquadronLane, fromNodes, generatedNodes.GroupNodes[firstDestination], path);

            for (int i = movingSquadronSlot; i < allSquadrons.Count; i++)
            {
                squadrons.Add(allSquadrons[i]);
                for (int j = 0; j < allSquadrons[i].Planes.Count; j++)
                {
                    var plane2 = allSquadrons[i].Planes[j];
                    plane2.RotationAtDestination = rotations[i][j];
                    //plane2.RotationConstraint = constraints[j];
                    //plane2.LaterRotationConstraint = constraints[j];
                    AddPlaneMove(plane2, speed, !fromLaunching);
                }
            }
        }

        for (int i = 1; i < squadron.Planes.Count; i++)
        {
            AddPlaneMove(squadron.Planes[i], speed, !fromLaunching);
        }
        Assert.IsFalse(allPlanes.Count == 0);
    }

    public void FromAirToHangar(PlaneSquadron squadron, float speed, bool crash, bool load)
    {
        Assert.IsFalse(inCancel);
        Assert.IsTrue(allPlanes.Count == 0);
        Assert.IsTrue(planesInDestination.Count == 0);
        Assert.IsTrue(squadrons.Count == 0);
        Assert.IsTrue(movingSquadrons.Count == 0);
        Assert.IsTrue(movedSquadrons.Count == 0);

        if (swapHangarOnRecovery)
        {
            generatedNodes.GroupNodes[EPlaneNodeGroup.Hangar].LineNodes[0][1] = hangarLanding;
        }

        squadronsIndices.Clear();

        sentCount = PlaneCount;
        if (crash)
        {
            hadCrash = true;
            sentCount--;
            if (!load)
            {
                CreateWreck(squadron.PlaneType);
            }
        }
        var nodes = generatedNodes.GroupNodes[EPlaneNodeGroup.Landing].LineNodes[0];
        if (!load)
        {
            CreatePlanes(squadron, nodes, false, sentCount);
        }
        else
        {
            if (squadron.Planes.Count > sentCount)
            {
                FreePlane(squadron);
            }
            Assert.IsTrue(squadron.Planes.Count == sentCount);
            for (int i = 0; i < squadron.Planes.Count; i++)
            {
                squadron.Planes[i].ResetNode(nodes[i], Quaternion.LookRotation(Vector3.back), false);
            }
        }
        currentMovement = EPlaneDirection.LandingToHangar;
        currentStageInMovement = EPlaneMoveStage.None;

        squadrons.Add(squadron);
        foreach (var plane in squadron.Planes)
        {
            AddPlaneMove(plane, speed, false);
            PrepareCrewmates(plane, true);
        }

        animationActive = true;
        sent = sentCount;

        var poses = generatedNodes.GroupNodes[EPlaneNodeGroup.Landing].LineNodes[0];
        for (int i = 0; i < squadron.Planes.Count; i++)
        {
            squadron.Planes[i].Trans.position = poses[i].Position;
            squadron.Planes[i].Land(OnAnimationFinished, i);
        }
        Assert.IsFalse(allPlanes.Count == 0);
    }

    public void FromAirToRecovering(PlaneSquadron squadron, float speed, bool crash, bool load)
    {
        FromAirToHangar(squadron, speed, crash, load);
        currentMovement = EPlaneDirection.LandingToDeckRecovering;

        squadronsIndices.Clear();
        squadronsIndices.Add(AircraftCarrierDeckManager.Instance.IndexOf(squadron));

        //Assert.IsFalse(inCancel);
        //Assert.IsTrue(allPlanes.Count == 0);
        //Assert.IsTrue(planesInDestination.Count == 0);
        //Assert.IsTrue(squadrons.Count == 0);
        //Assert.IsTrue(movingSquadrons.Count == 0);
        //Assert.IsTrue(movedSquadrons.Count == 0);

        //CreatePlanes(squadron, generatedNodes.GroupNodes[EPlaneNodeGroup.Landing].LineNodes[0], true);
        //currentMovement = EPlaneDirection.LandingToDeckRecovering;

        //squadrons.Add(squadron);
        //foreach (var plane in squadron.Planes)
        //{
        //    AddPlaneMove(plane, speed, false);
        //}

        //animationActive = true;
        //sent = squadron.Planes.Count;
        //for (int i = 0; i < squadron.Planes.Count; i++)
        //{
        //    squadron.Planes[i].Land(OnAnimationFinished, i);
        //}
    }

    public void BetweenLaunchingRecovering(List<PlaneSquadron> squadrons, EPlaneNodeGroup from, float speed)
    {
        Assert.IsFalse(inCancel);
        Assert.IsTrue(allPlanes.Count == 0);
        Assert.IsTrue(planesInDestination.Count == 0);
        Assert.IsTrue(this.squadrons.Count == 0);
        Assert.IsTrue(movingSquadrons.Count == 0);
        Assert.IsTrue(movedSquadrons.Count == 0);
        Assert.IsTrue(squadronsInState.Count == 0);

        squadronsInState.AddRange(squadrons);
        bool toRecovering = from == EPlaneNodeGroup.DeckLaunching;
        EPlaneNodeGroup to;
        List<float> delays;
        //List<EPlaneRotation> constraints;
        if (toRecovering)
        {
            currentMovement = EPlaneDirection.DeckLaunchingToDeckRecovering;
            to = EPlaneNodeGroup.DeckRecovering;

            delays = SwitchModeFromLaunchLineDelay;

            //constraints = deckLaunchingRotationConstraints;
        }
        else
        {
            currentMovement = EPlaneDirection.DeckRecoveringToDeckLaunching;
            to = EPlaneNodeGroup.DeckLaunching;

            delays = SwitchModeFromRecoveringLineDelay;

            //constraints = deckRecoveringRotationConstraints;
        }
        currentStageInMovement = EPlaneMoveStage.None;

        Assert.IsTrue(toRecovering || from == EPlaneNodeGroup.DeckRecovering);
        var path = generatedNodes.PathNodes[from][to];
        var fromNodes = generatedNodes.GroupNodes[from];
        var toNodes = generatedNodes.GroupNodes[to];
        var planeCrewMovMan = PlaneCrewMovementManager.Instance;
        for (int i = 0; i < squadrons.Count; i++)
        {
            this.squadrons.Add(squadrons[i]);

            for (int j = 0; j < squadrons[i].Planes.Count; j++)
            {
                var plane = squadrons[i].Planes[j];
                planeCrewMovMan.MovePlaneCrewToPlane(plane, from);
                //plane.RotationConstraint = constraints[j];
                AddPlaneMove(plane, speed, false);
                //plane.MoveBackwards = !toRecovering;
                plane.Delay = delays[j] + SwitchModeRowDelay[squadrons.Count - i - 1];
                StartPlaneLaunchingRecovering(plane, j, i, squadrons.Count - i - 1, fromNodes, toNodes, path);
                foreach (var squadron in squadrons)
                {
                    foreach (var plane2 in squadron.Planes)
                    {
                        plane.Dependencies.Add(plane2);
                    }
                }
                plane.Dependencies.Remove(plane);
            }
        }
        Assert.IsFalse(allPlanes.Count == 0);
    }

    public void Launch(List<PlaneSquadron> squadronsToMove, List<PlaneSquadron> allSquadrons, float speed)
    {
        Assert.IsFalse(inCancel);
        Assert.IsTrue(allPlanes.Count == 0);
        Assert.IsTrue(planesInDestination.Count == 0);
        Assert.IsTrue(squadrons.Count == 0);
        Assert.IsTrue(movingSquadrons.Count == 0);
        Assert.IsTrue(movedSquadrons.Count == 0);
        Assert.IsTrue(squadronsInState.Count == 0);
        squadronsInState.AddRange(allSquadrons);
        movingSquadrons.AddRange(squadronsToMove);
        movingSquadronSlot = allSquadrons.IndexOf(squadronsToMove[0]);
        Assert.IsFalse(movingSquadronSlot < 0);
        Assert.IsTrue(movingSquadronSlot < allSquadrons.Count);

        var planeCrewMan = PlaneCrewMovementManager.Instance;
        int index = movingSquadronSlot;
        foreach (var squadron in squadronsToMove)
        {
            int newIndex = allSquadrons.IndexOf(squadron);
            Assert.IsFalse(index < newIndex);
            index = newIndex;
        }

        for (int i = index; i < allSquadrons.Count; i++)
        {
            var planes = allSquadrons[i].Planes;
            for (int j = 0; j < planes.Count; j++)
            {
                planeCrewMan.MovePlaneCrewToPlane(planes[j], EPlaneNodeGroup.DeckLaunching);
            }
        }

        var from = generatedNodes.GroupNodes[EPlaneNodeGroup.DeckLaunching];
        currentMovement = EPlaneDirection.DeckLaunchingToAirLaunching;
        lastLane = (movingSquadronSlot + 1) == allSquadrons.Count;

        squadronsIndices.Clear();
        var deck = AircraftCarrierDeckManager.Instance;
        foreach (var squadron in squadronsToMove)
        {
            squadronsIndices.Add(deck.IndexOf(squadron));
        }
        if (lastLane)
        {
            //move straight
            currentStageInMovement = EPlaneMoveStage.DeckLaunchingToStarting_Loop;

            var path = generatedNodes.PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.Starting];
            var to = generatedNodes.GroupNodes[EPlaneNodeGroup.Starting];
            for (int i = 0; i < squadronsToMove[0].Planes.Count; i++)
            {
                var plane = squadronsToMove[0].Planes[i];
                StartPlaneDeckBasic(plane, movingSquadronSlot, i, i, from, to, path);
                plane.Delay = launchDelay[i];
            }
        }
        else
        {
            currentStageInMovement = EPlaneMoveStage.DeckLaunchingToLaunchingWait;

            currentSquadronLane = allSquadrons.Count - 1;

            var path = generatedNodes.PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.WaitLaunching];
            StartPlaneDeckToWaitBasic(allSquadrons[currentSquadronLane].Planes[0], currentSquadronLane, from, generatedNodes.GroupNodes[EPlaneNodeGroup.WaitLaunching], path);
        }

        foreach (var squadron in squadronsToMove)
        {
            squadrons.Add(squadron);
        }
        foreach (var squadron in allSquadrons)
        {
            foreach (var plane in squadron.Planes)
            {
                AddPlaneMove(plane, speed, false);
            }
        }
        Assert.IsFalse(allPlanes.Count == 0);
    }

    public void Swap(PlaneSquadron squadronA, PlaneSquadron squadronB, List<PlaneSquadron> allSquadrons, EPlaneNodeGroup from, float speed, out int indexA, out int newIndexA, out int newIndexB)
    {
        Assert.IsFalse(inCancel);
        Assert.IsTrue(allPlanes.Count == 0);
        Assert.IsTrue(planesInDestination.Count == 0);
        Assert.IsTrue(squadrons.Count == 0);
        Assert.IsTrue(movingSquadrons.Count == 0);
        Assert.IsTrue(movedSquadrons.Count == 0);

        bool fromLaunching = from == EPlaneNodeGroup.DeckLaunching;
        Assert.IsTrue(fromLaunching || from == EPlaneNodeGroup.DeckRecovering);

        squadronsIndices.Clear();
        var deck = AircraftCarrierDeckManager.Instance;
        squadronsIndices.Add(deck.IndexOf(squadronA));
        squadronsIndices.Add(deck.IndexOf(squadronB));

        EPlaneMoveStage waitStage;
        EPlaneMoveStage toSwapWaitStage;
        EPlaneNodeGroup waitDestination;
        EPlaneNodeGroup swapWaitDestination;
        //List<EPlaneRotation> constraints;
        List<List<Quaternion>> rotations;

        if (fromLaunching)
        {
            currentMovement = EPlaneDirection.SwapLaunching;
            waitStage = EPlaneMoveStage.DeckLaunchingAToSwapLaunching;
            toSwapWaitStage = EPlaneMoveStage.DeckLaunchingXToLaunchingWait;
            waitDestination = EPlaneNodeGroup.WaitLaunching;
            swapWaitDestination = EPlaneNodeGroup.SwapLaunching;

            //constraints = deckLaunchingRotationConstraints;
            rotations = generatedNodes.DeckLaunchingRotations;
        }
        else
        {
            currentMovement = EPlaneDirection.SwapRecovering;
            waitStage = EPlaneMoveStage.DeckRecoveringAToSwapRecovering;
            toSwapWaitStage = EPlaneMoveStage.DeckRecoveringXToRecoveringWait;
            waitDestination = EPlaneNodeGroup.WaitRecovering;
            swapWaitDestination = EPlaneNodeGroup.SwapRecovering;

            //constraints = deckRecoveringRotationConstraints;
            rotations = generatedNodes.DeckRecoveringRotations;
        }
        Assert.IsTrue(squadronsInState.Count == 0);
        squadronsInState.AddRange(allSquadrons);

        indexA = lineA = allSquadrons.IndexOf(squadronA);
        Assert.IsTrue(lineA >= 0 && lineA < allSquadrons.Count);
        lineB = allSquadrons.IndexOf(squadronB);
        Assert.IsTrue(lineB >= 0 && lineB < allSquadrons.Count);
        //set last line to swap
        if (lineA < lineB)
        {
            (lineA, lineB) = (lineB, lineA);
        }
        newIndexA = lineA;
        newIndexB = lineB;

        var planeCrewMan = PlaneCrewMovementManager.Instance;
        for (int i = lineB; i < allSquadrons.Count; i++)
        {
            planeCrewMan.MovePlaneCrewToPlane(allSquadrons[i].Planes[0], from);
        }
        for (int i = 1; i < squadronA.Planes.Count; i++)
        {
            planeCrewMan.MovePlaneCrewToPlane(squadronA.Planes[i], from);
        }
        for (int i = 1; i < squadronB.Planes.Count; i++)
        {
            planeCrewMan.MovePlaneCrewToPlane(squadronB.Planes[i], from);
        }

        movingSquadronSlot = lineA;
        currentSquadronLane = allSquadrons.Count - 1;
        lastLane = movingSquadronSlot == currentSquadronLane;

        var fromNodes = generatedNodes.GroupNodes[from];
        var planesA = allSquadrons[lineA].Planes;
        if (lastLane)
        {
            //move straight
            currentStageInMovement = waitStage;

            var path = generatedNodes.PathNodes[from][swapWaitDestination];
            var to = generatedNodes.GroupNodes[swapWaitDestination];
            for (int i = 0; i < planesA.Count; i++)
            {
                planesA[i].Delay = swapLastLineDelay[i];
                StartPlaneDeckBasic(planesA[i], movingSquadronSlot, i, i, fromNodes, to, path);
            }
        }
        else
        {
            currentStageInMovement = toSwapWaitStage;

            var plane = allSquadrons[currentSquadronLane].Planes[0];
            //plane.MoveBackwards = !fromLaunching;
            var path = generatedNodes.PathNodes[from][waitDestination];
            StartPlaneDeckToWaitBasic(plane, currentSquadronLane, fromNodes, generatedNodes.GroupNodes[waitDestination], path);
        }
        for (int i = lineB; i < allSquadrons.Count; i++)
        {
            var planes = allSquadrons[i].Planes;
            for (int j = 0; j < planes.Count; j++)
            {
                planes[j].RotationAtDestination = rotations[i][j];
                //planes[j].RotationConstraint = constraints[j];
                //planes[j].LaterRotationConstraint = constraints[j];
                AddPlaneMove(planes[j], speed, !fromLaunching);
            }
        }

        allPlanes.Clear();
        for (int j = 0; j < planesA.Count; j++)
        {
            allPlanes.Add(planesA[j]);
        }
        Assert.IsFalse(allPlanes.Count == 0);
    }

    public void SwapToFront(PlaneSquadron squadron, List<PlaneSquadron> allSquadrons, EPlaneNodeGroup from, float speed, out int index)
    {
        Assert.IsFalse(inCancel);
        Assert.IsTrue(allPlanes.Count == 0);
        Assert.IsTrue(planesInDestination.Count == 0);
        Assert.IsTrue(squadrons.Count == 0);
        Assert.IsTrue(movingSquadrons.Count == 0);
        Assert.IsTrue(movedSquadrons.Count == 0);

        bool fromLaunching = from == EPlaneNodeGroup.DeckLaunching;
        Assert.IsTrue(fromLaunching || from == EPlaneNodeGroup.DeckRecovering);

        EPlaneMoveStage stage;
        EPlaneNodeGroup waitDestination;
        //List<EPlaneRotation> constraints;
        List<List<Quaternion>> rotations;

        squadronsIndices.Clear();
        squadronsIndices.Add(AircraftCarrierDeckManager.Instance.IndexOf(squadron));

        if (fromLaunching)
        {
            currentMovement = EPlaneDirection.SwapFrontLaunching;
            stage = EPlaneMoveStage.DeckLaunchingXToLaunchingWait_Front;
            waitDestination = EPlaneNodeGroup.WaitLaunching;

            //constraints = deckLaunchingRotationConstraints;
            rotations = generatedNodes.DeckLaunchingRotations;
        }
        else
        {
            currentMovement = EPlaneDirection.SwapFrontRecovering;
            stage = EPlaneMoveStage.DeckRecoveringXToRecoveringWait_Front;
            waitDestination = EPlaneNodeGroup.WaitRecovering;

            //constraints = deckRecoveringRotationConstraints;
            rotations = generatedNodes.DeckRecoveringRotations;
        }
        Assert.IsTrue(squadronsInState.Count == 0);
        squadronsInState.AddRange(allSquadrons);

        index = lineA = allSquadrons.IndexOf(squadron);
        Assert.IsTrue(lineA >= 0 && lineA < allSquadrons.Count, lineA.ToString());
        movingSquadronSlot = lineA;
        currentSquadronLane = allSquadrons.Count - 1;
        Assert.IsFalse(movingSquadronSlot == currentSquadronLane);

        var planeCrewMan = PlaneCrewMovementManager.Instance;
        for (int i = lineA; i < allSquadrons.Count; i++)
        {
            var planes = allSquadrons[i].Planes;
            for (int j = 0; j < planes.Count; j++)
            {
                planeCrewMan.MovePlaneCrewToPlane(planes[j], from);
            }
        }

        currentStageInMovement = stage;

        currentSquadronLane++;
        for (int i = lineA; i < allSquadrons.Count; i++)
        {
            var planes = allSquadrons[i].Planes;
            for (int j = 0; j < planes.Count; j++)
            {
                planes[j].RotationAtDestination = rotations[i][j];
                //planes[j].RotationConstraint = constraints[j];
                //planes[j].LaterRotationConstraint = constraints[j];
                AddPlaneMove(planes[j], speed, !fromLaunching);
            }
        }

        bool x = PlanesToWait(generatedNodes.GroupNodes[from], from, waitDestination);
        Assert.IsFalse(x);
        Assert.IsFalse(allPlanes.Count == 0);
    }

    public void Cancel(List<PlaneSquadron> alreadySent, out PlaneSquadron rearranged)
    {
        Assert.IsFalse(inCancel);
        Assert.IsFalse(allPlanes.Count == 0);
        rearranged = null;
        inCancel = true;
        if (animationActive || (currentMovement == EPlaneDirection.DeckLaunchingToAirLaunching && currentStageInMovement == EPlaneMoveStage.StartingToAirLaunching))
        {
            Assert.IsTrue(currentMovement == EPlaneDirection.DeckLaunchingToAirLaunching);
            if (movingSquadrons.Count == 1)
            {
                inCancel = false;
            }
            alreadySent.AddRange(movedSquadrons);
            alreadySent.Add(movingSquadrons[0]);
            return;
        }

        foreach (var plane in rotatingPlanes)
        {
            plane.RotationFinished -= OnRotationFinished;
        }
        rotatingPlanes.Clear();

        switch (currentMovement)
        {
            case EPlaneDirection.DeckLaunchingToDeckRecovering:
                CancelBetweenLaunchingRecovering(EPlaneNodeGroup.DeckLaunching);
                break;
            case EPlaneDirection.DeckRecoveringToDeckLaunching:
                CancelBetweenLaunchingRecovering(EPlaneNodeGroup.DeckRecovering);
                break;
            case EPlaneDirection.HangarToDeckLaunching:
                CancelFromHangar(EPlaneNodeGroup.DeckLaunching);
                break;
            case EPlaneDirection.HangarToDeckRecovering:
                CancelFromHangar(EPlaneNodeGroup.DeckRecovering);
                break;
            case EPlaneDirection.DeckLaunchingToHangar:
            case EPlaneDirection.DeckRecoveringToHangar:
                if (CancelToHangar(currentMovement == EPlaneDirection.DeckLaunchingToHangar ? EPlaneNodeGroup.DeckLaunching : EPlaneNodeGroup.DeckRecovering))
                {
                    rearranged = movingSquadrons[0];
                }
                return;
            case EPlaneDirection.DeckLaunchingToAirLaunching:
                switch (currentStageInMovement)
                {
                    case EPlaneMoveStage.DeckLaunchingToLaunchingWait:
                        SetWaitingPlanesAfterMovingPlanes();
                        var plane = squadronsInState[currentSquadronLane].Planes[0];
                        Assert.IsFalse(plane.Path.Count == 0);
                        plane.Return();
                        allPlanes.Add(plane);
                        break;
                    case EPlaneMoveStage.DeckLaunchingToStarting_Loop:
                        Assert.IsTrue(movingSquadrons.Count > 0);
                        if (lastLane)
                        {
                            Assert.IsTrue(movingSquadronSlot == squadronsInState.IndexOf(movingSquadrons[0]));
                            var deck = generatedNodes.GroupNodes[EPlaneNodeGroup.DeckLaunching];

                            var path = generatedNodes.PathNodes[EPlaneNodeGroup.Starting][EPlaneNodeGroup.DeckLaunching];
                            for (int i = 0; i < movingSquadrons[0].Planes.Count; i++)
                            {
                                var plane2 = movingSquadrons[0].Planes[i];
                                if (planesInDestination.Contains(plane2))
                                {
                                    Assert.IsTrue(plane2.Path.Count == 0);
                                    StartPlaneDeckReturn(plane2, movingSquadronSlot, i, deck, path);
                                }
                                else
                                {
                                    plane2.Return();
                                }
                            }
                            planesInDestination.Clear();
                        }
                        else
                        {
                            currentSquadronLane++;
                            Assert.IsTrue(planesInDestination.Count < movingSquadrons[0].Planes.Count);
                            foreach (var plane2 in planesInDestination)
                            {
                                Assert.IsTrue(plane2.Squadron == movingSquadrons[0]);
                            }
                            int planeIndex;
                            for (planeIndex = 0; planeIndex < movingSquadrons[0].Planes.Count; planeIndex++)
                            {
                                var plane2 = movingSquadrons[0].Planes[planeIndex];
                                if (!planesInDestination.Contains(plane2))
                                {
                                    plane2.Return();
                                    break;
                                }
                            }
                            Assert.IsFalse(planeIndex == movingSquadrons[0].Planes.Count);
                            planesInDestination.Clear();

                            for (planeIndex++; planeIndex < movingSquadrons[0].Planes.Count; planeIndex++)
                            {
                                planesInDestination.Add(movingSquadrons[0].Planes[planeIndex]);
                            }
                        }
                        break;
                    case EPlaneMoveStage.LaunchingWaitToDeckLaunching:
                        //all squadrons already left
                        inCancel = false;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                alreadySent.AddRange(movedSquadrons);
                break;
            case EPlaneDirection.SwapLaunching:
                CancelSwap(EPlaneNodeGroup.DeckLaunching, EPlaneNodeGroup.WaitLaunching);
                break;
            case EPlaneDirection.SwapRecovering:
                CancelSwap(EPlaneNodeGroup.DeckRecovering, EPlaneNodeGroup.WaitRecovering);
                break;
            case EPlaneDirection.LandingToHangar:
            case EPlaneDirection.LandingToDeckRecovering:
            default:
                Assert.IsTrue(false);
                break;
        }
        Assert.IsFalse(allPlanes.Count == 0);
    }

    public PlaneSquadron CreateHollow(EPlaneType planeType)
    {
        var squadron = new PlaneSquadron(planeType);
        for (int i = 0; i < PlaneCount; i++)
        {
            var plane = Instantiate(PlaneHollowPrefabs[squadron.PlaneType], transform, false);
            squadron.Hollows.Add(plane);
        }
        return squadron;
    }

    public void CreateWreck(EPlaneType type)
    {
        int index;
        var deck = AircraftCarrierDeckManager.Instance;
        switch (type)
        {
            case EPlaneType.Fighter:
                index = 3 + deck.FighterLv;
                break;
            case EPlaneType.TorpedoBomber:
                index = 6 + deck.TorpedoLv;
                break;
            case EPlaneType.Bomber:
            default:
                index = 0 + deck.BomberLv;
                break;
        }

        CreateWreck(index, EWreckType.Wreck);
    }

    public void CreateWreck(EPlaneType type, int lv)
    {
        int index = lv;
        switch (type)
        {
            case EPlaneType.Fighter:
                index += 3;
                break;
            case EPlaneType.TorpedoBomber:
                index += 6;
                break;
            case EPlaneType.Bomber:
            default:
                index += 0;
                break;
        }

        CreateWreck(index, EWreckType.Wreck);
    }

    public void CreateKamikaze(bool front)
    {
        var type = front ? EWreckType.FrontKamikaze : EWreckType.EndKamikaze;
        CreateWreck(WreckPrefabs.Count - (int)type, type);
    }

    public void CreateWreck(int wreck, EWreckType type)
    {
        lastWreckIndex = wreck;

        var wreckObj = Instantiate(WreckPrefabs[wreck]);
        CurrentWrecks[(int)type] = wreckObj;
        wreckObj.transform.position = wreckSpawnPositions[(int)type].position;
        var deck = AircraftCarrierDeckManager.Instance;
        switch (type)
        {
            case EWreckType.Wreck:
                deck.HasWreck = true;
                break;
            case EWreckType.FrontKamikaze:
                deck.HasKamikazeFront = true;
                break;
            case EWreckType.EndKamikaze:
                deck.HasKamikazeEnd = true;
                break;
        }
    }

    public void ResetMovement()
    {
        foreach (var wreck in CurrentWrecks)
        {
            if (wreck != null)
            {
                wreck.InstantRemove();
            }
        }
        AircraftCarrierDeckManager.Instance.IsRunwayDamaged = false;

        helper.StopCoroutine();
        foreach (var plane in rotatingPlanes)
        {
            if (plane != null)
            {
                plane.RotationFinished -= OnRotationFinished;
            }
        }
        rotatingPlanes.Clear();
        foreach (var plane in registered)
        {
            plane.Arrived -= OnPlaneArrived;
            plane.RotationFinished -= OnRotationFinished;
        }
        registered.Clear();
        inCancel = false;

        Clear();

        foreach (var plane in planes)
        {
            plane.ResetMovement();
            plane.Clear();
        }

        PlaneCrewMovementManager.Instance.ResetCrew();
    }

    public PlaneNode GetNode(int index)
    {
        return generatedNodes.GetNode(index);
    }

    public int IndexOf(PlaneNode node)
    {
        int result = generatedNodes.IndexOf(node);
        Assert.IsFalse(result == -1);
        return result;
    }

    public void Tick()
    {
        if (inTickable)
        {
            var deck = AircraftCarrierDeckManager.Instance;
            PlaneCrewMovementManager.Instance.MovePlaneCrewToSide((deck.DeckMode == EDeckMode.Starting ? EPlaneNodeGroup.DeckLaunching : EPlaneNodeGroup.DeckRecovering), GetAllPlanes(deck.DeckSquadrons));
            RemoveTick();
        }
        ignoreTick = false;
    }

    public void SetOverrideTick(bool ignore)
    {
        inTickable = ignore;
        ignoreTick = ignore;
    }

    private void OnPlaneArrived(PlaneMovement plane)
    {
        Assert.IsFalse(allPlanes.Count == 0);
        Unregister(plane);
        //Assert.IsTrue(allPlanes.Contains(plane));
        Assert.IsFalse(planesInDestination.Contains(plane));
        planesInDestination.Add(plane);
        Assert.IsFalse(allPlanes.Contains(plane) && planesInDestination.Count > allPlanes.Count);
        if (inCancel && (currentMovement != EPlaneDirection.DeckLaunchingToAirLaunching || currentStageInMovement != EPlaneMoveStage.StartingToAirLaunching))
        {
            OnCancelledPlaneArrived(plane);
            return;
        }
        switch (currentMovement)
        {
            case EPlaneDirection.DeckLaunchingToDeckRecovering:
            case EPlaneDirection.DeckRecoveringToDeckLaunching:
                //List<EPlaneRotation> constraints;
                List<List<Quaternion>> rotations;
                if (currentMovement == EPlaneDirection.DeckRecoveringToDeckLaunching)
                {
                    //constraints = deckLaunchingRotationConstraints;
                    rotations = generatedNodes.DeckLaunchingRotations;
                }
                else
                {
                    //constraints = deckRecoveringRotationConstraints;
                    rotations = generatedNodes.DeckRecoveringRotations;
                }
                //plane.RotationConstraint = constraints[plane.Squadron.Planes.IndexOf(plane)];
                int squadronIndex = squadronsInState.IndexOf(plane.Squadron);
                if (squadronIndex == -1)
                {
                    Debug.LogError("Squadron not found");
                }
                plane.Rotate(rotations[squadronsInState.Count - squadronIndex - 1][plane.Squadron.Planes.IndexOf(plane)]);
                if (allPlanes.Count == planesInDestination.Count)
                {
                    FinishMovement();
                }
                break;
            case EPlaneDirection.HangarToDeckLaunching:
                FromHangarNextStep(plane, EPlaneMoveStage.HangarToLiftLaunching, EPlaneMoveStage.LiftLaunchingToDeckLaunching, EPlaneNodeGroup.LiftLaunchingToDeck, EPlaneNodeGroup.DeckLaunching);
                break;
            case EPlaneDirection.HangarToDeckRecovering:
                FromHangarNextStep(plane, EPlaneMoveStage.HangarToLiftRecovering, EPlaneMoveStage.LiftRecoveringToDeckRecovering, EPlaneNodeGroup.LiftRecoveringToDeck, EPlaneNodeGroup.DeckRecovering);
                break;
            case EPlaneDirection.LandingToHangar:
                Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
                if (allPlanes.Count == planesInDestination.Count)
                {
                    foreach (var squadron in squadrons)
                    {
                        FreePlanes(squadron);
                    }
                    FinishMovement();
                }
                else
                {
                    //added handling multiple lifts
                    int index = plane.Squadron.Planes.IndexOf(plane);
                    if (parallelLifts == 1 || (parallelLifts == 2 && index == 1 && plane.Squadron.Planes.Count > 2))
                    {
                        plane = GetNextPlane(plane, index, out index);
                        //AircraftCarrierDeckManager.Instance.ElevatorState = 1f;
                        StartPlaneSingleLane(plane, EPlaneNodeGroup.Landing, EPlaneNodeGroup.Hangar, index);
                    }
                }
                break;
            case EPlaneDirection.LandingToDeckRecovering:
                PlaneRotateLateConstraint(plane);

                Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
                if (allPlanes.Count == planesInDestination.Count)
                {
                    FinishMovement();
                }
                break;
            case EPlaneDirection.DeckLaunchingToHangar:
                ToHangarNextStep(plane, EPlaneNodeGroup.DeckLaunching, EPlaneNodeGroup.WaitLaunching, EPlaneNodeGroup.LiftLaunchingToHangar);
                break;
            case EPlaneDirection.DeckRecoveringToHangar:
                ToHangarNextStep(plane, EPlaneNodeGroup.DeckRecovering, EPlaneNodeGroup.WaitRecovering, EPlaneNodeGroup.LiftRecoveringToHangar);
                break;
            case EPlaneDirection.DeckLaunchingToAirLaunching:
                switch (currentStageInMovement)
                {
                    case EPlaneMoveStage.DeckLaunchingToLaunchingWait:
                        var fromDeck = generatedNodes.GroupNodes[EPlaneNodeGroup.DeckLaunching];
                        //moved all planes out of way?
                        if (--currentSquadronLane == movingSquadronSlot)
                        {
                            planesInDestination.Clear();
                            currentStageInMovement = EPlaneMoveStage.DeckLaunchingToStarting_Loop;

                            var path = generatedNodes.PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.Starting];
                            StartPlaneDeckBasic(movingSquadrons[0].Planes[0], movingSquadronSlot, 0, 0, fromDeck, generatedNodes.GroupNodes[EPlaneNodeGroup.Starting], path);
                        }
                        else
                        {
                            var path = generatedNodes.PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.WaitLaunching];
                            StartPlaneDeckToWaitBasic(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, fromDeck, generatedNodes.GroupNodes[EPlaneNodeGroup.WaitLaunching], path);
                        }
                        break;
                    case EPlaneMoveStage.DeckLaunchingToStarting_Loop:
                        Assert.IsTrue(movingSquadrons.Count > 0);
                        Assert.IsTrue(plane.Squadron == movingSquadrons[0]);
                        foreach (var plane2 in planesInDestination)
                        {
                            Assert.IsTrue(movingSquadrons[0].Planes.Contains(plane2));
                        }
                        Assert.IsFalse(planesInDestination.Count > plane.Squadron.Planes.Count);
                        plane.SetWings(true);
                        if (plane.Squadron.Planes.Count == planesInDestination.Count)
                        {
                            planesInDestination.Clear();
                            currentStageInMovement = EPlaneMoveStage.StartingToAirLaunching;
                            plane = plane.Squadron.Planes[0];
                            plane.Path.Add(plane.CurrentNode);
                            plane.Path.Add(generatedNodes.GroupNodes[EPlaneNodeGroup.AirLaunching].LineNodes[0][0]);
                            Register(plane);
                            plane.StartMovement();
                        }
                        //if it is last lane, they are all moving, no need to move plane
                        else if (!lastLane)
                        {
                            plane = GetNextPlane(plane, out int index2);
                            Assert.IsTrue(plane.Path.Count == 0);
                            var fromDeck2 = generatedNodes.GroupNodes[EPlaneNodeGroup.DeckLaunching];
                            var path = generatedNodes.PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.Starting];
                            StartPlaneDeckBasic(plane, movingSquadronSlot, index2, 0, fromDeck2, generatedNodes.GroupNodes[EPlaneNodeGroup.Starting], path, true);
                        }
                        break;
                    case EPlaneMoveStage.StartingToAirLaunching:
                        planesInDestination.Clear();
                        animationActive = true;
                        int index = plane.Squadron.Planes.IndexOf(plane);
                        //  plane.StopCrewmates();
                        plane.Fly(OnAnimationFinished, index, 1f);// startingDelay[index]);
                        break;
                    case EPlaneMoveStage.LaunchingWaitToDeckLaunching:
                        plane.Rotate(plane.RotationAtDestination);
                        Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
                        if (allPlanes.Count == planesInDestination.Count)
                        {
                            FinishMovement();
                        }
                        //is it a plane from waiting, move another plane from waiting if there is any
                        else if (plane.Squadron.Planes[0] == plane)
                        {
                            currentSquadronLane++;
                            if (currentSquadronLane < squadronsInState.Count)
                            {
                                var path = generatedNodes.PathNodes[EPlaneNodeGroup.WaitLaunching][EPlaneNodeGroup.DeckLaunching];
                                StartPlaneDeckReturn(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, 0, generatedNodes.GroupNodes[EPlaneNodeGroup.DeckLaunching], path);
                            }
                        }
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case EPlaneDirection.SwapLaunching:
                SwapNextStep(plane, EPlaneNodeGroup.DeckLaunching, EPlaneNodeGroup.WaitLaunching, EPlaneNodeGroup.SwapLaunching, generatedNodes.DeckLaunchingRotations);
                break;
            case EPlaneDirection.SwapRecovering:
                SwapNextStep(plane, EPlaneNodeGroup.DeckRecovering, EPlaneNodeGroup.WaitRecovering, EPlaneNodeGroup.SwapRecovering, generatedNodes.DeckRecoveringRotations);
                break;
            case EPlaneDirection.SwapFrontLaunching:
                SwapToFrontNextStep(plane, EPlaneNodeGroup.DeckLaunching, EPlaneNodeGroup.WaitLaunching, EPlaneNodeGroup.SwapLaunching, generatedNodes.DeckLaunchingRotations);
                break;
            case EPlaneDirection.SwapFrontRecovering:
                SwapToFrontNextStep(plane, EPlaneNodeGroup.DeckRecovering, EPlaneNodeGroup.WaitRecovering, EPlaneNodeGroup.SwapRecovering, generatedNodes.DeckRecoveringRotations);
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
    }

    private void OnRotationFinished(PlaneMovement plane)
    {
        //plane.RotationFinished -= OnRotationFinished;

        //rotatingPlanes.Remove(plane);

        foreach (var plane2 in rotatingPlanes)
        {
            if (plane2 != null)
            {
                plane2.RotationFinished -= OnRotationFinished;
            }
        }
        rotatingPlanes.Clear();

        helper.StopCoroutine();
        FinishMovement();
    }

    private void OnCancelledPlaneArrived(PlaneMovement plane)
    {
        switch (currentMovement)
        {
            case EPlaneDirection.DeckLaunchingToDeckRecovering:
            case EPlaneDirection.DeckRecoveringToDeckLaunching:
                //List<EPlaneRotation> constraints;
                List<List<Quaternion>> rotations;
                if (currentMovement == EPlaneDirection.DeckRecoveringToDeckLaunching)
                {
                    //constraints = deckRecoveringRotationConstraints;
                    rotations = generatedNodes.DeckRecoveringRotations;
                }
                else
                {
                    //constraints = deckLaunchingRotationConstraints;
                    rotations = generatedNodes.DeckLaunchingRotations;
                }
                //plane.RotationConstraint = constraints[plane.Squadron.Planes.IndexOf(plane)];
                int squadronIndex = squadronsInState.IndexOf(plane.Squadron);
                if (squadronIndex == -1)
                {
                    Debug.LogError("Squadron not found");
                }
                plane.Rotate(rotations[squadronsInState.Count - squadronIndex - 1][plane.Squadron.Planes.IndexOf(plane)]);

                Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
                if (allPlanes.Count == planesInDestination.Count)
                {
                    FinishMovement();
                }
                break;
            case EPlaneDirection.HangarToDeckLaunching:
                CancelFromHangarNextStep(plane, EPlaneNodeGroup.LiftLaunchingToHangar);
                break;
            case EPlaneDirection.HangarToDeckRecovering:
                CancelFromHangarNextStep(plane, EPlaneNodeGroup.LiftRecoveringToHangar);
                break;
            case EPlaneDirection.DeckLaunchingToHangar:
                CancelToHangarNextStep(plane, EPlaneNodeGroup.DeckLaunching);
                break;
            case EPlaneDirection.DeckRecoveringToHangar:
                CancelToHangarNextStep(plane, EPlaneNodeGroup.DeckRecovering);
                break;
            case EPlaneDirection.DeckLaunchingToAirLaunching:
                switch (currentStageInMovement)
                {
                    case EPlaneMoveStage.DeckLaunchingToLaunchingWait:
                        Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
                        Assert.IsTrue(currentSquadronLane > 0);
                        Assert.IsTrue(currentSquadronLane < squadronsInState.Count);
                        Assert.IsNotNull(squadronsInState[currentSquadronLane]);
                        PlaneRotateLateConstraint(plane);
                        if (allPlanes.Count == planesInDestination.Count)
                        {
                            if (movingSquadronSlot == -1)
                            {
                                FinishMovement();
                            }
                            else
                            {
                                //same as normal last stage
                                inCancel = false;
                                FillPlaneLanes();
                            }
                        }
                        else
                        {
                            currentSquadronLane++;
                            Assert.IsTrue(currentSquadronLane < squadronsInState.Count);
                            Assert.IsNotNull(squadronsInState[currentSquadronLane]);

                            var path = generatedNodes.PathNodes[EPlaneNodeGroup.WaitLaunching][EPlaneNodeGroup.DeckLaunching];
                            StartPlaneDeckReturn(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, 0, generatedNodes.GroupNodes[EPlaneNodeGroup.DeckLaunching], path);
                        }
                        break;
                    case EPlaneMoveStage.DeckLaunchingToStarting_Loop:
                        Assert.IsTrue(movingSquadrons.Count > 0);
                        Assert.IsTrue(movingSquadrons[0] == plane.Squadron);

                        PlaneRotateLateConstraint(plane);
                        if (lastLane)
                        {
                            bool all = true;
                            foreach (var plane2 in plane.Squadron.Planes)
                            {
                                if (!planesInDestination.Contains(plane2))
                                {
                                    all = false;
                                    break;
                                }
                            }
                            if (all)
                            {
                                int i;
                                for (i = 0; i < squadronsInState.Count; i++)
                                {
                                    Assert.IsTrue((squadronsInState[i] == null) == ((squadronsInState.Count - i - 1) < movedSquadrons.Count));
                                }
                                FinishMovement();
                            }
                        }
                        else
                        {
                            var deck = generatedNodes.GroupNodes[EPlaneNodeGroup.DeckLaunching];
                            if (plane.Squadron.Planes[0] == plane)
                            {
                                currentStageInMovement = EPlaneMoveStage.DeckLaunchingToLaunchingWait;
                                SetWaitingPlanesAfterMovingPlanes();

                                if (squadronsInState[currentSquadronLane] == null)
                                {
                                    if (movingSquadronSlot == -1)
                                    {
                                        allPlanes.Add(plane);
                                        Assert.IsFalse(allPlanes.Count == 0);
                                        FinishMovement();
                                    }
                                    else
                                    {
                                        //same as normal last stage
                                        inCancel = false;
                                        FillPlaneLanes();
                                        Assert.IsFalse(allPlanes.Count == 0);
                                    }
                                }
                                else
                                {
                                    var plane2 = squadronsInState[currentSquadronLane].Planes[0];
                                    Assert.IsTrue(plane2.Path.Count == 0);
                                    allPlanes.Add(plane2);

                                    var path = generatedNodes.PathNodes[EPlaneNodeGroup.WaitLaunching][EPlaneNodeGroup.DeckLaunching];
                                    StartPlaneDeckReturn(plane2, currentSquadronLane, 0, deck, path);
                                    Assert.IsFalse(allPlanes.Count == 0);
                                }
                            }
                            else
                            {
                                //plane.Rotate(startingRotation);
                                plane = GetPrevPlane(plane, out int index);
                                Assert.IsTrue(plane.Path.Count == 0);
                                Assert.IsFalse(planesInDestination.Contains(plane));

                                var path = generatedNodes.PathNodes[EPlaneNodeGroup.Starting][EPlaneNodeGroup.DeckLaunching];
                                StartPlaneDeckReturnThroughFirstLine(plane, movingSquadronSlot, index, index, deck, path);
                            }
                        }
                        break;
                    case EPlaneMoveStage.LaunchingWaitToDeckLaunching:
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case EPlaneDirection.SwapLaunching:
                CancelSwapNextStep(plane, EPlaneNodeGroup.DeckLaunching, EPlaneNodeGroup.WaitLaunching, EPlaneNodeGroup.SwapLaunching, generatedNodes.DeckLaunchingRotations);
                break;
            case EPlaneDirection.SwapRecovering:
                CancelSwapNextStep(plane, EPlaneNodeGroup.DeckRecovering, EPlaneNodeGroup.WaitRecovering, EPlaneNodeGroup.SwapRecovering, generatedNodes.DeckRecoveringRotations);
                break;
            case EPlaneDirection.LandingToHangar:
            case EPlaneDirection.LandingToDeckRecovering:
            default:
                Assert.IsTrue(false);
                break;
        }
    }

    private void OnAnimationFinished(PlaneMovement plane)
    {
        var planes = plane.Squadron.Planes;
        Assert.IsFalse(inCancel && currentMovement != EPlaneDirection.DeckLaunchingToAirLaunching);

        var planeCrewMan = PlaneCrewMovementManager.Instance;
        if (currentMovement == EPlaneDirection.LandingToHangar)
        {
            planeCrewMan.MoveCrewToLandingPlane(plane, sentCount - sent);
            if (--sent == 0)
            {
                animationActive = false;
                //added handling multiple lifts
                int count = parallelLifts;
                if (parallelLifts > 2 && plane.Squadron.Planes.Count < 3)
                {
                    count = 2;
                }
                for (int i = 0; i < count; i++)
                {
                    var plane2 = plane.Squadron.Planes[i];
                    plane2.SnapRotation = false;
                    StartPlaneSingleLane(plane2, EPlaneNodeGroup.Landing, EPlaneNodeGroup.Hangar, i, i);
                }
            }
        }
        else if (currentMovement == EPlaneDirection.LandingToDeckRecovering)
        {
            planeCrewMan.MoveCrewToLandingPlane(plane, sentCount - sent);

            if (--sent == 0)
            {
                animationActive = false;
                var to = generatedNodes.GroupNodes[EPlaneNodeGroup.DeckRecovering];
                int slot = GetFreeSlot(to);
                var path = generatedNodes.PathNodes[EPlaneNodeGroup.Landing][EPlaneNodeGroup.DeckRecovering];
                var rotations = generatedNodes.DeckRecoveringRotations[slot];
                for (int i = 0; i < planes.Count; i++)
                {
                    var plane2 = planes[i];
                    plane2.RotationAtDestination = rotations[i];
                    //plane2.LaterRotationConstraint = deckRecoveringRotationConstraints[i];
                    StartPlaneDeckReturn(plane2, slot, i, to, path);
                }
            }

            //if (plane == planes[planes.Count - 1])
            //{
            //    animationActive = false;
            //    var to = generatedNodes.GroupNodes[EPlaneNodeGroup.DeckRecovering];
            //    int slot = GetFreeSlot(to);
            //    var path = generatedNodes.PathNodes[EPlaneNodeGroup.Landing][EPlaneNodeGroup.DeckRecovering];
            //    for (int i = 0; i < planes.Count; i++)
            //    {
            //        StartPlaneDeckReturn(planes[i], slot, i, to, path);
            //    }
            //}
            //else
            //{
            //    plane = GetNextPlane(plane, out int index);
            //    plane.gameObject.SetActive(true);
            //    plane.Land(OnAnimationFinished, index);
            //}
        }
        else
        {
            animationActive = false;
            Assert.IsTrue(currentMovement == EPlaneDirection.DeckLaunchingToAirLaunching);
            if (plane == planes[planes.Count - 1])
            {
                movingSquadrons.RemoveAt(0);
                movedSquadrons.Add(plane.Squadron);
                squadrons.Remove(plane.Squadron);

                foreach (var plane2 in plane.Squadron.Planes)
                {
                    Assert.IsFalse(registered.Contains(plane2));
                }
                squadronsInState[movingSquadronSlot] = null;

                if (inCancel)
                {
                    movingSquadrons.Clear();
                    inCancel = false;
                }

                if (movingSquadrons.Count == 0)
                {
                    bool squadronsToMove = false;
                    bool emptySquadron = false;
                    for (int i = 0; i < squadronsInState.Count; i++)
                    {
                        if (squadronsInState[i] == null)
                        {
                            emptySquadron = true;
                        }
                        else if (emptySquadron)
                        {
                            squadronsToMove = true;
                            break;
                        }
                    }
                    //no more planes on deck launching
                    if (squadronsToMove)
                    {
                        FillPlaneLanes();
                    }
                    else
                    {
                        FinishMovement();
                    }
                }
                //loop - another squadron
                else
                {
                    int oldSlot = movingSquadronSlot;
                    movingSquadronSlot = squadronsInState.IndexOf(movingSquadrons[0]);
                    Assert.IsFalse(movingSquadronSlot < 0);
                    Assert.IsTrue(movingSquadronSlot < oldSlot);
                    foreach (var squadron in movingSquadrons)
                    {
                        Assert.IsFalse(movingSquadronSlot < squadronsInState.IndexOf(squadron));
                    }
                    var fromDeck = generatedNodes.GroupNodes[EPlaneNodeGroup.DeckLaunching];

                    lastLane = lastLane && oldSlot == (movingSquadronSlot + 1);

                    if (lastLane)
                    {
                        //move straight
                        currentStageInMovement = EPlaneMoveStage.DeckLaunchingToStarting_Loop;

                        var path = generatedNodes.PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.Starting];
                        var to = generatedNodes.GroupNodes[EPlaneNodeGroup.Starting];
                        for (int i = 0; i < movingSquadrons[0].Planes.Count; i++)
                        {
                            var plane2 = movingSquadrons[0].Planes[i];
                            StartPlaneDeckBasic(plane2, movingSquadronSlot, i, i, fromDeck, to, path);
                            plane2.Delay = launchDelay[i];
                        }
                    }
                    else
                    {
                        currentSquadronLane = oldSlot - 1;
                        if (currentSquadronLane == movingSquadronSlot)
                        {
                            planesInDestination.Clear();
                            currentStageInMovement = EPlaneMoveStage.DeckLaunchingToStarting_Loop;

                            var path = generatedNodes.PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.Starting];
                            StartPlaneDeckBasic(movingSquadrons[0].Planes[0], movingSquadronSlot, 0, 0, fromDeck, generatedNodes.GroupNodes[EPlaneNodeGroup.Starting], path);
                        }
                        else
                        {
                            currentStageInMovement = EPlaneMoveStage.DeckLaunchingToLaunchingWait;

                            var path = generatedNodes.PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.WaitLaunching];
                            StartPlaneDeckToWaitBasic(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, fromDeck, generatedNodes.GroupNodes[EPlaneNodeGroup.WaitLaunching], path);
                        }
                    }
                }
            }
            else
            {
                plane = GetNextPlane(plane, out _);
                plane.Path.Add(plane.CurrentNode);
                plane.Path.Add(generatedNodes.GroupNodes[EPlaneNodeGroup.AirLaunching].LineNodes[0][0]);
                Register(plane);
                plane.StartMovement();
            }
        }
    }

    private void FromHangarNextStep(PlaneMovement plane, EPlaneMoveStage firstStage, EPlaneMoveStage secondStage, EPlaneNodeGroup firstDestination, EPlaneNodeGroup secondDestination)
    {
        Assert.IsTrue(currentMovement == EPlaneDirection.HangarToDeckLaunching ? (firstStage == EPlaneMoveStage.HangarToLiftLaunching) : (firstStage == EPlaneMoveStage.HangarToLiftRecovering));
        Assert.IsTrue(currentMovement == EPlaneDirection.HangarToDeckLaunching ? (secondStage == EPlaneMoveStage.LiftLaunchingToDeckLaunching) : (secondStage == EPlaneMoveStage.LiftRecoveringToDeckRecovering));
        Assert.IsTrue(currentMovement == EPlaneDirection.HangarToDeckLaunching ? (firstDestination == EPlaneNodeGroup.LiftLaunchingToDeck) : (firstDestination == EPlaneNodeGroup.LiftRecoveringToDeck));
        Assert.IsTrue(currentMovement == EPlaneDirection.HangarToDeckLaunching ? (secondDestination == EPlaneNodeGroup.DeckLaunching) : (secondDestination == EPlaneNodeGroup.DeckRecovering));
        if (currentStageInMovement == firstStage)
        {
            Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
            if (allPlanes.Count == planesInDestination.Count)
            {
                currentStageInMovement = secondStage;
                SetLiftsState(1f);

                var group = generatedNodes.GroupNodes[secondDestination];
                int slot = GetFreeSlot(group);

                planesInDestination.Clear();

                var planes = plane.Squadron.Planes;
                for (int i = planes.Count; i > 0; i--)
                {
                    for (int j = planes.Count; j > i; j--)
                    {
                        planes[i - 1].Dependencies.Add(planes[j - 1]);
                    }
                }

                var path = generatedNodes.PathNodes[firstDestination][secondDestination];
                for (int i = 0; i < planes.Count; i++)
                {
                    StartPlaneDeckReturn(planes[i], slot, i, group, path);
                }
            }
            else
            {
                //added handling multiple lifts
                int index = plane.Squadron.Planes.IndexOf(plane);
                if (parallelLifts == 1 || (parallelLifts == 2 && index == 1))
                {
                    plane = GetNextPlane(plane, index, out index);
                    StartPlaneSingleLane(plane, EPlaneNodeGroup.Hangar, firstDestination, (parallelLifts == 1 ? 0 : 2), index);
                }
            }
        }
        else
        {
            Assert.IsTrue(currentStageInMovement == secondStage);
            Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
            PlaneRotateLateConstraint(plane);

            if (planesInDestination.Count == allPlanes.Count)
            {
                plane.Squadron.FromHangar = false;
                FinishMovement();
            }
        }
    }

    private void ToHangarNextStep(PlaneMovement plane, EPlaneNodeGroup from, EPlaneNodeGroup firstDestination, EPlaneNodeGroup secondDestination)
    {
        Assert.IsTrue(currentStageInMovement == EPlaneMoveStage.DeckLaunchingToLaunchingWait_Hangar || currentStageInMovement == EPlaneMoveStage.DeckLaunchingToLiftLaunching ||
            currentStageInMovement == EPlaneMoveStage.LiftLaunchingToHangar || currentStageInMovement == EPlaneMoveStage.LauchingWaitToDeckLaunching);
        Assert.IsTrue(from == EPlaneNodeGroup.DeckLaunching || from == EPlaneNodeGroup.DeckRecovering);
        Assert.IsTrue(from == EPlaneNodeGroup.DeckLaunching ? firstDestination == EPlaneNodeGroup.WaitLaunching : firstDestination == EPlaneNodeGroup.WaitRecovering);
        Assert.IsTrue(from == EPlaneNodeGroup.DeckLaunching ? secondDestination == EPlaneNodeGroup.LiftLaunchingToHangar : secondDestination == EPlaneNodeGroup.LiftRecoveringToHangar);
        switch (currentStageInMovement)
        {
            case EPlaneMoveStage.DeckLaunchingToLaunchingWait_Hangar:
                //case EPlaneMoveStage.DeckRecoveringToRecoveringWait:
                var fromDeck = generatedNodes.GroupNodes[from];
                //moved all planes out of way?
                if (--currentSquadronLane == movingSquadronSlot)
                {
                    planesInDestination.Clear();
                    currentStageInMovement = EPlaneMoveStage.DeckLaunchingToLiftLaunching;

                    var path = generatedNodes.PathNodes[from][secondDestination];
                    Assert.IsTrue(movingSquadrons.Count == 1);
                    StartPlaneDeckBasic(movingSquadrons[0].Planes[0], movingSquadronSlot, 0, 0, fromDeck, generatedNodes.GroupNodes[secondDestination], path);
                }
                else
                {
                    var path = generatedNodes.PathNodes[from][firstDestination];
                    StartPlaneDeckToWaitBasic(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, fromDeck, generatedNodes.GroupNodes[firstDestination], path);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingToLiftLaunching:
                //case EPlaneMoveStage.DeckRecoveringToLiftRecovering:
                //move planes to lift position
                Assert.IsTrue(movingSquadrons.Count == 1);
                Assert.IsTrue(plane.Squadron == movingSquadrons[0]);
                foreach (var plane2 in planesInDestination)
                {
                    Assert.IsTrue(plane.Squadron.Planes.Contains(plane2));
                }
                //moved all planes from moving squadron to lift?
                Assert.IsFalse(planesInDestination.Count > movingSquadrons[0].Planes.Count);
                if (planesInDestination.Count == plane.Squadron.Planes.Count)
                {
                    planesInDestination.Clear();
                    allPlanes.Clear();
                    foreach (var plane2 in movingSquadrons[0].Planes)
                    {
                        allPlanes.Add(plane2);
                    }
                    Assert.IsFalse(allPlanes.Count == 0);
                    currentStageInMovement = EPlaneMoveStage.LiftLaunchingToHangar;

                    for (int i = 0; i < parallelLifts; i++)
                    {
                        var plane2 = movingSquadrons[0].Planes[i];
                        plane2.IgnoreOthers = true;
                        StartPlaneSingleLane(plane2, secondDestination, EPlaneNodeGroup.Hangar, i, i);
                    }
                    //move from waiting to their positions, from later lanes than movingSquadronSlot move 1 more
                }
                //if it is last lane, they are all moving, no need to move plane
                else if (!lastLane)
                {
                    plane = GetNextPlane(plane, out int index);
                    Assert.IsTrue(plane.Path.Count == 0);
                    var fromDeck2 = generatedNodes.GroupNodes[from];
                    var path = generatedNodes.PathNodes[from][secondDestination];
                    StartPlaneDeckBasic(plane, movingSquadronSlot, index, 0, fromDeck2, generatedNodes.GroupNodes[secondDestination], path, true);
                }
                break;
            case EPlaneMoveStage.LiftLaunchingToHangar:
                Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
                plane.IgnoreOthers = false;
                //finished?
                if (allPlanes.Count == planesInDestination.Count)
                {
                    if (lastLane)
                    {
                        foreach (var squadron in movingSquadrons)
                        {
                            FreePlanes(squadron);
                        }
                        FinishMovement();
                    }
                    else
                    {
                        currentStageInMovement = EPlaneMoveStage.LauchingWaitToDeckLaunching;

                        planesInDestination.Clear();
                        var toDeck = generatedNodes.GroupNodes[from];
                        for (int i = movingSquadronSlot + 1; i < squadronsInState.Count; i++)
                        {
                            var lane = toDeck.LineNodes[i - 1];
                            //move plane1, plane2 from next lane to prev lane
                            for (int j = 1; j < squadronsInState[i].Planes.Count; j++)
                            {
                                var plane2 = squadronsInState[i].Planes[j];
                                //plane2.MoveBackwards = false;
                                StartPlaneOneNode(plane2, lane[j]);
                            }
                        }
                        currentSquadronLane = movingSquadronSlot + 1;
                        plane = squadronsInState[currentSquadronLane].Planes[0];
                        //plane.MoveBackwards = false;
                        //move plane 0 from waiting to prevLane
                        var path = generatedNodes.PathNodes[firstDestination][from];
                        StartPlaneDeckReturn(plane, movingSquadronSlot, 0, toDeck, path);
                    }
                }
                else
                {
                    //added handling multiple lifts
                    //move another plane from his squadron
                    //AircraftCarrierDeckManager.Instance.ElevatorState = 1f;
                    int index = plane.Squadron.Planes.IndexOf(plane);

                    if (parallelLifts == 1 || (parallelLifts == 2 && index == 1))
                    {
                        plane = GetNextPlane(plane, index, out index);
                        StartPlaneSingleLane(plane, secondDestination, EPlaneNodeGroup.Hangar, (parallelLifts == 1 ? 0 : 2), index);
                    }
                }
                break;
            case EPlaneMoveStage.LauchingWaitToDeckLaunching:
                plane.Rotate(plane.RotationAtDestination);

                //move another plane from waiting if it is first planae
                if (plane == plane.Squadron.Planes[0])
                {
                    //if there is any not in place
                    if (++currentSquadronLane < squadronsInState.Count)
                    {
                        plane = squadronsInState[currentSquadronLane].Planes[0];
                        //plane.MoveBackwards = false;

                        var path = generatedNodes.PathNodes[firstDestination][from];
                        StartPlaneDeckReturn(plane, currentSquadronLane - 1, 0, generatedNodes.GroupNodes[from], path);
                    }
                    else
                    {
                        foreach (var squadron in movingSquadrons)
                        {
                            FreePlanes(squadron);
                        }
                        FinishMovement();
                    }
                }
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
    }

    private void SwapNextStep(PlaneMovement plane, EPlaneNodeGroup from, EPlaneNodeGroup waiting, EPlaneNodeGroup swapWaiting, List<List<Quaternion>> rotations)
    {
        Assert.IsTrue(from == EPlaneNodeGroup.DeckLaunching || from == EPlaneNodeGroup.DeckRecovering);
        Assert.IsTrue(from == EPlaneNodeGroup.DeckLaunching ? waiting == EPlaneNodeGroup.WaitLaunching : waiting == EPlaneNodeGroup.WaitRecovering);
        Assert.IsTrue(from == EPlaneNodeGroup.DeckLaunching ? swapWaiting == EPlaneNodeGroup.SwapLaunching : swapWaiting == EPlaneNodeGroup.SwapRecovering);

        var fromNodes = generatedNodes.GroupNodes[from];
        switch (currentStageInMovement)
        {
            case EPlaneMoveStage.DeckLaunchingXToLaunchingWait:
                if (PlanesToWait(fromNodes, from, waiting))
                {
                    currentStageInMovement = EPlaneMoveStage.DeckLaunchingAToSwapLaunching;

                    var planesA = squadronsInState[movingSquadronSlot].Planes;
                    StartPlaneDeckToSwap(planesA[0], movingSquadronSlot, 0, 0, planesA.Count - 1, fromNodes, generatedNodes.GroupNodes[swapWaiting], generatedNodes.PathNodes[from][swapWaiting]);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingAToSwapLaunching:
                if (allPlanes.Count == planesInDestination.Count)
                {
                    movingSquadronSlot = lineB;
                    allPlanes.Clear();
                    var planesB = squadronsInState[lineB].Planes;
                    for (int j = 0; j < planesB.Count; j++)
                    {
                        allPlanes.Add(planesB[j]);
                    }
                    Assert.IsFalse(allPlanes.Count == 0);

                    lastLaneB = --currentSquadronLane == movingSquadronSlot;
                    if (lastLaneB)
                    {
                        //no lanes between A&B

                        planesInDestination.Clear();
                        currentStageInMovement = EPlaneMoveStage.DeckLaunchingBSwap;

                        waitingBLine = StartPlaneDeckToWaitBasic(planesB[0], lineB, fromNodes, generatedNodes.GroupNodes[waiting], generatedNodes.PathNodes[from][waiting]);
                        ChangeRotationConstraints(lineB, rotations);
                        for (int j = 1; j < planesB.Count; j++)
                        {
                            StartPlaneOneNode(planesB[j], fromNodes.LineNodes[lineA][j]);
                        }
                    }
                    else
                    {
                        currentStageInMovement = EPlaneMoveStage.DeckLaunchingYToLaunchingWait;

                        var path = generatedNodes.PathNodes[from][waiting];
                        StartPlaneDeckToWaitBasic(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, fromNodes, generatedNodes.GroupNodes[waiting], path);
                    }
                }
                //last lane is moving all
                else if (!lastLane)
                {
                    plane = GetNextPlane(plane, out int index);
                    int toIndex = plane.Squadron.Planes.Count - index - 1;
                    StartPlaneDeckToSwap(plane, movingSquadronSlot, index, 0, toIndex, fromNodes, generatedNodes.GroupNodes[swapWaiting], generatedNodes.PathNodes[from][swapWaiting]);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingYToLaunchingWait:
                if (PlanesToWait(fromNodes, from, waiting))
                {
                    currentStageInMovement = EPlaneMoveStage.DeckLaunchingBSwap;

                    ChangeRotationConstraints(lineB, rotations);
                    StartPlaneSwap(squadronsInState[lineB].Planes[0], 0, lineB, lineA, fromNodes);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingBSwap:
                int count = planesInDestination.Count;
                if (allPlanes.Count == count)
                {
                    if (!lastLaneB)
                    {
                        PlaneRotateLateConstraint(plane);
                    }
                    allPlanes.Clear();
                    planesInDestination.Clear();
                    var planesA = squadronsInState[lineA].Planes;
                    for (int i = 0; i < planesA.Count; i++)
                    {
                        allPlanes.Add(planesA[i]);
                    }
                    Assert.IsFalse(allPlanes.Count == 0);
                    ReturnPlanesA(planesA, fromNodes, from, swapWaiting, rotations);
                }
                else
                {
                    PlaneRotateLateConstraint(plane);
                    if (!lastLaneB)
                    {
                        plane = GetNextPlane(plane, out int index);
                        if (allPlanes.Count == ++count)
                        {
                            waitingBLine = StartPlaneDeckToWaitBasic(plane, lineB, fromNodes, generatedNodes.GroupNodes[waiting], generatedNodes.PathNodes[from][waiting]);
                        }
                        else
                        {
                            StartPlaneSwap(plane, index, lineB, lineA, fromNodes);
                        }
                    }
                }
                break;
            case EPlaneMoveStage.DeckLaunchingASwap:
                PlaneRotateLateConstraint(plane);
                var swapNodes = generatedNodes.GroupNodes[swapWaiting];
                if (allPlanes.Count == planesInDestination.Count)
                {
                    var planes = squadronsInState[lineB].Planes;
                    if (lastLaneB)
                    {
                        currentStageInMovement = EPlaneMoveStage.SwapLaunchingBToDeckLaunching;
                        StartPlaneDeckReturnThroughFirstLine(planes[0], lineA, 0, 0, fromNodes, generatedNodes.PathNodes[waiting][from]);
                    }
                    else
                    {
                        currentStageInMovement = EPlaneMoveStage.LaunchingWaitBToSwapLaunching;
                        StartPlaneWaitToSwap(planes[planes.Count - 1], swapNodes, generatedNodes.PathNodes[waiting][swapWaiting], waitingBLine);
                    }
                }
                else
                {
                    int index;
                    int index2 = plane.Squadron.Planes.Count - 1;
                    if (lastLane)
                    {
                        plane = GetNextPlane(plane, out index);
                        index2 -= index;
                    }
                    else
                    {
                        plane = GetPrevPlane(plane, out index);
                        index2 -= index;
                        (index, index2) = (index2, index);
                    }
                    StartPlaneDeckSwapReturn(plane, lineB, index, index2, swapNodes, fromNodes, generatedNodes.PathNodes[swapWaiting][from]);
                }
                break;
            case EPlaneMoveStage.LaunchingWaitBToSwapLaunching:
                currentStageInMovement = EPlaneMoveStage.LaunchingWaitYToDeckLaunching;
                movingSquadronSlot = lineA;
                currentSquadronLane = lineB;
                bool x = PlanesFromWait(fromNodes, from, waiting);
                Assert.IsFalse(x);
                break;
            case EPlaneMoveStage.LaunchingWaitYToDeckLaunching:
                PlaneRotateLateConstraint(plane);
                if (PlanesFromWait(fromNodes, from, waiting))
                {
                    currentStageInMovement = EPlaneMoveStage.SwapLaunchingBToDeckLaunching;
                    var planesB = squadronsInState[lineB].Planes;
                    StartPlaneDeckReturnThroughFirstLine(planesB[lastLaneB ? 0 : planesB.Count - 1], lineA, 0, 0, fromNodes, generatedNodes.PathNodes[swapWaiting][from]);
                }
                break;
            case EPlaneMoveStage.SwapLaunchingBToDeckLaunching:
                PlaneRotateLateConstraint(plane);
                if (lastLane)
                {
                    FinishMovement();
                }
                else
                {
                    currentStageInMovement = EPlaneMoveStage.LaunchingWaitXToDeckLaunching;
                    movingSquadronSlot = squadronsInState.Count;
                    currentSquadronLane = lineA;
                    bool y = PlanesFromWait(fromNodes, from, waiting);
                    Assert.IsFalse(y);
                    if ((currentSquadronLane + 1) == movingSquadronSlot)
                    {
                        currentSquadronLane.ToString();
                    }
                }
                break;
            case EPlaneMoveStage.LaunchingWaitXToDeckLaunching:
                PlaneRotateLateConstraint(plane);
                if (PlanesFromWait(fromNodes, from, waiting))
                {
                    FinishMovement();
                }
                if ((currentSquadronLane + 1) == movingSquadronSlot)
                {
                    currentSquadronLane.ToString();
                }
                break;
        }
    }

    private void SwapToFrontNextStep(PlaneMovement plane, EPlaneNodeGroup deck, EPlaneNodeGroup waiting, EPlaneNodeGroup swapWaiting, List<List<Quaternion>> rotations)
    {
        Assert.IsTrue(deck == EPlaneNodeGroup.DeckLaunching || deck == EPlaneNodeGroup.DeckRecovering);
        Assert.IsTrue(deck == EPlaneNodeGroup.DeckLaunching ? waiting == EPlaneNodeGroup.WaitLaunching : waiting == EPlaneNodeGroup.WaitRecovering);
        Assert.IsTrue(deck == EPlaneNodeGroup.DeckLaunching ? swapWaiting == EPlaneNodeGroup.SwapLaunching : swapWaiting == EPlaneNodeGroup.SwapRecovering);

        var deckNodes = generatedNodes.GroupNodes[deck];
        switch (currentStageInMovement)
        {
            case EPlaneMoveStage.DeckLaunchingXToLaunchingWait_Front:
                if (PlanesToWait(deckNodes, deck, waiting))
                {
                    currentStageInMovement = EPlaneMoveStage.DeckLaunchingAToSwapLaunching_Front;

                    allPlanes.Clear();
                    planesInDestination.Clear();
                    var planesA = squadronsInState[movingSquadronSlot].Planes;
                    foreach (var planeA in planesA)
                    {
                        allPlanes.Add(planeA);
                    }
                    Assert.IsFalse(allPlanes.Count == 0);
                    StartPlaneDeckToSwap(planesA[0], movingSquadronSlot, 0, 0, planesA.Count - 1, deckNodes, generatedNodes.GroupNodes[swapWaiting], generatedNodes.PathNodes[deck][swapWaiting]);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingAToSwapLaunching_Front:
                if (allPlanes.Count == planesInDestination.Count)
                {
                    allPlanes.Clear();
                    planesInDestination.Clear();
                    currentStageInMovement = EPlaneMoveStage.LaunchingWaitXToDeckLaunchingAll;
                    currentSquadronLane = lineA + 1;
                    StartPlaneDeckReturnThroughFirstLine(squadronsInState[currentSquadronLane].Planes[0], lineA, 0, 0, deckNodes, generatedNodes.PathNodes[deck][waiting]);
                    for (int i = currentSquadronLane; i < squadronsInState.Count; i++)
                    {
                        var planes = squadronsInState[i].Planes;
                        allPlanes.Add(planes[0]);
                        for (int j = 1; j < planes.Count; j++)
                        {
                            allPlanes.Add(planes[j]);
                            for (int k = i + 1; k < squadronsInState.Count; k++)
                            {
                                var planes2 = squadronsInState[k].Planes;
                                for (int l = 1; l < planes2.Count; l++)
                                {
                                    planes[j].Dependencies.Add(planes2[l]);
                                }
                            }
                            StartPlaneOneNode(planes[j], deckNodes.LineNodes[i - 1][j]);
                        }
                    }
                    Assert.IsFalse(allPlanes.Count == 0);
                }
                else
                {
                    plane = GetNextPlane(plane, out int index);
                    int toIndex = plane.Squadron.Planes.Count - index - 1;
                    StartPlaneDeckToSwap(plane, movingSquadronSlot, index, 0, toIndex, deckNodes, generatedNodes.GroupNodes[swapWaiting], generatedNodes.PathNodes[deck][swapWaiting]);
                }
                break;
            case EPlaneMoveStage.LaunchingWaitXToDeckLaunchingAll:
                PlaneRotateLateConstraint(plane);
                if (allPlanes.Count == planesInDestination.Count)
                {
                    currentStageInMovement = EPlaneMoveStage.SwapLaunchingAToDeckLaunching;
                    allPlanes.Clear();
                    planesInDestination.Clear();
                    var planesA = squadronsInState[lineA].Planes;
                    var path = generatedNodes.PathNodes[swapWaiting][deck];
                    int index = squadronsInState.Count - 1;
                    for (int i = 0; i < planesA.Count; i++)
                    {
                        int planeIndex = planesA.Count - i - 1;
                        allPlanes.Add(planesA[planeIndex]);
                        planesA[planeIndex].RotationAtDestination = rotations[index][i];
                        StartPlaneDeckReturn(planesA[planeIndex], index, i, deckNodes, path);
                    }
                    Assert.IsFalse(allPlanes.Count == 0);
                }
                else if (plane.Squadron.Planes.IndexOf(plane) == 0 && ++currentSquadronLane < squadronsInState.Count)
                {
                    var path = generatedNodes.PathNodes[waiting][deck];
                    StartPlaneDeckReturnThroughFirstLine(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane - 1, 0, 0, deckNodes, path);
                }
                break;
            case EPlaneMoveStage.SwapLaunchingAToDeckLaunching:
                PlaneRotateLateConstraint(plane);
                if (allPlanes.Count == planesInDestination.Count)
                {
                    FinishMovement();
                }
                break;
        }
    }

    private void CancelBetweenLaunchingRecovering(EPlaneNodeGroup formerFrom)
    {
        bool formerFromLaunching = formerFrom == EPlaneNodeGroup.DeckLaunching;
        Assert.IsTrue(formerFromLaunching || formerFrom == EPlaneNodeGroup.DeckRecovering);
        var formerTo = formerFromLaunching ? EPlaneNodeGroup.DeckRecovering : EPlaneNodeGroup.DeckLaunching;
        var formerFromNodes = generatedNodes.GroupNodes[formerFrom];
        var formerToNodes = generatedNodes.GroupNodes[formerTo];
        var path = generatedNodes.PathNodes[formerTo][formerFrom];
        foreach (var plane in allPlanes)
        {
            //plane.MoveBackwards = !plane.MoveBackwards;
            if (planesInDestination.Contains(plane))
            {
                int toSlot = squadronsInState.IndexOf(plane.Squadron);
                int fromSlot = squadronsInState.Count - toSlot - 1;

                StartPlaneLaunchingRecovering(plane, plane.Squadron.Planes.IndexOf(plane), fromSlot, toSlot, formerToNodes, formerFromNodes, path);
            }
            else
            {
                plane.Return();
            }
            Assert.IsTrue(registered.Contains(plane));
        }
        planesInDestination.Clear();
    }

    private void CancelFromHangar(EPlaneNodeGroup formerTo)
    {
        EPlaneMoveStage firstStage;
        EPlaneMoveStage secondStage;
        EPlaneNodeGroup liftDestination;
        if (formerTo == EPlaneNodeGroup.DeckLaunching)
        {
            firstStage = EPlaneMoveStage.HangarToLiftLaunching;
            secondStage = EPlaneMoveStage.LiftLaunchingToDeckLaunching;
            liftDestination = EPlaneNodeGroup.LiftLaunchingToDeck;
        }
        else
        {
            Assert.IsTrue(formerTo == EPlaneNodeGroup.DeckRecovering);
            firstStage = EPlaneMoveStage.HangarToLiftRecovering;
            secondStage = EPlaneMoveStage.LiftRecoveringToDeckRecovering;
            liftDestination = EPlaneNodeGroup.LiftRecoveringToDeck;
        }

        Assert.IsTrue(allPlanes.Count == PlaneCount);
        SetLiftsState(1f);
        if (currentStageInMovement == firstStage)
        {
            foreach (var plane2 in planesInDestination)
            {
                Assert.IsFalse(plane2.Squadron.Planes.IndexOf(plane2) == -1);
                Assert.IsTrue(plane2.Squadron.Planes.IndexOf(plane2) < planesInDestination.Count);
            }
            Assert.IsFalse(planesInDestination.Count == allPlanes.Count);

            int start = planesInDestination.Count;
            var squadron = allPlanes.First().Squadron;
            //var plane = .Planes[start];
            for (int i = start; i < squadron.Planes.Count; i++)
            {
                Assert.IsFalse(planesInDestination.Contains(squadron.Planes[i]));
            }
            planesInDestination.Clear();
            for (int i = start + 1; i < squadron.Planes.Count; i++)
            {
                var plane2 = squadron.Planes[i];
                Assert.IsFalse((i == start) == (plane2.Path.Count == 0));
                planesInDestination.Add(plane2);
                Assert.IsFalse(registered.Contains(plane2));
                Assert.IsTrue(plane2.Path.Count == 0);
            }
            var plane = squadron.Planes[start];
            Assert.IsFalse(plane.Path.Count == 0);
            Assert.IsTrue(registered.Contains(plane));
            plane.Return();
        }
        else
        {
            Assert.IsTrue(currentStageInMovement == secondStage);
            var deck = generatedNodes.GroupNodes[formerTo];
            var lift = generatedNodes.GroupNodes[liftDestination];
            var path = generatedNodes.PathNodes[formerTo][liftDestination];
            foreach (var plane in allPlanes)
            {
                //plane.MoveBackwards = false;
                if (planesInDestination.Contains(plane))
                {
                    int planeIndex = plane.Squadron.Planes.IndexOf(plane);
                    int slot = -1;
                    for (int i = 0; i < deck.LineNodes.Count; i++)
                    {
                        if (deck.LineNodes[i].Contains(plane.CurrentNode))
                        {
                            slot = i;
                            break;
                        }
                    }
                    Assert.IsFalse(slot == -1);
                    Assert.IsTrue(plane.Path.Count == 0);
                    Assert.IsTrue(plane.CurrentNode == deck.LineNodes[slot][planeIndex]);
                    for (int i = slot; i < deck.LineNodes.Count; i++)
                    {
                        plane.Path.Add(deck.LineNodes[i][planeIndex]);
                    }
                    plane.Path.AddRange(path.LineNodes[0]);
                    plane.Path.Add(lift.LineNodes[0][planeIndex]);
                    Register(plane);
                    plane.StartMovement();
                }
                else
                {
                    plane.Return();
                }
                Assert.IsTrue(registered.Contains(plane));
            }
            planesInDestination.Clear();
        }
    }

    private bool CancelToHangar(EPlaneNodeGroup deckGroup)
    {
        EPlaneNodeGroup lift;
        if (deckGroup == EPlaneNodeGroup.DeckLaunching)
        {
            lift = EPlaneNodeGroup.LiftLaunchingToHangar;
        }
        else
        {
            Assert.IsTrue(deckGroup == EPlaneNodeGroup.DeckRecovering);
            lift = EPlaneNodeGroup.LiftRecoveringToHangar;
        }

        //foreach (var plane2 in allPlanes)
        //{
        //    plane2.MoveBackwards = false;
        //}
        switch (currentStageInMovement)
        {
            case EPlaneMoveStage.DeckLaunchingToLaunchingWait_Hangar:
                planesInDestination.Clear();
                allPlanes.Clear();
                for (int i = currentSquadronLane + 1; i < squadronsInState.Count; i++)
                {
                    var plane2 = squadronsInState[i].Planes[0];
                    //plane2.MoveBackwards = false;
                    allPlanes.Add(plane2);
                    Assert.IsTrue(plane2.Path.Count == 0);
                }
                for (int i = 0; i < currentSquadronLane; i++)
                {
                    var plane2 = squadronsInState[i].Planes[0];
                    //plane2.MoveBackwards = false;
                    Assert.IsTrue(plane2.Path.Count == 0);
                }
                var plane = squadronsInState[currentSquadronLane].Planes[0];
                //plane.MoveBackwards = false;
                allPlanes.Add(plane);
                Assert.IsFalse(plane.Path.Count == 0);
                squadronsInState[currentSquadronLane].Planes[0].Return();
                Assert.IsFalse(allPlanes.Count == 0);
                break;
            case EPlaneMoveStage.DeckLaunchingToLiftLaunching:
                if (lastLane)
                {
                    var deck = generatedNodes.GroupNodes[deckGroup];
                    Assert.IsTrue(movingSquadrons.Count == 1);
                    Assert.IsTrue(movingSquadronSlot == squadronsInState.IndexOf(movingSquadrons[0]));
                    var path = generatedNodes.PathNodes[lift][deckGroup];
                    foreach (var plane2 in allPlanes)
                    {
                        Assert.IsTrue(plane2.Squadron == movingSquadrons[0]);
                        //plane2.MoveBackwards = false;
                        if (planesInDestination.Contains(plane2))
                        {
                            StartPlaneDeckReturn(plane2, movingSquadronSlot, plane2.Squadron.Planes.IndexOf(plane2), deck, path);
                        }
                        else
                        {
                            plane2.Return();
                        }
                    }
                    planesInDestination.Clear();
                }
                else
                {
                    ReturnMovingPlanes(true);
                }
                break;
            //just continue movement of all planes as it were, move back moving squadron to last lane
            case EPlaneMoveStage.LiftLaunchingToHangar:
                SetLiftsState(0f);
                ReturnMovingPlanes(false);
                return true;
            default:
                Assert.IsTrue(false);
                break;
        }
        return false;
    }

    private void CancelSwap(EPlaneNodeGroup deck, EPlaneNodeGroup waiting)
    {
        var deckNodes = generatedNodes.GroupNodes[deck];
        var planesB = squadronsInState[lineB].Planes;
        switch (currentStageInMovement)
        {
            case EPlaneMoveStage.DeckLaunchingXToLaunchingWait:
                planesInDestination.Clear();
                movingSquadronSlot = squadronsInState.Count;
                ReturnFromWait();
                break;
            case EPlaneMoveStage.DeckLaunchingAToSwapLaunching:
                var planesA = squadronsInState[lineA].Planes;
                var path = generatedNodes.PathNodes[waiting][deck];
                if (lastLane)
                {
                    foreach (var plane in planesInDestination)
                    {
                        int index = planesA.IndexOf(plane);
                        Assert.IsFalse(index == -1);
                        StartPlaneDeckReturn(plane, lineA, index, deckNodes, path);
                    }
                    for (int i = 0; i < planesA.Count; i++)
                    {
                        if (!planesInDestination.Contains(planesA[i]))
                        {
                            planesA[i].Return();
                        }
                    }
                    planesInDestination.Clear();
                }
                else
                {
                    ReturnSwapMove(planesA, 0, 1);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingYToLaunchingWait:
                planesInDestination.Clear();
                movingSquadronSlot = lineA;
                ReturnFromWait();
                break;
            case EPlaneMoveStage.DeckLaunchingBSwap:
                if (lastLaneB)
                {
                    foreach (var plane in planesInDestination)
                    {
                        int index = planesB.IndexOf(plane);
                        Assert.IsFalse(index == -1);
                        if (index == 0)
                        {
                            StartPlaneDeckReturn(plane, lineB, index, deckNodes, generatedNodes.PathNodes[waiting][deck]);
                        }
                        else
                        {
                            StartPlaneOneNode(plane, deckNodes.LineNodes[lineB][index]);
                        }
                    }
                    for (int i = 0; i < planesB.Count; i++)
                    {
                        if (!planesInDestination.Contains(planesB[i]))
                        {
                            planesB[i].Return();
                        }
                    }
                    planesInDestination.Clear();
                }
                else
                {
                    ReturnSwapMove(planesB, 0, 1);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingASwap:
                var planesA2 = squadronsInState[lineA].Planes;
                if (lastLane)
                {
                    ReturnSwapMove(planesA2, 0, 1);
                }
                else
                {
                    ReturnSwapMove(planesA2, planesA2.Count - 1, -1);
                }
                break;
            case EPlaneMoveStage.LaunchingWaitBToSwapLaunching:
                planesInDestination.Clear();
                planesB[planesB.Count - 1].Return();
                break;
            case EPlaneMoveStage.LaunchingWaitYToDeckLaunching:
                planesInDestination.Clear();
                movingSquadronSlot = lineB;
                ReturnFromWait();
                break;
            case EPlaneMoveStage.SwapLaunchingBToDeckLaunching:
                planesInDestination.Clear();
                planesB[lastLaneB ? 0 : planesB.Count - 1].Return();
                break;
            case EPlaneMoveStage.LaunchingWaitXToDeckLaunching:
                planesInDestination.Clear();
                movingSquadronSlot = lineA;
                ReturnFromWait();
                break;
        }
    }

    private void CancelFromHangarNextStep(PlaneMovement plane, EPlaneNodeGroup lift)
    {
        EPlaneMoveStage firstStage;
        EPlaneMoveStage secondStage;
        if (lift == EPlaneNodeGroup.LiftLaunchingToDeck)
        {
            firstStage = EPlaneMoveStage.HangarToLiftLaunching;
            secondStage = EPlaneMoveStage.LiftLaunchingToDeckLaunching;
        }
        else
        {
            //Assert.IsTrue(lift == EPlaneNodeGroup.LiftRecoveringToDeck);
            firstStage = EPlaneMoveStage.HangarToLiftRecovering;
            secondStage = EPlaneMoveStage.LiftRecoveringToDeckRecovering;
        }
        if (currentStageInMovement == firstStage)
        {
            Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
            if (allPlanes.Count == planesInDestination.Count)
            {
                foreach (var squadron in squadrons)
                {
                    FreePlanes(squadron);
                }
                FinishMovement();
            }
            else
            {
                plane = GetPrevPlane(plane, out int index);
                StartPlaneSingleLane(plane, lift, EPlaneNodeGroup.Hangar, index);
            }
        }
        else
        {
            Assert.IsTrue(currentStageInMovement == secondStage);
            if (allPlanes.Count == planesInDestination.Count)
            {
                currentStageInMovement = firstStage;
                planesInDestination.Clear();

                int index = plane.Squadron.Planes.Count - 1;
                plane = plane.Squadron.Planes[index];
                StartPlaneSingleLane(plane, lift, EPlaneNodeGroup.Hangar, index);
            }
        }
    }

    private void CancelToHangarNextStep(PlaneMovement plane, EPlaneNodeGroup deck)
    {
        EPlaneNodeGroup lift;
        EPlaneNodeGroup wait;
        if (deck == EPlaneNodeGroup.DeckLaunching)
        {
            lift = EPlaneNodeGroup.LiftLaunchingToHangar;
            wait = EPlaneNodeGroup.WaitLaunching;
        }
        else
        {
            Assert.IsTrue(deck == EPlaneNodeGroup.DeckRecovering);
            lift = EPlaneNodeGroup.LiftRecoveringToHangar;
            wait = EPlaneNodeGroup.WaitRecovering;
        }

        switch (currentStageInMovement)
        {
            case EPlaneMoveStage.DeckLaunchingToLaunchingWait_Hangar:
                Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
                PlaneRotateLateConstraint(plane);
                if (allPlanes.Count == planesInDestination.Count)
                {
                    FinishMovement();
                }
                else
                {
                    currentSquadronLane++;
                    Assert.IsTrue(currentSquadronLane < squadronsInState.Count);

                    var path = generatedNodes.PathNodes[wait][deck];
                    StartPlaneDeckReturn(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, 0, generatedNodes.GroupNodes[deck], path);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingToLiftLaunching:
                Assert.IsTrue(movingSquadrons.Count == 1);
                Assert.IsTrue(plane.Squadron == movingSquadrons[0]);
                //if (deck == EPlaneNodeGroup.DeckRecovering)
                //{
                //    plane.Rotate(recoveringRotation);
                //}
                //else
                //{
                //    plane.Rotate(startingRotation);
                //}
                plane.Rotate(plane.RotationAtDestination);
                if (lastLane)
                {
                    Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
                    if (allPlanes.Count == planesInDestination.Count)
                    {
                        FinishMovement();
                    }
                }
                else
                {
                    Assert.IsTrue(movingSquadronSlot == squadronsInState.IndexOf(movingSquadrons[0]));
                    var to = generatedNodes.GroupNodes[deck];
                    if (plane.Squadron.Planes[0] == plane)
                    {
                        foreach (var plane2 in plane.Squadron.Planes)
                        {
                            Assert.IsTrue(planesInDestination.Contains(plane2));
                        }
                        currentStageInMovement = EPlaneMoveStage.DeckLaunchingToLaunchingWait_Hangar;
                        currentSquadronLane = movingSquadronSlot + 1;
                        allPlanes.Clear();
                        for (int i = currentSquadronLane; i < squadronsInState.Count; i++)
                        {
                            allPlanes.Add(squadronsInState[i].Planes[0]);
                        }
                        Assert.IsFalse(allPlanes.Count == 0);
                        planesInDestination.Clear();

                        var path = generatedNodes.PathNodes[wait][deck];
                        StartPlaneDeckReturn(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, 0, to, path);
                    }
                    else
                    {
                        plane = GetPrevPlane(plane, out int index);
                        var path = generatedNodes.PathNodes[lift][deck];
                        StartPlaneDeckReturnThroughFirstLine(plane, movingSquadronSlot, index, index, to, path);
                    }
                }
                break;
            case EPlaneMoveStage.LiftLaunchingToHangar:
                Assert.IsFalse(planesInDestination.Count > allPlanes.Count);
                Assert.IsTrue(movingSquadrons.Count == 1);
                //if (plane.Squadron != movingSquadrons[0])
                //{
                //    if (deck == EPlaneNodeGroup.DeckLaunching)
                //    {
                //        plane.Rotate(startingRotation);
                //    }
                //    else
                //    {
                //        plane.Rotate(recoveringRotation);
                //    }
                //}
                if (allPlanes.Count == planesInDestination.Count)
                {
                    lastLane = true;
                    currentStageInMovement = EPlaneMoveStage.DeckLaunchingToLiftLaunching;
                    var to = generatedNodes.GroupNodes[deck];
                    Assert.IsTrue(GetFreeSlot(to) == (squadronsInState.Count - 1));
                    for (int i = 0; i < movingSquadrons[0].Planes.Count; i++)
                    {
                        var plane2 = movingSquadrons[0].Planes[i];
                        Assert.IsTrue(planesInDestination.Contains(plane2));
                        planesInDestination.Remove(plane2);

                        var path = generatedNodes.PathNodes[lift][deck];
                        StartPlaneDeckReturn(plane2, squadronsInState.Count - 1, i, to, path);
                    }
                }
                //move moving squadron
                else if (plane.Squadron == movingSquadrons[0])
                {
                    //if there is any not in place
                    if (plane.Squadron.Planes[0] != plane)
                    {
                        plane = GetPrevPlane(plane, out int index);
                        StartPlaneSingleLane(plane, EPlaneNodeGroup.Hangar, lift, index);
                    }
                }
                //move planes in waiting
                else if (plane.Squadron.Planes[0] == plane)
                {
                    plane.Rotate(plane.RotationAtDestination);
                    currentSquadronLane++;
                    //if there is any not in place
                    if (currentSquadronLane < squadronsInState.Count)
                    {
                        var path = generatedNodes.PathNodes[wait][deck];
                        StartPlaneDeckReturn(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane - 1, 0, generatedNodes.GroupNodes[deck], path);
                    }
                }
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
    }

    private void CancelSwapNextStep(PlaneMovement plane, EPlaneNodeGroup deck, EPlaneNodeGroup waiting, EPlaneNodeGroup swapWaiting, List<List<Quaternion>> rotations)
    {
        var deckNodes = generatedNodes.GroupNodes[deck];
        var swapNodes = generatedNodes.GroupNodes[swapWaiting];
        var swapDeckPath = generatedNodes.PathNodes[swapWaiting][deck];
        switch (currentStageInMovement)
        {
            case EPlaneMoveStage.DeckLaunchingXToLaunchingWait:
                PlaneRotateLateConstraint(plane);
                if (PlanesFromWait(deckNodes, deck, waiting))
                {
                    FinishMovement();
                }
                break;
            case EPlaneMoveStage.DeckLaunchingAToSwapLaunching:
                PlaneRotateLateConstraint(plane);
                if (planesInDestination.Count == allPlanes.Count)
                {
                    if (lastLane)
                    {
                        FinishMovement();
                    }
                    else
                    {
                        currentStageInMovement = EPlaneMoveStage.DeckLaunchingXToLaunchingWait;
                        currentSquadronLane = lineA;
                        movingSquadronSlot = squadronsInState.Count;
                        bool x = PlanesFromWait(deckNodes, deck, waiting);
                        Assert.IsFalse(x);
                    }
                }
                //last lane is moving all
                else if (!lastLane)
                {
                    plane = GetPrevPlane(plane, out int index);
                    StartPlaneDeckSwapReturn(plane, lineA, index, index, swapNodes, deckNodes, swapDeckPath);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingYToLaunchingWait:
                PlaneRotateLateConstraint(plane);
                if (PlanesFromWait(deckNodes, deck, waiting))
                {
                    CancelReturnPlanesA(deckNodes, swapNodes, swapDeckPath);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingBSwap:
                //plane.MoveBackwards = !plane.MoveBackwards;
                PlaneRotateLateConstraint(plane);
                if (planesInDestination.Count == allPlanes.Count)
                {
                    if (lastLaneB)
                    {
                        CancelReturnPlanesA(deckNodes, swapNodes, swapDeckPath);
                    }
                    else
                    {
                        currentStageInMovement = EPlaneMoveStage.DeckLaunchingYToLaunchingWait;
                        currentSquadronLane = lineB;
                        movingSquadronSlot = lineA;
                        bool x = PlanesFromWait(deckNodes, deck, waiting);
                        Assert.IsFalse(x);
                    }
                }
                //last lane is moving all
                else if (!lastLaneB)
                {
                    plane = GetPrevPlane(plane, out int prevIndex);
                    StartPlaneReverseSwap(plane, prevIndex, lineA, lineB, deckNodes);
                }
                break;
            case EPlaneMoveStage.DeckLaunchingASwap:
                if (planesInDestination.Count == allPlanes.Count)
                {
                    planesInDestination.Clear();
                    allPlanes.Clear();
                    var planesB = squadronsInState[lineB].Planes;
                    for (int i = 0; i < planesB.Count; i++)
                    {
                        //planesB[i].MoveBackwards = !plane.MoveBackwards;
                        planesB[i].RotationAtDestination = rotations[lineB][i];
                        allPlanes.Add(planesB[i]);
                    }
                    Assert.IsFalse(allPlanes.Count == 0);

                    currentStageInMovement = EPlaneMoveStage.DeckLaunchingBSwap;
                    int index;
                    if (lastLaneB)
                    {
                        index = 0;
                        for (int i = 1; i < planesB.Count; i++)
                        {
                            StartPlaneOneNode(planesB[i], deckNodes.LineNodes[lineB][i]);
                        }
                    }
                    else
                    {
                        index = planesB.Count - 1;
                        //StartPlaneReverseSwap(planesB[index], index, lineA, lineB, deckNodes);
                    }
                    StartPlaneDeckReturnThroughFirstLine(planesB[index], lineB, index, 0, deckNodes, generatedNodes.PathNodes[waiting][deck]);
                }
                else
                {
                    int index;
                    int index2 = plane.Squadron.Planes.Count - 1;
                    if (lastLane)
                    {
                        plane = GetPrevPlane(plane, out index);
                        index2 -= index;
                        (index, index2) = (index2, index);
                    }
                    else
                    {
                        plane = GetNextPlane(plane, out index);
                        index2 -= index;
                    }
                    StartPlaneDeckToSwap(plane, lineB, index, 0, index2, deckNodes, swapNodes, generatedNodes.PathNodes[deck][swapWaiting]);
                }
                break;
            case EPlaneMoveStage.LaunchingWaitBToSwapLaunching:
                CancelToASwap(deckNodes, swapNodes, deck, swapWaiting);
                break;
            case EPlaneMoveStage.LaunchingWaitYToDeckLaunching:
                if (PlanesToWait(deckNodes, deck, waiting))
                {
                    currentStageInMovement = EPlaneMoveStage.LaunchingWaitBToSwapLaunching;
                    var planesB = squadronsInState[lineB].Planes;
                    StartPlaneWaitToSwap(planesB[planesB.Count - 1], generatedNodes.GroupNodes[waiting], generatedNodes.PathNodes[swapWaiting][waiting], waitingBLine);
                }
                break;
            case EPlaneMoveStage.SwapLaunchingBToDeckLaunching:
                if (lastLaneB)
                {
                    CancelToASwap(deckNodes, swapNodes, deck, swapWaiting);
                }
                else
                {
                    currentStageInMovement = EPlaneMoveStage.LaunchingWaitYToDeckLaunching;
                    currentSquadronLane = lineA;
                    movingSquadronSlot = lineB;
                    bool x = PlanesToWait(deckNodes, deck, waiting);
                    Assert.IsFalse(x);
                }
                break;
            case EPlaneMoveStage.LaunchingWaitXToDeckLaunching:
                if (PlanesToWait(deckNodes, deck, waiting))
                {
                    currentStageInMovement = EPlaneMoveStage.SwapLaunchingBToDeckLaunching;
                    var planesB = squadronsInState[lineB].Planes;
                    if (lastLaneB)
                    {
                        StartPlaneDeckToWaitBasic(planesB[0], lineA, deckNodes, generatedNodes.GroupNodes[waiting], generatedNodes.PathNodes[deck][waiting]);
                    }
                    else
                    {
                        StartPlaneDeckToSwap(planesB[planesB.Count - 1], lineA, 0, 0, 0, deckNodes, swapNodes, generatedNodes.PathNodes[deck][swapWaiting]);
                    }
                }
                break;
        }
    }

    private PlaneMovement GetPrevPlane(PlaneMovement plane, out int index)
    {
        index = plane.Squadron.Planes.IndexOf(plane);
        Assert.IsFalse(index < 1);
        Assert.IsTrue(index < plane.Squadron.Planes.Count);
        for (int i = plane.Squadron.Planes.Count; i > index; i--)
        {
            Assert.IsTrue(allPlanes.Contains(plane.Squadron.Planes[i - 1]));
            Assert.IsTrue(planesInDestination.Contains(plane.Squadron.Planes[i - 1]));
        }
        index--;
        plane = plane.Squadron.Planes[index];
        Assert.IsTrue(allPlanes.Contains(plane));
        Assert.IsFalse(planesInDestination.Contains(plane));
        Assert.IsTrue(plane.Path.Count == 0);
        return plane;
    }

    private PlaneMovement GetNextPlane(PlaneMovement plane, out int index)
    {
        return GetNextPlane(plane, plane.Squadron.Planes.IndexOf(plane), out index);
    }

    private PlaneMovement GetNextPlane(PlaneMovement plane, int startIndex, out int index)
    {
        index = startIndex;
        Assert.IsFalse(index < 0);
        Assert.IsTrue(index < (plane.Squadron.Planes.Count - 1));
        for (int i = 0; i <= index; i++)
        {
            Assert.IsTrue(allPlanes.Contains(plane.Squadron.Planes[i]));
            //Assert.IsTrue(planesInDestination.Contains(plane.Squadron.Planes[i]));
        }
        index++;
        plane = plane.Squadron.Planes[index];
        Assert.IsTrue(allPlanes.Contains(plane));
        //Assert.IsFalse(planesInDestination.Contains(plane));
        Assert.IsTrue(plane.Path.Count == 0);
        return plane;
    }

    private int GetFreeSlot(PlaneNodeGroup group)
    {
        int result;
        for (result = 0; result < group.LineNodes.Count; result++)
        {
            if (group.LineNodes[result][0].OccupiedBy == null)
            {
                break;
            }
        }
        Assert.IsFalse(result == group.LineNodes.Count);
        return result;
    }

    private void ChangeRotationConstraints(int line, List<List<Quaternion>> rotations)
    {
        var planes = squadronsInState[line].Planes;
        for (int j = 0; j < planes.Count; j++)
        {
            planes[j].RotationAtDestination = rotations[lineA][j];
        }
    }

    private void ReturnMovingPlanes(bool clearAll)
    {
        Assert.IsTrue(movingSquadrons.Count == 1);
        int planeIndex;
        for (planeIndex = 0; planeIndex < movingSquadrons[0].Planes.Count; planeIndex++)
        {
            var plane = movingSquadrons[0].Planes[planeIndex];
            Assert.IsTrue(allPlanes.Contains(plane));
            if (planesInDestination.Contains(plane))
            {
                Assert.IsTrue(plane.Path.Count == 0);
                planesInDestination.Remove(plane);
            }
            else
            {
                Assert.IsFalse(plane.Path.Count == 0);
                plane.Return();
                break;
            }
        }
        Assert.IsFalse(planeIndex == movingSquadrons[0].Planes.Count);
        Assert.IsFalse(clearAll && planesInDestination.Count > 0);

        for (planeIndex++; planeIndex < movingSquadrons[0].Planes.Count; planeIndex++)
        {
            var plane = movingSquadrons[0].Planes[planeIndex];
            Assert.IsTrue(allPlanes.Contains(plane));
            planesInDestination.Add(plane);
        }
    }

    private void FillPlaneLanes()
    {
        Assert.IsTrue(currentMovement == EPlaneDirection.DeckLaunchingToAirLaunching);
        currentStageInMovement = EPlaneMoveStage.LaunchingWaitToDeckLaunching;
        allPlanes.Clear();
        planesInDestination.Clear();
        //move planes to fill out empty slots & return back planes from wait slots
        currentSquadronLane = -1;
        var toDeck = generatedNodes.GroupNodes[EPlaneNodeGroup.DeckLaunching];
        for (int i = 0; i < squadronsInState.Count; i++)
        {
            if (squadronsInState[i] == null)
            {
                for (int j = i; j < squadronsInState.Count; j++)
                {
                    if (squadronsInState[j] != null)
                    {
                        squadronsInState[i] = squadronsInState[j];
                        squadronsInState[j] = null;
                        if (currentSquadronLane == -1)
                        {
                            currentSquadronLane = i;
                        }
                        foreach (var plane2 in squadronsInState[i].Planes)
                        {
                            allPlanes.Add(plane2);
                        }

                        for (int k = 1; k < squadronsInState[i].Planes.Count; k++)
                        {
                            StartPlaneNodes(squadronsInState[i].Planes[k], k, toDeck, j, i);
                        }
                        break;
                    }
                }
                //no more squadrons
                if (squadronsInState[i] == null)
                {
                    while (squadronsInState[squadronsInState.Count - 1] == null)
                    {
                        squadronsInState.RemoveAt(squadronsInState.Count - 1);
                    }
                    foreach (var squadron in squadronsInState)
                    {
                        Assert.IsNotNull(squadron);
                    }

                    break;
                }
            }
        }
        for (int i = 0; i < squadronsInState.Count; i++)
        {
            var planes = squadronsInState[i].Planes;
            var rotations = generatedNodes.DeckLaunchingRotations[i];
            for (int j = 0; j < planes.Count; j++)
            {
                planes[j].RotationAtDestination = rotations[j];
            }
        }

        var path = generatedNodes.PathNodes[EPlaneNodeGroup.WaitLaunching][EPlaneNodeGroup.DeckLaunching];
        StartPlaneDeckReturn(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, 0, toDeck, path);
        Assert.IsFalse(allPlanes.Count == 0);
    }

    private void SetWaitingPlanesAfterMovingPlanes()
    {
        Assert.IsTrue(movingSquadrons.Count > 0);
        Assert.IsTrue(movingSquadronSlot == squadronsInState.IndexOf(movingSquadrons[0]));

        allPlanes.Clear();
        for (movingSquadronSlot = currentSquadronLane; movingSquadronSlot < squadronsInState.Count; movingSquadronSlot++)
        {
            if (squadronsInState[movingSquadronSlot] == null)
            {
                break;
            }

            if (movingSquadronSlot > currentSquadronLane)
            {
                var plane2 = squadronsInState[movingSquadronSlot].Planes[0];
                Assert.IsTrue(plane2.Path.Count == 0);
                allPlanes.Add(plane2);
            }
        }
        planesInDestination.Clear();

        bool found = false;
        for (int i = movingSquadronSlot; i < squadronsInState.Count; i++)
        {
            if (squadronsInState[i] != null)
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            movingSquadronSlot = -1;
        }
        Assert.IsFalse(allPlanes.Count == 0);
    }

    private bool PlanesToWait(PlaneNodeGroup fromNodes, EPlaneNodeGroup from, EPlaneNodeGroup waiting)
    {
        //moved all planes out of way?
        if (--currentSquadronLane == movingSquadronSlot)
        {
            planesInDestination.Clear();
            return true;
        }
        else
        {
            var path = generatedNodes.PathNodes[from][waiting];
            StartPlaneDeckToWaitBasic(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, fromNodes, generatedNodes.GroupNodes[waiting], path);
        }
        return false;
    }

    private bool PlanesFromWait(PlaneNodeGroup fromNodes, EPlaneNodeGroup from, EPlaneNodeGroup waiting)
    {
        //moved all planes out of way?
        if (++currentSquadronLane == movingSquadronSlot)
        {
            planesInDestination.Clear();
            return true;
        }
        else
        {
            var path = generatedNodes.PathNodes[waiting][from];
            StartPlaneDeckReturnThroughFirstLine(squadronsInState[currentSquadronLane].Planes[0], currentSquadronLane, 0, 0, fromNodes, path);
        }
        return false;
    }

    private void ReturnPlanesA(List<PlaneMovement> planesA, PlaneNodeGroup fromNodes, EPlaneNodeGroup from, EPlaneNodeGroup swapWaiting, List<List<Quaternion>> rotations)
    {
        currentStageInMovement = EPlaneMoveStage.DeckLaunchingASwap;
        ChangeRotationConstraints(lineA, rotations);
        int index = planesA.Count - 1;
        var swapNodes = generatedNodes.GroupNodes[swapWaiting];
        var path = generatedNodes.PathNodes[swapWaiting][from];
        if (lastLane)
        {
            StartPlaneDeckSwapReturn(planesA[0], lineB, 0, index, swapNodes, fromNodes, path);
        }
        else
        {
            StartPlaneDeckSwapReturn(planesA[index], lineB, 0, index, swapNodes, fromNodes, path);
        }
    }

    private void CancelReturnPlanesA(PlaneNodeGroup deckNodes, PlaneNodeGroup swapNodes, PlaneNodeGroup path)
    {
        currentStageInMovement = EPlaneMoveStage.DeckLaunchingAToSwapLaunching;
        var planesA = squadronsInState[lineA].Planes;

        allPlanes.Clear();
        planesInDestination.Clear();
        for (int i = 0; i < planesA.Count; i++)
        {
            allPlanes.Add(planesA[i]);
        }

        if (lastLane)
        {
            for (int i = 0; i < planesA.Count; i++)
            {
                StartPlaneDeckReturn(planesA[i], lineA, i, deckNodes, path);
            }
        }
        else
        {
            int index = planesA.Count - 1;
            StartPlaneDeckSwapReturn(planesA[index], lineA, index, index, swapNodes, deckNodes, path);
        }
        Assert.IsFalse(allPlanes.Count == 0);
    }

    private void CancelToASwap(PlaneNodeGroup deckNodes, PlaneNodeGroup swapNodes, EPlaneNodeGroup deck, EPlaneNodeGroup swapWaiting)
    {
        planesInDestination.Clear();
        currentStageInMovement = EPlaneMoveStage.DeckLaunchingASwap;
        var planesA = squadronsInState[lineA].Planes;
        int lastIndex = planesA.Count - 1;
        StartPlaneDeckToSwap(planesA[lastLane ? lastIndex : 0], lineB, 0, 0, lastIndex, deckNodes, swapNodes, generatedNodes.PathNodes[deck][swapWaiting]);
    }

    private void StartPlaneOneNode(PlaneMovement plane, PlaneNode destination)
    {
        Assert.IsTrue(plane.Path.Count == 0);
        plane.Path.Add(plane.CurrentNode);
        plane.Path.Add(destination);
        Register(plane);
        plane.StartMovement();
    }

    private void StartPlaneNodes(PlaneMovement plane, int planeIndex, PlaneNodeGroup group, int from, int to)
    {
        Assert.IsTrue(plane.Path.Count == 0);
        Assert.IsTrue(plane.CurrentNode == group.LineNodes[from][planeIndex]);

        int min = to - 1;
        for (int i = from; i > min; i--)
        {
            plane.Path.Add(group.LineNodes[i][planeIndex]);
        }
        Register(plane);
        plane.StartMovement();
    }

    private void StartPlaneSingleLane(PlaneMovement plane, EPlaneNodeGroup from, EPlaneNodeGroup to, int planeIndex)
    {
        StartPlaneSingleLane(plane, from, to, 0, planeIndex);
    }

    private void StartPlaneSingleLane(PlaneMovement plane, EPlaneNodeGroup from, EPlaneNodeGroup to, int index, int planeIndex)
    {
        Assert.IsTrue(plane.Path.Count == 0);
        plane.Path.Add(plane.CurrentNode);
        plane.Path.AddRange(generatedNodes.PathNodes[from][to].LineNodes[index]);
        plane.Path.Add(generatedNodes.GroupNodes[to].LineNodes[0][planeIndex]);
        Register(plane);
        plane.StartMovement();
    }

    private void StartPlaneDeckBasic(PlaneMovement plane, int slot, int planeIndex, int planeLineIndex, PlaneNodeGroup from, PlaneNodeGroup to, PlaneNodeGroup path, bool ignoreAssert = false)
    {
        Assert.IsTrue(ignoreAssert || plane.Path.Count == 0);

        Assert.IsTrue(plane.CurrentNode == from.LineNodes[slot][planeIndex],
            slot.ToString() + "; " + planeIndex + "; " + plane.CurrentNode.Position.ToString("F3") + "; " + from.LineNodes[slot][planeIndex].Position.ToString("F3"));
        if (planeIndex != planeLineIndex)
        {
            plane.Path.Add(plane.CurrentNode);
        }
        for (int j = slot; j < from.LineNodes.Count; j++)
        {
            plane.Path.Add(from.LineNodes[j][planeLineIndex]);
        }
        plane.Path.AddRange(path.LineNodes[planeIndex]);
        plane.Path.Add(to.LineNodes[0][planeIndex]);

        Register(plane);
        plane.StartMovement();
    }

    private int StartPlaneDeckToWaitBasic(PlaneMovement plane, int slot, PlaneNodeGroup from, PlaneNodeGroup to, PlaneNodeGroup path, bool ignoreAssert = false)
    {
        Assert.IsTrue(ignoreAssert || plane.Path.Count == 0);
        if (plane.CurrentNode != from.LineNodes[slot][0])
        {
            Assert.IsTrue(plane.CurrentNode == from.LineNodes[slot][plane.Squadron.Planes.IndexOf(plane)],
                plane.CurrentNode.Position.ToString("F3") + "; " + from.LineNodes[slot][plane.Squadron.Planes.IndexOf(plane)].Position.ToString("F3"));
            plane.Path.Add(plane.CurrentNode);
        }
        for (int j = slot; j < from.LineNodes.Count; j++)
        {
            plane.Path.Add(from.LineNodes[j][0]);
        }
        plane.Path.AddRange(path.LineNodes[0]);
        var nodes = to.LineNodes[0];
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].OccupiedBy == null)
            {
                plane.Path.Add(nodes[i]);
                Register(plane);
                plane.StartMovement();
                return i;
            }
        }
        Assert.IsTrue(false);
        return 0;
    }

    private void StartPlaneLaunchingRecovering(PlaneMovement plane, int planeIndex, int fromSlot, int toSlot, PlaneNodeGroup from, PlaneNodeGroup to, PlaneNodeGroup path)
    {
        Assert.IsTrue(plane.Path.Count == 0);
        Assert.IsTrue(plane.CurrentNode == from.LineNodes[fromSlot][planeIndex]);
        Assert.IsFalse(planeIndex < 0);
        Assert.IsTrue(planeIndex < PlaneCount);
        Assert.IsFalse(fromSlot < 0);
        Assert.IsFalse(toSlot < 0);
        Assert.IsTrue(fromSlot < from.LineNodes.Count);
        Assert.IsTrue(toSlot < to.LineNodes.Count);
        for (int i = fromSlot; i < from.LineNodes.Count; i++)
        {
            plane.Path.Add(from.LineNodes[i][planeIndex]);
        }
        plane.Path.AddRange(path.LineNodes[planeIndex]);
        for (int i = to.LineNodes.Count; i > toSlot; i--)
        {
            plane.Path.Add(to.LineNodes[i - 1][planeIndex]);
        }
        Register(plane);
        plane.StartMovement();
    }

    private void StartPlaneSwap(PlaneMovement plane, int planeIndex, int fromSlot, int toSlot, PlaneNodeGroup deckGroup)
    {
        Assert.IsTrue(plane.Path.Count == 0);
        Assert.IsTrue(plane.CurrentNode == deckGroup.LineNodes[fromSlot][planeIndex]);
        Assert.IsFalse(planeIndex < 0);
        Assert.IsTrue(planeIndex < PlaneCount);
        Assert.IsFalse(fromSlot < 0);
        Assert.IsFalse(toSlot < 0);
        Assert.IsTrue(fromSlot < deckGroup.LineNodes.Count);
        Assert.IsTrue(toSlot < deckGroup.LineNodes.Count);

        plane.Path.Add(plane.CurrentNode);
        for (int i = fromSlot; i <= toSlot; i++)
        {
            plane.Path.Add(deckGroup.LineNodes[i][0]);
        }
        planeIndex = plane.Squadron.Planes.Count - planeIndex - 1;
        if (planeIndex != 0)
        {
            plane.Path.Add(deckGroup.LineNodes[toSlot][planeIndex]);
        }
        Register(plane);
        plane.StartMovement();
    }

    private void StartPlaneDeckToSwap(PlaneMovement plane, int slot, int planeIndex, int planeLineIndex, int toPlaneIndex, PlaneNodeGroup from, PlaneNodeGroup to, PlaneNodeGroup path, bool ignoreAssert = false)
    {
        Assert.IsTrue(ignoreAssert || plane.Path.Count == 0);
        Assert.IsTrue(plane.CurrentNode == from.LineNodes[slot][planeIndex]);
        if (planeIndex != planeLineIndex)
        {
            plane.Path.Add(plane.CurrentNode);
        }
        for (int j = slot; j < from.LineNodes.Count; j++)
        {
            plane.Path.Add(from.LineNodes[j][planeLineIndex]);
        }
        plane.Path.AddRange(path.LineNodes[0]);
        for (int j = 0; j <= toPlaneIndex; j++)
        {
            plane.Path.Add(to.LineNodes[0][j]);
        }

        Register(plane);
        plane.StartMovement();
    }

    private void StartPlaneWaitToSwap(PlaneMovement plane, PlaneNodeGroup to, PlaneNodeGroup path, int waitIndex)
    {
        Assert.IsTrue(plane.Path.Count == 0);
        plane.Path.Add(plane.CurrentNode);
        plane.Path.AddRange(path.LineNodes[waitIndex]);
        var nodes = to.LineNodes[0];
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].OccupiedBy == null)
            {
                plane.Path.Add(nodes[i]);
                Register(plane);
                plane.StartMovement();
                return;
            }
        }
        Assert.IsTrue(false);
    }

    private void StartPlaneReverseSwap(PlaneMovement plane, int planeIndex, int fromSlot, int toSlot, PlaneNodeGroup deckGroup)
    {
        Assert.IsTrue(plane.Path.Count == 0);
        Assert.IsFalse(planeIndex < 0);
        Assert.IsTrue(planeIndex < PlaneCount);
        Assert.IsFalse(fromSlot < 0);
        Assert.IsFalse(toSlot < 0);
        Assert.IsTrue(fromSlot < deckGroup.LineNodes.Count);
        Assert.IsTrue(toSlot < deckGroup.LineNodes.Count);

        plane.Path.Add(plane.CurrentNode);
        for (int i = fromSlot; i >= toSlot; i--)
        {
            plane.Path.Add(deckGroup.LineNodes[i][0]);
        }
        if (planeIndex != 0)
        {
            plane.Path.Add(deckGroup.LineNodes[toSlot][planeIndex]);
        }
        Register(plane);
        plane.StartMovement();
    }

    private void StartPlaneDeckReturn(PlaneMovement plane, int slot, int planeIndex, PlaneNodeGroup to, PlaneNodeGroup path)
    {
        Assert.IsTrue(plane.Path.Count == 0);
        plane.Path.Add(plane.CurrentNode);
        plane.Path.AddRange(path.LineNodes[planeIndex]);
        for (int j = to.LineNodes.Count - 1; j > slot; j--)
        {
            plane.Path.Add(to.LineNodes[j][planeIndex]);
        }
        plane.Path.Add(to.LineNodes[slot][planeIndex]);

        Register(plane);
        plane.StartMovement();
    }

    private void StartPlaneDeckReturnThroughFirstLine(PlaneMovement plane, int slot, int planeIndex, int planePath, PlaneNodeGroup to, PlaneNodeGroup path)
    {
        Assert.IsTrue(plane.Path.Count == 0);
        plane.Path.Add(plane.CurrentNode);
        plane.Path.AddRange(path.LineNodes[planePath]);
        for (int i = to.LineNodes.Count - 1; i > slot; i--)
        {
            plane.Path.Add(to.LineNodes[i][0]);
        }
        for (int i = 0; i <= planeIndex; i++)
        {
            plane.Path.Add(to.LineNodes[slot][i]);
        }

        Register(plane);
        plane.StartMovement();
    }

    private void StartPlaneDeckSwapReturn(PlaneMovement plane, int slot, int planeIndex, int toPlaneIndex, PlaneNodeGroup from, PlaneNodeGroup to, PlaneNodeGroup path)
    {
        Assert.IsTrue(plane.Path.Count == 0);
        plane.Path.Add(plane.CurrentNode);
        if (planeIndex != 0)
        {
            plane.Path.Add(from.LineNodes[0][0]);
        }
        plane.Path.AddRange(path.LineNodes[0]);
        for (int i = to.LineNodes.Count - 1; i > slot; i--)
        {
            plane.Path.Add(to.LineNodes[i][0]);
        }
        for (int i = 0; i <= toPlaneIndex; i++)
        {
            plane.Path.Add(to.LineNodes[slot][i]);
        }

        Register(plane);
        plane.StartMovement();
    }

    private void ReturnFromWait()
    {
        squadronsInState[currentSquadronLane].Planes[0].Return();
    }

    private void ReturnSwapMove(List<PlaneMovement> planes, int startIndex, int sign)
    {
        bool returned = false;
        for (int i = 0; i < planes.Count; i++)
        {
            int x = startIndex + sign * i;
            if (returned)
            {
                planesInDestination.Add(planes[x]);
            }
            else if (!planesInDestination.Contains(planes[x]))
            {
                planes[x].Return();
                returned = true;
                planesInDestination.Clear();
            }
        }
    }

    private void PlaneRotateLateConstraint(PlaneMovement plane)
    {
        //plane.RotationConstraint = plane.LaterRotationConstraint;
        plane.Rotate(plane.RotationAtDestination);
    }

    private void AddPlaneMove(PlaneMovement plane, float newSpeed, bool moveBackwards)
    {
        allPlanes.Add(plane);
        plane.Speed = newSpeed;
        //plane.MoveBackwards = moveBackwards;
    }

    private void Register(PlaneMovement plane)
    {
        plane.Arrived += OnPlaneArrived;
        bool added = registered.Add(plane);
        Assert.IsTrue(added);
    }

    private void Unregister(PlaneMovement plane)
    {
        plane.Arrived -= OnPlaneArrived;
        bool removed = registered.Remove(plane);
        Assert.IsTrue(removed);
    }

    private void FinishMovement()
    {
        Assert.IsTrue(rotatingPlanes.Count == 0);
        switch (currentMovement)
        {
            case EPlaneDirection.DeckLaunchingToHangar:
            case EPlaneDirection.DeckRecoveringToHangar:
            case EPlaneDirection.LandingToHangar:
                break;
            default:
                foreach (var plane in planes)
                {
                    var found = movedSquadrons.Find(x => x.Planes.Contains(plane));
                    if (plane.IsRotating() && found == null && rotatingPlanes.Add(plane))
                    {
                        plane.RotationFinished += OnRotationFinished;
                    }
                }
                break;
        }

        if (rotatingPlanes.Count > 0)
        {
            helper.StartCoroutine(() => OnRotationFinished(null));
            return;
        }
        helper.StopCoroutine();
        var wasInCancel = inCancel;
        inCancel = false;

        foreach (var plane in allPlanes)
        {
            //plane.MoveBackwards = false;
            //plane.RotationConstraint = EPlaneRotation.Shorter;
            plane.RotationFinished -= OnRotationFinished;
        }

        if (currentMovement == EPlaneDirection.DeckLaunchingToAirLaunching)
        {
            foreach (var squadron in movedSquadrons)
            {
                var plane = squadron.Planes[squadron.Planes.Count - 1];
                if (plane.IsPlayingAnimation)
                {
                    plane.Callback = (x) => FreePlanes(x.Squadron);
                }
                else
                {
                    FreePlanes(squadron);
                }
            }
        }

        Assert.IsFalse(allPlanes.Count == 0);
        Clear();

        SetLiftsState(1f);
        MovementFinished();

        if (!ignoreTick)
        {
            Assert.IsFalse(inTickable);
            inTickable = true;
            TimeManager.Instance.AddTickable(this);
        }
    }

    private void PrepareCrewmates(PlaneMovement plane, bool shouldWalk)
    {
        var planeCrewMan = PlaneCrewMovementManager.Instance;
        planeCrewMan.AskForCrew(plane, () =>
        {
            plane.CrewIsReady = !shouldWalk;
        });
        if (shouldWalk)
        {
            planeCrewMan.MovePlaneCrewUnderDeck(plane);
        }
    }

    private IEnumerable<PlaneMovement> GetAllPassivePlanes()
    {
        set.Clear();
        foreach (var squadron in movedSquadrons)
        {
            foreach (var plane in squadron.Planes)
            {
                set.Add(plane);
            }
        }
        foreach (var plane in planes)
        {
            if (!set.Contains(plane))
            {
                yield return plane;
            }
        }
    }

    private IEnumerable<PlaneMovement> GetAllPlanesInSwapping()
    {
        for (int i = lineB + 1; i < squadronsInState.Count; i++)
        {
            if (i != lineA)
            {
                yield return squadronsInState[i].Planes[0];
            }
        }
        //var planes = squadronsInState[lineA].Planes;
        //for (int i = 1; i < planes.Count; i++)
        //{
        //    yield return planes[i];
        //}
        //planes = squadronsInState[lineB].Planes;
        //for (int i = 1; i < planes.Count; i++)
        //{
        //    yield return planes[i];
        //}
    }

    private IEnumerable<PlaneMovement> GetAllPlanesInSwappingFront()
    {
        for (int i = lineA; i < (squadronsInState.Count - 1); i++)
        {
            yield return squadronsInState[i].Planes[0];
        }
        //var planes = squadronsInState[lineA].Planes;
        //for (int i = 1; i < planes.Count; i++)
        //{
        //    yield return planes[i];
        //}
        //planes = squadronsInState[lineB].Planes;
        //for (int i = 1; i < planes.Count; i++)
        //{
        //    yield return planes[i];
        //}
    }

    private IEnumerable<PlaneMovement> GetAllPlanes(List<PlaneSquadron> squadrons)
    {
        float z = float.PositiveInfinity;
        foreach (var squadron in squadrons)
        {
            var pos = squadron.Planes[0].CurrentNode.Position;
            Assert.IsTrue(pos.x > 0f);
            foreach (var plane in squadron.Planes)
            {
                Assert.IsTrue(Mathf.Abs(z) > Mathf.Abs(plane.CurrentNode.Position.z));
                yield return plane;
            }
            Assert.IsTrue(squadron.Planes.Count > 2 || squadron.Planes[2].CurrentNode.Position.x < 0f);
            z = pos.z;
        }
    }

    private void Clear()
    {
        Assert.IsTrue(registered.Count == 0);
        allPlanes.Clear();
        planesInDestination.Clear();
        squadrons.Clear();
        squadronsInState.Clear();
        movingSquadrons.Clear();
        movedSquadrons.Clear();

        squadronsIndices.Clear();
        hadCrash = false;

        SetLiftsState(1f);
    }

    private void RemoveTick()
    {
        if (inTickable)
        {
            TimeManager.Instance.RemoveTickable(this);
            inTickable = false;
        }
    }

    private void SaveNoMovement(ref PlaneMovementSaveData data)
    {
        data.Movement = false;

        Assert.IsTrue(registered.Count == 0);
        Assert.IsTrue(planesInDestination.Count == 0);
        Assert.IsTrue(squadrons.Count == 0);
        Assert.IsTrue(squadronsInState.Count == 0);
        Assert.IsTrue(movingSquadrons.Count == 0);
        Assert.IsTrue(movedSquadrons.Count == 0);
        Assert.IsTrue(squadronsIndices.Count == 0);

        var deck = AircraftCarrierDeckManager.Instance;
        if (deck.OrderQueue.Count > 0 && deck.OrderQueue[0] is LandingOrder order && deck.FinishOrderTime > 10)
        {
            Assert.IsFalse(true, "wtf " + order.Mission.SentSquadrons.Count);
        }
    }

    private void SaveActionParams(ref PlaneMovementSaveData data)
    {
        if (!data.FinishMovement)
        {
            data.Params.Clear();
            switch (currentMovement)
            {
                case EPlaneDirection.DeckLaunchingToAirLaunching:
                    data.Params.AddRange(squadronsIndices);
                    break;
                case EPlaneDirection.DeckLaunchingToHangar:
                case EPlaneDirection.DeckRecoveringToHangar:
                case EPlaneDirection.HangarToDeckLaunching:
                case EPlaneDirection.HangarToDeckRecovering:
                case EPlaneDirection.SwapFrontLaunching:
                case EPlaneDirection.SwapFrontRecovering:
                    data.Params.Add(squadronsIndices[0]);
                    break;
                case EPlaneDirection.SwapLaunching:
                case EPlaneDirection.SwapRecovering:
                    data.Params.Add(squadronsIndices[0]);
                    data.Params.Add(squadronsIndices[1]);
                    break;
                case EPlaneDirection.LandingToDeckRecovering:
                    data.Params.Add(squadronsIndices[0]);
                    break;
                case EPlaneDirection.LandingToHangar:
                    using (var enumer = squadrons.GetEnumerator())
                    {
                        enumer.MoveNext();
                        data.Params.Add((int)enumer.Current.PlaneType);
                    }
                    data.Params.Add(hadCrash ? 1 : 0);
                    break;
            }
        }
    }

    private void SaveDeckSquadronPlanes(ref PlaneMovementSaveData data, List<PlaneSquadron> squadrons, HashSet<PlaneSquadron> set)
    {
        for (int i = squadrons.Count; i > 0; i--)
        {
            int index = i - 1;
            var squadron = squadrons[index];
            set.Add(squadron);
            var pathSaveData = new PlanePathSaveData() { Squadron = index };
            for (int j = 0; j < squadron.Planes.Count; j++)
            {
                pathSaveData.Index = j;
                squadron.Planes[j].SaveData(ref pathSaveData, !data.FinishMovement);
                data.Planes.Add(pathSaveData);
            }
        }
    }

    private void SaveOtherSquadronPlanes(ref PlaneMovementSaveData data, HashSet<PlaneSquadron> set)
    {
        var pathData = new PlanePathSaveData();
        int w = 0;
        Assert.IsNotNull(this.planes, "c001");
        Assert.IsNotNull(set, "c002");
        Assert.IsNotNull(data.Planes, "c003");
        foreach (var plane in this.planes)
        {
            Assert.IsNotNull(plane, "c004");
            if (plane.Squadron != null && set.Add(plane.Squadron))
            {
                var planes = plane.Squadron.Planes;
                Assert.IsNotNull(planes, "c006");
                pathData.Squadron = -(++w);
                pathData.Type = plane.Type;
                pathData.PlaneCount = planes.Count;
                for (int i = 0; i < planes.Count; i++)
                {
                    pathData.Index = i;
                    planes[i].SaveData(ref pathData, !data.FinishMovement);
                    data.Planes.Add(pathData);
                }
            }
        }
    }

    private void SetLiftsState(float state)
    {
        var deck = AircraftCarrierDeckManager.Instance;
        if (parallelLifts > 1)
        {
            deck.SetLiftState(1, state, false);
            deck.SetConditionalLift(state);
        }
        else
        {
            deck.SetLiftState(0, state, false);
        }
    }
}
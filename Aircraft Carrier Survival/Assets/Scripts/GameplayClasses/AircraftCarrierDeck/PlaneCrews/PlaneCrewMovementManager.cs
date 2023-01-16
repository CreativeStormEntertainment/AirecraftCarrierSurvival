using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlaneCrewMovementManager : MonoBehaviour
{
    public static PlaneCrewMovementManager Instance;

    private const string CrewLeftPoint = "crewwaypointl";
    private const string CrewRightPoint = "crewwaypointr";

    public List<Transform> RallyPointLookAt = new List<Transform>();

    [SerializeField]
    private PlaneCrewMovement crewmatePrefab = null;
    [SerializeField]
    private Transform spawnPosition = null;
    [SerializeField]
    private int poolSize = 0;
    [SerializeField]
    private int PlaneCrewSize = 2;

    [SerializeField]
    private List<PlaneCrewMovement> pooledObjects = null;

    [SerializeField]
    private List<PlaneCrewMovement> eventPlaneCrews = null;

    [SerializeField]
    private Transform pointsRoot = null;
    [SerializeField]
    private float crewDelay = 2f;

    [SerializeField]
    private int jumpUpAnimationsCount = 1;
    [SerializeField]
    private int jumpDownAnimationsCount = 1;
    [SerializeField]
    private int[] subIdleCounts = new int[] { 4, 2, 2, 3, 1, 2 };
    [SerializeField]
    private float switchPushTime = 1f;
    [SerializeField]
    private float switchPushEndDelay = 0.1f;

    [SerializeField]
    private float moveToPlaneDelay = 0.3f;

    private Transform rallypointsRoot = null;
    private Dictionary<EPlaneNodeGroup, List<List<List<PlaneCrewRallyWay>>>> rallyPoints = new Dictionary<EPlaneNodeGroup, List<List<List<PlaneCrewRallyWay>>>>();

    private Transform waypointsRoot = null;
    private Dictionary<EPlaneNodeGroup, List<PlaneCrewWaypoints>> waypoints = new Dictionary<EPlaneNodeGroup, List<PlaneCrewWaypoints>>();

    private HashSet<PlaneCrewMovement> registered;
    private HashSet<PlaneCrewMovement> registeredAway;

    private List<int> jumpUpHashes = new List<int>();
    private List<int> jumpDownHashes = new List<int>();

    private List<List<int>> idleHashes = new List<List<int>>();

    private float moveToPlaneDelaySum = 0;

    private List<PlaneCrewMovement> crews;

    private int rowsCount;

    private void Awake()
    {
        Instance = this;
        registered = new HashSet<PlaneCrewMovement>();
        registeredAway = new HashSet<PlaneCrewMovement>();
        crews = new List<PlaneCrewMovement>();
    }

    private void Start()
    {
        EnemyAttacksManager.Instance.AttackAnimationStateChanged += OnAttackAnimationStateChanged;
        pooledObjects = new List<PlaneCrewMovement>();
        for (int i = 0; i < poolSize; i++)
        {
            PlaneCrewMovement obj = Instantiate(crewmatePrefab, spawnPosition.transform.position, Quaternion.identity, spawnPosition);
            obj.gameObject.SetActive(false);
            pooledObjects.Add(obj);
        }
        rallypointsRoot = pointsRoot.GetChild(0);
        waypointsRoot = pointsRoot.GetChild(1);
        CreateRallypoints();
        CreateWaypoints();

        for (int i = 0; i < 6; ++i)
        {
            List<int> list = new List<int>();
            list.Add(Animator.StringToHash("Deck_Idle" + (i + 1)));
            for (int j = 0; j < subIdleCounts[i]; ++j)
            {
                list.Add(Animator.StringToHash("Deck_Idle" + (i + 1) + "_action" + (j + 1)));
            }
            idleHashes.Add(list);
        }

        for (int i = 0; i < jumpUpAnimationsCount; ++i)
        {
            jumpUpHashes.Add(Animator.StringToHash("Deck_Climb_" + (i+1)));
        }
        for (int i = 0; i < jumpDownAnimationsCount; ++i)
        {
            jumpDownHashes.Add(Animator.StringToHash("Deck_JumpDown_" + (i+1)));
        }
    }

    private void LateUpdate()
    {
        foreach (var crew in pooledObjects)
        {
            if (crew.MyUpdate(Time.deltaTime))
            {
                eventPlaneCrews.Add(crew);
            }
        }

        foreach (var crew in eventPlaneCrews)
        {
            if (crew.UpdateEvent())
            {
                if (registered.Contains(crew))
                {
                    OnCrewArrived(crew);
                }
                if (registeredAway.Contains(crew))
                {
                    OnCrewAway(crew);
                }
            }
        }
        eventPlaneCrews.Clear();
        if (moveToPlaneDelaySum > 0f)
        {
            moveToPlaneDelaySum -= Time.deltaTime;
        }
        else
        {
            moveToPlaneDelaySum = 0f;
        }
    }

    public void Load()
    {
        int loopBreak = 0;
        int ok = 0;
        do
        {
            if (loopBreak++ > 100_000)
            {
                Assert.IsTrue(false);
            }
            foreach (var crew in pooledObjects)
            {
                crew.LoadUpdate();
                if (crew.MyUpdate(.1f))
                {
                    eventPlaneCrews.Add(crew);

                    if (crew.CrewPathState == PlaneCrewMovement.EPlaneCrewPathState.AT_PLANE)
                    {
                        crew.transform.localPosition = Vector3.zero;
                    }
                }
            }

            foreach (var crew in eventPlaneCrews)
            {
                if (crew.UpdateEvent())
                {
                    if (registered.Contains(crew))
                    {
                        OnCrewArrived(crew);
                    }
                    if (registeredAway.Contains(crew))
                    {
                        OnCrewAway(crew);
                    }
                }
            }
            moveToPlaneDelaySum = 0f;

            crews.Clear();
            foreach (var crew in pooledObjects)
            {
                if ((crew.Path == null || crew.Path.Count == 0) && !crew.IsSwitchingPush)
                {
                    crews.Add(crew);
                }
            }
            if (crews.Count == pooledObjects.Count && eventPlaneCrews.Count == pooledObjects.Count)
            {
                ok++;
            }
            else
            {
                ok = 0;
            }
            eventPlaneCrews.Clear();
        }
        while (ok < 3);
    }

    public void AskForCrew(PlaneMovement plane, Action prepareCrew)
    {
        if (plane.Crew.Count == 0)
        {
            for (int i = 0; i < PlaneCrewSize; ++i)
            {
                SpawnSailor(plane, plane.gameObject.transform.position, plane.gameObject.transform.rotation);
            }
        }
        plane.SetCrew(true);
        prepareCrew();
    }

    public void AskForCrewAtStart(PlaneMovement plane, bool launching)
    {
        for (int i = 0; i < PlaneCrewSize; ++i)
        {
            var s = SpawnSailor(plane, plane.gameObject.transform.position, plane.gameObject.transform.rotation);
            s.name = "sailorPusher" + i + "_" + plane.name;
        }
        TeleportPlaneCrewToSide(launching ? EPlaneNodeGroup.DeckLaunching : EPlaneNodeGroup.DeckRecovering, plane, true);
    }

    public void FreeCrews(PlaneMovement plane)
    {
        foreach (var planeCrew in plane.Crew)
        {
            planeCrew.SetupCrew(spawnPosition, false);
            RemoveCallbacks(planeCrew);
        }
        plane.Crew.Clear();
    }

    public void MovePlaneCrewToPlane(PlaneMovement plane, EPlaneNodeGroup ePlaneNode)
    {
        bool moveToRecovery = ePlaneNode == EPlaneNodeGroup.DeckLaunching;
        int planeNum = plane.Squadron.Planes.IndexOf(plane);
        AircraftCarrierDeckManager deckMan = AircraftCarrierDeckManager.Instance;
        int originalSquadronIndex = deckMan.DeckSquadrons.IndexOf(plane.Squadron);
        int squadronIndex = originalSquadronIndex;
        for (int i = 0; i < plane.Crew.Count; i++)
        {
            PlaneCrewMovement crew = plane.Crew[i];

            if (crew.CrewPathState == PlaneCrewMovement.EPlaneCrewPathState.AT_PLANE || crew.CrewPathState == PlaneCrewMovement.EPlaneCrewPathState.TO_PLANE)
            {
                continue;
            }
            RemoveCallbacks(crew);
            crew.CrewPathState = PlaneCrewMovement.EPlaneCrewPathState.TO_PLANE;
            crew.SquadronIndex = originalSquadronIndex;
            //    crew.MoveBackwards = moveToRecovery;
            List<PlaneCrewNode> positions = new List<PlaneCrewNode>();
            positions = i == 0 ? waypoints[ePlaneNode][squadronIndex].Right[planeNum] : waypoints[ePlaneNode][squadronIndex].Left[planeNum];

            bool findNearest = crew.Path.Count > 0;

            var newPath = new List<PlaneCrewNode>();
            newPath.AddRange(positions);
            if (crew.RallyPoint != null)
            {
                crew.RallyPoint.IsFree = true;
                newPath.Add(crew.RallyPoint.Way.JumpUp);
                newPath.AddRange(crew.RallyPoint.Way.Additional);
                newPath.Add(new PlaneCrewNode(crew.RallyPoint.Transf.position));
                crew.RallyPoint = null;
            }
            newPath.Reverse();
            if (findNearest)
            {
                float lowest = Mathf.Infinity;
                int lowestIndex = -1;
                crew.Path = new List<PlaneCrewNode>();
                for (int j = newPath.Count-1; j >= 0; --j)
                {
                    float dist = Vector3.Distance(crew.transform.position, newPath[j].Position);
                    if (dist < lowest)
                    {
                        lowest = dist;
                        lowestIndex = j;
                    }
                }
                if (lowestIndex == -1 || lowestIndex == newPath.Count - 1)
                {
                    crew.Path.Add(newPath[newPath.Count - 1]);
                }
                else
                {
                    crew.Path.AddRange(newPath.GetRange(lowestIndex, newPath.Count - lowestIndex));
                }
                crew.Path.Insert(0, new PlaneCrewNode(crew.transform.position));
            }
            else
            {
                crew.Path = newPath;
            }

            if (moveToPlaneDelaySum == 0f)
            {
                crew.Delay = 0f;
            }
            else
            {
                crew.Delay = moveToPlaneDelaySum;
            }
            crew.SetupAnimation(crew.Path[0], moveToRecovery ? plane.PushFront[i] : plane.PushBottom[i], true);
            Register(crew);
        }
        moveToPlaneDelaySum += moveToPlaneDelay;
    }

    public void MoveCrewToLandingPlane(PlaneMovement plane, int number)
    {
        AircraftCarrierDeckManager deckMan = AircraftCarrierDeckManager.Instance;
        for (int i = 0; i < plane.Crew.Count; i++)
        {
            PlaneCrewMovement crew = plane.Crew[i];
            RemoveCallbacks(crew);

            crew.PlaneToHideWings = plane;
            List<PlaneCrewNode> positions = new List<PlaneCrewNode>(i == 0 ? waypoints[EPlaneNodeGroup.Landing][0].Right[number] : waypoints[EPlaneNodeGroup.Landing][0].Left[number]);
            crew.CrewPathState = PlaneCrewMovement.EPlaneCrewPathState.TO_PLANE;
            crew.Path = positions;
            crew.SetupAnimation(crew.Path[0], plane.PushFront[i]);
            Register(crew);
        }
    }

    public void MovePlaneCrewOnPlaneStart(PlaneMovement plane)
    {
        int planeNum = plane.Squadron.Planes.IndexOf(plane);
        for (int i = 0; i < plane.Crew.Count; i++)
        {
            PlaneCrewMovement crew = plane.Crew[i];
            crew.Path = new List<PlaneCrewNode>();
            crew.Path.Clear();
            var positions = i == 0 ? waypoints[EPlaneNodeGroup.Starting][0].Left[0] : waypoints[EPlaneNodeGroup.Starting][0].Right[0];
            crew.CrewPathState = PlaneCrewMovement.EPlaneCrewPathState.TO_SIDE;
            crew.Path.Add(new PlaneCrewNode(crew.transform.position));
            crew.Path.AddRange(positions);
            crew.SetupAnimation(positions[0], null, true);
            Register(crew);
        }
    }

    public void MovePlaneCrewUnderDeck(PlaneMovement plane)
    {
        foreach (var crew in plane.Crew)
        {
            crew.SetupCrew(spawnPosition, true);
            Register(crew);
        }
    }

    public float MovePlaneCrewToSide(EPlaneNodeGroup ePlaneNode, PlaneMovement plane, float delay)
    {
        if (plane.Squadron != null)
        {
            bool moveDirection = ePlaneNode == EPlaneNodeGroup.DeckLaunching;
            int planeNum = plane.Squadron.Planes.IndexOf(plane);
            int squadronIndex = AircraftCarrierDeckManager.Instance.DeckSquadrons.IndexOf(plane.Squadron);

            if (plane.IsRotating())
            {
                plane.SnapRotate();
            }

            for (int j = 0; j < plane.Crew.Count; j++)
            {
                PlaneCrewMovement crew = plane.Crew[j];
                if (crew.CrewPathState == PlaneCrewMovement.EPlaneCrewPathState.AT_SIDE)
                {
                    continue;
                }

                crew.SquadronIndex = squadronIndex;
                int squadronFixedIndex = squadronIndex;
                if (!moveDirection)
                {
                    squadronFixedIndex = squadronIndex + (plane.IsBehindWings(j) ? rowsCount : 0);
                }
                var positions = j == 0 ? waypoints[ePlaneNode][squadronFixedIndex].Right[planeNum] : waypoints[ePlaneNode][squadronFixedIndex].Left[planeNum];
                crew.CrewPathState = PlaneCrewMovement.EPlaneCrewPathState.TO_SIDE;
                if (crew.ReadyState || (!crew.ReadyState && crew.IsSwitchingPush))
                {
                    crew.Delay = delay;
                    delay += crewDelay;

                    crew.MoveBackwards = moveDirection;
                    crew.Path.Clear();
                    crew.Path.Add(new PlaneCrewNode(crew.transform.position));

                    if (crew.RallyPoint != null)
                    {
                        crew.RallyPoint.IsFree = true;
                    }
                    var rallyPoint = GetFreeRallyPoint(ePlaneNode, squadronIndex, planeNum);
                    crew.RallyPoint = rallyPoint;

                    crew.Path.AddRange(positions);

                    crew.Path.Add(rallyPoint.Way.JumpDown);
                    crew.Path.AddRange(rallyPoint.Way.Additional);

                    crew.Path.Add(new PlaneCrewNode(rallyPoint.Transf.position));
                    crew.SetupAnimation(crew.Path[0], null);
                    RegisterAway(crew);
                }
            }
        }
        return delay;
    }

    public void MovePlaneCrewToSide(EPlaneNodeGroup ePlaneNode, IEnumerable<PlaneMovement> planes)
    {
        float sumCrewDelay = 0f;
        foreach (var plane in planes)
        {
            sumCrewDelay = MovePlaneCrewToSide(ePlaneNode, plane, sumCrewDelay);
        }
    }

    public void TeleportPlaneCrewToSide(EPlaneNodeGroup ePlaneNode, HashSet<PlaneMovement> planes)
    {
        foreach (var plane in planes)
        {
            TeleportPlaneCrewToSide(ePlaneNode, plane, false);
        }
    }

    public void TeleportPlaneCrewToSide(EPlaneNodeGroup ePlaneNode, PlaneMovement plane, bool onStart)
    {
        //return;
        var moveDirection = ePlaneNode == EPlaneNodeGroup.DeckLaunching;
        int squadronIndex = 0;
        if (plane.Squadron != null)
        {
            int planeNum = plane.Squadron.Planes.IndexOf(plane);
            AircraftCarrierDeckManager deckMan = AircraftCarrierDeckManager.Instance;
            squadronIndex = deckMan.DeckSquadrons.IndexOf(plane.Squadron);

            for (int j = 0; j < plane.Crew.Count; j++)
            {
                PlaneCrewMovement crew = plane.Crew[j];
                crew.CrewPathState = PlaneCrewMovement.EPlaneCrewPathState.AT_SIDE;
                crew.Path.Clear();
                crew.TeleportToRallyPoint(GetFreeRallyPoint(ePlaneNode, squadronIndex, planeNum), ePlaneNode);
                if (!onStart)
                {
                    RegisterAway(crew);
                }
            }
        }
    }

    public void OnCrewAway(PlaneCrewMovement crew)
    {
        crew.Path = new List<PlaneCrewNode>();
        crew.Away();
        UnregisterAway(crew);
    }

    public void OnCrewArrived(PlaneCrewMovement crew)
    {
        crew.Path = new List<PlaneCrewNode>();
        crew.ReadyState = true;
        if (crew.PlaneToHideWings != null)
        {
            crew.PlaneToHideWings.HideWingsByCrew();
            crew.PlaneToHideWings = null;
        }
        Unregister(crew);
    }

    public void ResetCrew()
    {
        registered.Clear();
        registeredAway.Clear();

        foreach (var planeCrew in pooledObjects)
        {
            planeCrew.ResetMovement(spawnPosition);
            RemoveCallbacks(planeCrew);
        }
        foreach (var deckRallies in rallyPoints)
        {
            foreach (var lineRallies in deckRallies.Value)
            {
                foreach (var planeRallies in lineRallies)
                {
                    foreach (var crewRally in planeRallies)
                    {
                        crewRally.Rally.IsFree = true;
                    }
                }
            }
        }
    }

    private PlaneCrewMovement GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].gameObject.activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        var obj = Instantiate(crewmatePrefab, spawnPosition.transform.position, Quaternion.identity, spawnPosition);
        obj.gameObject.SetActive(false);
        pooledObjects.Add(obj);
        return obj;
    }

    private PlaneCrewMovement SpawnSailor(PlaneMovement plane, Vector3 position, Quaternion rotation)
    {
        PlaneCrewMovement sailor = GetPooledObject();
        sailor.ReadyState = true;
        plane.Crew.Add(sailor);
        sailor.gameObject.SetActive(true);
        sailor.transform.position = position;
        sailor.transform.rotation = rotation;
        sailor.Init(plane.HasStaticWings, idleHashes, jumpUpHashes, jumpDownHashes, switchPushTime, switchPushEndDelay);
        return sailor;
    }

    private void Register(PlaneCrewMovement crew)
    {
        crew.ReadyState = false;
        registered.Add(crew);
    }

    private void RegisterAway(PlaneCrewMovement crew)
    {
        crew.ReadyState = false;
        bool added = registeredAway.Add(crew);
    }

    private void UnregisterAway(PlaneCrewMovement crew)
    {
        bool removed = registeredAway.Remove(crew);
    }

    private void Unregister(PlaneCrewMovement crew)
    {
        bool removed = registered.Remove(crew);
    }

    private PlaneCrewRallyPoint GetFreeRallyPoint(EPlaneNodeGroup group, int line, int planeNum)
    {
        var variants = rallyPoints[group][line][planeNum].FindAll(x => x.Rally.IsFree);

        if (variants.Count > 0)
        {
            var way = RandomUtils.GetRandom(variants);
            way.Rally.IsFree = false;
            return way.Rally;
        }
        Debug.LogError("No rally way found on " + group.ToString() + " " + line + " " + planeNum);
        return rallyPoints[group][line][planeNum][0].Rally;
    }

    private void OnAttackAnimationStateChanged(bool state)
    {
        for (int i = 0; i < pooledObjects.Count; ++i)
        {
            pooledObjects[i].SetFear(state);
        }
    }

    private void CreateRallypoints()
    {
        rallyPoints.Clear();
        for (int i = 0; i < rallypointsRoot.childCount; ++i)
        {
            List<List<List<PlaneCrewRallyWay>>> rowList = new List<List<List<PlaneCrewRallyWay>>>();
            var mode = rallypointsRoot.GetChild(i);
            for (int j = 0; j < mode.childCount; ++j)
            {
                List<List<PlaneCrewRallyWay>> planeList = new List<List<PlaneCrewRallyWay>>();
                var row = mode.GetChild(j);
                for (int k = 0; k < 3; ++k)
                {
                    var plane = row.GetChild(k);
                    List<PlaneCrewRallyWay> variantList = new List<PlaneCrewRallyWay>();
                    for (int l = 0; l < plane.childCount; ++l)
                    {
                        var variant = plane.GetChild(l);
                        var way = new PlaneCrewRallyWay();
                        for (int m = 0; m < variant.childCount; ++m)
                        {
                            var child = variant.GetChild(m);
                            if (m == variant.childCount - 1)
                            {
                                way.Rally = child.GetComponent<PlaneCrewRallyPoint>();
                                if (way.Rally == null)
                                {
                                    Debug.LogError(i + " " + j + " " + k + " " + l + " " + m + " " + child.name);
                                }
                                way.Rally.Init(way);
                            }
                            else
                            {
                                PlaneCrewNode node = new PlaneCrewNode(child.position);
                                if (m == 0)
                                {
                                    node.Rotation = child.rotation;
                                    node.IsJump = 0;
                                    way.JumpDown = node;
                                }
                                else if (m == 1)
                                {
                                    node.Rotation = child.rotation;
                                    node.IsJump = 1;
                                    way.JumpUp = node;
                                }
                                else
                                {
                                    way.Additional.Add(node);
                                }
                            }
                        }
                        variantList.Add(way);
                    }
                    planeList.Add(variantList);
                }
                rowList.Add(planeList);
            }

            if (i == 0)
            {
                rallyPoints.Add(EPlaneNodeGroup.DeckLaunching, rowList);
                rowsCount = rowList.Count;
            }
            else
            {
                rallyPoints.Add(EPlaneNodeGroup.DeckRecovering, rowList);
            }
        }
    }

    private void CreateWaypoints()
    {
        waypoints.Clear();

        for (int i = 0; i < waypointsRoot.childCount; ++i)
        {
            List<PlaneCrewWaypoints> list = new List<PlaneCrewWaypoints>();
            var mode = waypointsRoot.GetChild(i);
            var points = new PlaneCrewWaypoints();
            switch (i)
            {
                //LAUNCHING
                //RECOVERING
                case 0:
                case 1:
                    for (int j = 0; j < mode.childCount; ++j)
                    {
                        var points2 = new PlaneCrewWaypoints();
                        var row = mode.GetChild(j);
                        for (int k = 0; k < row.childCount; ++k)
                        {
                            var left = new List<PlaneCrewNode>();
                            var right = new List<PlaneCrewNode>();
                            var both = new List<PlaneCrewNode>();
                            var planeNr = row.GetChild(k);
                            for (int l = 0; l < planeNr.childCount; ++l)
                            {
                                var point = planeNr.GetChild(l);
                                string name = point.name.ToLower();
                                var node = new PlaneCrewNode(point.position);
                                if (name.Contains(CrewLeftPoint))
                                {
                                    left.Add(node);
                                }
                                else if (name.Contains(CrewRightPoint))
                                {
                                    right.Add(node);
                                }
                                else
                                {
                                    both.Add(node);
                                }
                                var plus = point.name.Contains("+");
                                var minus = point.name.Contains("-");
                                if (point.name.Contains("*") && !plus && !minus)
                                {
                                    node.ThisLineCrouch = true;
                                }
                                else if (plus)
                                {
                                    node.PrevLineCrouch = true;
                                }
                                else if (minus)
                                {
                                    node.NextLineCrouch = true;
                                }
                            }
                            left.AddRange(both);
                            right.AddRange(both);
                            points2.Left.Add(left);
                            points2.Right.Add(right);
                        }
                        list.Add(points2);
                    }
                    if (i == 0)
                    {
                        waypoints.Add(EPlaneNodeGroup.DeckLaunching, list);
                    }
                    else if (i == 1)
                    {
                        waypoints.Add(EPlaneNodeGroup.DeckRecovering, list);
                    }
                    break;
                //STARTING
                case 2:
                    for (int j = 0; j < mode.childCount; ++j)
                    {
                        var nodes = new List<PlaneCrewNode>();
                        var leftRight = mode.GetChild(j);
                        for (int k = 0; k < leftRight.childCount; ++k)
                        {
                            var child = leftRight.GetChild(k);
                            var node = new PlaneCrewNode(child.position);
                            var plus = child.name.Contains("+");
                            var minus = child.name.Contains("-");
                            if (child.name.Contains("*") && !plus && !minus)
                            {
                                node.ThisLineCrouch = true;
                            }
                            else if (plus)
                            {
                                node.PrevLineCrouch = true;
                            }
                            else if (minus)
                            {
                                node.NextLineCrouch = true;
                            }
                            nodes.Add(node);
                        }
                        if (j == 0)
                        {
                            points.Left.Add(nodes);
                        }
                        else
                        {
                            points.Right.Add(nodes);
                        }
                    }
                    list.Add(points);
                    waypoints.Add(EPlaneNodeGroup.Starting, list);
                    break;
                //LANDING
                case 3:
                    for (int j = 0; j < mode.childCount; ++j)
                    {
                        var row = mode.GetChild(j);
                        for (int k = 0; k < row.childCount; ++k)
                        {
                            var nodes = new List<PlaneCrewNode>();
                            var leftRight = row.GetChild(k);
                            for (int l = 0; l < leftRight.childCount; ++l)
                            {
                                var child = leftRight.GetChild(l);
                                var node = new PlaneCrewNode(child.position);
                                if (child.name.Contains("*"))
                                {
                                    node.ThisLineCrouch = true;
                                }
                                nodes.Add(node);
                            }
                            if (k == 0)
                            {
                                points.Left.Add(nodes);
                            }
                            else
                            {
                                points.Right.Add(nodes);
                            }
                        }
                    }
                    list.Add(points);
                    waypoints.Add(EPlaneNodeGroup.Landing, list);
                    break;
            }
        }
    }

    private void RemoveCallbacks(PlaneCrewMovement crew)
    {
        registered.Remove(crew);
        registeredAway.Remove(crew);
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class Door : MonoBehaviour//, ITickable
{
    //public SectionSegment Parent1;
    //public SectionSegment Parent2;
    //public FillableDanger DoorBurst;
    //public Waypoint ParentWaypoint1;

    //public Waypoint ParentWaypoint2;

    //public List<Waypoint> Parent1WaypointsOverride;
    //public List<Waypoint> Parent2WaypointsOverride;
    ///*
    //[SerializeField]
    //private GameObject doorMesh = null;
    //private Vector3 doorPosToCalculate = new Vector3();
    //*/

    //public SingleIcon DoorIcon;

    //[NonSerialized]
    //public Dictionary<SectionSegment, List<Waypoint>> NodesForDoor;

    //private bool destroyed;
    //public bool Destroyed
    //{
    //    get => destroyed;
    //    private set
    //    {
    //        if (destroyed != value)
    //        {
    //            destroyed = value;
    //            Assert.IsTrue(value);
    //            foreach (var animator in animators)
    //            {
    //                animator.SetBool("destroy", true);
    //            }
    //        }
    //    }
    //}

    //private int time;
    //private int finishTime;

    //private HashSet<SectionSegment> sides;

    //private float oldValue;
    //private float timer;

    //private Animator[] animators;

    //private bool canShowIcon;

    //private bool initDict;
    //private bool leak;

    //private void Awake()
    //{
    //    if (Parent1 == null)
    //    {
    //        Debug.LogError(name, this);
    //    }
    //    Assert.IsNotNull(Parent1);
    //    if (Parent2 == null)
    //    {
    //        Debug.LogError(name, this);
    //    }
    //    Assert.IsNotNull(Parent2);
    //    DoorBurst = new FillableDanger(true);

    //    sides = new HashSet<SectionSegment>();

    //    //DoorIcon.GetComponent<DoorButton>().Door = this;
    //}

    //private void Start()
    //{
    //    CameraManager.Instance.ViewChanged += OnViewChanged;

    //    CheckDoor(null, Parent1);
    //    CheckDoor(null, Parent2);

    //    InitDict();

    //    var dcMan = DamageControlManager.Instance;
    //    finishTime = dcMan.InitialStartLeakTime;

    //    DoorBurst.RepairData.Max = dcMan.RepairLeakTime;
    //    DoorBurst.EventData.Max = 100_000_000;
    //    DoorBurst.FillData.Max = dcMan.BurstDoorTime;

    //    DoorBurst.RepairData.ReachedMax += OnRepairReachedMax;
    //    DoorBurst.FillData.ReachedMax += OnLeakReachedMax;

    //    animators = GetComponentsInChildren<Animator>();
    //    Assert.IsNotNull(animators);
    //    foreach (var animator in animators)
    //    {
    //        animator.Play("DoorObject-opening", 0, 1f);
    //    }

    //    TimeManager.Instance.AddTickable(this);

    //    /*
    //    var waypoint1Pos = ParentWaypoint1.Trans.position;
    //    var waypoint2Pos = ParentWaypoint2.Trans.position;

    //    doorPosToCalculate.z = waypoint1Pos.z + ((waypoint2Pos.z - waypoint1Pos.z) / 2);
    //    doorPosToCalculate.x = waypoint1Pos.x;
    //    doorPosToCalculate.y = waypoint1Pos.y;

    //    doorMesh.transform.position = doorPosToCalculate;
    //    */
    //}

    //private void Update()
    //{
    //    if (!Destroyed)
    //    {
    //        if (DoorIcon != null)
    //        {
    //            if (DoorBurst.Exists)
    //            {
    //                //DoorIcon.gameObject.SetActive(canShowIcon);
    //                timer = Mathf.Max(timer + Time.deltaTime, 1f);
    //                //DoorIcon.SetFill(1f - Mathf.Lerp(oldValue, DoorBurst.FillData.Percent, time));
    //            }
    //            else
    //            {
    //                //DoorIcon.gameObject.SetActive(false);
    //            }
    //        }
    //        foreach (var animator in animators)
    //        {
    //            animator.SetBool("open", !Parent1.IsFlooding() && !Parent2.IsFlooding());
    //            animator.SetBool("leak", DoorBurst.Exists);
    //        }
    //    }
    //}

    //public void Tick()
    //{
    //    if (leak || DoorBurst.Exists)
    //    {
    //        if (!HudManager.Instance.HasNo(ETutorialMode.DisableDCEvents) || Destroyed || Parent1.Group.IsFullyFlooded() == Parent2.Group.IsFullyFlooded()
    //            || sides.Count == 2 || sides.Count == 0)
    //        {
    //            return;
    //        }
    //        timer = 0f;
    //        oldValue = DoorBurst.FillData.Percent;
    //        DoorBurst.Update();
    //        if (!DoorBurst.Exists && ++time == finishTime)
    //        {
    //            MakeLeak();
    //        }
    //    }
    //}

    //public void SideFull(SectionSegment segment)
    //{
    //    Assert.IsTrue(segment == Parent1 || segment == Parent2);
    //    if (!sides.Contains(segment))
    //    {
    //        sides.Add(segment);
    //        Assert.IsTrue(sides.Count < 3);
    //        if (sides.Count == 2)
    //        {
    //            foreach (var side in sides)
    //            {
    //                if (side != segment)
    //                {
    //                    //side.Group.StartLeak();
    //                }
    //            }
    //            HideLeak();
    //        }
    //    }
    //}

    //public void SideEmpty(SectionSegment segment)
    //{
    //    Assert.IsTrue(segment == Parent1 || segment == Parent2);
    //    Assert.IsTrue(sides.Contains(segment));

    //    sides.Remove(segment);
    //    foreach (var newSegment in sides)
    //    {
    //        newSegment.Group.StartLeak();
    //    }
    //    Assert.IsTrue(sides.Count < 2);
    //    if (sides.Count == 0)
    //    {
    //        Assert.IsTrue(sides.Count == 0);
    //        HideLeak();
    //        leak = false;
    //    }
    //}

    //public bool CanLeak()
    //{
    //    Assert.IsFalse(sides.Count == 0);
    //    return sides.Count == 1;
    //}

    //public void StartLeak()
    //{
    //    leak = true;
    //}

    //public List<Waypoint> GetWaypoints(SectionSegment side)
    //{
    //    InitDict();
    //    return NodesForDoor[side];
    //}

    //public bool HasLeak(SectionSegment segment)
    //{
    //    InitDict();
    //    Assert.IsNotNull(NodesForDoor);
    //    Assert.IsTrue(NodesForDoor.ContainsKey(segment), NodesForDoor.Count.ToString());
    //    Assert.IsFalse(NodesForDoor[segment].Count == 0);
    //    //return showLeak && sides.Contains(segment);
    //    return !Destroyed && DoorBurst.Exists;
    //}

    //public void MakeLeak()
    //{
    //    if (!DoorBurst.Exists)
    //    {
    //        DoorBurst.Exists = true;
    //        DoorBurst.ActiveFill = true;
    //        time = 0;
    //    }
    //}

    //public void CheckDoor(StringBuilder builder, SectionSegment segment)
    //{
    //    var waypoint = ParentWaypoint1;
    //    var waypointsOverride = Parent1WaypointsOverride;
    //    if (Parent2 == segment)
    //    {
    //        waypoint = ParentWaypoint2;
    //        waypointsOverride = Parent2WaypointsOverride;
    //    }
    //    else
    //    {
    //        Assert.IsTrue(Parent1 == segment);
    //    }
    //    int count = waypointsOverride.Count;
    //    if (count == 0)
    //    {
    //        foreach (var branch in waypoint.Branches)
    //        {
    //            if (branch.Data.PossibleTasks == EWaypointTaskType.RepairDoor)
    //            {
    //                count++;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        foreach (var waypointu in waypointsOverride)
    //        {
    //            if (waypointu.Data.PossibleTasks != EWaypointTaskType.RepairDoor)
    //            {
    //                var erroru = name + " one of overriden waypoints is not for door repairing";
    //                if (builder != null)
    //                {
    //                    builder.AppendLine(erroru);
    //                }
    //                else
    //                {
    //                    Debug.LogWarning(erroru, this);
    //                }
    //            }
    //        }
    //    }

    //    string error = "";
    //    if (count > 3)
    //    {
    //        error = ("segment " + segment.name + " for door " + name + " has more repair doors waypoints than needed, " + count + ">3");
    //    }
    //    else if (count < 3)
    //    {
    //        error = ("segment " + segment.name + " for door " + name + " has less repair doors waypoints than needed, " + count + "<3");
    //    }

    //    if (!string.IsNullOrEmpty(error))
    //    {
    //        if (builder != null)
    //        {
    //            builder.AppendLine(error);
    //        }
    //        else
    //        {
    //            Debug.LogWarning(error, this);
    //        }
    //    }
    //}

    //public void Check(SectionSegment segment)
    //{
    //    InitDict();
    //    Assert.IsTrue(NodesForDoor.ContainsKey(segment), NodesForDoor.Count.ToString());
    //}

    //private void OnRepairReachedMax()
    //{
    //    finishTime = DamageControlManager.Instance.SealedStartLeakTime;
    //    DoorBurst.Exists = false;
    //    DoorBurst.ActiveFill = false;
    //    HideLeak();
    //}

    //private void OnLeakReachedMax()
    //{
    //    leak = false;
    //    Destroyed = true;
    //    DoorBurst.Exists = false;
    //    DoorBurst.ActiveFill = false;
    //    BreachDoors(Parent1, Parent2);
    //    BreachDoors(Parent2, Parent1);

    //    var segment = Parent1;
    //    if (!segment.Group.IsFloodActive() || segment.Group.Flood.FillData.Percent < 1f)
    //    {
    //        segment = Parent2;
    //        Assert.IsTrue(segment.Group.IsFloodActive() && segment.Group.Flood.FillData.Percent == 1f);
    //    }
    //    //segment.Group.FloodAllNeighbours();

    //    HideLeak();
    //    //gameObject.SetActive(false);

    //    foreach (var side in sides)
    //    {
    //        side.Group.LeakStopped();
    //    }
    //}

    //private void OnViewChanged(ECameraView view)
    //{
    //    canShowIcon = view == ECameraView.Sections;
    //}

    //private void HideLeak()
    //{
    //    time = 0;
    //    DoorBurst.FillData.Current = 0f;
    //    if (sides.Count == 1 && !Destroyed)
    //    {
    //        leak = false;
    //    }

    //    if (DoorBurst.ActiveFill)
    //    {
    //        DoorBurst.FillData.Current = 0f;
    //    }
    //}

    //private void CreateWaypoints(SectionSegment parent, Waypoint parentWaypoint, List<Waypoint> waypointsOverride)
    //{
    //    var list = waypointsOverride;
    //    if (list == null || list.Count == 0)
    //    {
    //        list = new List<Waypoint>();
    //        foreach (var waypoint in parentWaypoint.Branches)
    //        {
    //            if (waypoint.Data.PossibleTasks == EWaypointTaskType.RepairDoor)
    //            {
    //                list.Add(waypoint);
    //            }
    //        }
    //    }
    //    NodesForDoor[parent] = list;
    //}

    //private void BreachDoors(SectionSegment segment, SectionSegment segment2)
    //{
    //    //if (segment.Dc != null && segment.Dc.Job == EWaypointTaskType.RepairDoor && segment.Dc.RepairedDoor == this)
    //    //{
    //    //    segment.Dc.StopJob();
    //    //}
    //    //var data = segment.NeighboursDictionary[segment2];
    //    //data.ObstacleType--;
    //    //Assert.IsTrue(data.ObstacleType == ESectionSegmentObstacle.Open || data.ObstacleType == ESectionSegmentObstacle.OpenHigher);
    //}

    //private void InitDict()
    //{
    //    if (!initDict)
    //    {
    //        initDict = true;
    //        NodesForDoor = new Dictionary<SectionSegment, List<Waypoint>>();
    //        CreateWaypoints(Parent1, ParentWaypoint1, Parent1WaypointsOverride);
    //        CreateWaypoints(Parent2, ParentWaypoint2, Parent2WaypointsOverride);
    //    }
    //}
}

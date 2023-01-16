using GambitUtils;
using GPUInstancer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DCInstanceGroup : InstanceGroup, IEnableable
{
    private const float LongWait = 1e9f;
    private const float TeleportFromExitDelay = 3f;
    private const float TeleportThroughSegmentDelay = 6f;
    private const float DelayBetweenInstances = .5f;
    public SectionSegment CurrentSegment
    {
        get => currentSegment;
        set
        {
            currentSegment = value;
            if (CurrentSegment != null && CurrentSegment == FinalSegment)
            {
                currentSegment.RemoveOutlineBlinking();
            }
        }
    }
    public int PortraitIndex
    {
        get;
        private set;
    }

    public bool PlayerOverriden
    {
        get;
        private set;
    }

    public bool CrashOnly
    {
        get;
        set;
    }

    public DCButton Button;
    public PortraitDC Portrait;

    public List<PathData> PathPos;
    public List<SectionSegment> Path;

    public EWaypointTaskType Job;

    public Door RepairedDoor;

    public int NextSegmentIndex;
    //private bool throughSegment;

    public EDcCategory Category;

    private WorkerPath destinationTree;
    private List<Waypoint> destinationPoints;

    private EWaypointTaskType priorityTask;

    public SectionSegment FinalSegment;

    private SectionSegment currentSegment;
    private bool exit;
    private bool wasExit;
    private bool exitNext;
    private bool teleportThroughSegment;
    private bool busy;

    private HashSet<InstanceData> movedPastDestination;
    private HashSet<int> movedPastNextDestinationIndices;
    private HashSet<int> oldPastDestinationIndices;
    private HashSet<InstanceData> waiting;
    private HashSet<InstanceData> idling;

    private HashSet<InstanceData> rescuing;
    private HashSet<InstanceData> pushingWreck;

    private bool setup;
    private bool inactive;

    private List<EWaypointTaskType> jobs;

    private bool hasDelay;
    private float delay;
    private float calculatedAll;
    private bool unsetExit;

    private List<SectionSegment> queue;

    private List<SectionSegmentPathData> toVisit;
    private HashSet<SectionSegment> visited;

    public DCInstanceGroup(DCButton button, PortraitDC portrait) : base(EWaypointTaskType.Firefighting)
    {
        Path = new List<SectionSegment>();
        PathPos = new List<PathData>();
        destinationPoints = new List<Waypoint>();
        Job = EWaypointTaskType.Normal;
        Category = EDcCategory.Fire;
        priorityTask = EWaypointTaskType.Normal;
        movedPastDestination = new HashSet<InstanceData>();
        movedPastNextDestinationIndices = new HashSet<int>();
        idling = new HashSet<InstanceData>();
        waiting = new HashSet<InstanceData>();

        rescuing = new HashSet<InstanceData>();
        pushingWreck = new HashSet<InstanceData>();

        Button = button;
        Portrait = portrait;

        inactive = true;
        //deckDC = new List<DeckDC>();

        queue = new List<SectionSegment>();

        toVisit = new List<SectionSegmentPathData>();
        visited = new HashSet<SectionSegment>();
    }

    public void SetEnable(bool enable)
    {
        Button.SetEnable(enable);
        Portrait.SetEnable(enable);
    }

    public void Setup()
    {
        Assert.IsFalse(setup);
        setup = true;

        Portrait.Setup(this);
        Button.Setup(this);

        jobs = new List<EWaypointTaskType>();
        for (int i = 0; i < Instances.Count; i++)
        {
            Instances[i].TaskType = EWaypointTaskType.Repair;
            Instances[i].UpdateSpeed(false, false);
            Instances[i].TaskType = EWaypointTaskType.Normal;
            Instances[i].Dc = true;

            jobs.Add(EWaypointTaskType.Normal);

            Wait(Instances[i]);
            //Instances[i].SetStart(destination);
            //Instances[i].Trans.position = destination.Trans.position;

            //var rotationVector = destination.Trans.position - destination.Branches[0].Trans.position;
            //Instances[i].Trans.rotation = Quaternion.LookRotation(rotationVector.normalized, Vector3.up);

            //DebugCheck(Instances[i]);
        }
        setup = false;
    }

    public void OnSpawned()
    {
        Portrait.SetButton(TacticManager.Instance.SOTacticMap.Overrides.EnableDCNoCategory ? EDcCategory.Destroyed : DamageControlManager.Instance.DefaultCategory, true);
    }

    public void SetPortrait(int portait, List<DcImages> portraits)
    {
        PortraitIndex = portait;
        Portrait.SetImage(portraits[portait].Square);
        Button.SetupIcon(portraits[portait].Circle);
    }

    public void LoadData(DCSaveData data, List<DcImages> portraits)
    {
        Assert.IsTrue(Path.Count == 0);
        Assert.IsTrue(Job == EWaypointTaskType.Normal);

        Portrait.SetButton(data.Category, true);
        SetPortrait(data.Portrait, portraits);

        var sectionMan = SectionRoomManager.Instance;
        var wreckSection = DamageControlManager.Instance.WreckSection;

        FinalSegment.Dc = null;
        CurrentSegment = FinalSegment = GetSectionSegment(data.CurrentSegment, wreckSection, sectionMan);
        FinalSegment.Dc = this;
        destinationTree = CurrentSegment.Parent.Path;
        Button.UpdateSegment();

        idling.Clear();
        waiting.Clear();

        var workerManager = WorkerInstancesManager.Instance;
        SetIdleDestinations(CurrentSegment, destinationTree);
        for (int i = 0; i < Instances.Count; i++)
        {
            Instances[i].Tree = destinationTree;
            Instances[i].TeleportAndRotate(destinationPoints[i]);
            if (Instances[i].CurrentAnim != null)
            {
                workerManager.StopAnim(Instances[i]);
            }
            movedPastDestination.Add(Instances[i]);
            AddIdle(Instances[i]);

            GPUInstancerAPI.UpdateTransformDataForInstance(workerManager.GPUICrowdManager, Instances[i].Ref);
        }

        DebugCheckAll();

        movedPastDestination.Clear();

        queue.Clear();
        foreach (var segment in data.Queue)
        {
            queue.Add(GetSectionSegment(segment, wreckSection, sectionMan));
        }

        if (data.CurrentSegment == data.FinalSegment)
        {
            SetJob();
        }
        else
        {
            SetPath(GetSectionSegment(data.FinalSegment, wreckSection, sectionMan), EWaypointTaskType.Normal, data.PlayerOverriden, false);
        }
    }

    public DCSaveData SaveData()
    {
        var result = new DCSaveData();

        result.Portrait = PortraitIndex;

        var sectionMan = SectionRoomManager.Instance;
        var wreckSection = DamageControlManager.Instance.WreckSection;
        result.CurrentSegment = GetSegmentIndex(CurrentSegment, wreckSection, sectionMan);
        result.FinalSegment = GetSegmentIndex(FinalSegment, wreckSection, sectionMan);
        result.PlayerOverriden = PlayerOverriden;
        result.Queue = new List<int>();
        result.Category = Category;
        foreach (var segment in queue)
        {
            result.Queue.Add(GetSegmentIndex(segment, wreckSection, sectionMan));
        }

        return result;
    }

    public void Update()
    {
        DebugCheckAll();
        if (hasDelay)
        {
            delay -= Time.deltaTime;
            if (delay <= 0f)
            {
                hasDelay = false;
                Assert.IsFalse(exit);
                Next();
            }
            else
            {
                return;
            }
        }

        if (!exit && IsPathToNextSegmentBlocked(NextSegmentIndex - 1))
        {
            //Debug.Log("Obstacle on path");
            exit = true;
            teleportThroughSegment = false;
            destinationPoints.Clear();
            foreach (var dcInstance in Instances)
            {
                bool inExit = !dcInstance.GoToNearestExit();
                if (inExit)
                {
                    TeleportedToExit(dcInstance);
                }
            }
        }

        var worInMan = WorkerInstancesManager.Instance;
        for (int i = 0; i < Instances.Count; i++)
        {
            if (!pushingWreck.Contains(Instances[i]) && !rescuing.Contains(Instances[i]) && Instances[i].CurrentAnim == null)
            {
                Instances[i].Delay -= Time.deltaTime;
                if (Instances[i].Delay < Mathf.Epsilon)
                {
                    Instances[i].Delay = 0f;
                    //teleported from whole segment
                    if (teleportThroughSegment)
                    {
                        //Debug.Log("teleport in backstage");
                        TeleportedToExit(Instances[i]);
                    }
                    else
                    {
                        if (Instances[i].Go(Time.deltaTime))
                        {
                            //if all in destination
                            if (InstanceInDestination(i))
                            {
                                Next();
                            }
                            DebugCheck(Instances[i]);
                        }
                    }
                    GPUInstancerAPI.UpdateTransformDataForInstance(worInMan.GPUICrowdManager, Instances[i].Ref);
                }
            }
        }

        ////active listen for door leak stop and for flood
        //if (Path.Count == 0 && !busy)
        //{
        //    if (//(Job == EWaypointTaskType.RepairDoor && !RepairedDoor.HasLeak(CurrentSegment)) ||
        //        (Job == EWaypointTaskType.Waterpump && (!InWaterPumps() || PumpedSegmentGroup == null || !PumpedSegmentGroup.Flood.Exists)))
        //    {
        //        //Debug.Log("Stopping job because of reasons");
        //        StopJob(true);
        //    }

        //}
    }

    public bool SetPath(SectionSegment destination, EWaypointTaskType job, bool playerOverride, bool clearQueue)
    {
        PlayerOverriden = playerOverride;
        var dcMan = DamageControlManager.Instance;
        if (job == EWaypointTaskType.RepairDoor)
        {
            return false;
        }
        if (destination.IsFlooded() || (destination.Dc != null && destination.Dc != this))
        {
            //Debug.Log("flooded or already another dc there");
            DebugCheckAll();
            return false;
        }
        if (Path.Count == 0)
        {
            if (dcMan.PumpsDCFreeze && CurrentSegment.CanPumpWater)
            {
                return false;
            }
            if (dcMan.MaintenanceDCFreeze)
            {
                foreach (var section in dcMan.MaintenanceRooms)
                {
                    if (section.IsWorking && section == CurrentSegment.Parent.ParentSection)
                    {
                        return false;
                    }
                }
            }
        }
        if (dcMan.IssueDestination != null && !CheckIssue(dcMan.IssueDestination.Value, destination))
        {
            return false;
        }
        //setup priority job
        if (job == EWaypointTaskType.RepairDoor)
        {
            RepairedDoor = null;
        }
        priorityTask = job;
        if (clearQueue)
        {
            queue.Clear();
        }

        //if instances are sent to same destination
        if (CurrentSegment == destination)
        {
            //Debug.Log("player-specified job in current segemnt");
            if (Path.Count == 0)
            {
                FinalSegment.RemoveOutlineBlinking();
                //Debug.Log("already in place");
                Assert.IsTrue(movedPastDestination.Count == 0 || (movedPastDestination.Count == idling.Count));
                foreach (var dcInstance in movedPastDestination)
                {
                    Assert.IsTrue(idling.Contains(dcInstance));
                }
                Assert.IsTrue(movedPastNextDestinationIndices.Count == 0);

                //if they are idling
                if (idling.Count == Instances.Count)
                {
                    //Debug.Log("all idling");
                    bool clearIdles = true;

                    //doors are exception
                    if (job == EWaypointTaskType.RepairDoor)
                    {
                        //if (door == null)
                        //{
                        clearIdles = CheckLeakedDoors();
                        //}
                        //else if (door.HasLeak(CurrentSegment))
                        //{
                        //    DoDoors(door);
                        //}
                        //else
                        //{
                        //    clearIdles = false;
                        //}
                    }
                    else if (job == EWaypointTaskType.Normal)
                    {
                        clearIdles = false;
                    }
                    else
                    {
                        SetJob();
                    }

                    //stop wait, stop idle only when there is job
                    if (clearIdles)
                    {
                        //?
                        RemoveWaits();
                        RemoveIdles();
                        Assert.IsTrue(movedPastDestination.Count == 0);
                    }
                }
                else if (job != EWaypointTaskType.Normal && job != Job)
                {
                    //Debug.Log("priority job");
                    priorityTask = job;
                    //check if they are already going to idle
                    if (!busy)
                    {
                        //stop and go to idle
                        RemoveWaits();
                        StopJob(true);
                    }
                }
            }
            else
            {
                //Debug.Log("stop moving, current segment is final");
                Assert.IsTrue(destination.Dc == null || destination.Dc == this);
                if (FinalSegment != null)
                {
                    FinalSegment.Dc = null;
                    FinalSegment.RemoveOutlineBlinking();
                }
                FinalSegment = destination;
                FinalSegment.RemoveOutlineBlinking();
                FinalSegment.Dc = this;
                //SegmentEntered();

                //stop moving to another segment
                if (hasDelay)
                {
                    hasDelay = false;
                    wasExit = false;
                    unsetExit = false;
                    RemoveWaits();
                }
                exit = false;

                movedPastDestination.Clear();
                movedPastNextDestinationIndices.Clear();
                Path.Clear();
                PathPos.Clear();
                Button.UpdateSegment();
                teleportThroughSegment = false;

                //move to idle
                SetAndMoveToIdleDestinations();

                priorityTask = job;
            }
        }
        else if (FinalSegment != destination)
        {
            //Debug.Log("new destination");
            //if (CurrentSegment == FinalSegment)
            //{
            //    SegmentQuitted();
            //}

            Assert.IsTrue(destination.Dc == null || destination.Dc == this);
            if (FinalSegment != null)
            {
                FinalSegment.Dc = null;
                FinalSegment.RemoveOutlineBlinking();
            }
            FinalSegment = destination;
            FinalSegment.Dc = this;
            FinalSegment.AddOutlineBlinking();

            if (Path.Count > 0)
            {
                //Debug.Log("cleared old destination");
                exitNext = false;
                teleportThroughSegment = false;
                busy = false;
                //stop moving to another segment
                hasDelay = false;
                wasExit = false;
                unsetExit = false;
                movedPastDestination.Clear();
                movedPastNextDestinationIndices.Clear();
            }
            else if (!busy && idling.Count == 0)
            {
                Assert.IsFalse(inactive, "inactive set path");
                RemoveWaits();
                movedPastDestination.Clear();
                //Debug.Log("looks like dc is working");
                //stop any job
                StopJob(false);
            }
            else if (idling.Count < Instances.Count)
            {
                //Debug.Log("not yet idling");
                movedPastDestination.Clear();
            }
#warning this can generate moved past?
            Assert.IsTrue(movedPastDestination.Count == 0);
            Assert.IsTrue(movedPastNextDestinationIndices.Count == 0);
            //instances start moving, reset waiting
            RemoveWaits();
            RemoveIdles();

            //set path proper
            NextSegmentIndex = 1;
            FindPath(destination);
            Button.UpdateSegment();

            var oldDestinationPoints = new List<Waypoint>();
            WorkerPath oldDestinationTree = null;

            //move to next segment now
            exit = SetNextDestinations(0, oldDestinationPoints, ref oldDestinationTree);
            Assert.IsFalse(Path.Count == 1);
            //if moving to exit
            if (exit)
            {
                //Debug.Log("moving to backstage");
                //clear destination
                destinationPoints.Clear();
            }
            else
            {
                //Debug.Log("next destination");
                //setup next next destination
                exitNext = SetNextDestinations(1, destinationPoints, ref destinationTree);
            }

            Assert.IsTrue(movedPastDestination.Count == 0);
            Assert.IsTrue(movedPastNextDestinationIndices.Count == 0);
            //have to know new destinations
            //if all in destinations, move next
            if (MoveInstancesToDestinations(oldDestinationTree, oldDestinationPoints))
            {
                //Debug.Log("in destination in set path");
                Next();
            }
        }
        dcMan.SetShowPath(this);

        DebugCheckAll();
        return true;
    }

    public bool CheckQueue()
    {
        if (queue.Count > 0)
        {
            if (SetPath(queue[0], EWaypointTaskType.Normal, true, false))
            {
                queue.RemoveAt(0);
                return true;
            }
            queue.RemoveAt(0);
            return CheckQueue();
        }
        return false;
    }

    public bool QueuePath(SectionSegment segment, ESectionUIState positiveState)
    {
        var sectionRoomMan = SectionRoomManager.Instance;
        var voiceSoundsMan = VoiceSoundsManager.Instance;
        var dcMan = DamageControlManager.Instance;
        if (queue.Count < dcMan.MaxDCQueue)
        {
            queue.Add(segment);
            dcMan.SetShowPath(this);
            if (queue.Count == 1 && Path.Count == 0 && !busy)
            {
                if (CheckQueue())
                {
                    sectionRoomMan.PlayEvent(positiveState);
                    voiceSoundsMan.PlayPositive(EVoiceType.DC);
                }
                else
                {
                    sectionRoomMan.PlayEvent(ESectionUIState.DCNegative);
                    voiceSoundsMan.PlayNegative(EVoiceType.DC);
                }
            }
            return true;
        }
        sectionRoomMan.PlayEvent(ESectionUIState.DCNegative);
        voiceSoundsMan.PlayNegative(EVoiceType.DC);
        return false;
    }

    public void StopJob()
    {
        //Debug.Log("stop job event, " + Time.frameCount);
        if (Job != EWaypointTaskType.Normal)
        {
            StopJob(true);
        }

        DebugCheckAll();
    }

    public void StopJob(bool moveToIdle, bool debug = true)
    {
        //Debug.Log("job stopped");
        //instances should not idle or wait on job??
        Assert.IsTrue(idling.Count == 0, "not empty idles " + moveToIdle);
        Assert.IsTrue(waiting.Count == 0, "not empty waiting " + moveToIdle);

        //exit anim
        foreach (var dcInstance in Instances)
        {
            if (dcInstance.CurrentAnim != null)
            {
                //Debug.Log("dc in anim");
                dcInstance.AnimRepeat = 0;
                dcInstance.FastAnim = true;
                dcInstance.IsExitting = true;
            }
        }

        ClearJob(debug);

        //move to idle
        if (moveToIdle)
        {
            //Debug.Log("after stopping, moving to idle");
            SetAndMoveToIdleDestinations();
        }
        if (debug)
        {
            DebugCheckAll();
        }
    }

    //add job for idling instances
    public void TryDo(EWaypointTaskType task)
    {
        if (!PlayerOverriden)
        {
            switch (Category)
            {
                case EDcCategory.Fire:
                    if (task != EWaypointTaskType.Firefighting)
                    {
                        return;
                    }
                    break;
                case EDcCategory.Water:
                    if (task != EWaypointTaskType.Waterpump)
                    {
                        return;
                    }
                    break;
                case EDcCategory.Injured:
                    if ((EWaypointTaskType.Rescues & task) == 0)
                    {
                        return;
                    }
                    break;
                case EDcCategory.Crash:
                    if (task != EWaypointTaskType.Repair || (CurrentSegment.Parent.Irrepairable && !CurrentSegment.Damage.Exists))
                    {
                        return;
                    }
                    break;
            }
        }
        if (idling.Count == Instances.Count)
        {
            //Debug.Log("try do, all idling");
            bool clearIdles = true;

            //doors are exception
            if (task == EWaypointTaskType.RepairDoor)
            {
                //may repair or may not
                clearIdles = CheckLeakedDoors();
            }
            else
            {
                Do(task);
            }

            //stop wait, stop idle only when there is job
            if (clearIdles)
            {
                //Debug.Log("can do");
                //?
                RemoveWaits();
                RemoveIdles();
                Assert.IsTrue(movedPastDestination.Count == 0);
            }
        }

        DebugCheckAll();
    }

    public void Kickout(bool onlyOKSegments)
    {
        //Debug.Log("kickout, segment flooded");
        if (!CheckQueue())
        {
            SectionSegment segment = null;
            if (onlyOKSegments)
            {
                segment = SectionRoomManager.Instance.FindEmptyOKSegment(null, CurrentSegment);
            }
            if (segment == null)
            {
                segment = SectionRoomManager.Instance.FindEmptySegment(null, CurrentSegment);
            }
            if (segment != null)
            {
                SetPath(segment, EWaypointTaskType.Normal, false, true);
            }
        }

        DebugCheckAll();
    }

    public void SetIdleRotation(InstanceData dcInstance)
    {
        dcInstance.Trans.rotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
    }

    public float SegmentTransitionPercent()
    {
        if (hasDelay)
        {
            return (calculatedAll - delay) / calculatedAll;
        }
        if (!(Path.Count > NextSegmentIndex))
        {
            return 1f;
        }
        Assert.IsTrue(Path.Count > NextSegmentIndex);
        float result = 1f;
        foreach (var dcInstance in Instances)
        {
            if (!movedPastDestination.Contains(dcInstance))
            {
                calculatedAll = exit ? TeleportFromExitDelay : 0f;
                float moved = 0f;
                Vector3 prevPoint = Vector3.zero;
                bool pointSet = false;
                for (int i = 0; i < dcInstance.SegmentPath.Count; i++)
                {
                    var data = dcInstance.SegmentPath[i];
                    int max;
                    if (i < dcInstance.CurrentSegment)
                    {
                        max = data.Points.Count;
                    }
                    else if (i == dcInstance.CurrentSegment)
                    {
                        max = dcInstance.CurrentPoint;
                    }
                    else
                    {
                        max = -1;
                    }

                    for (int j = 0; j < data.Points.Count; j++)
                    {
                        if (pointSet)
                        {
                            float dist = Vector3.Distance(prevPoint, data.Points[j]);
                            calculatedAll += dist;
                            if (j < max)
                            {
                                moved += dist;
                            }
                            else if (j == max)
                            {
                                moved += dist * dcInstance.TimePassed / dcInstance.TimeLen;
                            }
                        }
                        prevPoint = data.Points[j];
                        pointSet = true;
                    }
                }
                float newValue = moved / calculatedAll;
                if (newValue < result)
                {
                    result = newValue;
                }
            }
        }
        return result;
    }

    public bool InWaterPumps()
    {
        return CurrentSegment != null && CurrentSegment.CanPumpWater && Path.Count == 0 && CurrentSegment.Parent.IsWorking;
    }

    public void QuitSubsection()
    {
        if (!CheckQueue())
        {
            SetPath(SectionRoomManager.Instance.FindEmptySegment(CurrentSegment.Parent, CurrentSegment), EWaypointTaskType.Normal, false, true);
        }
    }

    public List<Waypoint> GetIdleDestinations()
    {
        SetIdleDestinations(CurrentSegment, CurrentSegment.Parent.Path);
        return destinationPoints;
    }

    public string GetStatus(InstanceData dcInstance)
    {
        DebugCheckAll();
        return "" + waiting.Contains(dcInstance) + " " + waiting.Count + " " + idling.Contains(dcInstance) + " " + idling.Count + " " +
            movedPastDestination.Contains(dcInstance) + " " + movedPastDestination.Count + " " + movedPastNextDestinationIndices.Count + " " +
            movedPastNextDestinationIndices.Contains(Instances.IndexOf(dcInstance)) + " " +
            Job.ToString() + " " + priorityTask.ToString() + " " + Path.Count + " " + FinalSegment.name + " " + exit + " " + exitNext + " " + busy;
    }

    public bool CanAssign(bool force)
    {
        if (Path.Count == 0 && Job == EWaypointTaskType.Normal && idling.Count == Instances.Count)
        {
            if (force)
            {
                return true;
            }
            else if (PlayerOverriden)
            {
                if (CurrentSegment.HasAnyRepairableIssue())
                {
                    return false;
                }
            }
            else
            {
                switch (Category)
                {
                    case EDcCategory.Fire:
                        return !CurrentSegment.Fire.Exists;
                    case EDcCategory.Water:
                        return !CurrentSegment.Group.Flood.Exists;
                    case EDcCategory.Injured:
                        return !CurrentSegment.CanRescue();
                    case EDcCategory.Crash:
                        return !CurrentSegment.Parent.BrokenRepairable && !CurrentSegment.Damage.Exists;
                }
            }
            return true;
        }
        return false;
    }

    public void ClearQueue()
    {
        queue.Clear();
    }

    public void RecheckJob()
    {
        SetJob();
    }

    public IEnumerable<SectionSegment> GetQueue()
    {
        foreach (var segment in queue)
        {
            yield return segment;
        }
    }

    public void Show(SectionSegment segment)
    {
        //Debug.Log("started");
        inactive = false;
        CurrentSegment = FinalSegment = segment;
        segment.Dc = this;

        destinationTree = segment.Parent.Path;

        Button.UpdateSegment();

        var worInMan = WorkerInstancesManager.Instance;
        var destination = destinationTree.ExitsBySegment[segment][0];

        waiting.Clear();
        float i = 0f;
        foreach (var dcInstance in Instances)
        {
            dcInstance.SetStart(destination);
            dcInstance.Trans.position = destination.Trans.position;
            dcInstance.Delay = DelayBetweenInstances * i++;
            if (dcInstance.CurrentAnim != null)
            {
                worInMan.StopAnim(dcInstance);
            }

            GPUInstancerAPI.UpdateTransformDataForInstance(worInMan.GPUICrowdManager, dcInstance.Ref);
        }
        SetAndMoveToIdleDestinations();
        SetIdleDestinations(segment, destinationTree);

        foreach (var dcInstance in Instances)
        {
            dcInstance.LookAtDir();
        }

        DebugCheckAll();
    }

    public void Hide()
    {
        //Debug.Log("removed");
        inactive = true;
        busy = false;

        if (Path.Count != 0)
        {
            if (FinalSegment != null)
            {
                FinalSegment.Dc = null;
                FinalSegment.RemoveOutlineBlinking();
                FinalSegment = null;
            }

            //stop moving to another segment
            movedPastDestination.Clear();
            movedPastNextDestinationIndices.Clear();
            Path.Clear();
            PathPos.Clear();
            Button.UpdateSegment();
            teleportThroughSegment = false;
            if (hasDelay)
            {
                hasDelay = false;
                wasExit = false;
                unsetExit = false;
                RemoveWaits();
            }
        }
        else
        {
            if (FinalSegment != null)
            {
                FinalSegment.Dc = null;
                FinalSegment.RemoveOutlineBlinking();
                //SegmentQuitted(this, CurrentSegment);
                FinalSegment = null;
            }
            RemoveIdles();
            RemoveWaits();
            movedPastDestination.Clear();
            StopJob(false, false);
        }
        destinationPoints.Clear();

        if (CurrentSegment != null)
        {
            destinationTree = CurrentSegment.Parent.Path;
            if (destinationTree.ExitsBySegment.TryGetValue(CurrentSegment, out var exits))
            {
                destinationPoints.Add(Instances[0].GetNearestWaypoint(Instances[0].Trans.position, exits));
                MoveInstancesToDestinations(destinationTree, destinationPoints);
            }
            else
            {
                Debug.LogError(destinationTree.name + " has no exit in " + CurrentSegment.name);
            }
        }

        var dcMan = DamageControlManager.Instance;
        if (dcMan.SelectedGroup == this)
        {
            dcMan.SelectedGroup = null;
        }
        CurrentSegment = null;
        DebugCheckAll();
    }

    private void Next()
    {
        Assert.IsFalse(hasDelay);
        if (exit)
        {
            //Assert.IsFalse(wasExit);
            wasExit = true;
            exit = false;
            hasDelay = true;

            if (teleportThroughSegment)
            {
                calculatedAll = TeleportThroughSegmentDelay;
                delay = TeleportThroughSegmentDelay;
            }
            else
            {
                delay = TeleportFromExitDelay;
            }
            DamageControlManager.Instance.SetShowPath(this);
            return;
        }

        //next segment
        NextSegmentIndex++;
        if (Path.Count < NextSegmentIndex)
        {
            Debug.LogError("not possible");
            CurrentSegment = Path[Path.Count - 1];
            Button.UpdateSegment();
            movedPastDestination.Clear();
            movedPastNextDestinationIndices.Clear();
            RemoveWaits();
            RemoveIdles();

            Path.Clear();
            PathPos.Clear();
            Assert.IsTrue(CurrentSegment == FinalSegment);
            DamageControlManager.Instance.SetShowPath(this);

            return;
        }
        CurrentSegment = Path[NextSegmentIndex - 1];
        Button.UpdateSegment();

        //Debug.Log("next segment in path");

        //no longer any instance is past destination
        movedPastDestination.Clear();

        //but remember those past next destination
        oldPastDestinationIndices = new HashSet<int>(movedPastNextDestinationIndices);
        movedPastNextDestinationIndices.Clear();

        //no longer instances should wait
        RemoveWaits();

        //there should be no idling ones??
        Assert.IsTrue(idling.Count == 0);

        //moved to segment from exit
        if (wasExit)
        {
            wasExit = false;
            //Debug.Log("obstacle was in path");
            var path = CurrentSegment.Parent.Path;
            Waypoint nearestExit;
            try
            { 
                nearestExit = Instances[0].GetNearestWaypoint(Instances[0].Trans.position, path.ExitsBySegment[CurrentSegment]);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{CurrentSegment.Parent.ParentSection.name}; {CurrentSegment.Parent.name}; {CurrentSegment.name}; {path.name}");
                Debug.LogException(ex);
#if TEST_ERRORS
                Application.Quit();
#endif
                throw ex;
            }

            teleportThroughSegment = IsPathToNextSegmentBlocked(NextSegmentIndex - 1);
            float delayBetweenInstances = teleportThroughSegment ? 0f : DelayBetweenInstances;
            //check if should they be teleported
            //add delay increasing with every instance to split them if they will be moving in this segment
            for (int i = 0; i < Instances.Count; i++)
            {
                Instances[i].Tree = path;
                Instances[i].Delay = .5f + i * delayBetweenInstances;
                Instances[i].TeleportToDest(nearestExit);
            }

            //if in this segment they are tepping they need no paths/destinations
            if (teleportThroughSegment)
            {
                unsetExit = !exit;
                exit = true;

                //Debug.Log("teleport");
                movedPastNextDestinationIndices.Clear();
                DebugCheckAll();

                DamageControlManager.Instance.SetShowPath(this);
                return;
            }

            if (Path.Count == NextSegmentIndex)
            {
                Assert.IsFalse(hasDelay);
                //Debug.Log("path completed");
                //clear path, they are already in final destination segment
                Path.Clear();
                PathPos.Clear();
                Assert.IsTrue(CurrentSegment == FinalSegment);

                //SegmentEntered(this, CurrentSegment);
                //instances go to idle
                SetAndMoveToIdleDestinations();
            }
            else
            {
                //Debug.Log("next segment");
                var oldDestinationPoints = new List<Waypoint>();
                WorkerPath oldDestinationTree = null;
                //get next destinations and move instances
                exit = SetNextDestinations(NextSegmentIndex - 1, oldDestinationPoints, ref oldDestinationTree);

                destinationPoints.Clear();
                //check if there are in destination segment
                if (!exit)
                {
                    //find destination step further, for instances to move past destination
                    exitNext = SetNextDestinations(NextSegmentIndex, destinationPoints, ref destinationTree);
                }
                //have to know new destinations
                //if all in destinations, move next
                if (MoveInstancesToDestinations(oldDestinationTree, oldDestinationPoints))
                {
                    //Debug.Log("in next segment?");
                    Next();
                }
            }
        }
        else
        {
            //Debug.Log("no exit");
            exit = exitNext;
            exitNext = false;

            destinationPoints.Clear();

            //all of them already moving to next destination
            //check if there are in destination segment
            if (Path.Count == NextSegmentIndex)
            {
                Assert.IsFalse(hasDelay);
                //Debug.Log("path completed");
                //clear path, they are already in final destination segment
                Path.Clear();
                PathPos.Clear();
                Assert.IsTrue(CurrentSegment == FinalSegment);

                //SegmentEntered(this, CurrentSegment);

                //instances go to idle
                busy = true;

                //set those already on idle
                bool shouldFireNext = false;
                foreach (int index in oldPastDestinationIndices)
                {
                    shouldFireNext = InstanceInDestination(index);
                }
                //everybody in next destination? wtf
                Assert.IsFalse(shouldFireNext);
            }
            else if (!exit)
            {
                //Debug.Log("next segment");
                //find destination step further, for instances to move past destination
                exitNext = SetNextDestinations(NextSegmentIndex, destinationPoints, ref destinationTree);

                //set those already on next
                bool shouldFireNext = false;
                foreach (int index in oldPastDestinationIndices)
                {
                    shouldFireNext = InstanceInDestination(index);
                }
                //everybody in next destination? wtf
                //Assert.IsFalse(shouldFireNext);
            }
        }

        DamageControlManager.Instance.SetShowPath(this);
        DebugCheckAll();
    }

    private void FindPath(SectionSegment destination)
    {
        Assert.IsFalse(hasDelay);
        //Debug.Log("new path");
        Path.Clear();
        PathPos.Clear();

        toVisit.Clear();
        toVisit.Add(new SectionSegmentPathData(CurrentSegment, null));
        visited.Clear();
        while (toVisit.Count > 0)
        {
            var path = toVisit[0];
            toVisit.RemoveAt(0);
            foreach (var key in path.Segment.NeighboursDirectionDictionary.Keys)
            {
                if (visited.Add(key))
                {
                    if (key == destination)
                    {
                        Path.Add(destination);
                        while (path != null)
                        {
                            Path.Insert(0, path.Segment);
                            path = path.Previous;
                        }
                        PathPos.Add(new PathData(Path[0].Center));
                        for (int i = 1; i < Path.Count; i++)
                        {
                            var data = new PathData(Path[i].Center);
                            var prevData = PathPos[i - 1];
                            var diff = data.Pos - prevData.Pos;
                            float diffY = Mathf.Abs(diff.y);
                            float diffZ = Mathf.Abs(diff.z);
                            if (diffY > .1f && diffZ > .1f)
                            {
                                bool horizontal = diff.z < 0f || diff.y < 0f;
                                float helperDiff;
                                if (i > 1)
                                {
                                    var preverData = PathPos[i - 2];
                                    var pos2 = preverData.HelperPos ?? preverData.Pos;
                                    var diff2 = prevData.Pos - pos2;
                                    if (horizontal)
                                    {
                                        if ((diff.z < 0f) != (diff2.z < 0f))
                                        {
                                            horizontal = diff2.z == 0f;
                                        }
                                    }
                                    else
                                    {
                                        if ((diff.y < 0f) != (diff2.y < 0f))
                                        {
                                            horizontal = diff2.y != 0f;
                                        }
                                    }
                                }
                                else if (Path.Count > 2)
                                {
                                    var diff2 = Path[2].Center - data.Pos;
                                    bool nextHorizontal = Mathf.Abs(diff2.z) > .1f;
                                    if (nextHorizontal != Mathf.Abs(diff2.y) > .1f)
                                    {
                                        if (horizontal)
                                        {
                                            horizontal = (diff.y < 0f) == (diff2.y < 0f);
                                        }
                                        else
                                        {
                                            horizontal = (diff.z < 0f) != (diff2.z < 0f);
                                        }
                                    }
                                }
                                var pos = data.Pos;
                                if (horizontal)
                                {
                                    pos.y = prevData.Pos.y;
                                    helperDiff = Mathf.Abs(pos.z - prevData.Pos.z);
                                }
                                else
                                {
                                    pos.z = prevData.Pos.z;
                                    helperDiff = Mathf.Abs(pos.y - prevData.Pos.y);
                                }

                                pos.x = (pos.x + prevData.Pos.x) / 2f;
                                prevData.HelperPos = pos;
                                prevData.Percent = helperDiff / (diffY + diffZ);
                            }
                            PathPos.Add(data);
                        }
                        //DebugCheckAll();
                        return;
                    }
                    toVisit.Add(new SectionSegmentPathData(key, path));
                }
            }
        }
        Assert.IsTrue(false);
        DebugCheckAll();
    }

    private void Do(EWaypointTaskType task)
    {
        Assert.IsTrue(jobs.Count > 1);
        //Debug.Log("starting actual job");
        Job = task;
        Assert.IsFalse(task == EWaypointTaskType.RepairDoor);

        for (int i = 0; i < jobs.Count; i++)
        {
            jobs[i] = Job;
        }

        RepairableDanger danger = null;
        switch (Job)
        {
            case EWaypointTaskType.Firefighting:
                danger = CurrentSegment.Fire;
                break;
            case EWaypointTaskType.Repair:
                danger = CurrentSegment.Parent.BrokenRepairable ? CurrentSegment.Parent.Destruction : CurrentSegment.Damage;
                break;
            case EWaypointTaskType.Waterpump:
                danger = CurrentSegment.Group.Flood;
                break;
        }
        if ((Job & EWaypointTaskType.Rescues) == 0)
        {
            var segment = CurrentSegment;
            var job = Job;
            var text = $"{CurrentSegment.name} {(CurrentSegment.Parent == null ? "" : CurrentSegment.Parent.name)} {(CurrentSegment.Parent == null || CurrentSegment.Parent.ParentSection == null ? "" : CurrentSegment.Parent.ParentSection.name)} {Job}";
            Assert.IsNotNull(danger, text);
            Assert.IsTrue(danger.Exists, text);
            DamageControlManager.Instance.StartCoroutineActionAfterFrames(() => 
                {
                    if (Job == job && CurrentSegment == segment)
                    {
                        StartRepair(danger);
                    }
                }, 1);
        }
        else
        {
            FinalSegment.RescueInProgress = true;
#warning dc count hardcode
            bool rescue2 = Job == EWaypointTaskType.Rescue2;
            if (rescue2 || Job == EWaypointTaskType.Rescue3)
            {
                jobs[0] = EWaypointTaskType.Rescue;
                jobs[1] = rescue2 ? EWaypointTaskType.Rescue : EWaypointTaskType.Rescue2;
            }
        }

        var worInMan = WorkerInstancesManager.Instance;
        for (int i = 0; i < Instances.Count; i++)
        {
            var dcInstance = Instances[i];
            dcInstance.TaskType = jobs[i];
            dcInstance.SectionSegment = CurrentSegment;
            dcInstance.IgnoreSegmentOnly = Job == EWaypointTaskType.Repair && CurrentSegment.Parent.BrokenRepairable;
            if (i == 0 && jobs[i] == EWaypointTaskType.Rescue)
            {
                dcInstance.SetFirstRescueDestination();
            }
            else
            {
                //if rescue add new destination apart from first rescue
                dcInstance.SetNewDestination();
            }
            if (dcInstance.CurrentAnim == null)
            {
                dcInstance.LookAtDir();
                GPUInstancerAPI.UpdateTransformDataForInstance(worInMan.GPUICrowdManager, dcInstance.Ref);
            }
            else
            {
                dcInstance.IsExitting = true;
            }
            DebugCheck(dcInstance);
        }
    }

    //private void DoDoors(Door door)
    //{
    //    Debug.Log("starting door repair");
    //    Job = EWaypointTaskType.RepairDoor;
    //    Assert.IsNotNull(door);
    //    RepairedDoor = door;
    //    StartRepair(door.DoorBurst);

    //    var worInMan = WorkerInstancesManager.Instance;
    //    var waypoints = door.GetWaypoints(CurrentSegment);
    //    for (int i = 0; i < Instances.Count; i++)
    //    {
    //        Instances[i].TaskType = Job;
    //        Instances[i].SectionSegment = CurrentSegment;
    //        Instances[i].IgnoreSegmentOnly = false;

    //        Instances[i].FreeAnimWaypoint(Instances[i].GetDestination());
    //        Instances[i].SetNewDestination(CurrentSegment.Parent.Path, waypoints[i]);
    //        if (Instances[i].CurrentAnim == null)
    //        {
    //            Instances[i].LookAtDir();
    //            GPUInstancerAPI.UpdateTransformDataForInstance(worInMan.GPUICrowdManager, Instances[i].Ref);
    //        }
    //        else
    //        {
    //            Instances[i].IsExitting = true;
    //        }
    //        DebugCheck(Instances[i]);
    //    }
    //}

    private void StartRepair(RepairableDanger data)
    {
        DamageControlManager.Instance.UpdateRepairTicks(data.RepairData);
        data.RepairData.ReachedMax += StopJob;
        data.Repair = true;
        DebugCheckAll();
        Portrait.StartJob(data);
    }

    private void ClearJob(bool debug)
    {
        try
        {
            //Debug.Log("job cleared");
            try
            {
                Portrait.StartJob(null);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("C01");
                throw ex;
            }
            RepairableDanger danger = null;
            switch (Job)
            {
                case EWaypointTaskType.Firefighting:
                    try
                    {
                        danger = CurrentSegment.Fire;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("C02");
                        throw ex;
                    }
                    break;
                case EWaypointTaskType.Rescue:
                case EWaypointTaskType.Rescue2:
                case EWaypointTaskType.Rescue3:
                    try
                    {
                        rescuing.Clear();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("C03");
                        throw ex;
                    }
                    try
                    {
                        if (CurrentSegment.Rescued)
                        {
                            CurrentSegment.FastAnimInjured();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("C04");
                        throw ex;
                    }
                    break;
                case EWaypointTaskType.Repair:
                    try
                    {
                        danger = CurrentSegment.Damage;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("C05");
                        throw ex;
                    }
                    try
                    {
                        if (CurrentSegment.Parent.ParentSection == DamageControlManager.Instance.WreckSection)
                        {
                            try
                            {
                                pushingWreck.Clear();
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogError("C06");
                                throw ex;
                            }
                            try
                            {
                                PlaneMovementManager.Instance.CurrentWrecks[Instances[0].WreckType].Move = false;
                            }
                            catch (System.Exception ex)
                            {
                                Debug.Log(PlaneMovementManager.Instance == null ? "null" : (PlaneMovementManager.Instance.CurrentWrecks == null ? "null01" : PlaneMovementManager.Instance.CurrentWrecks.Count.ToString()));
                                Debug.Log(Instances == null ? "null2" : Instances.Count.ToString());
                                if (Instances != null && Instances.Count > 0)
                                {
                                    Debug.Log(Instances[0].WreckType);
                                }
                                Debug.LogError("C07");
                                throw ex;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("C08");
                        throw ex;
                    }

                    try
                    {
                        CurrentSegment.Parent.Destruction.RepairData.ReachedMax -= StopJob;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("C09");
                        throw ex;
                    }
                    try
                    {
                        CurrentSegment.Parent.Destruction.Repair = false;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("C10");
                        throw ex;
                    }
                    break;
                case EWaypointTaskType.RepairDoor:
                    //danger = RepairedDoor.DoorBurst;
                    //RepairedDoor = null;
                    break;
                case EWaypointTaskType.Waterpump:
                    try
                    {
                        danger = CurrentSegment.Group.Flood;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("C11");
                        throw ex;
                    }
                    break;
            }
            try
            {
                if (danger != null)
                {
                    danger.RepairData.ReachedMax -= StopJob;
                    danger.Repair = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("C12");
                throw ex;
            }

            try
            {
                Job = EWaypointTaskType.Normal;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("C13");
                throw ex;
            }

            try
            {
                foreach (var dcInstance in Instances)
                {
                    try
                    {
                        dcInstance.IgnoreSegmentOnly = false;
                        dcInstance.TaskType = EWaypointTaskType.Normal;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("C14");
                        throw ex;
                    }
                    try
                    {
                        if (debug)
                        {
                            DebugCheck(dcInstance);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("C15");
                        throw ex;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("C16");
                throw ex;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("ERROR");
            Debug.LogException(ex);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
            Application.Quit();
#endif
        }
    }

    private void RemoveIdles()
    {
        //Debug.Log("no more idling");
        float i = 0f;
        var worInMan = WorkerInstancesManager.Instance;
        foreach (var dcInstance in idling)
        {
            worInMan.StopAnim(dcInstance);
            dcInstance.IsExitting = false;
            dcInstance.WalkAnim();

            dcInstance.Delay = DelayBetweenInstances * i++;
        }
        idling.Clear();
    }

    private void AddIdle(InstanceData dcInstance)
    {
        //Debug.Log("1 idling");
        WorkerInstancesManager.Instance.StartAnim(dcInstance, dcInstance.IdleAnimation());
        idling.Add(dcInstance);
        Assert.IsTrue(movedPastDestination.Count == idling.Count, movedPastDestination.Count + ";" + idling.Count);
    }

    private void SetJob()
    {
        //Debug.Log("idling, looking for job");
        busy = false;
        bool clearIdles = true;
        var task = priorityTask;
        priorityTask = EWaypointTaskType.Normal;

        if (task == EWaypointTaskType.Normal || !PlayerOverriden)
        {
            switch (Category)
            {
                case EDcCategory.Fire:
                    task = EWaypointTaskType.Firefighting;
                    break;
                case EDcCategory.Water:
                    task = EWaypointTaskType.Waterpump;
                    break;
                case EDcCategory.Injured:
                    task = EWaypointTaskType.Rescue;
                    break;
                case EDcCategory.Crash:
                    task = EWaypointTaskType.Repair;
                    break;
            }
        }

        //check first priority task
        switch (task)
        {
            case EWaypointTaskType.Firefighting:
                if (!CurrentSegment.Fire.Exists)
                {
                    task = EWaypointTaskType.Normal;
                }
                break;
            case EWaypointTaskType.Repair:
                if (!CurrentSegment.Damage.Exists && !CurrentSegment.Parent.BrokenRepairable)
                {
                    task = EWaypointTaskType.Normal;
                }
                break;
            case EWaypointTaskType.RepairDoor:
                //no repair door
                task = EWaypointTaskType.Normal;
                break;
                if (RepairedDoor == null)
                {
                    RepairedDoor = CurrentSegment.GetLeakedDoors();
                }
                //if (RepairedDoor == null || !RepairedDoor.HasLeak(CurrentSegment))
                //{
                //    task = EWaypointTaskType.Normal;
                //}
                break;
            case EWaypointTaskType.Rescue:
            case EWaypointTaskType.Rescue2:
            case EWaypointTaskType.Rescue3:
            case EWaypointTaskType.Rescues:
                if (CurrentSegment.CanRescue())
                {
                    task = CurrentSegment.RescueType;
                }
                else
                {
                    task = EWaypointTaskType.Normal;
                }
                break;
            case EWaypointTaskType.Waterpump:
                if (!CurrentSegment.Group.Flood.Exists)
                {
                    task = EWaypointTaskType.Normal;
                }
                break;
        }

        if (task != EWaypointTaskType.Normal)
        {
            //Debug.Log("priority job");
            Assert.IsFalse(task == EWaypointTaskType.RepairDoor);
            //doors are exception
            //if (task == EWaypointTaskType.RepairDoor)
            //{
            //    DoDoors(RepairedDoor);
            //}
            //else
            {
                Do(task);
            }
        }
        //no priority task, check if there is any
        else if (!PlayerOverriden)
        {
            clearIdles = false;
        }
        else
        {
            if (CurrentSegment.CanRescue())
            {
                Do(CurrentSegment.RescueType);
            }
            else if (CurrentSegment.Group.Flood.Exists)
            {
                Do(EWaypointTaskType.Waterpump);
            }
            else if (CurrentSegment.Fire.Exists)
            {
                Do(EWaypointTaskType.Firefighting);
            }
            else if (CurrentSegment.Parent.BrokenRepairable && !HudManager.Instance.InWorldMap)
            {
                Do(EWaypointTaskType.Repair);
            }
            else if (!CurrentSegment.Parent.BrokenRepairable && CurrentSegment.Damage.Exists && !HudManager.Instance.InWorldMap && !CurrentSegment.Damage.Repair)
            {
                Do(EWaypointTaskType.Repair);
            }
            else if (CheckLeakedDoors())
            {
                //job done in function
            }
            else
            {
                clearIdles = false;
            }
        }
        //clear only when in job
        if (clearIdles)
        {
            movedPastDestination.Clear();
            RemoveWaits();
            RemoveIdles();
            Assert.IsTrue(movedPastDestination.Count == 0);
            Assert.IsTrue(movedPastNextDestinationIndices.Count == 0);
        }
        DebugCheckAll();
    }

    private bool CheckLeakedDoors()
    {
        return false;
        //Debug.Log("has leaked doors?");
        //var leakedDoors = CurrentSegment.GetLeakedDoors();
        //if (leakedDoors != null)
        //{
        //    DoDoors(leakedDoors);
        //    DebugCheckAll();
        //    return true;
        //}
        //DebugCheckAll();
        //return false;
    }

    private void Wait(InstanceData dcInstance)
    {
        //Debug.Log("1 waiting");
        dcInstance.Delay = LongWait;
        waiting.Add(dcInstance);
#warning dc stopping 1 dest issue
        //Waypoint w = null;
        //foreach (var instance in waiting)
        //{
        //    if (w == null)
        //    {
        //        w = instance.GetDestination();
        //    }

        //    else if (w != instance.GetDestination())
        //    {
        //        dcInstance.ToString();
        //    }
        //}
    }

    private void RemoveWaits()
    {
        //Debug.Log("no more waiting, " + Time.frameCount);
        float i = 0f;
        foreach (var dcInstance in waiting)
        {
            dcInstance.Delay = DelayBetweenInstances * i++;
        }
        waiting.Clear();
    }

    private bool IsPathToNextSegmentBlocked(int currentIndex)
    {
        bool result = false;
        if ((currentIndex + 1) < Path.Count)
        {
            //dc cannot move in flooded segment
            var currentSegment = Path[currentIndex];
            result = currentSegment.IsFlooded();

            //check if next segment is blocked
            if (!result)
            {
                var nextSegment = Path[currentIndex + 1];
                var dir = currentSegment.NeighboursDirectionDictionary[nextSegment];
                result = (dir != ENeighbourDirection.Left && dir != ENeighbourDirection.Right) || nextSegment.IsFlooded();
            }
        }
        return result;
    }

    //true if moving to exit
    private bool SetNextDestinations(int fromSegmentIndex, List<Waypoint> destinationPoints, ref WorkerPath destinationTree)
    {
        //Debug.Log("looking for next segment in path");
        destinationPoints.Clear();

        var fromSegment = Path[fromSegmentIndex];
        destinationTree = fromSegment.Parent.Path;
        //if this is destination
        if ((fromSegmentIndex + 1) == Path.Count)
        {
            SetIdleDestinations(fromSegment, destinationTree);
        }
        else if (IsPathToNextSegmentBlocked(fromSegmentIndex))
        {
            //get nearest exit

            try
            {
                destinationPoints.Add(Instances[0].GetNearestWaypoint(Instances[0].Trans.position, destinationTree.ExitsBySegment[fromSegment]));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{CurrentSegment.Parent.ParentSection.name}; {CurrentSegment.Parent.name}; {CurrentSegment.name}; {destinationTree.name}");
                Debug.LogException(ex);
#if TEST_ERRORS
                Application.Quit();
#endif
                throw ex;
            }
            //DebugCheckAll();
            return true;
        }
        else
        {
            var toSegment = Path[fromSegmentIndex + 1];
            try
            {
                destinationPoints.Add(destinationTree.DCSegmentTransition[fromSegment][toSegment]);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{CurrentSegment.Parent.ParentSection.name}; {CurrentSegment.Parent.name}; {CurrentSegment.name}; {destinationTree.name}");
                Debug.LogError(fromSegment + "\n" + ex.ToString(), destinationTree);
                Debug.LogException(ex);
#if TEST_ERRORS
                Application.Quit();
#endif
                throw ex;
            }
        }
        return false;
    }

    private void TeleportedToExit(InstanceData dcInstance)
    {
        //Debug.Log("teleport");
        movedPastDestination.Add(dcInstance);
        //wait for others
        Wait(dcInstance);
        //if all in destination move group to next
        if (movedPastDestination.Count == Instances.Count)
        {
            //Debug.Log("all teleported");
            if (Path.Count == 0)
            {
                teleportThroughSegment = false;

                //no longer instances should wait
                RemoveWaits();
                movedPastDestination.Clear();

                //instances go to idle
                SetAndMoveToIdleDestinations();
            }
            else
            {
                Assert.IsFalse(Path.Count < NextSegmentIndex, Path.Count.ToString());
                Assert.IsFalse(hasDelay);
                Assert.IsFalse(wasExit);
                if (Path.Count == NextSegmentIndex || unsetExit)
                {
                    exit = false;
                    if (Path.Count != NextSegmentIndex)
                    {
                        wasExit = true;
                    }
                }
                Assert.IsFalse(exit);

                hasDelay = true;

                calculatedAll = TeleportThroughSegmentDelay;
                delay = TeleportThroughSegmentDelay;
                DamageControlManager.Instance.SetShowPath(this);
            }
        }
        DebugCheckAll();
    }

    private void SetIdleDestinations(SectionSegment segment, WorkerPath tree)
    {
        //Debug.Log("destination to idle");
        destinationPoints.Clear();
        destinationTree = tree;
        Assert.IsTrue(tree.DCIdle.ContainsKey(segment), segment.name);
        var idleWaypoints = tree.DCIdle[segment];
        for (int i = 0; i < Instances.Count; i++)
        {
            Waypoint idleDestination;
            do
            {
                idleDestination = RandomUtils.GetRandom(idleWaypoints);
                if (destinationPoints.Count >= idleWaypoints.Count)
                {
                    Debug.Log("MISSING IDLE IN " + segment.name + " " + segment.Parent.ParentSection.name);
                    break;
                }
            }
            while (destinationPoints.Contains(idleDestination));
            destinationPoints.Add(idleDestination);
        }
    }

    private void SetAndMoveToIdleDestinations()
    {
        //Debug.Log("moving to idle");
        busy = true;

        var tree = CurrentSegment.Parent.Path;
        SetIdleDestinations(CurrentSegment, tree);
        var oldDestinationPoints = destinationPoints;
        //destinationPoints must be empty
        destinationPoints = new List<Waypoint>();

        Assert.IsTrue(Path.Count == 0);
        Assert.IsTrue(Job == EWaypointTaskType.Normal);

        //should not return true - only in path
        bool next = MoveInstancesToDestinations(tree, oldDestinationPoints);
        Assert.IsFalse(next);
        DebugCheckAll();
    }

    private bool MoveInstancesToDestinations(WorkerPath path, List<Waypoint> currentPoints)
    {
        //Debug.Log("moving to next destination");
        bool result = false;
        for (int i = 0; i < Instances.Count; i++)
        {
            //only last may notify that all in destination
            Assert.IsFalse(result);
            result = MoveInstanceToDestination(i, path, currentPoints);
            DebugCheck(Instances[i]);
        }
        return result;
    }

    private bool MoveInstanceToDestination(int index, WorkerPath path, List<Waypoint> currentPoints)
    {
        Assert.IsFalse(currentPoints.Count == 0);
        //Debug.Log("moving 1. to next destination");
        var newDestination = currentPoints[Mathf.Min(index, currentPoints.Count - 1)];
        Instances[index].Tree = path;
        var start = Instances[index].GetNearestWaypoint(Instances[index].Trans.position, Instances[index].Tree.Waypoints);
        Instances[index].FreeAnimWaypoint(Instances[index].GetDestination());
        if (start == newDestination)
        {
            //true if should fire next
            return InstanceInDestination(index);
        }
        else
        {
            Instances[index].SetNewDestination(path, newDestination);
        }
        return false;
    }

    private bool InstanceInDestination(int index)
    {
        //Debug.Log("dc in destination");
        if (Path.Count == 0)
        {
            //if temps going to quit
            if (inactive)
            {
                //Debug.Log("temps going to quit");
                Wait(Instances[index]);
            }
            //going to idle
            else if (Job == EWaypointTaskType.Normal)
            {
                //Debug.Log("going to idle");
                movedPastDestination.Add(Instances[index]);
                //wait for others
                AddIdle(Instances[index]);
                //check if all are in idle
                if (movedPastDestination.Count == Instances.Count)
                {
                    movedPastDestination.Clear();
                    //check job
                    SetJob();
                    if (idling.Count > 0)
                    {
                        CheckQueue();
                    }
                }
            }
            //is working, fire animation
            else
            {
                //Debug.Log("started job, anim time");
                Assert.IsFalse(Instances[index].GetDestination().Data.AnimID == -1);

                var worInMan = WorkerInstancesManager.Instance;
                if ((Job & EWaypointTaskType.Rescues) != 0)
                {
                    //Debug.Log("rescue anim");
                    rescuing.Add(Instances[index]);
                    if (rescuing.Count == Instances.Count)
                    {
                        var list = new List<Waypoint>();
                        foreach (var dcInstance in Instances)
                        {
                            list.Add(dcInstance.GetDestination());
                        }
                        CurrentSegment.Rescued = true;
                        for (int i = 0; i < Instances.Count; i++)
                        {
                            worInMan.StartAnim(Instances[i], Instances[i].StartRescueAnim(CurrentSegment, list[i], this));
                        }
                    }
                }
                else if (Job == EWaypointTaskType.Repair && CurrentSegment.Parent.ParentSection == DamageControlManager.Instance.WreckSection)
                {
                    //Debug.Log("wreck");
                    pushingWreck.Add(Instances[index]);
                    if (pushingWreck.Count == Instances.Count)
                    {
                        int wreckType = CurrentSegment.Parent.ParentSection.SubsectionRooms.IndexOf(CurrentSegment.Parent);
                        var wreck = PlaneMovementManager.Instance.CurrentWrecks[wreckType];
                        foreach (var dcInstance in Instances)
                        {
                            worInMan.StartAnim(dcInstance, dcInstance.StartPushWreck(wreckType));
                        }
                        wreck.DC.Clear();
                        wreck.DC.AddRange(Instances);
                    }
                }
                else
                {
                    //Debug.Log("basic anim, " + Time.frameCount);
                    worInMan.StartAnim(Instances[index], Instances[index].StartAnimation());
                }
            }
        }
        else
        {
            //Debug.Log("has path, reached destination");
            //add to moved past destination, if not exiting move to next destination
            if (movedPastDestination.Add(Instances[index]) && !exit)
            {
                //ignore if all in destination, cause it is checked individually further in function
                MoveInstanceToDestination(index, destinationTree, destinationPoints);
                DebugCheck(Instances[index]);
                //Instances[index].SetNewDestination(destinationTree, destinationPoints[Mathf.Min(index, destinationPoints.Count - 1)]);
            }
            //wait for others
            else
            {
                //Debug.Log("past/past destination");
                if (!exit)
                {
                    movedPastNextDestinationIndices.Add(index);
                }
                Wait(Instances[index]);
            }

            //if all in destination move group to next
            if (movedPastDestination.Count == Instances.Count)
            {
                //Debug.Log("all past destination, next");
                //should fire next
                DebugCheckAll();
                return true;
            }
        }
        DebugCheckAll();
        return false;
    }

    private bool CheckIssue(EIssue issue, SectionSegment destination)
    {
        switch (issue)
        {
            case EIssue.Fire:
                return destination.Fire.Exists;
            case EIssue.Flood:
                return destination.Group.Flood.Exists;
            case EIssue.Fault:
                return destination.Damage.Exists || destination.Parent.BrokenRepairable;
            case EIssue.Injured:
                return destination.Injured();
            case EIssue.Any:
                return CheckIssue(EIssue.Fire, destination) || CheckIssue(EIssue.Flood, destination) || CheckIssue(EIssue.Fault, destination) || CheckIssue(EIssue.Injured, destination);
        }
        Assert.IsTrue(false);
        return false;
    }

    private int GetSegmentIndex(SectionSegment segment, SectionRoom wreckSection, SectionRoomManager sectionMan)
    {
        if (segment.Parent.ParentSection == wreckSection)
        {
            int result = -2;
            foreach (var segment2 in wreckSection.GetAllSegments(true))
            {
                if (segment2 == segment)
                {
                    return result;
                }
                result--;
            }

            var text = wreckSection.name + ";;" + segment.name;
            int tries = 0;
            var trans = segment.transform.parent;
            while (trans!= null && ++tries < 1000)
            {
                text += "; " + trans.name;
                trans = trans.parent;
            }

            Assert.IsTrue(false, text);
            return -1;
        }
        else
        {
            return sectionMan.IndexOfSafe(segment);
        }
    }

    private SectionSegment GetSectionSegment(int segmentIndex, SectionRoom wreckSection, SectionRoomManager sectionMan)
    {
        if (segmentIndex < -1)
        {
            int value = -1 - segmentIndex;

            foreach (var segment in wreckSection.GetAllSegments(true))
            {
                if (--value == 0)
                {
                    return segment;
                }
            }
            Assert.IsTrue(false);
            return null;
        }
        else
        {
            return sectionMan.GetSegment(segmentIndex);
        }
    }

    private void DebugCheckAll()
    {
        foreach (var dcInstance in Instances)
        {
            DebugCheck(dcInstance);
        }
    }

    private bool once;
    private void DebugCheck(InstanceData dcInstance)
    {
        if (setup)
        {
            return;
        }
        if (!pushingWreck.Contains(dcInstance) && !rescuing.Contains(dcInstance) && dcInstance.CurrentAnim == null && dcInstance.Delay < 1f)
        {
            if (dcInstance.SegmentPath.Count <= dcInstance.CurrentSegment)
            {
                //Debug.LogError(Instances.IndexOf(dcInstance) + " has no path");
            }
        }

        if (!once)
        {
            once = true;
            DebugCheck(dcInstance);
        }
    }
}

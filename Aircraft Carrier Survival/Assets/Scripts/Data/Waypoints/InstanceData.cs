using GPUInstancer;
using GPUInstancer.CrowdAnimations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class InstanceData
{
    public const float BlendTime = .1f;
    public bool InUse
    {
        get => inUse;
        set
        {
            inUse = value;
            //if (value)
            //{
            //    if (Group.ConnectedSubcrewman != null && Group.ConnectedSubcrewman.Parent.Selected)
            //    {
            //        SetSelect(true);
            //    }
            //}
            //else
            //{
            //    SetSelect(false);
            //}
        }
    }

    public bool FastAnim
    {
        get => animSpeed != 1f;
        set
        {
            animSpeedChanged = animSpeedChanged || value;
            animSpeed = value ? 3f : 1f;
        }
    }

    public InstanceGroup Group;
    public WorkerPath Tree;
    public Vector3 StartPos;
    public List<SegmentData> SegmentPath;
    public int CurrentSegment;
    public int CurrentPoint;
    public IEnumerator CurrentAnim;
    public Coroutine CoroutineAnim;
    public int AnimRepeat;
    public Vector3 Dir;
    public float TimeLen;
    public float TimePassed;
    public Transform Trans;
    //public bool InTransition;
    public GPUICrowdPrefab Ref;
    public float Delay;
    public EWaypointTaskType TaskType = EWaypointTaskType.Normal;
    public AnimationClip WalkClip;
    public AnimationClip PrevWalkClip;
    public AnimationClip InjuredOutClip;
    public float Speed = 1f;
    public SectionSegment SectionSegment;
    public bool IgnoreSegmentOnly;

    public AnimationClip CurrentClip;

    public bool IsExitting = false;

    public bool Dc;

    public InstanceSounds Sounds;

    public bool FrightenOnly;

    public int WreckType;

    private bool killRescueIdle;

    private bool inUse;
    private float animSpeed;
    private bool animSpeedChanged = false;

    private float animTime;
    private bool frighten;
    private IEnumerator oldAnim;

    private bool canFrighten;
    private bool mayFrighten;

    private IEnumerator enumerAnim;

    public static List<Waypoint> ShortestPath(Waypoint current, Waypoint dest)
    {
        Assert.IsFalse(current == dest);
        var visited = new HashSet<Waypoint>() { current };
        var paths = new List<ShortestPathData>() { new ShortestPathData(current, dest) };

        while (true)
        {
            if (paths.Count == 0)
            {
                var parent = current.transform;
                WorkerPath workerPath;
                while (!parent.TryGetComponent(out workerPath))
                {
                    parent = parent.parent;
                    if (parent == null)
                    {
                        var text = "Cannot find path, couldn't find worker path for: ";
                        var text2 = current.name;
                        parent = current.transform.parent;

                        while (parent != null)
                        {
                            text = parent.name + "\\" + text;
                            parent = parent.parent;
                        }
                        Debug.Log("from", current);
                        Debug.Log("to", dest);
                        Debug.Log(text);
                        Assert.IsTrue(false);
                    }
                }

                Debug.Log("from", current);
                Debug.Log("to", dest);
                Debug.Log("Cannot find path", workerPath);
                Assert.IsTrue(false);
            }
            var path = paths[paths.Count - 1];
            paths.RemoveAt(paths.Count - 1);

            current = path.Path[path.Path.Count - 1];
            foreach (var branch in current.Branches)
            {
                if (visited.Add(branch))
                {
                    var data = new ShortestPathData(path, branch, dest);
                    if (branch == dest)
                    {
                        return data.Path;
                    }
                    paths.Add(data);
                }
            }
            paths.Sort((node, node2) => Comparer<float>.Default.Compare(node2.CurrentLength + node2.DistLeft, node.CurrentLength + node.DistLeft));
        }
    }

    public InstanceData(InstanceGroup group, WorkerPath tree, GPUICrowdPrefab Ref, AnimationClip walkClip)
    {
        Group = group;
        Tree = tree;
        WalkClip = walkClip;

        TimePassed = TimeLen = 1f;
        Trans = Ref.transform;
        this.Ref = Ref;

        Ref.TryGetComponent(out Sounds);

        animSpeed = 1f;

        canFrighten = true;
    }

    public void SetStart(Waypoint waypoint)
    {
        SegmentPath = new List<SegmentData>() { new SegmentData(waypoint) };
        CurrentSegment = 0;
        CurrentPoint = 0;
    }

    public Waypoint GetNewDestination(Waypoint exclude = null)
    {
        var freeWaypoints = GetFreeWaypoints(SectionSegment, TaskType, IgnoreSegmentOnly, false);

        bool revert = exclude != null && freeWaypoints.Remove(exclude);
        Assert.IsFalse(freeWaypoints.Count == 0, Tree.name + " " + Tree.transform.parent.parent.name + " " + TaskType);
        var result = RandomUtils.GetRandom(freeWaypoints);
        if (revert)
        {
            freeWaypoints.Add(exclude);
        }

        bool removed = freeWaypoints.Remove(result);
//#warning commentout?
//        Assert.IsTrue(removed);

        Assert.IsTrue(TaskType == EWaypointTaskType.Repair || !IgnoreSegmentOnly);
        if (TaskType == EWaypointTaskType.Repair)
        {
            Assert.IsTrue(TaskType == EWaypointTaskType.Repair);
            freeWaypoints = GetFreeWaypoints(result.Data.Segment, TaskType, !IgnoreSegmentOnly, true);
            Assert.IsFalse(freeWaypoints.Count == 0);
            Assert.IsTrue(freeWaypoints.Count > 0);
            removed = freeWaypoints.Remove(result);
            Assert.IsTrue(removed);
        }

        return result;
    }

    public void SetNewDestination()
    {
        Assert.IsFalse(CurrentSegment > SegmentPath.Count);
        if (CurrentSegment == SegmentPath.Count)
        {
            CurrentSegment = SegmentPath.Count - 1;
            bool near = Mathf.Approximately((SegmentPath[CurrentSegment].Points[SegmentPath[CurrentSegment].Points.Count - 1] - SegmentPath[CurrentSegment].Waypoint.Trans.position).sqrMagnitude, 0f);
            if (!near)
            {
                Debug.LogError(Tree.name + " " + Tree.transform.parent.name, Ref.gameObject);
                Debug.LogError(Tree.name + " " + Tree.transform.parent.name, SectionSegment == null ? Tree.gameObject : SectionSegment.gameObject);
            }
            //Assert.IsTrue(near, Tree.name + " " + Tree.transform.parent.name);
        }
        var start = SegmentPath[CurrentSegment];
        SegmentPath = ShortestPath(start, GetNewDestination(start.Waypoint));
        Assert.IsTrue(GetDestination().Data.AnimType == EWaypointAnimType.ActionAnim);
        FreeAnimWaypoint(start.Waypoint);

        StartPos = start.Waypoint.Trans.position;
        CurrentSegment = 1;
        CurrentPoint = 0;
        TimePassed = TimeLen = 1f;
        InitDestination();
    }

    public void SetNewDestination(WorkerPath tree, Waypoint destination, bool removeWaypoint = true)
    {
        Tree = tree;
        var start = GetNearestWaypoint(Trans.position, Tree.Waypoints);
        SegmentPath = ShortestPath(new SegmentData(start), destination);
        if (removeWaypoint && destination.Data.AnimType == EWaypointAnimType.ActionAnim)
        {
            Assert.IsFalse(IgnoreSegmentOnly);
            var freeWaypoints = GetFreeWaypoints(destination.Data.Segment, destination.Data.PossibleTasks, false, false);
            bool removed = freeWaypoints.Remove(destination);
//#warning commentout?
//            Assert.IsTrue(removed);
        }

        StartPos = Trans.position;
        CurrentSegment = start.Trans.position == StartPos ? 1 : 0;
        CurrentPoint = 0;
        Assert.IsTrue(SegmentPath.Count > CurrentSegment);
        Assert.IsTrue(SegmentPath[CurrentSegment].Points.Count > CurrentPoint);
        TimePassed = TimeLen = 1f;
        InitDestination();
    }

    public Waypoint SetStartAndDestination(Waypoint dest)
    {
        //var exits = Tree.CategorisedExits[dest.Data.PossibleTasks];
        var exits = Tree.Exits;
        var start = GetNearestWaypoint(dest.Trans.position, exits);
        if (start.Data.AnimType != EWaypointAnimType.Exit)
        {
            Debug.LogError("Waypoint exit error", start);
        }
        Assert.IsTrue(start.Data.AnimType == EWaypointAnimType.Exit);
        if (dest.Data.AnimType != EWaypointAnimType.ActionAnim)
        {
            Debug.LogError("Waypoint anim error", dest);
        }

        SegmentPath = ShortestPath(new SegmentData(start), dest);
        StartPos = start.Trans.position;
        CurrentSegment = 1;
        CurrentPoint = 0;
        InitDestination();
        WalkAnim();
        LookAtDir();

        return start;
    }

    public void SetNewDestinationThroughExit(Waypoint destination)
    {
        var start = GetNearestWaypoint(Trans.position, Tree.Waypoints);
        Waypoint exitWaypoint = GetNearestExit(Tree, Trans.position, out float dist);
        Assert.IsTrue(!Mathf.Approximately(dist, 0f));
        SegmentPath = ShortestPath(new SegmentData(start), exitWaypoint);

        var path = ShortestPath(new SegmentData(exitWaypoint), destination);
        for (int i = 1; i < path.Count; i++)
        {
            SegmentPath.Add(path[i]);
        }

        StartPos = Trans.position;
        CurrentSegment = start.Trans.position == StartPos ? 1 : 0;
        CurrentPoint = 0;
        Assert.IsTrue(SegmentPath.Count > CurrentSegment);
        Assert.IsTrue(SegmentPath[CurrentSegment].Points.Count > CurrentPoint);
        TimePassed = TimeLen = 1f;
        InitDestination();
    }

    public void SetFirstRescueDestination()
    {
        Assert.IsFalse(CurrentSegment > SegmentPath.Count);
        if (CurrentSegment == SegmentPath.Count)
        {
            CurrentSegment = SegmentPath.Count - 1;
            Assert.IsTrue(Mathf.Approximately((SegmentPath[CurrentSegment].Points[SegmentPath[CurrentSegment].Points.Count - 1] - SegmentPath[CurrentSegment].Waypoint.Trans.position).sqrMagnitude, 0f));
        }
        var start = SegmentPath[CurrentSegment];

        var freeWaypoints = GetFreeWaypoints(SectionSegment, TaskType, IgnoreSegmentOnly, false);
        Waypoint injuredWaypoint = null;
        foreach (var waypoint in freeWaypoints)
        {
            if (waypoint.Data.InjuredWaypoint)
            {
                injuredWaypoint = waypoint;
                break;
            }
        }
        Assert.IsNotNull(injuredWaypoint);
        freeWaypoints.Remove(injuredWaypoint);

        SegmentPath = ShortestPath(start, injuredWaypoint);
        Assert.IsTrue(GetDestination().Data.AnimType == EWaypointAnimType.ActionAnim);
        FreeAnimWaypoint(start.Waypoint);

        StartPos = start.Waypoint.Trans.position;
        CurrentSegment = 1;
        CurrentPoint = 0;
        TimePassed = TimeLen = 1f;
        InitDestination();
    }

    public void TeleportToDest(Waypoint dest)
    {
        StartPos = dest.Trans.position;
        StartPos.x -= 1f;
        TimePassed = TimeLen = 1f;
        SegmentPath = new List<SegmentData>() { new SegmentData(dest) };
        CurrentSegment = 0;
        CurrentPoint = 0;
        InitDestination();
        Trans.position = StartPos = dest.Trans.position;
        TimePassed = TimeLen = 1f;
    }

    public void TeleportAndRotate(Waypoint dest)
    {
        TeleportToDest(dest);

        Dir = Vector3.Normalize(StartPos - RandomUtils.GetRandom(dest.Branches).Trans.position);
        LookAtDir();
    }

    public void InitDestination()
    {
        Assert.IsTrue(TimePassed == TimeLen);
        Trans.position = StartPos;
        //optimalize with nextdir?
        Dir = SegmentPath[CurrentSegment].Points[CurrentPoint] - StartPos;
        TimeLen = Dir.magnitude;

        TimePassed = 0f;
        if (!Mathf.Approximately(TimeLen, 0f))
        {
            Dir /= TimeLen;
        }
        else
        {
            var waypoint = SegmentPath[0].Waypoint;
            var waypoint2 = SegmentPath[CurrentSegment].Waypoint;
            var waypoint3 = GetDestination();
            string text;
            if (waypoint.Trans.parent.parent.name.StartsWith("Subsect"))
            {
                text = waypoint.Trans.parent.parent.parent.parent.name;
                text += " ";
                text += waypoint.Trans.parent.parent.name;
            }
            else
            {
                text = waypoint.Trans.parent.parent.name;
                text += " ";
                text += waypoint.Trans.parent.name;
            }
            text += " ";
            if (waypoint.Trans.parent.TryGetComponent(out WorkerPath path) || waypoint.Trans.parent.parent.TryGetComponent(out path))
            {
                text += path.Waypoints.IndexOf(waypoint);
                text += ", ";
                text += path.Waypoints.IndexOf(waypoint2);
                text += ", ";
                text += path.Waypoints.IndexOf(waypoint3);
            }
            Debug.LogError((CurrentAnim == null ? "no anim;; " : "has anim;; ") + text + "||" + CurrentSegment.ToString() + ":" + CurrentPoint.ToString() + ", " + waypoint.gameObject.GetInstanceID() + ", " + waypoint2.gameObject.GetInstanceID() + 
                ", " + waypoint2.Trans.position.ToString("F3") + ", " + StartPos.ToString("F3") + ", " + waypoint3.gameObject.GetInstanceID() + ", " + waypoint3.Trans.position.ToString("F3") + 
                ", " + Ref.prefabPrototype.name + ":" + Ref.gpuInstancerID);
        }
    }

    public bool Go(float step)
    {
        Assert.IsNotNull(SegmentPath);
        Assert.IsNull(CurrentAnim);
        TimePassed += Speed * step;

        if (TimePassed >= TimeLen)
        {
            TimeLen = TimePassed;
            if (SegmentPath.Count <= CurrentSegment)
            {
                foreach (var dc in DamageControlManager.Instance.CurrentGroups)
                {
                    if (dc.Instances.Contains(this))
                    {
                        Debug.LogWarning(dc.CurrentSegment.name + " " + dc.Instances.IndexOf(this) + " " + dc.GetStatus(this) + Time.frameCount, Ref);
                        return true;
                    }
                }
            }
            if (SegmentPath.Count <= CurrentSegment)
            {
                return true;
            }
            Assert.IsTrue(SegmentPath[CurrentSegment].Points.Count > CurrentPoint);
            StartPos = SegmentPath[CurrentSegment].Points[CurrentPoint];
            CurrentPoint = (CurrentPoint + 1) % SegmentPath[CurrentSegment].Points.Count;
            if (CurrentPoint > 0 || (++CurrentSegment < SegmentPath.Count))
            {
                InitDestination();
                LookAtDir();
                return false;
            }
            Trans.position = StartPos;
            return true;
        }
        Trans.position = StartPos + Dir * TimePassed;
        return false;
    }

    public void GoAll()
    {
        Assert.IsNotNull(SegmentPath);
        Assert.IsNull(CurrentAnim);
        CurrentSegment = SegmentPath.Count - 1;
        CurrentPoint = SegmentPath[CurrentSegment].Points.Count - 1;
        TimePassed = TimeLen = 1f;
        LookAtDir();

        GPUInstancerAPI.UpdateTransformDataForInstance(WorkerInstancesManager.Instance.GPUICrowdManager, Ref);
    }

    public void WalkAnim()
    {
        SetAnimationNoBlend(WalkClip);
    }

    public void SetAnimation(AnimationClip clip, float startTime = -1f)
    {
        SetAnimation(clip, animSpeed, startTime);
    }

    public void SetAnimation(AnimationClip clip, float animSpeed, float startTime)
    {
        if (CurrentClip == clip)
        {
            SetAnimationNoBlend(clip, animSpeed, startTime);
        }
        else
        {
            GPUICrowdAPI.StartAnimation(Ref, clip, startTime, animSpeed, BlendTime);
            CurrentClip = clip;
        }
    }

    public void SetAnimationNoBlend(AnimationClip clip, float startTime = -1f)
    {
        SetAnimationNoBlend(clip, animSpeed, startTime);
    }

    public void SetAnimationNoBlend(AnimationClip clip, float animSpeed, float startTime = -1f)
    {
        GPUICrowdAPI.StartAnimation(Ref, clip, startTime, animSpeed);
        CurrentClip = clip;
    }

    public void LookAtDir()
    {
        if (!Mathf.Approximately(Dir.x, 0f) || !Mathf.Approximately(Dir.z, 0f))
        {
            Trans.rotation = Quaternion.LookRotation(new Vector3(-Dir.x, 0f, -Dir.z).normalized, Vector3.up);
        }
    }

    public IEnumerator MasterAnim(IEnumerator inner)
    {
        var frighten = SetFrighten();
        if (inner == null)
        {
            FrightenOnly = true;
            inner = DummyAnim();
        }
        while (inner.MoveNext())
        {
            var value = inner.Current;
            Assert.IsNull(value);
            yield return value;

            if (frighten.MoveNext())
            {
                yield return frighten.Current;
                bool x = frighten.MoveNext();
                Assert.IsFalse(x);
            }
            frighten = SetFrighten();
        }
        if (frighten.MoveNext())
        {
            yield return frighten.Current;
            bool x = frighten.MoveNext();
            Assert.IsFalse(x);
        }
        FrightenOnly = false;
        oldAnim = null;
        CurrentAnim = null;
        CoroutineAnim = null;
    }

    public IEnumerator IdleAnimation()
    {
        AnimationClip prevClip = null;
        while (true)
        {
            AnimationClip clip;
            do
            {
                clip = RandomUtils.GetRandom(AnimationManager.Instance.IdleClips);
            }
            while (prevClip == clip);
            prevClip = clip;
            SetAnimationNoBlend(clip);
            var checker = CheckSpeedChange(clip);
            while (checker.MoveNext())
            {
                yield return null;
            }
        }
    }

    public IEnumerator StartAnimation()
    {
        if (Sounds != null)
        {
            Sounds.Play(TaskType);
        }
        var dest = GetDestination();

        canFrighten = dest.Data.CanFrighten;

        AnimRepeat = UnityEngine.Random.Range(dest.Data.MinRepeat, dest.Data.MaxRepeat);
        bool revertFastAnim = false;
        if (TaskType != EWaypointTaskType.Normal)// && !IgnoreSegmentOnly)
        {
            AnimRepeat = 100_000_000;
            revertFastAnim = true;
        }
        if (Dc)
        {
            //Debug.Log(AnimRepeat);
        }

        var animMan = AnimationManager.Instance;
        var anims = animMan.AnimData.AnimGroups[dest.Data.AnimID];

        var workInMan = WorkerInstancesManager.Instance;
        var clipDataList = workInMan.InstancePrototype.animationData.clipDataList;
        var clip = clipDataList[anims.inID].animationClip;

        var damageControlMan = DamageControlManager.Instance;
        ExtinguisherHandler extinguisher = null;
        if (dest.Data.PossibleTasks == EWaypointTaskType.Firefighting)
        {
            extinguisher = damageControlMan.GetExtinguisher(clip);
            extinguisher.transform.position = Trans.position;
            extinguisher.transform.rotation = Trans.rotation;
            extinguisher.SetSpeed(animSpeed);
            extinguisher.Play(true);
        }

        SetAnimationNoBlend(clip);
        var checker = CheckSpeedChange(clip);
        while (checker.MoveNext())
        {
            if (extinguisher != null)
            {
                extinguisher.SetSpeed(animSpeed);
            }
            yield return null;
        }

        for (int i = 0; i < AnimRepeat; i++)
        {
            clip = clipDataList[RandomUtils.GetRandom(anims.loopIDs)].animationClip;
            SetAnimationNoBlend(clip);
            checker = CheckSpeedChange(clip);
            while (checker.MoveNext())
            {
                if (extinguisher != null)
                {
                    extinguisher.SetSpeed(animSpeed);
                }
                yield return null;
            }
        }

        if (extinguisher != null)
        {
            extinguisher.Play(false);
        }

        clip = clipDataList[anims.outID].animationClip;
        SetAnimationNoBlend(clip);
        checker = CheckSpeedChange(clip);
        while (checker.MoveNext())
        {
            if (extinguisher != null)
            {
                extinguisher.SetSpeed(animSpeed);
            }
            yield return null;
        }
        canFrighten = false;
        mayFrighten = true;

        if (extinguisher != null)
        {
            damageControlMan.ReturnExtinguisher(extinguisher);
        }

        clip = animMan.RotateClip;
        SetAnimationNoBlend(clip);
        checker = CheckSpeedChange(clip);
        while (checker.MoveNext())
        {
            yield return null;
        }

        canFrighten = true;
        mayFrighten = false;

        if (revertFastAnim)
        {
            FastAnim = false;
        }

        if (Sounds != null)
        {
            Sounds.Stop();
        }
        WalkAnim();
        if (!IsExitting)
        {
            SetNewDestination();
        }
        IsExitting = false;

        oldAnim = CurrentAnim;
        CurrentAnim = null;
        CoroutineAnim = null;
        LookAtDir();
        GPUInstancerAPI.UpdateTransformDataForInstance(workInMan.GPUICrowdManager, Ref);

        if (Dc)
        {
            //Debug.Log("finished");
        }
    }

    public IEnumerator StartPushWreck(int wreckType)
    {
        var animMan = AnimationManager.Instance;

        WreckType = wreckType;
        var wreck = PlaneMovementManager.Instance.CurrentWrecks[wreckType];

        Trans.eulerAngles = new Vector3(0f, (WreckType == (int)EWreckType.FrontKamikaze ? - 90f : 90f), 0f);
        WorkerInstancesManager.Instance.GPUICrowdManager.UpdateTransformDataForInstance(Ref);

        var clip = animMan.PushInWreckClip;
        SetAnimationNoBlend(clip);
        var checker = CheckSpeedChange(clip);
        while (checker.MoveNext())
        {
            yield return null;
        }

        SetAnimationNoBlend(animMan.PushLoopWreckClip);

        if (!wreck.Move)
        {
            wreck.Move = true;
        }

        while (!IsExitting)
        {
            yield return null;
            animTime += animSpeed * Time.deltaTime;
        }
        IsExitting = false;

        clip = animMan.PushOutWreckClip;
        SetAnimationNoBlend(clip);
        checker = CheckSpeedChange(clip);
        while (checker.MoveNext())
        {
            yield return null;
        }

        //clip = animMan.RotateClip;
        //SetAnimation(clip);
        //checker = CheckSpeedChange(clip);
        //while (checker.MoveNext())
        //{
        //    yield return null;
        //}


        FastAnim = false;

        CurrentAnim = null;
        CoroutineAnim = null;

        LookAtDir();
        GPUInstancerAPI.UpdateTransformDataForInstance(WorkerInstancesManager.Instance.GPUICrowdManager, Ref);

        WalkAnim();
    }

    public IEnumerator StartRescueAnim(SectionSegment segment, Waypoint destination, DCInstanceGroup group)
    {
        yield return null;
        var animMan = AnimationManager.Instance;
        var workInMan = WorkerInstancesManager.Instance;

        PrevWalkClip = WalkClip;

        CurrentSegment = SegmentPath.Count - 1;
        CurrentSegment = 1;
        CurrentPoint = 0;
        AnimationClip clip;
        if (destination.Data.InjuredWaypoint)
        {
            if (Sounds != null)
            {
                Sounds.RescueSounds.PlayEvent(ERescueSound.Heal);
            }

            segment.GetInjured(destination.Data.PossibleTasks, out var injuredInstance, out clip);
            bool ok = injuredInstance.GoToNearestExit(true);
            Assert.IsTrue(ok);
            if (injuredInstance.CoroutineAnim != null)
            {
                workInMan.StopAnim(injuredInstance);
            }
            injuredInstance.Speed = Speed;
            injuredInstance.FastAnim = FastAnim;

            StartCustomAnim(injuredInstance, injuredInstance.StartRescueeAnim());
            CustomAnimPlay();

            SetAnimationNoBlend(clip);
            var checker = CheckSpeedChange(clip, () => injuredInstance.FastAnim = true);
            while (checker.MoveNext())
            {
                yield return null;
                CustomAnimPlay();
            }

            if (Sounds != null)
            {
                Sounds.RescueSounds.Stop(true);
            }

            //only first DC rescuer tells destination
            if (destination.Data.PossibleTasks == EWaypointTaskType.Rescue)
            {
                segment.Rescuers = group;
                foreach (var dc in group.Instances)
                {
                    dc.killRescueIdle = true;
                }
                if (!IsExitting)
                {
                    group.StopJob(false);
                    var destinations = group.GetIdleDestinations();
                    for (int i = 0; i < group.Instances.Count; i++)
                    {
                        group.Instances[i].Tree = group.CurrentSegment.Parent.Path;
                        group.Instances[i].SetNewDestinationThroughExit(destinations[i]);
                    }
                }
            }

            clip = animMan.RescuerRotateClip;
            SetAnimationNoBlend(clip);
            checker = CheckSpeedChange(clip, () => injuredInstance.FastAnim = true);
            while (checker.MoveNext())
            {
                yield return null;
                CustomAnimPlay();
            }

            WalkClip = animMan.RescuerWalkClip;
            WalkAnim();
            FinishCustomAnim();

            injuredInstance.FastAnim = false;
        }
        else
        {
            clip = animMan.RescuerIdleClip;
            SetAnimationNoBlend(clip);
            while (!killRescueIdle)
            {
                yield return null;
            }

            clip = animMan.RotateClip;
            SetAnimationNoBlend(clip);
            var checker = CheckSpeedChange(clip);
            while (checker.MoveNext())
            {
                yield return null;
            }
            WalkAnim();
        }
        FreeAnimWaypoint(destination);

        FastAnim = false;

        CurrentAnim = null;
        CoroutineAnim = null;
        LookAtDir();
        GPUInstancerAPI.UpdateTransformDataForInstance(workInMan.GPUICrowdManager, Ref);
    }

    public IEnumerator StartInjuredAnimation(AnimGroupData anims)
    {
        var clipDataList = WorkerInstancesManager.Instance.InstancePrototype.animationData.clipDataList;
        InjuredOutClip = clipDataList[anims.outID].animationClip;
        var clip = clipDataList[anims.inID].animationClip;
        SetAnimationNoBlend(clip);
        var checker = CheckSpeedChange(clip);
        while (checker.MoveNext())
        {
            yield return null;
        }

        clip = clipDataList[RandomUtils.GetRandom(anims.loopIDs)].animationClip;
        SetAnimationNoBlend(clip);
        while (true)
        {
            yield return null;
            animTime += animSpeed * Time.deltaTime;
        }
    }

    public void ResetAnimTime()
    {
        animTime = 0f;
    }

    private IEnumerator StartRescueeAnim()
    {
        var clip = InjuredOutClip;
        SetAnimationNoBlend(clip);
        var checker = CheckSpeedChange(clip);
        while (checker.MoveNext())
        {
            yield return null;
        }

        var animMan = AnimationManager.Instance;
        PrevWalkClip = WalkClip;
        WalkClip = animMan.RescueeWalkClip;

        clip = animMan.RescueeRotateClip;
        SetAnimationNoBlend(clip);
        checker = CheckSpeedChange(clip);
        while (checker.MoveNext())
        {
            yield return null;
        }

        FastAnim = false;

        WalkAnim();

        CurrentAnim = null;
        CoroutineAnim = null;
        LookAtDir();
        GPUInstancerAPI.UpdateTransformDataForInstance(WorkerInstancesManager.Instance.GPUICrowdManager, Ref);
    }

    public void Frighten()
    {
        frighten = true;
        if (CurrentAnim == null)
        {
            WorkerInstancesManager.Instance.StartAnim(this, MasterAnim(null));
        }
    }

    public bool GoToNearestExit(bool next = false)
    {
        Waypoint exitWaypoint = GetNearestExit(Tree, (CurrentAnim == null ? Trans.position : StartPos), out float dist);
        FreeAnimWaypoint(GetDestination());
        AnimRepeat = 0;
        if (CurrentAnim != null)
        {
            IsExitting = true;
        }
        if (Mathf.Approximately(dist, 0f))
        {
            TeleportToDest(exitWaypoint);
            return false;
        }
        else
        {
            int startSegment = 0;
            if (CurrentAnim != null)
            {
                startSegment = 1;
                if (CurrentSegment > 0)
                {
                    CurrentSegment--;
                }
                CurrentPoint = 0;
            }
            int index = CurrentSegment;
            if (SegmentPath.Count == CurrentSegment)
            {
                index--;
            }

            if (SegmentPath[index].Waypoint == exitWaypoint)
            {
                SegmentPath = new List<SegmentData>() { new SegmentData(SegmentPath[index].Waypoint) };
                startSegment = 0;
                CurrentPoint = 0;
            }
            else
            {
                SegmentPath = ShortestPath(SegmentPath[index], exitWaypoint);
                if (next)
                {
                    startSegment = 1;
                }

            }
            if (CurrentAnim == null)
            {
                StartPos = Trans.position;
            }
            CurrentSegment = startSegment;
            TimePassed = TimeLen = 1f;
            InitDestination();
            return true;
        }
    }

    public void FreeAnimWaypoint(Waypoint waypoint)
    {
        if (waypoint.Data.AnimType == EWaypointAnimType.ActionAnim)
        {
            var freeWaypoints = GetFreeWaypoints(waypoint.Data.Segment, waypoint.Data.PossibleTasks, false, true);
            bool added = freeWaypoints.Add(waypoint);
            //Assert.IsTrue(added);

            if (waypoint.Data.PossibleTasks == EWaypointTaskType.Repair)
            {
                freeWaypoints = GetFreeWaypoints(waypoint.Data.Segment, waypoint.Data.PossibleTasks, true, true);
                added = freeWaypoints.Add(waypoint);
                //Assert.IsTrue(added);
            }
        }
    }

    public void UpdateSpeed(bool fast, bool quit)
    {
        var animMan = AnimationManager.Instance;
        if (fast || (quit && FastAnim))
        {
            FastAnim = true;
            Speed = animMan.RunSpeed;
            WalkClip = animMan.RunClip;
        }
        else
        {
            FastAnim = false;
            if (TaskType == EWaypointTaskType.Normal)
            {
                Speed = quit ? animMan.NormalQuitSpeed : animMan.WalkSpeed;
                WalkClip = animMan.WalkClip;
            }
            else
            {
                Speed = Parameters.Instance.DCWalkSpeedMetersPerSecond;
                if (TaskType == EWaypointTaskType.Firefighting)
                {
                    WalkClip = animMan.FirefightWalkClip;
                }
                else
                {
                    WalkClip = animMan.DCWalkClip;
                }
            }
        }
    }

    public Waypoint GetDestination()
    {
        return SegmentPath[SegmentPath.Count - 1].Waypoint;
    }

    public void SetSelect(bool select)
    {
        var workerInMan = WorkerInstancesManager.Instance;
        GPUInstancerAPI.UpdateVariation(workerInMan.GPUICrowdManager, Ref, WorkerInstancesManager.SaturationBufferName,
            select ? WorkerInstancesManager.Instance.SaturationSelected : WorkerInstancesManager.Instance.SaturationNotSelected);
    }

    public Waypoint GetNearestExit(WorkerPath path, Vector3 pos, out float dist)
    {
        Waypoint result = null;
        dist = float.MaxValue;
        //foreach (var exit in Tree.CategorisedExits[TaskType])
        foreach (var exit in path.Exits)
        {
            float newDist = (exit.Trans.position - pos).sqrMagnitude;
            if (newDist < dist)
            {
                result = exit;
                dist = newDist;
            }
        }
        return result;
    }

    public Waypoint GetNearestWaypoint(Vector3 pos, List<Waypoint> waypoints)
    {
        float dist = float.MaxValue;
        Waypoint result = null;
        foreach (var waypoint in waypoints)
        {
            string text = Tree.name;
            if (Group is DCInstanceGroup dcGroup)
            {
                text += $";{dcGroup.CurrentSegment.Parent.ParentSection.name};{dcGroup.CurrentSegment.Parent.name};{dcGroup.CurrentSegment.name}";
            }
            Assert.IsNotNull(waypoint, text);
            Assert.IsNotNull(waypoint.Trans, text + "trans");
            float newDist = (pos - waypoint.Trans.position).sqrMagnitude;
            if (newDist < dist)
            {
                dist = newDist;
                result = waypoint;
            }
        }
        return result;
    }

    public HashSet<Waypoint> GetFreeWaypoints(SectionSegment segment, EWaypointTaskType taskType, bool ignoreSegmentOnly, bool nolog)
    {
        List<HashSet<Waypoint>> waypoints;
        if(taskType != EWaypointTaskType.Normal && taskType != EWaypointTaskType.Firefighting && ((taskType & EWaypointTaskType.Rescues) == 0) &&
            taskType != EWaypointTaskType.Repair && taskType != EWaypointTaskType.Waterpump)
        {
            var section = segment.Parent.ParentSection;
            Debug.LogError($"{taskType}, {segment.name}, {segment.Parent.name}, {(section == null ? "" : section.name)}", segment);
            Assert.IsTrue(false);
        }
        if (!ignoreSegmentOnly && taskType != EWaypointTaskType.Normal)
        {
            if (!Tree.FreeAnimWaypointsBySegments.ContainsKey(segment))
            {
                Debug.LogError(segment.name, segment);
                Debug.LogError(taskType.ToString());
            }
            if (!Tree.FreeAnimWaypointsBySegments[segment].ContainsKey(taskType))
            {
                Debug.LogError(segment.name, segment);
                Debug.LogError(taskType.ToString());
            }
            waypoints = Tree.FreeAnimWaypointsBySegments[segment][taskType];
        }
        else
        {
            Assert.IsTrue(taskType == EWaypointTaskType.Normal || taskType == EWaypointTaskType.Repair);
            waypoints = Tree.FreeAnimWaypoints[taskType];
        }
        if (waypoints[0].Count == 0)
        {
            foreach (var waypoint in waypoints[1])
            {
                waypoints[0].Add(waypoint);
            }
        }
        return waypoints[0];
    }

    private IEnumerator DummyAnim()
    {
        yield return null;
    }

    private List<SegmentData> ShortestPath(SegmentData start, Waypoint dest)
    {
        try
        {
            var path = ShortestPath(start.Waypoint, dest);
            try
            {
                var result = new List<SegmentData>() { start };
                try
                {
                    var list = new List<Vector3>();
                    try
                    {
                        for (int i = 2; i < path.Count; i++)
                        {
                            Assert.IsNotNull(path[i - 1].PathsDict, $"null PathsDict, {path[i - 1]}");
                            try
                            {
                                Assert.IsTrue(path[i - 1].PathsDict.ContainsKey(path[i - 2]), $"key miss {path[i - 1]}, {path[i - 2].Trans.position}");
                                try
                                {
                                    Assert.IsNotNull(path[i - 1].PathsDict[path[i - 2]], $"null value, {path[i - 1]}, {path[i - 2]}");
                                    try
                                    {
                                        Assert.IsTrue(path[i - 1].PathsDict[path[i - 2]].ContainsKey(path[i]), $"key miss, {path[i - 1]}, {path[i - 2]}, {path[i]}");
                                        try
                                        {
                                            Assert.IsNotNull(path[i - 1].PathsDict[path[i - 2]][path[i]], $"null value, {path[i - 1]}, {path[i - 2]}, {path[i]}");
                                            if ((i - 2) < 0)
                                            {
                                                Debug.Log($"Path 0 index error ({start.Waypoint.Trans.position: F3}) -> ({dest.Trans.position: F3})");
                                            }
                                            else if (path.Count <= i)
                                            {
                                                Debug.Log($"Path index error ({start.Waypoint.Trans.position: F3}) -> ({dest.Trans.position: F3})");
                                            }
                                            else if (!path[i - 1].PathsDict.ContainsKey(path[i - 2]))
                                            {
                                                Debug.Log($"Path dict no key ({path[i - 1].Trans.position: F3}) -> ({path[i - 2].Trans.position: F3})");
                                            }
                                            else if (!path[i - 1].PathsDict[path[i - 2]].ContainsKey(path[i]))
                                            {
                                                Debug.Log($"Path inner dict no key ({path[i - 1].Trans.position: F3}) -> ({path[i - 2].Trans.position: F3}) -> ({path[i].Trans.position: F3})");
                                            }
                                            try
                                            {
                                                Assert.IsTrue(result[result.Count - 1].Waypoint == path[i - 2], $"path mismatch, {path[i - 2]}");
                                                try
                                                {
                                                    var points = path[i - 1].PathsDict[path[i - 2]][path[i]];
                                                    try
                                                    {
                                                        Assert.IsTrue(points.Count > 1, $"points count, {path[i - 1]}, {path[i - 2]}, {path[i]}");
                                                        try
                                                        {
                                                            int j = 0;
                                                            try
                                                            {
                                                                if (list.Count != 0 && points.Count != 0 && Mathf.Approximately((list[list.Count - 1] - points[0]).sqrMagnitude, 0f))
                                                                {
                                                                    try
                                                                    {
                                                                        j = 1;
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        Debug.LogError("A25");
                                                                        Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
                                                                        Debug.LogException(ex);
#if UNITY_EDITOR
                                                                        UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                                        Application.Quit();
#endif
                                                                        throw ex;
                                                                    }
                                                                }
                                                                try
                                                                {
                                                                    for (; j < points.Count / 2; j++)
                                                                    {
                                                                        try
                                                                        {
                                                                            Assert.IsTrue(list.Count == 0 || !Mathf.Approximately((list[list.Count - 1] - points[j]).sqrMagnitude, 0f),
                                                                                j.ToString() + ", " + start.Waypoint.gameObject.GetInstanceID() + ", " + dest.gameObject.GetInstanceID() + ", " + Tree.name +
                                                                                    (Tree.transform.parent == null ? "" : " - " + Tree.transform.parent.name +
                                                                                        (Tree.transform.parent.parent == null ? "" : " - " + Tree.transform.parent.parent.name)));
                                                                            try
                                                                            {
                                                                                list.Add(points[j]);
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                Debug.LogError("A24");
                                                                                Debug.LogException(ex);
                                                                                Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                                                UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                                                Application.Quit();
#endif
                                                                                throw ex;
                                                                            }
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            Debug.LogError("A23");
                                                                            Debug.LogException(ex);
                                                                            Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                                            UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                                            Application.Quit();
#endif
                                                                            throw ex;
                                                                        }
                                                                    }
                                                                    try
                                                                    {
                                                                        result.Add(new SegmentData(path[i - 1], new List<Vector3>(list)));
                                                                        try
                                                                        {
                                                                            list.Clear();
                                                                            try
                                                                            {
                                                                                for (; j < points.Count; j++)
                                                                                {
                                                                                    try
                                                                                    {
                                                                                        Assert.IsTrue(list.Count == 0 || !Mathf.Approximately((list[list.Count - 1] - points[j]).sqrMagnitude, 0f),
                                                                                            j.ToString() + ", " + start.Waypoint.gameObject.GetInstanceID() + ", " + dest.gameObject.GetInstanceID() + ", " + Tree.name +
                                                                                                (Tree.transform.parent == null ? "" : " - " + Tree.transform.parent.name +
                                                                                                    (Tree.transform.parent.parent == null ? "" : " - " + Tree.transform.parent.parent.name)));
                                                                                        try
                                                                                        {
                                                                                            list.Add(points[j]);
                                                                                        }
                                                                                        catch (Exception ex)
                                                                                        {
                                                                                            Debug.LogError("A22");
                                                                                            Debug.LogException(ex);
                                                                                            Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                                                            UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                                                            Application.Quit();
#endif
                                                                                            throw ex;
                                                                                        }
                                                                                    }
                                                                                    catch (Exception ex)
                                                                                    {
                                                                                        Debug.LogError("A21");
                                                                                        Debug.LogException(ex);
                                                                                        Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                                                        UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                                                        Application.Quit();
#endif
                                                                                        throw ex;
                                                                                    }
                                                                                }
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                Debug.LogError("A17");
                                                                                Debug.LogException(ex);
                                                                                Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                                                UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                                                Application.Quit();
#endif
                                                                                throw ex;
                                                                            }
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            Debug.LogError("A16");
                                                                            Debug.LogException(ex);
                                                                            Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                                            UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                                            Application.Quit();
#endif
                                                                            throw ex;
                                                                        }
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        Debug.LogError("A15");
                                                                        Debug.LogException(ex);
                                                                        Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                                        UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                                        Application.Quit();
#endif
                                                                        throw ex;
                                                                    }
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    Debug.LogError("A14");
                                                                    Debug.LogException(ex);
                                                                    Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                                    UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                                    Application.Quit();
#endif
                                                                    throw ex;
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                Debug.LogError("A13");
                                                                Debug.LogException(ex);
                                                                Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                                UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                                Application.Quit();
#endif
                                                                throw ex;
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Debug.LogError("A12");
                                                            Debug.LogException(ex);
                                                            Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                            UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                            Application.Quit();
#endif
                                                            throw ex;
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Debug.LogError("A11");
                                                        Debug.LogException(ex);
                                                        Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                        UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                        Application.Quit();
#endif
                                                        throw ex;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Debug.LogError("A10");
                                                    Debug.LogException(ex);
                                                    Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                    UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                    Application.Quit();
#endif
                                                    throw ex;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Debug.LogError("A9");
                                                Debug.LogException(ex);
                                                Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                                UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                                Application.Quit();
#endif
                                                throw ex;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.LogError("A8");
                                            Debug.LogException(ex);
                                            Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                            UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                            Application.Quit();
#endif
                                            throw ex;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError("A7");
                                        Debug.LogException(ex);
                                        Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                        UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                        Application.Quit();
#endif
                                        throw ex;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError("A6");
                                    Debug.LogException(ex);
                                    Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                    UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                    Application.Quit();
#endif
                                    throw ex;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("A5");
                                Debug.LogException(ex);
                                Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                Application.Quit();
#endif
                                throw ex;
                            }
                        }
                        try
                        {
                            Assert.IsTrue(path[path.Count - 1] == dest, "not in destination");
                            try
                            {
                                list.Add(dest.Trans.position);
                                try
                                {
                                    result.Add(new SegmentData(dest, new List<Vector3>(list)));
                                    return result;
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError("A20");
                                    Debug.LogException(ex);
                                    Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                    UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                    Application.Quit();
#endif
                                    throw ex;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("A19");
                                Debug.LogException(ex);
                                Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                                Application.Quit();
#endif
                                throw ex;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("A18");
                            Debug.LogException(ex);
                            Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                            Application.Quit();
#endif
                            throw ex;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("A4");
                        Debug.LogException(ex);
                        Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                        Application.Quit();
#endif
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("A3");
                    Debug.LogException(ex);
                    Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                    Application.Quit();
#endif
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("A2");
                Debug.LogException(ex);
                Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
                Application.Quit();
#endif
                throw ex;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("A1");
            Debug.LogException(ex);
            Debug.LogError(GameSceneManager.Instance.gameObject.scene.name);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
#if TEST_ERRORS
            Application.Quit();
#endif
            throw ex;
        }
    }

    private IEnumerator CheckSpeedChange(AnimationClip clip, Action callback = null)
    {
        float endTime = clip.length / animSpeed - BlendTime;
        animTime = 0f;
        while ((endTime - animTime) > Mathf.Epsilon)
        {
            yield return null;
            animTime += animSpeed * Time.deltaTime;
            if (animSpeedChanged)
            {
                SetAnimationNoBlend(clip, animTime);
                callback?.Invoke();
                animSpeedChanged = false;
            }
        }
        animTime = 0f;
    }

    private IEnumerator SetFrighten()
    {
        if (frighten)
        {
            if (canFrighten)
            {
                if (CurrentAnim == null)
                {
                    CurrentAnim = oldAnim;
                    oldAnim = null;
                }
                frighten = false;
                var clips = AnimationManager.Instance.FrightenClips;
                var clip = RandomUtils.GetRandom(clips);

                float speed = animSpeed;
                var prevClip = CurrentClip;
                animSpeed = 1f;

                SetAnimation(clip);

                animSpeed = speed;
                yield return new WaitForSeconds(clip.length - BlendTime);

                SetAnimation(prevClip, animTime);
            }
            else
            {
                frighten = mayFrighten;
            }
        }
    }

    private void StartCustomAnim(InstanceData data, IEnumerator enumer)
    {
        Assert.IsNull(enumerAnim);
        Assert.IsNull(data.CurrentAnim);
        Assert.IsNull(data.CoroutineAnim);
        data.CurrentAnim = enumer;
        enumerAnim = enumer;
    }

    private void CustomAnimPlay()
    {
        if (enumerAnim != null)
        {
            if (!enumerAnim.MoveNext())
            {
                enumerAnim = null;
            }
        }
    }

    private void FinishCustomAnim()
    {
        while (enumerAnim != null)
        {
            CustomAnimPlay();
        }
    }
}
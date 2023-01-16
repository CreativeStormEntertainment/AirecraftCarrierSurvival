using FMODUnity;
using GPUInstancer;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class SubSectionRoom : Room, ITickable
{
    //[SerializeField]
    //private ESubSectionRoomState currentState;

    //public ESubSectionRoomState CurrentState
    //{
    //    get => currentState;
    //    set
    //    {
    //        for (int i = 1; i < (int)EDCType.Count; i++)
    //        {
    //            EDCType type = (EDCType)i;
    //            ESubSectionRoomState dcFlag = (ESubSectionRoomState)(1 << i);
    //            bool current = (CurrentState & dcFlag) == dcFlag;
    //            bool fired = (value & dcFlag) == dcFlag;

    //            if (type == EDCType.WaterPump)
    //            {
    //                current = (CurrentState & ESubSectionRoomState.ActiveFlood) != 0;
    //                fired = (value & ESubSectionRoomState.ActiveFlood) != 0;
    //            }

    //            if (current != fired)
    //            {
    //                var data = StateDatas[type];
    //                Assert.IsFalse(fired && data.Progresser.IsWorking);

    //                switch(type)
    //                {
    //                    case EDCType.Repair:
    //                        if (Icons != null)
    //                        {
    //                            Icons.Break.gameObject.SetActive(fired);
    //                        }
    //                        if (fired)
    //                        {
    //                            if (!MajorDamage)
    //                            {
    //                                Assert.IsTrue(CurrentMinorBreaks == 0);
    //                                CurrentMinorBreaks = 1;

    //                                DelayVisual(EDelayedVisual.MinorDamageStart);
    //                            }
    //                        }
    //                        break;
    //                    case EDCType.Firefight:
    //                        if (Icons != null)
    //                        {
    //                            Icons.Fire.gameObject.SetActive(fired);
    //                        }
    //                        if (fired)
    //                        {
    //                            DelayVisual(EDelayedVisual.Fire);
    //                        }
    //                        else
    //                        {
    //                            HideVisual(EDelayedVisual.Fire);
    //                        }
    //                        break;
    //                    case EDCType.WaterPump:
    //                        break;
    //                    case EDCType.Rescue:
    //                        if (Icons != null)
    //                        {
    //                            Icons.Injured.gameObject.SetActive(fired);
    //                        }
    //                        if (!fired)
    //                        {
    //                            ParentSection.UpdateCrew();
    //                        }
    //                        break;
    //                }
    //                SetProgresser(data.Progresser, fired, false);
    //            }
    //        }
    //        currentState = value;

    //        bool showWater = (CurrentState & ESubSectionRoomState.AnyFlood) != 0;
    //        if (WaterEffect != null)
    //        {
    //            WaterEffect.gameObject.SetActive(showWater);
    //            if (!showWater)
    //            {
    //                currentFloodLevel = oldFloodLv = olderFloodLv = 0f;
    //            }
    //        }
    //        if (Icons != null)
    //        {
    //            Icons.Water.gameObject.SetActive(showWater);
    //        }

    //        ParentSection.SetSubsectionFlooded(this, IsFlooded());

    //        if (Icons != null)
    //        {
    //            Icons.Locked.gameObject.SetActive((value & ESubSectionRoomState.Locked) == ESubSectionRoomState.Locked);
    //        }
    //        ESubSectionRoomState state = value & ~ESubSectionRoomState.HasInjured;
    //        IsActive = state == 0 || (state == ESubSectionRoomState.Damaged && !MajorDamage);

    //        if ((value & ESubSectionRoomState.AnyFlood) != 0 && (value & ESubSectionRoomState.Fire) == ESubSectionRoomState.Fire)
    //        {
    //            value &= ~ESubSectionRoomState.Fire;
    //            CurrentState = value;
    //            DamageControlManager.Instance.FinishDC(EDCType.Firefight, this, true);
    //        }
    //    }
    //}

    public bool IsBroken
    {
        get => isBroken;
        set
        {
#if ALLOW_CHEATS
            if (HudManager.Instance.Invincibility && value && ParentSection.name != "WreckSection")
            {
                return;
            }
#endif
            if (IsBroken != value)
            {
                isBroken = value;
                SectionRoomManager.Instance.ChangeBrokenSections(IsBroken, this);
                Destruction.Exists = value;
                //CheckIsWorking();
                if (DestructionEffect != null)
                {
                    DestructionEffect.SetActive(value);
                }
                if (value)
                {
                    ParentSection.ChangeFrame();
                    foreach (var segment in Segments)
                    {
                        segment.RemoveDamage();
                        segment.KillInjured();
                        if (segment.Untouchable)
                        {
                            segment.StartDC(EWaypointTaskType.Repair);
                        }
                        else if (segment.Dc != null && !IsLoading)
                        {
                            segment.Dc.QuitSubsection();
                        }
                        if (Irrepairable)
                        {
                            segment.ChangeFrame();
                        }
                    }
                    if (!Segments[0].Untouchable && !IsLoading)
                    {
                        foreach (var subsection in neighbours)
                        {
                            subsection.FrightenWorkers();
                        }
                    }

                    if (!IsLoading && ParentSection == SectionRoomManager.Instance.Hangar)
                    {
                        AircraftCarrierDeckManager.Instance.CheckSquadronDamage(squadronDamagePower, Segments[0]);
                    }
                }
                else
                {
                    foreach (var segment in Segments)
                    {
                        segment.StopDC(EWaypointTaskType.Repair);
                        DamageControlManager.Instance.CheckDCButton(segment, false, true);
                    }
                }
                damagedSections.Clear();

                UpdateActive();
                UpdateVisuals(value ? ERoomLightStates.Destroyed : ERoomLightStates.Working);
            }
        }
    }

    public bool IsWorking
    {
        get;
        private set;
    }

    public bool IsActive
    {
        get => isActive;
        private set
        {
            if (IsActive != value)
            {
                isActive = value;
                if (!IsActive)
                {
                    QuitWorkers();
                }
                ParentSection.UpdateCrew();
            }
        }
    }

    public bool IsLoading
    {
        get;
        set;
    }

    public bool Irrepairable
    {
        get;
        set;
    }

    public bool BrokenRepairable => IsBroken && !Irrepairable;

    public string Title => title;
#if UNITY_EDITOR
    public string Destroyed => destroyed;
    public string NotDestroyed => notDestroyed;
#endif

    public List<SubSectionRoom> NeighboursList = new List<SubSectionRoom>();

    public SubSectionRoom NeighbourInSection;

    public List<SectionSegment> Segments;

    public List<Mesh> RoomMeshes = new List<Mesh>();

    private MeshFilter filter;

    public GameObject DestructionEffect;
    public StudioEventEmitter FireDestruction;
    public StudioEventEmitter BombDestruction;
    public StudioEventEmitter TorpedoDestruction;

    public List<InstanceGroup> WorkerGroups;
    //public Dictionary<EDCType, SubsectionStateData> StateDatas;
    //public List<GameObject> DamagedEffects;
    //public GameObject MajorDamagedEffect;
    //public GameObject FireEffect;
    public SkinnedMeshRenderer WaterEffect;
    //private VisualDelay visualDelay;
    //private WaitForSeconds delay;
    //public SubsectionIcons Icons;
    public EWorkerType SailorMeshes = EWorkerType.Sailor;
    //public EWorkerType OfficerMeshes = EWorkerType.Officer;

    public bool NeverActive;

    public RepairableDanger Destruction;

    [HideInInspector]
    public SectionRoom ParentSection;

    [HideInInspector]
    public WorkerPath Path;

    [HideInInspector]
    public MeshRenderer Renderer;
    //private bool majorDamage;
    //public bool MajorDamage
    //{
    //    get => majorDamage;
    //    set
    //    {
    //        if (value != majorDamage)
    //        {
    //            majorDamage = value;
    //if (value)
    //{
    //    CurrentState |= ESubSectionRoomState.Damaged;
    //    StateDatas[EDCType.Repair].State = 0f;
    //    CurrentMinorBreaks = 0;

    //    for (int i = 0; i < DamagedEffects.Count; i++)
    //    {
    //        HideVisual(EDelayedVisual.MinorDamageStart + i);
    //    }
    //    DelayVisual(EDelayedVisual.MajorDamage);
    //}
    //        }
    //    }
    //}
    //private float olderFloodLv = 1f;
    //private float oldFloodLv = 1f;
    //public int CurrentMinorBreaks;
    //private float progressToBreak = 1f;
    //private float timeToBreak;
    //const float MinTimeToBreak = 6f;
    //const float MaxTimeToBreak = 12f;

    //private float currentFloodLevel = 1f;

    [HideInInspector]
    public Dictionary<EWaypointTaskType, HashSet<int>> ExitIndicesSet = new Dictionary<EWaypointTaskType, HashSet<int>>();

    private bool isBroken;

    private ERoomLightStates currentLightState = (ERoomLightStates)int.MaxValue;

    private HashSet<SectionSegment> damagedSections;

    //private Dictionary<InstanceData, Waypoint> injuredWorkerDict;
    //public Dictionary<InstanceData, Waypoint> FreeInjuredWorkerDict;

    private HashSet<SubSectionRoom> neighbours;

    [SerializeField]
    private int squadronDamagePower = 6;

    [Header("Tooltip")]
    [SerializeField]
    private string title = "name";
    [SerializeField]
    private string destroyed = "destroyed effect";
    [SerializeField]
    private string notDestroyed = "not destroyed effect";
    [SerializeField]
    private float showDelay = 0.75f;
    [SerializeField]
    private float hideDelay = 1f;

    private bool hovered;
    private bool isShown;

    private Coroutine showTooltipCoroutine;
    private Coroutine hideTooltipCoroutine;

    private Tooltip tooltip;

    private SectionSegment hoveredSegment;
    private SectionSegment nextHoveredSegment;

    private bool wasWorking;

    private void Awake()
    {
        hideDelay = .25f;
        damagedSections = new HashSet<SectionSegment>();

        neighbours = new HashSet<SubSectionRoom>();
        foreach (var segment in Segments)
        {
            if (segment.Parent != null)
            {
                Debug.LogError(segment.Parent.name, this);
            }
            segment.Parent = this;
        }

        var locMan = LocalizationManager.Instance;
        title = locMan.GetText(title);
        if (destroyed != "-")
        {
            destroyed = locMan.GetText(destroyed);
        }
        notDestroyed = locMan.GetText(notDestroyed);

        Path = GetComponent<WorkerPath>();
        filter = GetComponent<MeshFilter>();
        Renderer = GetComponent<MeshRenderer>();
        if (filter.mesh == null)
        {
            gameObject.SetActive(false);
        }

        Destruction = new RepairableDanger();
        Destruction.isMajor = true;

        IsWorking = true;
        //CheckIsWorking();
    }

    private void Start()
    {
        tooltip = Tooltip.Instance;

        WorkerGroups = CreateNormalGroups(1);

        UpdateVisuals(ERoomLightStates.Working);

        ExitIndicesSet.Clear();
        foreach (var key in Path.CategorisedExits.Keys)
        {
            ExitIndicesSet[key] = new HashSet<int>();
        }

        Assert.IsTrue(Segments.Count > 0);

        Destruction.RepairPower = 1f;
        if (ParentSection == DamageControlManager.Instance.WreckSection)
        {
            Destruction.RepairData.Max = 1e9f;
        }
        else
        {

            Destruction.RepairData.TemplateMax = Parameters.Instance.DestroyedFixTickTime;
        }
        Destruction.RepairData.ReachedMax += OnRepairReachedMax;
        TimeManager.Instance.AddTickable(this);

        foreach (var segment in Segments)
        {
            foreach (var segment2 in segment.NeighboursDirectionDictionary.Keys)
            {
                neighbours.Add(segment2.Parent);
            }
        }
        neighbours.Remove(this);
    }

    private void Update()
    {
        if (hovered && !isShown && showTooltipCoroutine == null)
        {
            showTooltipCoroutine = StartCoroutine(ShowTooltip(showDelay));
        }
        else if (!hovered && isShown && hideTooltipCoroutine == null)
        {
            hideTooltipCoroutine = StartCoroutine(HideTooltip(hideDelay));
        }
        if (tooltip.CurrentRoom == this && isShown)
        {
            SetText(out string desc, out string status);
            tooltip.UpdateSectionDesc(desc, status);
        }

        if (Destruction.Exists != IsBroken)
        {
            IsBroken = Destruction.Exists;
        }
        if (wasWorking != IsWorking)
        {
            var eventMan = EventManager.Instance;
            foreach (var segment in Segments)
            {
                if (IsWorking)
                {
                    eventMan.RemoveDynamicEvent(segment);
                }
                else
                {
                    eventMan.AddDynamicEvent(segment, EDynamicEventType.SectionDamage);
                }
            }
        }
        wasWorking = IsWorking;
        CheckIsWorking();

        var workerInMan = WorkerInstancesManager.Instance;
        for (int i = 0; i < WorkerGroups.Count; i++)
        {
            if (WorkerGroups[i].ActionType != EWorkerActionType.None)
            {
                bool check = false;
                foreach (var workerInstance in WorkerGroups[i].Instances)
                {
                    if (workerInstance.InUse && workerInstance.CurrentAnim == null)
                    {
                        workerInstance.Delay -= Time.deltaTime;
                        if (workerInstance.Delay < Mathf.Epsilon)
                        {
                            workerInstance.Delay = 0f;
                            if (workerInstance.Go(Time.deltaTime))
                            {
                                if (workerInstance.GetDestination().Data.AnimType == EWaypointAnimType.Exit)
                                {
                                    Assert.IsTrue(WorkerGroups[i].ActionType == EWorkerActionType.Quit, WorkerGroups[i].ActionType.ToString());
                                    workerInstance.InUse = false;
                                    check = true;
                                }
                                else
                                {
                                    Assert.IsFalse(workerInstance.GetDestination().Data.AnimType == EWaypointAnimType.BasicAnim);
                                    workerInMan.StartAnim(workerInstance, workerInstance.MasterAnim(workerInstance.StartAnimation()));
                                }
                            }
                            GPUInstancerAPI.UpdateTransformDataForInstance(workerInMan.GPUICrowdManager, workerInstance.Ref);
                        }
                    }
                }

                if (check)
                {
                    CheckGroupForNewAction(WorkerGroups[i]);
                }
            }
        }
    }

    public void Setup()
    {
        GetAllErrors();
        //if (ParentSection.name == "1.1_meteo")
        //{
        foreach (var segment in Segments)
        {
            if (!Path.ExitsBySegment.ContainsKey(segment))
            {
                Debug.LogWarning(ParentSection.name + ", segment " + segment.name + " has no exits", segment);
            }
            if (!Path.ActionsBySegments.ContainsKey(segment))
            {
                //Debug.LogWarning(ParentSection.name + ", segment " + segment.name + " has no action waypoints whatsoever");
            }
            else
            {
                //Check(segment, EWaypointTaskType.Firefighting);
                //Check(segment, EWaypointTaskType.Repair);
                //Check(segment, EWaypointTaskType.Rescue);

                //if (segment.CanPumpWater)
                //{
                //    Check(segment, EWaypointTaskType.Waterpump);
                //}

                //int totalCount = 0;
                //foreach (var data in segment.Neighbours)
                //{
                //    if (data.Door != null)
                //    {
                //        int count = 0;
                //        var waypoint = data.Door.ParentWaypoint1;
                //        if (data.Door.Parent2 == segment)
                //        {
                //            waypoint = data.Door.ParentWaypoint2;
                //        }
                //        else
                //        {
                //            Assert.IsTrue(data.Door.Parent1 == this);
                //        }
                //        foreach (var branch in waypoint.Branches)
                //        {
                //            if (branch.Data.PossibleTasks == EWaypointTaskType.RepairDoor)
                //            {
                //                count++;
                //                totalCount++;
                //            }
                //        }
                //        if (count > 3)
                //        {
                //            Debug.LogWarning("segment " + segment.name + " for door " + data.Door.name + " has more repair doors waypoints than needed, " + count + ">3", data.Door);
                //        }
                //        else if (count < 3)
                //        {
                //            Debug.LogWarning("segment " + segment.name + " for door " + data.Door.name + " has less repair doors waypoints than needed, " + count + "<3", data.Door);
                //        }
                //    }
                //}
                //var animWaypoints = Path.ActionsBySegments[segment];
                //if (animWaypoints.ContainsKey(EWaypointTaskType.RepairDoor))
                //{
                //    if (animWaypoints[EWaypointTaskType.RepairDoor].Count != totalCount)
                //    {
                //        Debug.LogWarning("segment " + segment.name + " has some unconnected repair doors waypoints", segment);
                //    }
                //}
            }
        }
        //}

        IsActive = !NeverActive;
    }

    public void Tick()
    {
        if (Destruction.Exists && HudManager.Instance.HasNo(ETutorialMode.DisableDCEvents))
        {
            if (!Segments[0].Untouchable)
            {
                var dcMan = DamageControlManager.Instance;
                Destruction.RepairPower = dcMan.RepairSpeedModifier;
                if (!dcMan.AutoDC)
                {
                    Destruction.RepairPower += Parameters.Instance.ManualDCRepairSpeedMultiplier;
                }
                Destruction.Update();
            }
        }
        UpdateActive();
    }

    public void UpdateActive()
    {
        bool active = !NeverActive && IsWorking;
        if (active)
        {
            foreach (var segment in Segments)
            {
                if (segment.Fire.Exists || segment.Group.IsFlooded())
                {
                    active = false;
                    break;
                }
            }
        }
        IsActive = active;
    }

    public void SegmentDamagedChanged(SectionSegment segment)
    {
        //if (segment.Damage.Exists || segment.Fire.Exists || segment.IsFlooding())
        //{
        //    damagedSections.Add(segment);
        //}
        //else
        //{
        //    damagedSections.Remove(segment);
        //}

        //CheckIsWorking();

        //if (!IsBroken && damagedSections.Add(segment) && allowMajor && ((damagedSections.Count * 2) > Segments.Count) && Random.value <= (((float)damagedSections.Count) / ((float)Segments.Count)))
        //{
        //    switch (damageType)
        //    {
        //        case EDamageType.Basic:
        //            if (FireDestruction != null)
        //            {
        //                FireDestruction.Play();
        //            }
        //            break;
        //        case EDamageType.Bomb:
        //            if (BombDestruction != null)
        //            {
        //                BombDestruction.Play();
        //            }
        //            break;
        //        case EDamageType.Torpedo:
        //            if (TorpedoDestruction != null)
        //            {
        //                TorpedoDestruction.Play();
        //            }
        //            break;
        //    }
        //    IsBroken = true;
        //}
    }

    //public void SegmentRepaired(SectionSegment segment)
    //{
    //    damagedSections.Remove(segment);
    //}

    public void UpdateVisuals(ERoomLightStates newLightstate)
    {
        filter.mesh = RoomMeshes[(int)newLightstate];

        if (currentLightState != newLightstate)
        {
            LightManager.Instance.SetLighting(this, newLightstate);
            currentLightState = newLightstate;
        }
    }

    public void StartWorkers()
    {
        foreach (var group in WorkerGroups)
        {
            if (group.ActionType == EWorkerActionType.None)
            {
                StartWorkers(group);
                break;
            }
        }
    }

    public void QuitWorkers()
    {
        var workerInMan = WorkerInstancesManager.Instance;
        foreach (var group in WorkerGroups)
        {
            bool check = false;
            foreach (var workerInstance in group.Instances)
            {
                workerInstance.UpdateSpeed(true, true);
                if (workerInstance.InUse && group.ActionType == EWorkerActionType.Action)
                {
                    if (workerInstance.GoToNearestExit())
                    {
                        if (workerInstance.CurrentAnim == null)
                        {
                            workerInstance.WalkAnim();
                            workerInstance.LookAtDir();
                            GPUInstancerAPI.UpdateTransformDataForInstance(workerInMan.GPUICrowdManager, workerInstance.Ref);
                        }
                    }
                    else
                    {
                        if (workerInstance.FrightenOnly)
                        {
                            workerInstance.FrightenOnly = false;
                            workerInMan.StopAnim(workerInstance);
                        }
                        Assert.IsNull(workerInstance.CurrentAnim);
                        workerInstance.WalkAnim();
                        workerInstance.InUse = false;
                        check = true;
                    }
                }
                Assert.IsTrue(workerInstance.InUse || group.ActionType == EWorkerActionType.None || workerInstance.GetDestination().Data.AnimType == EWaypointAnimType.Exit);
            }
            if (group.ActionType == EWorkerActionType.Action)
            {
                group.ActionType = EWorkerActionType.Quit;
            }

            if (check)
            {
                CheckGroupForNewAction(group);
            }
        }
    }

    public void GetErrors(StringBuilder errors)
    {
        foreach (var segment in Segments)
        {
            foreach (var segment2 in Segments)
            {
                if (segment2 != segment && segment.HorizontalNeighbours.Contains(segment2))
                {
                    if (!Path.DCSegmentTransition.ContainsKey(segment) || !Path.DCSegmentTransition[segment].ContainsKey(segment2))
                    {
                        errors.AppendLine(segment.name + " has no transition waypoint to " + segment2.name);
                    }
                }
            }
        }
    }

    private void GetAllErrors()
    {
        foreach (var segment in Segments)
        {
            foreach (var segment2 in Segments)
            {
                if (segment.HorizontalNeighbours.Contains(segment2) && (!Path.DCSegmentTransition.ContainsKey(segment) || !Path.DCSegmentTransition[segment].ContainsKey(segment2)))
                {
                    Debug.LogError(segment.name + " has no transition waypoint to " + segment2.name);
                }
            }
        }
    }

    private List<InstanceGroup> CreateNormalGroups(int count)
    {
        var workerInMan = WorkerInstancesManager.Instance;
        var trans = transform;
        var result = new List<InstanceGroup>();
        var walkClip = AnimationManager.Instance.WalkClip;
        for (int i = 0; i < count; i++)
        {
            var group = new InstanceGroup(EWaypointTaskType.Normal);
            workerInMan.CreateWorkers(group, SailorMeshes, workerInMan.SailorInstancesPerSection, EWaypointTaskType.Normal, Path, walkClip, trans);
            //workerInMan.CreateWorkers(group, OfficerMeshes, workerInMan.OfficerInstancesPerSection, EWaypointTaskType.Normal, Path, walkClip, trans);
            result.Add(group);
        }
        return result;
    }

    private void StartWorkers(InstanceGroup group)
    {
        var workerInMan = WorkerInstancesManager.Instance;
        Assert.IsTrue(group.ActionType == EWorkerActionType.None);

        group.ActionType = EWorkerActionType.Action;
        var dict = new Dictionary<Waypoint, float>();
        foreach (var workerInstance in group.Instances)
        {
            Assert.IsFalse(workerInstance.InUse);
            Waypoint dest;
            dest = workerInstance.GetNewDestination();
            workerInstance.InUse = true;
            workerInstance.UpdateSpeed(false, false);
            var start = workerInstance.SetStartAndDestination(dest);
            if (!dict.ContainsKey(start))
            {
                dict[start] = 0f;
            }
            workerInstance.Delay = workerInMan.SameEntryDelay * dict[start];
            dict[start]++;
            GPUInstancerAPI.UpdateTransformDataForInstance(workerInMan.GPUICrowdManager, workerInstance.Ref);
        }

        //group.ConnectedSubcrewman = ParentSection.Subcrewman;
        //group.ConnectedSubcrewman.Groups.Add(group);
        //group.SetSelected(group.ConnectedSubcrewman.Parent.Selected);
    }

    private void FrightenWorkers()
    {
        foreach (var group in WorkerGroups)
        {
            if (group.ActionType != EWorkerActionType.None)
            {
                foreach (var workerInstance in group.Instances)
                {
                    workerInstance.Frighten();
                }
            }
        }
    }

    private void CheckGroupForNewAction(InstanceGroup group)
    {
        bool check = false;
        foreach (var workerInstance in group.Instances)
        {
            if (workerInstance.InUse)
            {
                check = true;
                break;
            }
        }
        if (!check)
        {
            group.SetSelected(false);
            //group.ConnectedSubcrewman.Groups.Remove(group);
            //group.ConnectedSubcrewman = null;

            group.ActionType = EWorkerActionType.None;
            foreach (var workerInstance in group.Instances)
            {
                workerInstance.TaskType = EWaypointTaskType.Normal;
            }
            //if (ParentSection.Subcrewman != null)
            {
                foreach (var group2 in WorkerGroups)
                {
                    if (group2.ActionType == EWorkerActionType.Action)
                    {
                        check = true;
                        break;
                    }
                }

                if (!check && IsActive)
                {
                    foreach (var group2 in WorkerGroups)
                    {
                        Assert.IsFalse(group2.ActionType == EWorkerActionType.Action);
                    }

                    StartWorkers(group);
                }
            }
        }
    }

    private void OnRepairReachedMax()
    {
        IsBroken = false;
    }

    private void CheckIsWorking()
    {
        bool damaged = true;
        foreach (var segment in Segments)
        {
            if (!segment.HasAnyDamage())
            {
                damaged = false;
                break;
            }
        }

        IsWorking = !IsBroken && !damaged;
    }

    ///TOOLTIP

    public void ShowTooltip(bool value, SectionSegment sectionSegment = null)
    {
        hovered = value;
        nextHoveredSegment = sectionSegment;
    }

    private void SetText(out string desc, out string status)
    {
        desc = ParentSection.IsWorking ? notDestroyed : destroyed;
        status = tooltip.SegmentStatusDesc;
        if (hoveredSegment != null && hoveredSegment.HasAnyIssue())
        {
            status += hoveredSegment.Fire.Exists ? "\n" + tooltip.SegmentStatusFire : "";
            status += hoveredSegment.IsFlooding() ? "\n" + tooltip.SegmentStatusFlood : "";
            status += hoveredSegment.HasInjured ? "\n" + tooltip.SegmentStatusInjured : "";
            status += hoveredSegment.Damage.Exists ? "\n" + tooltip.SegmentStatusFault : "";
            status += IsBroken ? "\n" + tooltip.SegmentStatusDestroyed : "";
        }
        else
        {
            status += "\n" + tooltip.SegmentStatusClear;
        }
    }

    private IEnumerator ShowTooltip(float timeDelay)
    {
        CameraManager.Instance.SectionView.CameraMoved += CursorMoved;
        Cursor.Instance.CursorMoved += CursorMoved;
        Cursor.Instance.SaveCursorPos();
        yield return StartCoroutine(WaitForRealSeconds(timeDelay));
        hoveredSegment = nextHoveredSegment;
        SetText(out string desc, out string status);
        tooltip.SetupSectionTooltip(title, desc, status, IsWorking, this);
        showTooltipCoroutine = null;
        hideTooltipCoroutine = null;
        isShown = true;
    }

    private IEnumerator HideTooltip(float timeDelay)
    {
        Cursor.Instance.CursorMoved -= CursorMoved;
        CameraManager.Instance.SectionView.CameraMoved -= CursorMoved;
        yield return StartCoroutine(WaitForRealSeconds(timeDelay));
        hoveredSegment = null;
        isShown = false;
        hideTooltipCoroutine = null;
        showTooltipCoroutine = null;
        if (Tooltip.Instance.CurrentRoom == this)
        {
            Tooltip.Instance.Hide();
        }
    }

    private IEnumerator WaitForRealSeconds(float seconds)
    {
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < seconds)
        {
            yield return null;
        }
    }

    private void CursorMoved()
    {
        Cursor.Instance.CursorMoved -= CursorMoved;
        CameraManager.Instance.SectionView.CameraMoved -= CursorMoved;
        if (showTooltipCoroutine != null)
        {
            StopCoroutine(showTooltipCoroutine);
            showTooltipCoroutine = null;
        }
        if (isShown)
        {
            hideTooltipCoroutine = StartCoroutine(HideTooltip(hideDelay));
        }
    }
}

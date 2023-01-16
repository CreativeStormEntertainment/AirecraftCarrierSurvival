using FMODUnity;
using GPUInstancer;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SectionSegment : MonoBehaviour, ITickable, IInteractive
{
    public event Action SegmentClicked = delegate { };
    public event Action<bool, ESectionCategory> DCChanged = delegate { };
    public event Action IssueFixed = delegate { };

    private static readonly int Color = Shader.PropertyToID("_Color");

    public DCInstanceGroup Dc
    {
        get => dc;
        set
        {
            var group = dc;
            dc = value;
            DCChanged(value != null, Parent.ParentSection.Category);
        }
    }

    public bool HasInjured
    {
        get;
        private set;
    }

    public bool Rescued
    {
        get => rescued;
        set
        {
            rescued = value;
            if (value)
            {
                IssueFixed();
                SectionRoomManager.Instance.FireIssueFixed();
                EventManager.Instance.RemoveDynamicEvent(this);
                SectionDamageNotifierManager.Instance.SetSegmentIssue(this, EIssue.Injured);
                if (injured != null)
                {
                    injured.SetState(ECrewUnitState.Healthy);
                    injured = null;
                    InjuryData.Current = 0f;
                }
            }
        }
    }

    public bool RescueInProgress
    {
        get;
        set;
    }

    public bool Untouchable
    {
        get;
        set;
    }

    public EWaypointTaskType RescueType
    {
        get;
        private set;
    }

    public DCInstanceGroup Rescuers
    {
        get;
        set;
    }

    [NonSerialized]
    public SubSectionRoom Parent;
    [NonSerialized]
    public SectionSegmentGroup Group;
    //public List<SectionSegmentData> Neighbours;
    public MeshCollider Collider;
    //public Dictionary<SectionSegment, SectionSegmentData> NeighboursDictionary;

    public Dictionary<SectionSegment, ENeighbourDirection> NeighboursDirectionDictionary;

    public HashSet<SectionSegment> HorizontalNeighbours;
    public HashSet<SectionSegment> FloodNeighbours;
    public HashSet<SectionSegment> FireNeighbours;

    public SpreadableDanger Damage;
    public SpreadableDanger Fire;

    public GameObject Selection;
    public SubsectionIcons Icons;

    public GameObject FireEffect;
    public GameObject DamageEffect;

    public StudioEventEmitter FireStartSound;
    public StudioEventEmitter FireSpreadSound;
    public StudioEventEmitter FireLoopSound;
    public StudioEventEmitter DamageSound;
    public StudioEventEmitter WaterFill;
    public StudioEventEmitter WaterDrop;

    public InjuredIcon InjuredIcon;

    public int SquadronDamagePower = 3;
    [NonSerialized]
    public bool CanPumpWater;

    public GameObject PumpingDCIcon;
    public GameObject RepairingDCIcon;

    public Vector3 Center;

    private DCInstanceGroup dc;
    private float damageTime;
    private float damagePrevValue;
    private float fireTime;
    private float firePrevValue;

    private float injureTime;
    private float injurePrevValue;

    private int injuredCount;
    private int injuredCountInPlace;
    private InstanceGroup injuredGroup;
    private List<AnimationClip> rescueClips;

    private PercentageData InjuryData;

    private bool inPlace;

    private bool hovered;

    private bool showIcons;

    private bool looping;
    private bool fill;
    private bool drop;

    private HashSet<SectionSegment> neighboursSet;

    private MeshRenderer mesh;
    private bool blinking;
    private Material material;
    private Shader shader;
    private bool rescued;

    private CrewUnit injured;

    private bool loading;

    private void Awake()
    {
        Damage = new SpreadableDanger();
        Fire = new SpreadableDanger();

        NeighboursDirectionDictionary = new Dictionary<SectionSegment, ENeighbourDirection>();
        HorizontalNeighbours = new HashSet<SectionSegment>();
        FloodNeighbours = new HashSet<SectionSegment>();
        FireNeighbours = new HashSet<SectionSegment>();

        if (Selection != null)
        {
            Selection.SetActive(false);
        }

        neighboursSet = new HashSet<SectionSegment>();
        mesh = Selection.GetComponent<MeshRenderer>();

        material = mesh.material;
        shader = material.shader;
    }

    private void Start()
    {
        Assert.IsNotNull(Parent);
        Assert.IsNotNull(Group, $"{name} has no water group");

        CameraManager.Instance.ViewChanged += OnViewChanged;

        var dcMan = DamageControlManager.Instance;
        var paramsMan = Parameters.Instance;

        Damage.RepairData.TemplateMax = paramsMan.FaultFixTickTime;
        Damage.SpreadData.TemplateMax = paramsMan.FaultSpreadTickTime;
        Damage.RepairPower = 1f;

        Fire.RepairData.TemplateMax = paramsMan.FireExtinguishTimeTicks;
        Fire.EventData.TemplateMax = dcMan.FireStaticEventTime;
        Fire.SpreadData.TemplateMax = paramsMan.FireSpreadTimeTicks;
        Fire.RepairPower = 1f;

        InjuryData = new PercentageData();

        Damage.RepairData.ReachedMax += OnRepairReachedMax;
        Damage.SpreadData.ReachedMax += OnDamageSpreadReachedMax;
        Fire.RepairData.ReachedMax += OnFirefightReachedMax;
        Fire.SpreadData.ReachedMax += OnFireSpreadReachedMax;
        InjuryData.ReachedMax += OnInjuredReachedMax;

        TimeManager.Instance.AddTickable(this);

        injuredGroup = new InstanceGroup(EWaypointTaskType.Normal);
#warning change mesh for injured
        WorkerInstancesManager.Instance.CreateWorkers(injuredGroup, EWorkerType.Injured, 3, EWaypointTaskType.Normal, Parent.Path, AnimationManager.Instance.WalkClip, transform);

        rescueClips = new List<AnimationClip>();
        for (int i = 0; i < injuredGroup.Instances.Count; i++)
        {
            rescueClips.Add(null);
        }
    }

    private void Update()
    {
        float currentFill = Group.CurrentFloodFill;
        bool stopDrop = false;
        bool stopFill = false;
        if (IsFlooding() && currentFill > 0f && currentFill < 1f)
        {
            if (Group.Flood.Repair)
            {
                if (!drop && WaterDrop != null)
                {
                    drop = true;
                    WaterDrop.Play();
                }
                stopFill = true;
            }
            else
            {
                if (!fill)
                {
                    if (WaterFill != null)
                    {
                        fill = true;
                        WaterFill.Play();
                    }
                }
                stopDrop = true;
            }
        }
        else
        {
            stopDrop = true;
            stopFill = true;
        }
        if (stopFill && fill)
        {
            fill = false;
            WaterFill.Stop();
        }
        if (stopDrop && drop)
        {
            drop = false;
            WaterDrop.Stop();
        }
        if (Icons != null)
        {
            UpdateDangerVisual(Damage, ref damageTime, damagePrevValue, DamageEffect, Icons.Break, Icons.BreakRenderer);
            UpdateDangerVisual(Fire, ref fireTime, firePrevValue, FireEffect, Icons.Fire, Icons.FireRenderer);
            if (looping && !Fire.Exists)
            {
                looping = false;
                FireLoopSound.Stop();
            }

            Icons.Water.SetActive(showIcons && IsFlooding());
            Icons.SetFill(Icons.WaterRenderer, 1f - currentFill);
            if (InjuredIcon != null)
            {
                UpdateDangerVisual(CanRescue(), InjuryData, ref injureTime, injurePrevValue, InjuredIcon.gameObject, InjuredIcon.IconRenderer);
            }
            Icons.Destruction.SetActive(showIcons && Parent.BrokenRepairable);
            if (Parent.IsBroken && Parent.Irrepairable)
            {
                Icons.TotalDestruction.SetActive(showIcons);
            }
        }
        if (injured != null)
        {
            int minutes = Mathf.RoundToInt(((InjuryData.Max - InjuryData.Current) / TimeManager.Instance.TicksForHour) * 60f);
            injured.SetTime(minutes);
        }

        if (HasInjured)
        {
            var animMan = AnimationManager.Instance;
            var paramsMan = Parameters.Instance;
            var workerInstanceMan = WorkerInstancesManager.Instance;

            float efficiencyBonus = CrewManager.Instance.DepartmentDict[EDepartments.Medical].EfficiencyMinutes;
            for (int i = 0; i < injuredGroup.Instances.Count; i++)
            {
                var injured = injuredGroup.Instances[i];
                if (injured.CurrentAnim == null && injured.SegmentPath != null)
                {
                    if (injured.Go(Time.deltaTime))
                    {
                        if (inPlace)
                        {
                            injured.SegmentPath.Clear();
                        }
                        else
                        {
                            animMan.RandomRescue(out var rescueeAnims, out var rescueClip);
                            rescueClips[i] = rescueClip;
                            workerInstanceMan.StartAnim(injured, injured.StartInjuredAnimation(rescueeAnims));
                        }
                        if (++injuredCountInPlace >= injuredCount)
                        {
                            injuredCountInPlace = 0;
                            if (inPlace)
                            {
                                inPlace = false;
                                HasInjured = false;
                                Rescued = false;
                                RescueInProgress = false;

                                foreach (var dcInstance in Rescuers.Instances)
                                {
                                    (dcInstance.PrevWalkClip, dcInstance.WalkClip) = (dcInstance.WalkClip, dcInstance.PrevWalkClip);
                                    if (dcInstance.CurrentClip == dcInstance.PrevWalkClip)
                                    {
                                        dcInstance.WalkAnim();
                                    }
                                }
                                Rescuers = null;
                            }
                            else
                            {
                                InitInjuredEvent(paramsMan.MinTimeToRescueBeforeInjured + UnityEngine.Random.value * paramsMan.RangeTimeToRescueBeforeInjured + efficiencyBonus, false);
                            }
                        }
                    }
                    GPUInstancerAPI.UpdateTransformDataForInstance(workerInstanceMan.GPUICrowdManager, injured.Ref);
                }
            }
        }
    }

    private void OnDestroy()
    {
        Destroy(material);
    }

    public void LoadData(SegmentSaveData data)
    {
        Assert.IsFalse(loading);
        loading = true;
        if (data.Damaged)
        {
            MakeDamage();
        }
        if (data.Destroyed)
        {
            Assert.IsFalse(Parent.IsLoading);
            Parent.IsLoading = true;
            Parent.IsBroken = true;
            Parent.IsLoading = false;
        }
        if (data.Irrepairable)
        {
            Assert.IsFalse(Parent.IsLoading);
            Parent.IsLoading = true;
            Parent.Irrepairable = true;
            Parent.IsLoading = false;
        }
        if (data.Fire)
        {
            MakeFire();
        }
        if (data.FloodLevel > 0f)
        {
            MakeFlood(!data.OriginalFlooded);
            if (data.FloodLevel >= 1f)
            {
                Group.LoadFlood(data.FloodLevel - 1f);
            }
        }
        if (data.CrewInjured > -1)
        {
            var injuredUnit = CrewManager.Instance.GetCrew(data.CrewInjured);
            injuredUnit.Segment = this;
            MakeInjured(EWaypointTaskType.Rescue, injuredUnit);
            if (data.InjuredAnim > -1)
            {
                var animMan = AnimationManager.Instance;
                var workerInstanceMan = WorkerInstancesManager.Instance;
                for (int i = 0; i < injuredCount; i++)
                {
                    var injured = injuredGroup.Instances[i];
                    injured.TeleportAndRotate(injured.GetDestination());

                    animMan.GetRescueAnim(data.InjuredAnim, out var rescueeAnims, out var rescueClip);
                    rescueClips[i] = rescueClip;
                    workerInstanceMan.StartAnim(injured, injured.StartInjuredAnimation(rescueeAnims));
                }

                InjuryData.Current = data.InjuredTime;
                InitInjuredEvent(data.MaxInjuredTime, true);
            }
        }
        Assert.IsTrue(loading);
        loading = false;
    }

    public SegmentSaveData SaveData()
    {
        var result = new SegmentSaveData();

        result.Damaged = Damage.Exists;
        if (Parent.Segments[0] == this)
        {
            result.Destroyed = Parent.IsBroken;
            result.Irrepairable = Parent.Irrepairable;
        }
        result.Fire = Fire.Exists;

        if (IsFlooding())
        {
            if (Group.Flood.Exists)
            {
                result.FloodLevel = Group.Flood.FillData.Percent + 1f;
            }
            else
            {
                result.FloodLevel = .5f;
            }
        }
        else
        {
            result.FloodLevel = -1f;
        }
        result.OriginalFlooded = Group.OriginalFlooded;

        result.CrewInjured = injured == null ? -1 : CrewManager.Instance.IndexOf(injured);
        result.InjuredAnim = inPlace ? AnimationManager.Instance.RescueIndexOf(rescueClips[0]) : -1;
        result.InjuredTime = (int)InjuryData.Current;
        result.MaxInjuredTime = InjuryData.TemplateMax;

        return result;
    }

    public void Tick()
    {
        if (!HudManager.Instance.HasNo(ETutorialMode.DisableDCEvents))
        {
            return;
        }

        float power = DamageControlManager.Instance.MaintenanceActive ? Parameters.Instance.FaultSpreadWithMaintenanceMultiplier : 1f;
        UpdateDanger(Damage, power, ref damageTime, ref damagePrevValue);
        UpdateDanger(Fire, 1f, ref fireTime, ref firePrevValue);
        if (CanRescue())
        {
            injureTime = 0f;
            if (CrewManager.Instance.FreezeRescueTime)
            {
                float minute = TimeManager.Instance.TicksForHour / 60f;
                if (minute <= (InjuryData.Max - InjuryData.Current))
                {
                    InjuryData.Current = InjuryData.Max - minute - 1f;
                }
            }

            injurePrevValue = InjuryData.Percent;
            InjuryData.Update();
        }
    }

    public void MakeDamage()
    {
#if ALLOW_CHEATS
        if (HudManager.Instance.Invincibility)
        {
            return;
        }
#endif
        if (!Untouchable && !Parent.IsBroken)
        {
            if (!Damage.Exists)
            {
                if (DamageSound != null)
                {
                    DamageSound.Play();
                }
                Damage.Exists = true;
                Parent.SegmentDamagedChanged(this);
                EventManager.Instance.AddDynamicEvent(this, EDynamicEventType.Damage);
                SectionDamageNotifierManager.Instance.SetSegmentIssue(this, EIssue.Fault);
                if (!loading && Parent.ParentSection == SectionRoomManager.Instance.Hangar)
                {
                    AircraftCarrierDeckManager.Instance.CheckSquadronDamage(SquadronDamagePower, this);
                }
            }
            StartDC(EWaypointTaskType.Repair);
        }
    }

    public void RemoveDamage()
    {
        DamageControlManager.Instance.CheckDCButton(this, false, false);
        EventManager.Instance.RemoveDynamicEvent(this);
        SectionDamageNotifierManager.Instance.SetSegmentIssue(this, EIssue.Fault);
        Damage.Exists = false;
        Damage.Repair = false;
        Parent.SegmentDamagedChanged(this);
        StopDC(EWaypointTaskType.Repair);
    }

    public void MakeFire(bool startedBySpreading = false)
    {
#if ALLOW_CHEATS
        if (HudManager.Instance.Invincibility)
        {
            return;
        }
#endif
        if (!Untouchable && !IsFlooding() && !Fire.Exists)
        {
            Parent.SegmentDamagedChanged(this);
            Fire.Exists = true;
            if (startedBySpreading)
            {
                if (FireSpreadSound != null)
                {
                    FireSpreadSound.Play();
                }
            }
            else
            {
                if (FireStartSound != null)
                {
                    FireStartSound.Play();
                }
            }
            EventManager.Instance.AddDynamicEvent(this, EDynamicEventType.Fire);
            SectionDamageNotifierManager.Instance.SetSegmentIssue(this, EIssue.Fire);
            if (FireLoopSound != null)
            {
                looping = true;
                FireLoopSound.Play();
            }
            KillInjured();
            Parent.UpdateActive();
            StartDC(EWaypointTaskType.Firefighting);

            if (!loading && Parent.ParentSection == SectionRoomManager.Instance.Hangar)
            {
                AircraftCarrierDeckManager.Instance.CheckSquadronDamage(SquadronDamagePower, this);
            }
        }
    }

    public void RemoveFire()
    {
        Parent.SegmentDamagedChanged(this);
        Fire.Exists = false;
        Fire.Repair = false;
        StopDC(EWaypointTaskType.Firefighting);
        EventManager.Instance.RemoveDynamicEvent(this);
        SectionDamageNotifierManager.Instance.SetSegmentIssue(this, EIssue.Fire);
    }

    public void MakeInjured(EWaypointTaskType rescueType, CrewUnit injuredUnit = null, bool instant = false)
    {
#if ALLOW_CHEATS
        if (HudManager.Instance.Invincibility)
        {
            return;
        }
#endif
        //todo more rescue
        rescueType = EWaypointTaskType.Rescue;
        if (!Untouchable && !IsFlooding() && !Fire.Exists && !Parent.IsBroken && !HasInjured)
        {
            SectionDamageNotifierManager.Instance.SetSegmentIssue(this, EIssue.Injured);

            if (rescueType == EWaypointTaskType.Rescue)
            {
                injuredCount = 1;
            }
            else if (rescueType == EWaypointTaskType.Rescue2)
            {
                injuredCount = 2;
            }
            else if (rescueType == EWaypointTaskType.Rescue3)
            {
                injuredCount = 3;
            }
            else
            {
                Assert.IsTrue(false);
            }
            RescueType = rescueType;

            //start injury
            HasInjured = true;
            inPlace = false;
            Rescued = false;
            RescueInProgress = false;
            InjuryData.Current = 0f;

            //move injured instance
            var animMan = AnimationManager.Instance;
            var injured = injuredGroup.Instances;
            for (int i = 0; i < injuredCount; i++)
            {
                var injuredInstance = injured[i];
                injuredInstance.FastAnim = false;
                injuredInstance.Speed = animMan.InjuredSpeed;
                injuredInstance.WalkClip = animMan.WalkInjuredClip;
                injuredInstance.WalkAnim();
            }

            this.injured = injuredUnit;
            MoveInjured(injured[0], EWaypointTaskType.Rescue);
            if (injuredCount > 1)
            {
                MoveInjured(injured[1], EWaypointTaskType.Rescue2);
                if (injuredCount > 2)
                {
                    MoveInjured(injured[2], EWaypointTaskType.Rescue3);
                }
            }

            if (instant)
            {
                for (int i = 0; i < injuredCount; i++)
                {
                    injured[i].GoAll();
                }
            }
        }
    }

    public void KillInjured()
    {
        if (HasInjured && !RescueInProgress)
        {
            InjuryData.FullFilled();
        }
    }

    public void MakeFlood(bool fromNeighbour)
    {
#if ALLOW_CHEATS
        if (HudManager.Instance.Invincibility)
        {
            return;
        }
#endif
        if (!Untouchable && !CanPumpWater && !IsFlooding())
        {
            KillInjured();
            //#warning water makes injured
            //if (Parent.IsActive)
            //{
            //    MakeInjured(false, true);
            //}
            if (Fire.Exists)
            {
                RemoveFire();
            }
            Group.MakeFlood(this, fromNeighbour);

            EventManager.Instance.AddDynamicEvent(this, EDynamicEventType.Water);
            SectionDamageNotifierManager.Instance.SetSegmentIssue(this, EIssue.Flood);

            Parent.SegmentDamagedChanged(this);

            if (!loading && Parent.ParentSection == SectionRoomManager.Instance.Hangar)
            {
                AircraftCarrierDeckManager.Instance.CheckSquadronDamage(SquadronDamagePower, this);
            }
        }
    }

    public void FloodNeighboursInGroup()
    {
        if (Group.IsFloodActive())
        {
            foreach (var segment in NeighboursDirectionDictionary.Keys)
            {
                if (segment.Group == Group)
                {
                    segment.MakeFlood(Group.NeighbourFlooding);
                }
            }
        }
    }

    public Door GetLeakedDoors()
    {
        return null;
    }

    public bool IsFlooding()
    {
        return Group.Flooding.Contains(this);
    }

    public bool IsFlooded()
    {
        return IsFlooding() && Group.IsOverflooded();
    }

    public bool CanRescue()
    {
        return HasInjured && inPlace && !Rescued;
    }

    public void StartDC(EWaypointTaskType task)
    {
        if (Dc != null)
        {
            Dc.TryDo(task);
        }
    }

    public void SetShowSelection(bool show)
    {
        Parent.ParentSection.Hover.SetActive(hovered || show);
        SetShowSegmentSelection(show);
    }

    public void SetShowSegmentSelection(bool show)
    {
        if (!blinking)
        {
            Selection.SetActive(hovered || show);
        }
    }

    public void GetInjured(EWaypointTaskType task, out InstanceData injured, out AnimationClip rescueClip)
    {
        int index = 0;
        if (task == EWaypointTaskType.Rescue)
        {
            index = 0;
        }
        else if (task == EWaypointTaskType.Rescue2)
        {
            index = 1;
        }
        else if (task == EWaypointTaskType.Rescue3)
        {
            index = 2;
        }
        else
        {
            Assert.IsTrue(false);
        }
        injured = injuredGroup.Instances[index];
        rescueClip = rescueClips[index];
    }

    public bool HasAnyDamage()
    {
        return Damage.Exists || Fire.Exists || IsFlooding();
    }

    public bool HasAnyDamageWithBroken()
    {
        return HasAnyDamage() || Parent.IsBroken;
    }

    public bool Injured()
    {
        return HasInjured && inPlace && !Rescued;
    }

    public bool HasAnyIssue()
    {
        return HasAnyDamageWithBroken() || Injured();
    }

    public bool HasAnyRepairableIssue()
    {
        return HasAnyDamage() || Injured() || (Parent.IsBroken && !Parent.Irrepairable);
    }

    public bool DcCanEnter()
    {
        return !IsFlooded() && dc == null;
    }

    public void FastAnimInjured()
    {
        foreach (var injuredInstance in injuredGroup.Instances)
        {
            injuredInstance.FastAnim = true;
        }
    }

    public void OnHoverEnter()
    {
        hovered = true;
        SetShowSelection(true);
        SectionRoomManager.Instance.PlayEvent(ESectionUIState.HoverRoom);
        Parent.ShowTooltip(true, this);
        var dcButton = DamageControlManager.Instance.SelectedButton;
        if (dcButton != null)
        {
            CursorV2.Instance.SetCursor(dcButton.IsFlood ? (IsFlooding() && !Group.Flood.Repair) : (Damage.Exists && !Damage.Repair));
        }
    }

    public void OnHoverExit()
    {
        hovered = false;
        SetShowSelection(false);
        Parent.ShowTooltip(false, null);
        CursorV2.Instance.SetCursor(false);
    }

    public void OnHoverStay()
    {

    }

    public float GetHoverStayTime()
    {
        return 0f;
    }

    public void AddOutlineBlinking()
    {
        mesh.material.shader = SectionRoomManager.Instance.BlinkingMaterial.shader;
        blinking = true;
        Selection.SetActive(showIcons);
    }

    public void RemoveOutlineBlinking()
    {
        mesh.material.shader = shader;
        blinking = false;
        Selection.SetActive(hovered);
    }

    public void OnClickStart()
    {
        if (!DamageControlManager.Instance.SetDCButtonAction(this))
        {
            var group = DamageControlManager.Instance.SelectedGroup;
            var sectionRoomMan = SectionRoomManager.Instance;
            if (group == null || DamageControlManager.Instance.AutoDC)
            {
                sectionRoomMan.PlayEvent(ESectionUIState.NoDCClick);
            }
            else
            {
                var voiceSoundsMan = VoiceSoundsManager.Instance;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    group.QueuePath(this, ESectionUIState.DCRoom);
                }
                else if (group.SetPath(this, EWaypointTaskType.Normal, true, true))
                {
                    sectionRoomMan.PlayEvent(ESectionUIState.DCRoom);
                    voiceSoundsMan.PlayPositive(EVoiceType.DC);
                }
                else
                {
                    sectionRoomMan.PlayEvent(ESectionUIState.DCNegative);
                    voiceSoundsMan.PlayNegative(EVoiceType.DC);
                }
                SegmentClicked();
            }
        }
    }

    public void OnClickHold()
    {

    }

    public void OnClickEnd(bool success)
    {

    }

    public float GetClickHoldTime()
    {
        return 2f;
    }

    public void FireIssueFixed()
    {
        IssueFixed();
        SectionRoomManager.Instance.FireIssueFixed();
    }

    public void StopDC(EWaypointTaskType task)
    {
        if (Dc != null && Dc.Job == task)
        {
            Dc.StopJob();
        }
    }

    public void ChangeFrame()
    {
        material.SetColor(Color, new Color32(0xC5, 0x40, 0x38, 255));
    }

    private void OnRepairReachedMax()
    {
        RemoveDamage();
        IssueFixed();
        SectionRoomManager.Instance.FireIssueFixed();
    }

    private void OnDamageSpreadReachedMax()
    {
        DamageControlManager.Instance.FireDamageRandom();
    }

    private void OnFirefightReachedMax()
    {
        RemoveFire();
        IssueFixed();
        SectionRoomManager.Instance.FireIssueFixed();
    }

    private void OnFireSpreadReachedMax()
    {
        if (SectionRoomManager.Instance.BlockedByIslandBuff || SectionRoomManager.Instance.DisableDangers)
        {
            return;
        }
        foreach (var neighbour in FireNeighbours)
        {
            if (!neighbour.Fire.Exists && !neighbour.IsFlooding())
            {
                neighboursSet.Add(neighbour);
            }
        }
        if (neighboursSet.Count > 0)
        {
            RandomUtils.GetRandom(neighboursSet).MakeFire(true);
        }
    }

    private void OnInjuredReachedMax()
    {
        HasInjured = false;
        inPlace = false;
        Rescued = false;
        RescueInProgress = false;

        var worInMan = WorkerInstancesManager.Instance;
        for (int i = 0; i < injuredCount; i++)
        {
            var injuredInstance = injuredGroup.Instances[i];
            if (injuredInstance.CurrentAnim != null)
            {
                worInMan.StopAnim(injuredInstance);
            }
            injuredInstance.SegmentPath.Clear();
            injuredInstance.TeleportToDest(RandomUtils.GetRandom(Parent.Path.ExitsBySegment[this]));
            injuredInstance.ResetAnimTime();
            GPUInstancerAPI.UpdateTransformDataForInstance(WorkerInstancesManager.Instance.GPUICrowdManager, injuredInstance.Ref);
        }
        StopDC(RescueType);

        if (injured != null)
        {
            CrewStatusManager.Instance.AddInjured(injured);
            injured = null;
        }
        EventManager.Instance.RemoveDynamicEvent(this);
        SectionDamageNotifierManager.Instance.SetSegmentIssue(this, EIssue.Injured);
    }

    private void OnViewChanged(ECameraView view)
    {
        showIcons = view == ECameraView.Sections;
        Selection.SetActive(blinking);
    }

    private void UpdateDanger(SpreadableDanger danger, float power, ref float time, ref float oldValue)
    {
        if (danger.Exists)
        {
            oldValue = danger.RepairData.Percent;
            danger.SpreadPower = power;

            var dcMan = DamageControlManager.Instance;
            danger.RepairPower = dcMan.RepairSpeedModifier;
            if (!dcMan.AutoDC)
            {
                danger.RepairPower += Parameters.Instance.ManualDCRepairSpeedMultiplier;
            }
            danger.Update();
            time = 0f;
        }
    }

    private void UpdateDangerVisual(RepairableDanger danger, ref float time, float oldValue, GameObject effect, GameObject icon, Renderer renderer)
    {
        if (effect != null && effect.activeSelf != danger.Exists)
        {
            effect.SetActive(danger.Exists);
        }
        UpdateDangerVisual(danger.Exists, danger.RepairData, ref time, oldValue, icon, renderer);
    }

    private void UpdateDangerVisual(bool exists, PercentageData data, ref float time, float oldValue, GameObject icon, Renderer renderer)
    {
        if (exists)
        {
            icon.SetActive(showIcons);
            time = Mathf.Max(time + Time.deltaTime, 1f);
            Icons.SetFill(renderer, Mathf.Lerp(oldValue, data.Percent, time));
        }
        else
        {
            icon.SetActive(false);
        }
    }


    private void MoveInjured(InstanceData injuredInstance, EWaypointTaskType rescue)
    {
        bool found = false;
        foreach (var waypoint in Parent.Path.ActionsBySegments[this][rescue])
        {
            if (waypoint.Data.InjuredWaypoint)
            {
                found = true;
                var start = injuredInstance.GetNearestWaypoint(waypoint.Trans.position, Parent.Path.ExitsBySegment[this]);
                injuredInstance.TeleportToDest(start);
                injuredInstance.SetNewDestination(Parent.Path, waypoint, false);
                injuredInstance.LookAtDir();
            }
        }
        if (!found)
        {
            Debug.LogError("No preconfigured injured waypoint");
        }
    }

    private void InitInjuredEvent(float maxInjuredTime, bool isLoading)
    {
        var crewStatusMan = CrewStatusManager.Instance;
        var eventMan = EventManager.Instance;

        InjuryData.TemplateMax = maxInjuredTime;

        //notify player of existance of injured
        inPlace = true;
        eventMan.AddDynamicEvent(this, EDynamicEventType.Injured);
        SectionDamageNotifierManager.Instance.SetSegmentIssue(this, EIssue.Injured);
        if (!isLoading)
        {
            InjuryData.Current = 0f;
            injured = crewStatusMan.AddPotentialInjured(this);
        }
        StartDC(RescueType);
    }
}
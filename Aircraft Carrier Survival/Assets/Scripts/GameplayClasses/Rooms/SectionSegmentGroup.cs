using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SectionSegmentGroup : MonoBehaviour, ITickable
{
    public List<SectionSegment> Group;
    public HashSet<SectionSegment> Flooding;

    public FillableDanger Flood;

    private float lastResult = -1f;

    public float CurrentFloodFill
    {
        get
        {
            var result = Mathf.Lerp(Flood.ActiveFill ? oldFloodValue : pumpoutFloodValue, Flood.FillData.Percent, fill);

            if (!Flood.ActiveFill && lastResult < result && result > 0f)
            {
                pumpoutFloodValue = lastResult;
                result = Mathf.Lerp(Flood.ActiveFill ? oldFloodValue : pumpoutFloodValue, Flood.FillData.Percent, fill);
                //Debug.Log($"lastResult: {lastResult} | result: {result}");
            }

            lastResult = result;
            return result;
        }
    }

    public int SpreadTime
    {
        get;
        set;
    }

    public bool ButtonRepair
    {
        get;
        set;
    }

    public bool OriginalFlooded
    {
        get;
        private set;
    }

    public bool NeighbourFlooding
    {
        get;
        private set;
    }

    private SkinnedMeshRenderer floodRenderer;

    private float oldFloodValue;
    private float pumpoutFloodValue;
    private float fill;

    private HashSet<SectionSegment> neighbours;
    private HashSet<SectionSegment> unfloodedNeighbours;

    private float timer;

    private void Awake()
    {
        foreach (var segment in Group)
        {
            if (segment.Group != null)
            {
                Debug.LogError($"{segment.name} is already in another group - {segment.Group.name}", this);
            }
            segment.Group = this;
        }

        Flooding = new HashSet<SectionSegment>();

        floodRenderer = GetComponent<SkinnedMeshRenderer>();
        floodRenderer.enabled = false;
    }

    private void Start()
    {
        Flood = new FillableDanger(true);

        var dcMan = DamageControlManager.Instance;
        var paramsMan = Parameters.Instance;

        SpreadTime = paramsMan.FloodSpreadTickTime;
        Flood.RepairData.TemplateMax = paramsMan.DefloodFullTickTime;
        Flood.EventData.TemplateMax = dcMan.FloodStaticEventTime;
        Flood.FillData.TemplateMax = paramsMan.FloodToFullTickTime;
        Flood.RepairPower = 1f;

        Flood.RepairData.ReachedMax += StopFlood;

        if (floodRenderer.sharedMesh == null)
        {
            Debug.LogError("no mesh " + name, this);
        }
        SectionRoomManager.Instance.RegisterGroup(this);

        unfloodedNeighbours = new HashSet<SectionSegment>();
    }

    private void Update()
    {
        if (floodRenderer.sharedMesh == null)
        {
            return;
        }
        if (!Flood.Exists)
        {
            Flood.FillData.Current = 0f;
        }
        fill = Mathf.Min(fill + Time.deltaTime, 1f);
        float value = CurrentFloodFill;
        floodRenderer.SetBlendShapeWeight(0, value * 100f);
        floodRenderer.enabled = !Mathf.Approximately(value, 0f);
    }

    public void Setup()
    {
        neighbours = new HashSet<SectionSegment>();
        foreach (var segment in Group)
        {
            Assert.IsNotNull(segment.Parent, segment.name);
            foreach (var segment2 in segment.FloodNeighbours)
            {
                if (segment2.Group != this)
                {
                    neighbours.Add(segment2);
                }
            }
        }
    }

    public void Tick()
    {
        if (!HudManager.Instance.HasNo(ETutorialMode.DisableDCEvents))
        {
            return;
        }
        fill = 0f;
        pumpoutFloodValue = oldFloodValue;
        oldFloodValue = Flood.FillData.Percent;

        var dcMan = DamageControlManager.Instance;
        var paramsMan = Parameters.Instance;
        if (Flood.Exists)
        {
            Flood.RepairPower = dcMan.RepairSpeedModifier;
            if (!dcMan.AutoDC)
            {
                Flood.RepairPower += paramsMan.ManualDCRepairSpeedMultiplier;
            }
            float dc = ButtonRepair ? 1f : 0f;
            foreach (var segment in Group)
            {
                if (segment.Dc != null && segment.Dc.Job == EWaypointTaskType.Waterpump)
                {
                    dc++;
                }
            }
            Flood.RepairPower *= Mathf.Max(1f, dc);
            bool hasRepair = Flood.Repair;
            bool flood = false;
            bool overflooded = IsOverflooded();
            if (!overflooded && IsFlooded())
            {
                Flood.Repair = true;
                flood = true;
            }

            Flood.Update();
            if (!hasRepair)
            {
                Flood.Repair = false;
            }

            if (flood)
            {
                float percent = Flood.FillData.Percent;
                if (percent < .27f)
                {
                    if (percent < .241f != hasRepair)
                    {
                        Flood.FillData.Percent = .241f;
                    }
                }
            }
            if (overflooded)
            {
                KickDC();
            }
        }
        if (IsFloodActive() && IsFlooded())
        {
            timer += (dcMan.PumpsActive ? paramsMan.FloodSpreadWithPumpsMultiplier : 1f);
            
            if (timer >= SpreadTime && !SectionRoomManager.Instance.DisableDangers)
            {
                unfloodedNeighbours.Clear();
                foreach (var segment in neighbours)
                {
                    if (!segment.IsFlooding())
                    {
                        unfloodedNeighbours.Add(segment);
                    }
                }

                if (unfloodedNeighbours.Count > 0)
                {
                    RandomUtils.GetRandom(unfloodedNeighbours).MakeFlood(true);
                }
                timer = 0f;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    public void LoadFlood(float level)
    {
        if (!Flood.Exists)
        {
            return;
        }
        fill = 0f;
        pumpoutFloodValue = level;
        oldFloodValue = level;

        Flood.FillData.Percent = level;
    }

    public void MakeFlood(SectionSegment segment, bool fromNeighbour)
    {
        if (Flooding.Count == 0)
        {
            NeighbourFlooding = fromNeighbour;
            OriginalFlooded = !NeighbourFlooding;
        }

        if (Flooding.Add(segment))
        {
            var timeMan = TimeManager.Instance;
            if (Flooding.Count == Group.Count)
            {
                Flood.Exists = true;
                Flood.ActiveFill = true;
                timeMan.AddTickable(this);
                fill = 0f;
                foreach (var segment2 in Group)
                {
                    segment2.StartDC(EWaypointTaskType.Waterpump);
                }
            }
            else
            {
                timeMan.Invoke(segment.FloodNeighboursInGroup, 1);
            }
        }
    }

    public bool IsFloodActive()
    {
        return Flooding.Count != 0 && !Flood.Repair;
    }

    public bool IsFlooded()
    {
        return Flood.Exists && Flood.FillData.Percent >= .24f;
    }

    public bool IsOverflooded()
    {
        bool neighboursFlooding = true;
        foreach (var segment in neighbours)
        {
            if (!segment.IsFlooding())
            {
                neighboursFlooding = false;
                break;
            }
        }
        return Flood.Exists && neighboursFlooding;
    }

    public bool IsFullyFlooded()
    {
        return Flood.FillData.Percent == 1f;
    }

    public void StopFlood()
    {
        OriginalFlooded = false;
        NeighbourFlooding = false;
        if (Flood.Exists)
        {
            TimeManager.Instance.RemoveTickable(this);
        }
        Flooding.Clear();
        var eventMan = EventManager.Instance;
        var notifierMan = SectionDamageNotifierManager.Instance;
        foreach (var segment in Group)
        {
            segment.FireIssueFixed();
            segment.Parent.SegmentDamagedChanged(segment);
            eventMan.RemoveDynamicEvent(segment);
            notifierMan.SetSegmentIssue(segment, EIssue.Flood);
        }

        Flood.FillData.Current = 0f;

        Flood.Exists = false;
        Flood.Repair = false;
        oldFloodValue = 0f;
        pumpoutFloodValue = 0f;
        fill = 0f;

        foreach (var segment in Group)
        {
            DamageControlManager.Instance.CheckDCButton(segment, true, false);
            segment.StopDC(EWaypointTaskType.Waterpump);
        }
    }

    private void KickDC()
    {
        foreach (var segment in Group)
        {
            if (segment.Dc != null)
            {
                segment.Dc.Kickout(true);
            }
        }
    }
}

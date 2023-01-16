using System;
using System.Collections.Generic;

public class TriggerData
{
    public List<int> Triggers;
    public Action OnTriggerReached;
    public Action<int> OnAnyTriggerReached;
    public List<int> IdsFired;
    public float RangeSqr;

    public TriggerData(int id, Action onTriggerReached, float rangeSqr)
    {
        Triggers = new List<int>();
        Triggers.Add(id);
        OnTriggerReached = onTriggerReached;
        IdsFired = new List<int>();
        RangeSqr = rangeSqr;
    }

    public TriggerData(List<int> ids, Action<int> onAnyTriggerReached, float rangeSqr)
    {
        Triggers = new List<int>();
        Triggers.AddRange(ids);
        OnAnyTriggerReached = onAnyTriggerReached;
        IdsFired = new List<int>();
        RangeSqr = rangeSqr;
    }
}

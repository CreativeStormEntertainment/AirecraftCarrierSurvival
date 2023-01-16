using UnityEngine;
using System.Collections;

public class TacticalFightUnitEvent : TacticalFightEvent
{
    protected TacticalFightBaseUnit unitForEvent;

    public TacticalFightUnitEvent(int roundsToActivate, TacticalFightBaseUnit unit) : base(roundsToActivate)
    {
        unitForEvent = unit;
    }

    public override void EndEvent()
    {
        base.EndEvent();

    }

    public TacticalFightBaseUnit GetUnitForEvent()
    {
        return unitForEvent;
    }
}

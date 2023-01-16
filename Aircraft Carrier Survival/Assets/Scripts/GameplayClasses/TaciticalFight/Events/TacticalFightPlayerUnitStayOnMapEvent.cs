using UnityEngine;
using System.Collections;

public class TacticalFightPlayerUnitStayOnMapEvent : TacticalFightBlockUnitEvent
{
    public TacticalFightPlayerUnitStayOnMapEvent(int roundsToActivate,TacticalFightPlayerUnit unit) : base(roundsToActivate, unit)
    {

    }

    public override void EndEvent()
    {
        base.EndEvent();
        ((TacticalFightPlayerUnit)unitForEvent).TakeFromMap();
    }
}

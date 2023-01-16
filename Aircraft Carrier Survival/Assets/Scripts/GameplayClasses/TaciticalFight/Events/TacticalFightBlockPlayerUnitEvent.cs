using UnityEngine;
using System.Collections;

public class TacticalFightBlockPlayerUnitEvent : TacticalFightUnitEvent
{
    public TacticalFightBlockPlayerUnitEvent(int roundsToBlockAmount, TacticalFightBaseUnit unitToBlock): base(roundsToBlockAmount, unitToBlock)
    {
        unitToBlock.BlockUnit();
    }

    public override void EndEvent()
    {
        base.EndEvent();
        unitForEvent.UnblockUnit();
    }
}

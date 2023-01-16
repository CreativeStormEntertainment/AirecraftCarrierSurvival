using UnityEngine;
using System.Collections;

public class TacticalFightBlockUnitEvent : TacticalFightUnitEvent
{
    public TacticalFightBlockUnitEvent(int roundsToBlockAmount, TacticalFightBaseUnit unitToBlock): base(roundsToBlockAmount, unitToBlock)
    {
        unitToBlock.BlockUnit(); 
        if (unitForEvent is TacticalFightPlayerUnit)
            TacticalFightHudManager.Instance.SetRoundCounterForPlaneButton(((TacticalFightPlayerUnit)unitForEvent).GetPilotPoolIndex(), currentroundsToActivate -1);
    }

    public override void DecreaseRoundsToActivate()
    {
        base.DecreaseRoundsToActivate();

        if (unitForEvent is TacticalFightPlayerUnit)
            TacticalFightHudManager.Instance.SetRoundCounterForPlaneButton(((TacticalFightPlayerUnit)unitForEvent).GetPilotPoolIndex(), currentroundsToActivate );
    }

    public override void EndEvent()
    {
        base.EndEvent();
        unitForEvent.UnblockUnit();
    }
}

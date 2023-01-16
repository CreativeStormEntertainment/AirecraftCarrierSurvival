using UnityEngine;
using System.Collections;

public class TacticalFightEnemyRetreatEvent : TacticalFightEvent
{
    public TacticalFightEnemyRetreatEvent(int roundsToActivate) : base(roundsToActivate)
    {

    }

    public override void EndEvent()
    {
        base.EndEvent();
        TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.Lose);
    }
}

using UnityEngine;
using System.Collections;

public class TacticalFightBadWeatherEvent : TacticalFightEvent
{
    public TacticalFightBadWeatherEvent(int roundsToActivate) : base(roundsToActivate)
    {

    }

    public override void EndEvent()
    {
        base.EndEvent();
        TacticalFightHudManager.Instance.ShowEndGamePanel(false, "The bad weather has came. The fight was no longer possible to be going. Your units retreated.");
        TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.Lose);
    }
}

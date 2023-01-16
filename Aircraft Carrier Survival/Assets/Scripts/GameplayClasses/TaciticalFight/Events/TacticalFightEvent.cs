using UnityEngine;
using System.Collections;

[System.Serializable]
public class TacticalFightEvent
{
    protected int currentroundsToActivate;

    public TacticalFightEvent(int roundsToEnd)
    {
        currentroundsToActivate = roundsToEnd + 1;
    }

    public virtual void EndEvent()
    {

    }

    public virtual void DecreaseRoundsToActivate()
    {
        currentroundsToActivate--;
    }

    public int GetCurrentRoundsToActivate()
    {
        return currentroundsToActivate;
    }
}

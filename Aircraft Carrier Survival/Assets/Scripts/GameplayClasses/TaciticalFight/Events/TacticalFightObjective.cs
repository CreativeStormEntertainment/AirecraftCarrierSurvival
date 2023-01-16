using UnityEngine;
using System.Collections;

public class TacticalFightObjective
{
    protected bool isObjectiveFailed;

    public TacticalFightObjective()
    {

    }

    public virtual bool CheckIsObjectiveComplete()
    {
        return false;
    }

    public virtual void OnObjectiveComplete()
    {
        TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.Victory);
    }

    public virtual void OnObjectiveNotCompleted()
    {

    }

    public bool GetIsObjectiveFailed()
    {
        return isObjectiveFailed;
    }
}

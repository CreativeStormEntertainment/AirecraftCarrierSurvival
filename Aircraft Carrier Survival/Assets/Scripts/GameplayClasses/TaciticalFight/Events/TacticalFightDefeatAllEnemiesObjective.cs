using UnityEngine;
using System.Collections;

public class TacticalFightDefeatAllEnemiesObjective : TacticalFightObjective
{
    public override bool CheckIsObjectiveComplete()
    {
        if (TacticalFightManager.Instance.GetEnemyListOnMap().Count > 0)
            return false;
        else
            return true;
    }
}

using UnityEngine;
using System.Collections;
using System.Linq;

public class TacticalFightSaveTankerObjective : TacticalFightObjective
{
    public override bool CheckIsObjectiveComplete()
    {
        if (TacticalFightManager.Instance.GetEnemyListOnMap().FirstOrDefault(x => x.GetIsNeutral()) != null)
        {
            if (TacticalFightManager.Instance.GetEnemyListOnMap().FirstOrDefault(x => x.GetIsNeutral()).GetCurrentHealth() <= 0)
                isObjectiveFailed = true;

            bool isPossibleToMove = TacticalFightManager.Instance.GetIsPossibleToMoveByUnit(TacticalFightManager.Instance.GetEnemyListOnMap().FirstOrDefault(x => x.GetIsNeutral()));
            if (isPossibleToMove)
                return false;
            else
                return true;
        }
        else
            return false;
    }

    public override void OnObjectiveNotCompleted()
    {
        TacticalFightHudManager.Instance.ShowEndGamePanel(false, "Enemy units destroyed the tanker.");
        TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.Lose);
    }

    public override void OnObjectiveComplete()
    {
        TacticalFightHudManager.Instance.ShowEndGamePanel(true, "Tanker has reached the encounter map edge.");
        base.OnObjectiveComplete();
    }
}

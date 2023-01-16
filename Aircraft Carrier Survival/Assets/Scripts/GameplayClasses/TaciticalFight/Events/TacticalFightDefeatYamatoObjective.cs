using UnityEngine;
using System.Collections;
using System.Linq;

public class TacticalFightDefeatYamatoObjective : TacticalFightObjective
{
    public override bool CheckIsObjectiveComplete()
    {
        if (TacticalFightManager.Instance.GetEnemyListOnMap().FirstOrDefault(x => x.GetEnemyType() == ETacticalFightEnemyType.Yamato) != null)
        {
            if (!TacticalFightManager.Instance.GetIsPossibleToMoveByUnit(TacticalFightManager.Instance.GetEnemyListOnMap().FirstOrDefault(x => x.GetEnemyType() == ETacticalFightEnemyType.Yamato)))
                isObjectiveFailed = true;

            return false;
        }
        else
            return true;
    }

    public override void OnObjectiveNotCompleted()
    {
        TacticalFightHudManager.Instance.ShowEndGamePanel(false, "Yamato has reachead the edge of the encounter map.");
        TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.Lose);
    }

    public override void OnObjectiveComplete()
    {
        TacticalFightHudManager.Instance.ShowEndGamePanel(true, "You have defeated yamato.");
        base.OnObjectiveComplete();
    }
}

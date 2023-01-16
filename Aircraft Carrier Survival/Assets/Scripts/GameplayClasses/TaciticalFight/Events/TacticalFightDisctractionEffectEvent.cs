using UnityEngine;
using System.Collections;

public class TacticalFightDisctractionEffectEvent : TacticalFightBlockUnitEvent
{
    int attackRangeBeforeEvent;

    public TacticalFightDisctractionEffectEvent(int roundsToBlock, TacticalFightEnemyUnit enemyToBlock) : base(roundsToBlock, enemyToBlock)
    {
        attackRangeBeforeEvent = enemyToBlock.GetAttackRange();
        enemyToBlock.SetAttackRange(0);
        enemyToBlock.UnSetVisualizationForAttack();
        enemyToBlock.SetVisualizationForAttack();
        enemyToBlock.SetCanBeAttackedByPlayer(false);
    }

    public override void EndEvent()
    {
        base.EndEvent();
        unitForEvent.SetAttackRange(attackRangeBeforeEvent);
        unitForEvent.UnSetVisualizationForAttack();
        unitForEvent.SetVisualizationForAttack();
        ((TacticalFightEnemyUnit)unitForEvent).SetCanBeAttackedByPlayer(true);
    }
}

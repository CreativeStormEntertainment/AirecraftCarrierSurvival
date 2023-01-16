using System;

[Serializable]
public class EnemySubmarineAttackTimer : EnemyAttackTimer<EnemySubmarineAttackData>
{
    public override void Do(EnemySubmarineAttackData data, int enemyID, bool detected, bool inRange)
    {
        EnemyAttacksManager.Instance.CreateEnemyAttack(new EnemyAttackData(EEnemyAttackType.Submarine), enemyID, detected, inRange);
    }
}

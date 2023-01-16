using System;

[Serializable]
public class EnemyReconAttackTimer : EnemyAttackTimer<EnemyAttacksReconData>
{
    public override void Do(EnemyAttacksReconData data, int enemyID, bool detected, bool inRange)
    {
        EnemyAttacksManager.Instance.CreateEnemyAttack(new EnemyAttackData(EEnemyAttackType.Scout), enemyID, detected, inRange);
    }
}

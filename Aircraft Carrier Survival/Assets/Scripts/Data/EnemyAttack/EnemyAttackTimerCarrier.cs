using System;

[Serializable]
public class EnemyAttackTimerCarrier : EnemyAttackTimer<EnemyAttackData>
{
    public override void Setup()
    {
        base.Setup();
        foreach (var data in datas)
        {
            data.CurrentTarget = data.Target;
        }
    }

    public override void Do(EnemyAttackData data, int enemyID, bool detected, bool inRange)
    {
        data.Type = EEnemyAttackType.Raid;
        if (data.Kamikaze)
        {
            data.CurrentTarget = data.Target = EEnemyAttackTarget.Carrier;
        }
        EnemyAttacksManager.Instance.CreateEnemyAttack(data, enemyID, detected, inRange);
    }
}

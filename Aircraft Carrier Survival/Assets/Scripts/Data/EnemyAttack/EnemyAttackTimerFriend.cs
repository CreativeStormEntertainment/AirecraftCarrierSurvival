using System;

[Serializable]
public class EnemyAttackTimerFriend : EnemyAttackTimer<EnemyAttackFriendData>
{
    public override void Do(EnemyAttackFriendData data, int enemyID, bool detected, bool inRange)
    {
        EnemyAttacksManager.Instance.CreateFriendlyAttack(data, enemyID);
    }
}

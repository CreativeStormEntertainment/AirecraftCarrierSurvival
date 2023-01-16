using System;
using System.Collections.Generic;

[Serializable]
public class EnemyAttackOnUs24h
{
    public List<EnemyAttackTimerCarrier> EnemyAttacks;

    public EnemyAttackOnUs24h()
    {
        EnemyAttacks = new List<EnemyAttackTimerCarrier>();
    }
}

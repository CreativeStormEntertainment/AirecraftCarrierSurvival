using System;
using System.Collections.Generic;

[Serializable]
public class EnemyAttackFriend24h
{
    public List<EnemyAttackTimerFriend> EnemyAttacks;

    public EnemyAttackFriend24h()
    {
        EnemyAttacks = new List<EnemyAttackTimerFriend>();
    }
}

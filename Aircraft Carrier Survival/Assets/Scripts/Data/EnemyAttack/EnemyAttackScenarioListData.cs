using System;
using System.Collections.Generic;

[Serializable]
public class EnemyAttackScenarioListData
{
    public List<EnemyAttackOnUs24h> EnemiesAttackOnUs;
    public List<EnemyAttackFriend24h> EnemiesAttacksOnFriends;
    public List<EnemySubmarineAttackTimer> EnemiesSubmarinesAttacks;
    public List<EnemyReconAttackTimer> EnemiesReconsAttacks;
}

using UnityEngine;

public class RadarEnemyData
{
    public EnemyAttackData Data;
    public RadarEnemy Enemy;
    public float VisualTimer;
    public float TickTimer;
    public float Speed;

    public RadarEnemyData(EnemyAttackData data, int tickTimer)
    {
        Data = data;
        TickTimer = tickTimer;
    }
}

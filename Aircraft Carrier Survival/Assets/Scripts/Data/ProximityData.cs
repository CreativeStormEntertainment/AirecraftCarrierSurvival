using System;

public struct ProximityData
{
    public TacticalEnemyShip Enemy;
    public float Proximity;
    public Action<bool> Callback;

    public ProximityData(TacticalEnemyShip enemy, float proximity, Action<bool> callback)
    {
        Enemy = enemy;
        Proximity = proximity;
        Callback = callback;
    }
}

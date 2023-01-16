using System;

[Serializable]
public struct DamageRangeSaveData
{
    public bool Active;

    public int Power;
    public int TicksToFire;
    public int Timer;
    public float Range;

    public int EnemyID;
}

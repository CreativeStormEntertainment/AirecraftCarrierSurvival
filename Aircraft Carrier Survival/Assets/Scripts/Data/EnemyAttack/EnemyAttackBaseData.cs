using System;

[Serializable]
public class EnemyAttackBaseData
{
    public int DelayInTicks
    {
        get;
        set;
    }

    public int Hour;
    public int Minute;
    public int AttackPower;
    [NonSerialized]
    public int CalculatedAttackPower;
}

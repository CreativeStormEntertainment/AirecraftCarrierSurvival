using System;
using System.Collections.Generic;

[Serializable]
public struct AttackSaveData
{
    public EEnemyAttackTarget Target;
    public MyVector2 Direction;
    public double Time;
    public int Power;
    public List<float> AATime;
    public int Anim;
    public double AlreadyAttackedTime;
    public int RadarTicks;
    public bool Kamikaze;

    public AttackSaveData Duplicate()
    {
        var result = this;

        if (AATime != null)
        {
            result.AATime = new List<float>(AATime);
        }

        return result;
    }
}

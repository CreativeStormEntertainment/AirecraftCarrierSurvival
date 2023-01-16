using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyAttackData : EnemyAttackBaseData
{
    [NonSerialized]
    public Vector2 Direction;
    [NonSerialized]
    public Vector2 StartPosition;

    public bool Kamikaze;

    [NonSerialized]
    public EEnemyAttackType Type;
    public EEnemyAttackTarget Target;
    [NonSerialized]
    public EEnemyAttackTarget CurrentTarget;

    [NonSerialized]
    public AttackAnimCameraData AnimData;

    [HideInInspector]
    public List<float> AATime;
    [NonSerialized]
    public double DirectorTimeAttack = 0f;

    public EnemyAttackData()
    {

    }

    public EnemyAttackData(EEnemyAttackType type)
    {
        Type = type;
    }

    public EnemyAttackData(AttackSaveData data)
    {
        Direction = data.Direction;
        CalculatedAttackPower = data.Power;
        CurrentTarget = Target = data.Target;
        Kamikaze = data.Kamikaze;
    }

    public void SaveData(ref AttackSaveData data)
    {
        data.Direction = Direction;
        data.Power = CalculatedAttackPower;
        data.Target = CurrentTarget;

        data.Time = AnimData.Director.time;
        data.Anim = AnimData.AnimIndex;

        data.AlreadyAttackedTime = DirectorTimeAttack;

        data.Kamikaze = Kamikaze;
    }
}

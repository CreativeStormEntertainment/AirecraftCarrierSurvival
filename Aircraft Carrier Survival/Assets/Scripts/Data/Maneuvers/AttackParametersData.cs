using System;
using UnityEngine;

[Serializable]
public struct AttackParametersData
{
    public float Attack;
    public float Defense;
    [HideInInspector]
    public int Durability;

    public static AttackParametersData operator+ (AttackParametersData a, AttackParametersData b)
    {
        a.Attack += b.Attack;
        a.Defense += b.Defense;
        return a;
    }

    public AttackParametersData(float attack, float defense)
    {
        Attack = attack;
        Defense = defense;
        Durability = 1;
    }
}

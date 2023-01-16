using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Datas/ParametersDifficulty", order = 1)]
public class ParametersDifficulty : ScriptableObject
{
    public int FireSpreadTicks;
    public int FireExtinguishingTicks;
    public int FloodToFullTicks;
    public int FloodSpreadTicks;
    public int FaultSpreadTicks;
    public int FaultFixTicks;
    public int DestroyedFixTicks;

    public int MinDeathHours;
    public int MaxDeathHours;

    public float FaultSpreadMaintenanceMultiplier;

    public bool QuickerAttacks;

    public float SquadronBreakChance;

    public int EnemyAirstrikePowerModifier;

    public bool IgnoreGlobalAttacksModifier;

    public int EnemyBlocksAttackModifier;
    public int EnemyBlocksDefenseModifier;
}

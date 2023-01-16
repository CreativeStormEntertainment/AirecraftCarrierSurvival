using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Parameters : MonoBehaviour
{
    public static Parameters Instance;

    public int FaultSpawnRateCount = 15;
    public int FireSpawnRateCount = 15;
    public int FloodSpawnRateCount = 15;
    public int InjuredSpawnRateCount = 15;

    public float InjuredSpawnRateDisabledCrewQuartersMultiplier = 1.5f;

    public float SpontaneousDamageHours = 1f;
    public int FireExtinguishTimeTicks = 12;
    public int FireSpreadTimeTicks = 12;
    public int FloodToFullTickTime = 80;
    public int DefloodFullTickTime = 10;
    public float PumpsDefloodingMultiplier = .1f;
    public int FloodSpreadTickTime = 10;
    public float FloodSpreadWithPumpsMultiplier = .1f;
    public int FaultSpreadTickTime = 180;
    public float FaultSpreadWithMaintenanceMultiplier = .1f;
    public int FaultFixTickTime = 12;
    public int DestroyedFixTickTime = 20;
    public int MinTimeToRescueBeforeInjured = 120;
    public int MaxTimeToRescueBeforeInjured = 300;
    public float MinTimeToDeathHours = 6f;
    public float MaxTimeToDeathHours = 12f;
    public float DCWalkSpeedMetersPerSecond = 6f;
    public int DCButtonFixTickTime = 12;
    [FormerlySerializedAs("FaulSpontaneousSpreadWithMaintenance")]
    public float FaultSpontaneousSpreadWithMaintenance = .5f;
    public float FloodSpontaneousSpreadWithPumps = 1f;

    public float ManualDCRepairSpeedMultiplier = 1.2f;

    public float BalancedForcesParameterX = .05f;
    public float BalancedForcesMissedBonus = .1f;

    public int MinHoursToFailSurvivor = 5;
    public int MaxHoursToFailSurvivor = 50;
    public float PlayerDetectionRange = 100f;
    public float EscortCarrierRetrievalSpeed = 0.8f;

    public int AttackHinderedMinPlanes = 5;
    public int AttackHinderedHours = 3;
    public int AttackCooldownHours = 24;

    public float IdentifyUORangeModifier = 0.5f;

    public float DetectSubmarineRange = 150f;

    [Header("IslandBuffs")]
    public int AddDcBuff = 2;
    public int TacticalMissionFocusExpireTime = 6;
    public float IncreaseSpottingRange = 1.5f;
    public int EnterDefencePositionCarrier = 2;
    public int EnterDefencePositionEscort = 1;
    public int HeavyManeuversDefence = 4;
    public int SurpriseAttackManeuversAttack = 3;
    public int CounterAttackExpireTime = 2;
    public int DecisiveBlowManeuversDefence = 5;
    public int PlaneFuelManagementRetrievalRange = 50;
    public float PlaneFuelManagementRetrievalTimeModifier = 1.5f;
    public float NightShiftDcSpeedModifier = 0.25f;
    public float HastenedRepairsDcSpeedModifier = 0.5f;
    public float HastenedRepairsInjuryChanceModifier = 0.2f;

    public int OfficerCooldownTicks = 360;

    [NonSerialized]
    public int RangeTimeToRescueBeforeInjured;
    [NonSerialized]
    public float RangeTimeTimeToDeathHours;
    [NonSerialized]
    public float DetectSubmarineRangeSqr;
    [NonSerialized]
    public ParametersDifficulty DifficultyParams;

    [SerializeField]
    private List<ParametersDifficulty> difficulties = null;

    private void Awake()
    {
        Instance = this;

        DifficultyParams = difficulties[(int)SaveManager.Instance.Data.Difficulty];

        FireSpreadTimeTicks = DifficultyParams.FireSpreadTicks;
        FireExtinguishTimeTicks = DifficultyParams.FireExtinguishingTicks;
        FloodToFullTickTime = DifficultyParams.FloodToFullTicks;
        FloodSpreadTickTime = DifficultyParams.FloodSpreadTicks;
        FaultSpreadTickTime = DifficultyParams.FaultSpreadTicks;
        FaultFixTickTime = DifficultyParams.FaultFixTicks;
        DestroyedFixTickTime = DifficultyParams.DestroyedFixTicks;
        MinTimeToDeathHours = DifficultyParams.MinDeathHours;
        MaxTimeToDeathHours = DifficultyParams.MaxDeathHours;
        FaultSpontaneousSpreadWithMaintenance = DifficultyParams.FaultSpreadMaintenanceMultiplier;
        
        RangeTimeToRescueBeforeInjured = MaxTimeToRescueBeforeInjured - MinTimeToRescueBeforeInjured;
        RangeTimeTimeToDeathHours = MaxTimeToDeathHours - MinTimeToDeathHours;
        DetectSubmarineRangeSqr = DetectSubmarineRange * DetectSubmarineRange;
    }
}

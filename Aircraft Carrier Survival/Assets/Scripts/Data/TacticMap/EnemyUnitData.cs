using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class EnemyUnitData
{
    [HideInInspector]
    public string Name = "Enemy";
    [HideInInspector]
    public Vector2 Position;
    public string NameID;
    [Header("Will be land enemy if there is no patrols")]
    public List<PatrolData> Patrols;

    [NonSerialized]
    public int Taken = -1;
    [NonSerialized]
    public List<int> RandomNodes;

    public float MaxOffset;
    [Header("Cannot attack if AttackRange == 0")]
    public float AttackRange;
    public List<EnemyManeuverData> BuildingBlocks;
    public List<int> InstantDeadBlocks;

    public float RevealRange;

    public float Speed;
    public bool IsAlly;

    public bool CanChase = true;
    public bool CanRetreat;

    public bool IsHidden;
    public bool IsDisabled;
    public bool IsDetectableMissionOnly;

    public float DetectRange;

    public float OverrideChanceToReveal = -1;

    public int SaveIndex;

    public bool AlternativePathfinding;

    public bool NotTargetable;

    [Header("Only for enemy, if received cap no damage from airstrike")]
    public bool CanReceiveCAP;

    public EnemyUnitData Duplicate()
    {
        var result = new EnemyUnitData();

        result.Name = Name;
        result.Position = Position;
        result.NameID = NameID;
        result.MaxOffset = MaxOffset;
        result.AttackRange = AttackRange;
        result.RevealRange = RevealRange;
        result.Speed = Speed;
        result.IsAlly = IsAlly;
        result.CanChase = CanChase;
        result.CanRetreat = CanRetreat;
        result.IsHidden = IsHidden;
        result.IsDisabled = IsDisabled;
        result.IsDetectableMissionOnly = IsDetectableMissionOnly;
        result.DetectRange = DetectRange;
        result.OverrideChanceToReveal = OverrideChanceToReveal;
        result.AlternativePathfinding = AlternativePathfinding;
        result.CanReceiveCAP = CanReceiveCAP;

        result.SaveIndex = SaveIndex;

        result.Patrols = new List<PatrolData>();
        Assert.IsTrue(Patrols.Count == 0);

        result.BuildingBlocks = new List<EnemyManeuverData>(BuildingBlocks);
        result.InstantDeadBlocks = new List<int>(InstantDeadBlocks);

        return result;
    }
}

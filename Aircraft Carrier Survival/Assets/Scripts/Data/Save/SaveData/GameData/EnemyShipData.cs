using System;
using System.Collections.Generic;

[Serializable]
public struct EnemyShipData
{
    public TacticalObjectData ObjectData;

    public bool Dead;
    public List<EnemyBlockSaveData> BlocksData;

    public bool CanChase;
    public bool Disabled;
    public bool Invisible;
    public bool GreaterInvisibility;
    public bool IsRetreating;
    public bool UpdateRealtime;

    public bool SearchAndDestroy;
    public int AttackTimer;
    public int TargetID;

    public bool FinishedPatrol;
    public int CurrentPatrol;
    public int PatrolNodeIndex;

    public bool NotByPlayer;

    public int PowerfulTicks;
    public int CooldownTicks;

    public bool Reconed;
    public bool Spotted;

    public bool Special;

    public int StartNode;
    public List<int> RandomNodes;

    public bool HasDistract;
    public MyVector2 DistractPosition;

    public RandomData AttackChanceData;

    public bool FromChase;

    public EnemyShipData Duplicate()
    {
        var result = this;
        result.BlocksData = new List<EnemyBlockSaveData>(BlocksData);
        if (RandomNodes != null)
        {
            result.RandomNodes = new List<int>(RandomNodes);
        }
        return result;
    }
}

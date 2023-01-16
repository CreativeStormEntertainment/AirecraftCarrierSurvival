using System;
using System.Collections.Generic;

[Serializable]
public struct TacticalMapSaveData
{
    public ShipSaveData Ship;

    //enemy position + status
    public List<EnemyShipData> EnemyShips;
    //UO position + status
    public List<TacticalObjectData> NeutralShips;
    public List<CustomTacticalObjectSaveData> CustomShips;

    public List<SurvivorData> Survivors;

    public int LostBombers;
    public int LostFighters;
    public int LostTorpedoes;
    public int ConsequenceManeuversAttack;

    public List<int> EnemiesRanges;

    public bool MagicIdentifyPermanentRemove;

    public int DestroyedBlocks;

    public int CurrentBonusBuff;

    public TacticalMapSaveData Duplicate()
    {
        var result = this;
        result.Ship = Ship.Duplicate();

        result.NeutralShips = new List<TacticalObjectData>(NeutralShips);
        if (CustomShips == null)
        {
            result.CustomShips = new List<CustomTacticalObjectSaveData>();
        }
        else
        {
            result.CustomShips = new List<CustomTacticalObjectSaveData>(CustomShips);
        }

        result.EnemyShips = new List<EnemyShipData>();
        for (int i = 0; i < EnemyShips.Count; i++)
        {
            result.EnemyShips.Add(EnemyShips[i].Duplicate());
        }

        result.Survivors = new List<SurvivorData>(Survivors);

        if (EnemiesRanges == null)
        {
            result.EnemiesRanges = new List<int>();
        }
        else
        {
            result.EnemiesRanges = new List<int>(EnemiesRanges);
        }

        return result;
    }
}

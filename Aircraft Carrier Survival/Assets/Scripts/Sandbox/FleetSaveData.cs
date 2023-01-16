using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct FleetSaveData
{
    public MyVector2 Position;
    public MyVector2 Destination;
    public EWorldMapFleetType FleetType;
    public EMissionDifficulty Difficulty;
    public int TicksToChangeSpeed;
    public int BuildingBlocks;
    public string ShipName;

    public FleetSaveData Duplicate()
    {
        var result = new FleetSaveData();
        result.Position = Position;
        result.Destination = Destination;
        result.FleetType = FleetType;
        result.FleetType = FleetType;
        result.Difficulty = Difficulty;
        result.TicksToChangeSpeed = TicksToChangeSpeed;
        result.BuildingBlocks = BuildingBlocks;
        result.ShipName = ShipName;
        return result;
    }
}

using System;
using System.Collections.Generic;

[Serializable]
public struct WorldMapSaveData
{
    public MyVector2 ShipPosition;
    public List<MyVector2> Waypoints;

    public WorldMapSaveData Duplicate()
    {
        var result = this;

        result.Waypoints = new List<MyVector2>(Waypoints);

        return result;
    }
}

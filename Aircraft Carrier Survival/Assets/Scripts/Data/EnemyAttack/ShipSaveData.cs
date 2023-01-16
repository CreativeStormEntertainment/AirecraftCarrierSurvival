using System;
using System.Collections.Generic;

[Serializable]
public struct ShipSaveData
{
    public int ShipSpeed;
    public MyVector2 ShipPosition;
    //ship waypoints
    public List<MyVector2> Waypoints;

    public bool HasAny;
    public int ForcedSpeed;
    public int MinSpeed;
    public int MaxSpeed;

    public ShipSaveData Duplicate()
    {
        var result = this;

        result.Waypoints = new List<MyVector2>(Waypoints);

        return result;
    }
}

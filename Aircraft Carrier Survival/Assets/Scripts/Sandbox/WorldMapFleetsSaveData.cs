using System;
using System.Collections.Generic;

[Serializable]
public struct WorldMapFleetsSaveData
{
    public List<FleetSaveData> Fleets;
    public int TicksToSpawnFleet;
    public int InstanceFinishedTimer;

    public WorldMapFleetsSaveData Duplicate()
    {
        var result = this;
        result.Fleets = new List<FleetSaveData>();
        if (Fleets != null)
        {
            foreach (var fleet in Fleets)
            {
                result.Fleets.Add(fleet.Duplicate());
            }
        }
        return result;
    }
}

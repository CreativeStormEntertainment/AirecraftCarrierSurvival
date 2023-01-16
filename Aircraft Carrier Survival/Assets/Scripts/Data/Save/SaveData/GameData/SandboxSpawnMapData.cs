using System;
using System.Collections.Generic;

[Serializable]
public struct SandboxSpawnMapData
{
    public List<string> Names;
    public List<MyVector2> Positions;
    public List<ListInt> Routes;
    public List<int> Blocks;
    public List<int> Custom;
    public int ObjectiveFleets;
    public int AdditionalEnemies;

    public List<int> CloudsBucket;
    public int CloudsPrefab;
    public MyVector2 CloudsDirection;

    public List<ListInt> FleetBucket;
    public List<ListInt> OneshotFleetBucket;
    public List<ListInt> OutpostBucket;

    public List<int> EnemyFleetNamesBucket;
    public List<int> EnemyOutpostNamesBucket;
    public List<int> FriendlyFleetNamesBucket;
    public List<int> FriendlyOutpostNamesBucket;

    public MapSpawnData MapSpawnData;

    public SandboxSpawnMapData Duplicate()
    {
        var result = this;
        if (Names == null)
        {
            return result;
        }

        result.Names = new List<string>(Names);
        result.Positions = new List<MyVector2>(Positions);
        result.Routes = new List<ListInt>();
        foreach (var route in Routes)
        {
            result.Routes.Add(new ListInt(route.List));
        }
        result.Blocks = new List<int>(Blocks);
        result.Custom = new List<int>(Custom);
        result.CloudsBucket = new List<int>(CloudsBucket);

        result.EnemyFleetNamesBucket = new List<int>(EnemyFleetNamesBucket);
        result.EnemyOutpostNamesBucket = new List<int>(EnemyOutpostNamesBucket);
        result.FriendlyFleetNamesBucket = new List<int>(FriendlyFleetNamesBucket);
        result.FriendlyOutpostNamesBucket = new List<int>(FriendlyOutpostNamesBucket);

        result.FleetBucket = new List<ListInt>();
        foreach (var fleet in FleetBucket)
        {
            result.FleetBucket.Add(new ListInt(fleet.List));
        }
        result.OneshotFleetBucket = new List<ListInt>();
        foreach (var fleet in OneshotFleetBucket)
        {
            result.OneshotFleetBucket.Add(new ListInt(fleet.List));
        }
        result.OutpostBucket = new List<ListInt>();
        foreach (var outpost in OutpostBucket)
        {
            result.OutpostBucket.Add(new ListInt(outpost.List));
        }
        if (MapSpawnData != null)
        {
            result.MapSpawnData = MapSpawnData.Duplicate();
        }
        return result;
    }
}

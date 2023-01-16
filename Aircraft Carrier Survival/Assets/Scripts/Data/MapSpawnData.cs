using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapSpawnData
{
    public ESandboxObjectiveType Type;
    public List<MyVector2> Nodes;
    public List<MyVector2> Outposts;
    public List<MyVector2> Edges;
    public List<int> FlagList;
    public List<long> LongFlagList;
    public List<int> TripletList;
    public List<int> AdditionalInt;
    public List<int> Undetected;

    public int HelperIndex;
    public int HelperIndex2;

    public MyVector2 PlayerPos;

    public EMissionDifficulty Difficulty;
    [NonSerialized]
    public int AdditionalFleets;
    [NonSerialized]
    public EEnemiesCount EnemiesCount;

    public MapSpawnData(ESandboxObjectiveType type, IEnumerable<Vector2> nodes, Vector2 playerPos) : this(type, playerPos)
    {
        foreach (var node in nodes)
        {
            Nodes.Add(node);
        }
    }

    private MapSpawnData(ESandboxObjectiveType type, Vector2 playerPos)
    {
        Type = type;
        Nodes = new List<MyVector2>();
        PlayerPos = playerPos;
    }

    private MapSpawnData(ESandboxObjectiveType type, IEnumerable<MyVector2> nodes, Vector2 playerPos) : this(type, playerPos)
    {
        Nodes.AddRange(nodes);
    }

    public MapSpawnData Duplicate()
    {
        var result = new MapSpawnData(Type, Nodes, PlayerPos);
        result.Outposts = new List<MyVector2>(Outposts);
        result.Edges = new List<MyVector2>(Edges);
        result.FlagList = new List<int>(FlagList);
        result.LongFlagList = new List<long>(LongFlagList);
        result.TripletList = new List<int>(TripletList);
        result.AdditionalInt = new List<int>(AdditionalInt);
        result.Undetected = new List<int>(Undetected);
        result.HelperIndex = HelperIndex;
        result.HelperIndex2 = HelperIndex2;
        return result;
    }
}

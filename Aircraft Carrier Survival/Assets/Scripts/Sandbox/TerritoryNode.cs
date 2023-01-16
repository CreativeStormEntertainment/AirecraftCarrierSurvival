using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerritoryNode
{
    public Vector2 Position;
    public List<int> Neighbours;
    public ETerritoryType TerritoryType;
    public ESectorType Sector;

    public bool IsFrontSector
    {
        get
        {
            foreach (var neighbour in Neighbours)
            {
                if (WorldMap.Instance.TerritoryNodes[neighbour].TerritoryType == oppositeTerritoryType)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private ETerritoryType oppositeTerritoryType;

    public TerritoryNode(Vector2 position)
    {
        Position = position;
    }

    public void SetTerritoryType(ETerritoryType type)
    {
        TerritoryType = type;
        oppositeTerritoryType = TerritoryType == ETerritoryType.RedWaters ? ETerritoryType.USA : ETerritoryType.RedWaters;
    }

    public void SetNeighbours(HashSet<int> list)
    {
        Neighbours = new List<int>(list);
    }
}

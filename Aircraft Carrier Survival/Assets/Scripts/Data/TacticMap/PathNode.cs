using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public Vector2 Position
    {
        get;
        private set;
    } = new Vector2();

    public Vector2Int MapSNode = new Vector2Int();
    public bool IsOnLand = false;
    public int GCost = 0;
    public int HCost = 0;
    public int FCost = 0;
    public PathNode CameFromNode = null;

    public List<PathNode> VisitedNodes = new List<PathNode>();
    public float StartNodeDistance;

    public PathNode(Vector2 position, Vector2Int mapSNode)
    {
        Position = position;
        MapSNode = mapSNode;
    }

    public void CalculateFCost()
    {
        FCost = GCost + HCost;
    }

    public List<PathNode> GetNeighbourList(TacticalMapGrid grid)
    {
        List<PathNode> neighboursList = new List<PathNode>();

        if (MapSNode.x - 1 >= 0)
        {
            neighboursList.Add(grid.GetPathNode(MapSNode.x - 1, MapSNode.y));

            if (MapSNode.y - 1 >= 0)
            {
                neighboursList.Add(grid.GetPathNode(MapSNode.x - 1, MapSNode.y - 1));
            }

            if (MapSNode.y + 1 < grid.ResY)
            {
                neighboursList.Add(grid.GetPathNode(MapSNode.x - 1, MapSNode.y + 1));
            }
        }

        if (MapSNode.x + 1 < grid.ResX)
        {
            neighboursList.Add(grid.GetPathNode(MapSNode.x + 1, MapSNode.y));

            if (MapSNode.y - 1 >= 0)
            {
                neighboursList.Add(grid.GetPathNode(MapSNode.x + 1, MapSNode.y - 1));
            }

            if (MapSNode.y + 1 < grid.ResY)
            {
                neighboursList.Add(grid.GetPathNode(MapSNode.x + 1, MapSNode.y + 1));
            }
        }

        if (MapSNode.y - 1 >= 0)
        {
            neighboursList.Add(grid.GetPathNode(MapSNode.x, MapSNode.y - 1));
        }

        if (MapSNode.y + 1 < grid.ResY)
        {
            neighboursList.Add(grid.GetPathNode(MapSNode.x, MapSNode.y + 1));
        }

        return neighboursList;
    }

}

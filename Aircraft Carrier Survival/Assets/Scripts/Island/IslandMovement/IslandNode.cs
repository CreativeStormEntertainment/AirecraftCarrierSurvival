using System.Collections.Generic;
using UnityEngine;

public class IslandNode
{
    public HashSet<IslandNode> ConnectedNodes;
    public Vector3 Position;
    public bool Occupied = false;

    public IslandNode()
    {
        ConnectedNodes = new HashSet<IslandNode>();
    }

    public static List<IslandNode> FindPath(IslandNode startNode, IslandNode endNode)
    {
        List<IslandNode> path = new List<IslandNode>();

        if (startNode != endNode)
        {

            HashSet<IslandNode> visited = new HashSet<IslandNode>();
            if (FindPath(path, visited, startNode, endNode))
            {
                path.Reverse();
            }
            else
            {
                Debug.LogError("No path finded");
            }
        }
        return path;
    }

    private static bool FindPath(List<IslandNode> path, HashSet<IslandNode> visited, IslandNode currentNode, IslandNode endNode)
    {
        if (currentNode == endNode)
        {
            path.Add(currentNode);
            return true;
        }
        else
        {
            visited.Add(currentNode);
            foreach (IslandNode node in currentNode.ConnectedNodes)
            {
                if (visited.Contains(node))
                {
                    continue;
                }
                if (FindPath(path, visited, node, endNode))
                {
                    path.Add(currentNode);
                    return true;
                }
            }
        }
        return false;
    }
}

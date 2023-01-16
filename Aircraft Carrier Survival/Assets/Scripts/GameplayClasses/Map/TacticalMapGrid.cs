using System.Collections.Generic;
using UnityEngine;

public class TacticalMapGrid
{
    public static float OffsetX;
    public static float OffsetY;
    public static float DiagonalOffset;

    public float Width = 0f;
    public float Height = 0f;

    public int ResX = 0;
    public int ResY = 0;

    public float MaskScale = 0;

    public Vector2 Scale;

    public PathNode[,] Nodes = null;
    private Map map = null;
    private Texture2D landMask = null;
    private List<Texture2D> landMasks = null;

    public TacticalMapGrid(Map tMap, float width, float height, int resX, int resY)
    {
        Width = width;
        Height = height;
        ResX = resX;
        ResY = resY;

        Scale = new Vector2(Width / ResX, Height / ResY);

        map = tMap;
        MaskScale = map.LandMask.width / Width;

        GenerateGrid();
    }

    public TacticalMapGrid(Texture2D landMask, float width, float height, int resX, int resY)
    {
        Width = width;
        Height = height;
        ResX = resX;
        ResY = resY;

        Scale = new Vector2(Width / ResX, Height / ResY);

        this.landMask = landMask;
        MaskScale = landMask.width / Width;

        GenerateGrid();
    }

    public TacticalMapGrid(List<Texture2D> landMasks, float width, float height, int resX, int resY)
    {
        Width = width;
        Height = height;
        ResX = resX;
        ResY = resY;

        Scale = new Vector2(Width / ResX, Height / ResY);

        this.landMasks = landMasks;
        MaskScale = landMasks[0].width / Width;
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        OffsetX = Width / ResX;
        OffsetY = Height / ResY;
        DiagonalOffset = Mathf.Sqrt(OffsetX * OffsetX + OffsetY * OffsetY);
        float xPoint = Width / -2f;
        float yPoint = Height / -2f;


        Nodes = new PathNode[ResX, ResY];

        for (int y = 0; y < ResY; y++)
        {
            for (int x = 0; x < ResX; x++)
            {
                Nodes[x, y] = new PathNode(new Vector2(xPoint, yPoint), new Vector2Int(x, y));
                if (map == null)
                {
                    if (landMask != null)
                    {
                        Nodes[x, y].IsOnLand = Map.IsOnLand(Nodes[x, y].Position, landMask, MaskScale);
                    }
                    else
                    {
                        var point = new Vector2(Nodes[x, y].Position.x + (Width / 2f), Nodes[x, y].Position.y + (Height / 2f));
                        Nodes[x, y].IsOnLand = Map.IsOnLand2(TacticalMapCreator.TransformWorldMapPointToBigMapPoint(point, out int mapIndex), landMasks[mapIndex], 1f);
                    }
                }
                else
                {
                    Nodes[x, y].IsOnLand = map.IsOnLand(Nodes[x, y].Position, MaskScale);
                }

                xPoint += OffsetX;
            }
            yPoint += OffsetY;
            xPoint = Width / -2f;
        }
    }

    public void UpdateIsOnLand()
    {
        for (int y = 0; y < ResY; y++)
        {
            for (int x = 0; x < ResX; x++)
            {
                Nodes[x, y].IsOnLand = map.IsOnLand(Nodes[x, y].Position, MaskScale);
            }
        }
    }

    public void UpdateIsOnLand(Vector2 mapOffset)
    {
        for (int y = 0; y < ResY; y++)
        {
            for (int x = 0; x < ResX; x++)
            {
                if (map == null && landMask == null)
                {
                    var point = TacticalMapCreator.TransformTacticMapPointToWorldMapPoint(Nodes[x, y].Position, mapOffset);
                    point = new Vector2(point.x + (Width / 2f), point.y + (Height / 2f));
                    Nodes[x, y].IsOnLand = Map.IsOnLand2(TacticalMapCreator.TransformWorldMapPointToBigMapPoint(point, out int mapIndex), landMasks[mapIndex], 1f);
                }
            }
        }
    }

    public static int CalculateRemainingDistance(Vector2Int a, Vector2Int b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return TacticManager.MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + TacticManager.MOVE_STRAIGHT_COST * remaining;
    }

    public static float CalculateRealDistance(Vector2Int a, Vector2Int b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return DiagonalOffset * Mathf.Min(xDistance, yDistance) + OffsetX * remaining;
    }

    public static PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        //PrintTime("GetLowestFCostNode", true);
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].FCost < lowestFCostNode.FCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        //PrintTime("GetLowestFCostNode", false);
        return lowestFCostNode;
    }

    public PathNode GetPathNode(int x, int y)
    {
        return Nodes[x, y];
    }

    public IEnumerable<PathNode> GetNodes()
    {
        foreach (var node in Nodes)
        {
            yield return node;
        }
    }

    public PathNode GetPathNode(Vector2Int sNode)
    {
        return GetPathNode(sNode.x, sNode.y);
    }

    public bool CanFind(Vector2 position, bool withLand)
    {
        var node = GetNullable(position);
        return node != null && (withLand || !node.IsOnLand);
    }

    public PathNode Find(int nodeIndex)
    {
        return Find(map.Nodes[nodeIndex]);
    }

    public PathNode Find(Vector2 position)
    {
        return GetPathNode(GetNodeInt(position));
    }

    public Vector2Int GetNodeInt(Vector2 position)
    {
        Vector2Int nodePos = new Vector2Int(
            (int)Mathf.Round((position.x + Width / 2f) / Scale.x),
            (int)Mathf.Round((position.y + Height / 2f) / Scale.y)
            );

        nodePos.x = Mathf.Clamp(nodePos.x, 0, ResX - 1);
        nodePos.y = Mathf.Clamp(nodePos.y, 0, ResY - 1);
        return nodePos;
    }

    public static List<PathNode> CalculatePathHelper(PathNode endNode)
    {
        //PrintTime("CalculatePath", true);
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.CameFromNode != null)
        {
            path.Add(currentNode.CameFromNode);
            currentNode = currentNode.CameFromNode;
        }
        path.Reverse();
        //PrintTime("CalculatePath", false);
        return path;
    }

    public PathNode Find(float x, float y)
    {
        return Find(new Vector2(x, y));
    }

    public List<PathNode> FindPath(Vector2 startNodePos, Vector2 endNodePos)
    {
        PathNode startNode = Find(startNodePos);
        PathNode endNode = Find(endNodePos);
        if (endNode.IsOnLand || startNode.IsOnLand)
        {
            Debug.LogError("Node is placed on Land");
            return new List<PathNode>();
        }

        //#QoL, hashSet is better suited if you want to use mainly Contains, 
        //dont use new if you don't have to, why unnecessary garbage?
        List<PathNode> openList = new List<PathNode>();
        openList.Add(startNode);
        HashSet<PathNode> closeList = new HashSet<PathNode>();

        //#QoL
        foreach (var pathNode in GetNodes())
        {
            pathNode.GCost = int.MaxValue;
            pathNode.CalculateFCost();
            pathNode.CameFromNode = null;
        }

        startNode.GCost = 0;
        startNode.HCost = CalculateRemainingDistance(startNode.MapSNode, endNode.MapSNode);
        startNode.CalculateFCost();

        bool checkLand = !endNode.IsOnLand;

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                //PrintTime("FindPath", false);
                return CalculatePathHelper(endNode);
            }

            openList.Remove(currentNode);
            closeList.Add(currentNode);

            foreach (PathNode nNode in currentNode.GetNeighbourList(this))
            {
                if (closeList.Contains(nNode))
                {
                    continue;
                }

                if (checkLand && nNode.IsOnLand)
                {
                    closeList.Add(nNode);
                    continue;
                }

                int tentativeGCost = currentNode.GCost + CalculateRemainingDistance(currentNode.MapSNode, nNode.MapSNode);
                if (tentativeGCost < nNode.GCost)
                {
                    nNode.CameFromNode = currentNode;
                    nNode.GCost = tentativeGCost;
                    nNode.HCost = CalculateRemainingDistance(nNode.MapSNode, endNode.MapSNode);
                    nNode.CalculateFCost();

                    if (!openList.Contains(nNode))
                    {
                        openList.Add(nNode);
                    }
                }
            }
        }

        Debug.LogError("Cannot find path to node.");
        //PrintTime("FindPath", false);
        return null;
    }

    private PathNode GetNullable(Vector2 position)
    {
        var node = new Vector2Int(
            (int)Mathf.Round((position.x + Width / 2f) / Scale.x),
            (int)Mathf.Round((position.y + Height / 2f) / Scale.y)
            );
        if (node.x >= 0 && node.y >= 0 && node.x < ResX && node.y < ResY)
        {
            return GetPathNode(node);
        }
        return null;
    }
}

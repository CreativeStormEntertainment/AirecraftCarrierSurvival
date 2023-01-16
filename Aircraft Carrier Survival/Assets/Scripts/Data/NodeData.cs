using System.Collections.Generic;
using UnityEngine;

public class NodeData : IComparer<NodeData>
{
    private Vector2 pos;
    private float dist;
    private Vector2 diff;
    private Vector2 mult;
    private Vector2 helper;
    private float currentDev;
    private float dir;

    public NodeData Setup(Vector2 ship, Vector2 diff)
    {
        pos = ship;
        if (diff.x == 0f)
        {
            pos.y = diff.y;
            this.diff = Vector2.right;
            mult = Vector2.up;
        }
        else
        {
            pos.x = diff.x;
            this.diff = Vector2.up;
            mult = Vector2.right;
        }
        helper = pos - ship;
        helper *= mult;
        dist = helper.sqrMagnitude;
        currentDev = 0f;
        dir = -1f;
        return this;
    }

    public Vector2 Next()
    {
        var result = pos + diff * dir * currentDev;
        dir = -dir;
        if (dir > 0f)
        {
            currentDev++;

            helper += diff;
            dist = helper.sqrMagnitude;
        }
        return result;
    }

    public int Compare(NodeData x, NodeData y)
    {
        return Comparer<float>.Default.Compare(x.dist, y.dist);
    }
}
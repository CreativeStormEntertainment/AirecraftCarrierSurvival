using UnityEngine;
using System;

[Serializable]
public class MapNode
{
    public MapNode NextNode;
    public MapNode ExitNode;
    public Vector2 Position;

    public MapNode(Vector2 pos)
    {
        Position = pos;
    }
}

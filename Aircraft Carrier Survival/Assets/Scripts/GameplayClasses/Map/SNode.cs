using System;
using System.Collections.Generic;

[Serializable]
public struct SNode
{
    public int x;
    public int y;

    public SNode(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public List<SNode> GetNeighbors()
    {
        List<SNode> result = new List<SNode>();

        var xMax = TacticManager.Instance.MapNodes.ResX;
        var yMax = TacticManager.Instance.MapNodes.ResY;

        if (x > 0)
        {
            if (y < yMax)
                result.Add(new SNode(x - 1, y + 1));
            result.Add(new SNode(x - 1, y));
            if (y > 0)
                result.Add(new SNode(x - 1, y - 1));
        }

        if (y < yMax)
            result.Add(new SNode(x, y + 1));
        if (y > 0)
            result.Add(new SNode(x, y - 1));

        if (x < xMax)
        {
            if (y < yMax)
                result.Add(new SNode(x + 1, y + 1));
            result.Add(new SNode(x + 1, y));
            if (y > 0)
                result.Add(new SNode(x + 1, y - 1));
        }

        return result;
    }
}

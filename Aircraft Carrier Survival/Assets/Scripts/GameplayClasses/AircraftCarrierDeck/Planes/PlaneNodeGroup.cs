using System.Collections.Generic;

public class PlaneNodeGroup
{
    public List<List<PlaneNode>> LineNodes;

    public PlaneNodeGroup()
    {
        LineNodes = new List<List<PlaneNode>>();
    }

    public PlaneNodeGroup Reverse()
    {
        var result = new PlaneNodeGroup();
        result.LineNodes = new List<List<PlaneNode>>();
        foreach (var line in LineNodes)
        {
            var reverseLine = new List<PlaneNode>(line);
            reverseLine.Reverse();
            result.LineNodes.Add(reverseLine);
        }
        return result;
    }
}

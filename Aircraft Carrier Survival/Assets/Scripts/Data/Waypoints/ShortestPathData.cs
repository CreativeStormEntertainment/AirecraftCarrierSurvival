using System.Collections.Generic;

public class ShortestPathData
{
    public float CurrentLength;
    public float DistLeft;
    public List<Waypoint> Path;

    public ShortestPathData(Waypoint current, Waypoint dest)
    {
        CurrentLength = 0f;
        DistLeft = Distance(current, dest);
        Path = new List<Waypoint>() { current };
    }

    public ShortestPathData(ShortestPathData other, Waypoint currentNode, Waypoint dest)
    {
        Path = new List<Waypoint>(other.Path);

        CurrentLength = other.CurrentLength + Distance(Path[Path.Count - 1], currentNode);
        DistLeft = Distance(currentNode, dest);

        Path.Add(currentNode);
    }

    float Distance(Waypoint from, Waypoint to)
    {
        return (to.Trans.position - from.Trans.position).sqrMagnitude;
    }
}

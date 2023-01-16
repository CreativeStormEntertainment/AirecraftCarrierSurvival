using System.Collections.Generic;
using UnityEngine;

public class SegmentData
{
    public Waypoint Waypoint;
    public List<Vector3> Points;

    public SegmentData(Waypoint waypoint) : this(waypoint, new List<Vector3>() { waypoint.Trans.position })
    {

    }

    public SegmentData(Waypoint waypoint, List<Vector3> points)
    {
        Waypoint = waypoint;
        Points = points;
    }
}

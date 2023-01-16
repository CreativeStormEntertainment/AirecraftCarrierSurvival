using System;

[Serializable]
public class SectionSegmentData
{
    public SectionSegment Segment;
    public ESectionSegmentObstacle ObstacleType;
    public Door Door;

    public bool HasDoor()
    {
        return ObstacleType == ESectionSegmentObstacle.Door || ObstacleType == ESectionSegmentObstacle.DoorHigher;
    }
}

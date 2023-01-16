using UnityEngine;

public class SegmentGroupNeighbourData
{
    public SectionSegment Segment;
    public SectionSegment Neighbour;

    public SegmentGroupNeighbourData(SectionSegment segment, SectionSegment neighbour)
    {
        Segment = segment;
        Neighbour = neighbour;
    }
}


public class SectionSegmentPathData
{
    public SectionSegmentPathData Previous;
    public SectionSegment Segment;

    public SectionSegmentPathData(SectionSegment segment, SectionSegmentPathData previous)
    {
        Segment = segment;
        Previous = previous;
    }
}

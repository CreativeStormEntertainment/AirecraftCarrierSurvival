
public struct PathCheckData
{
    public SectionSegment CurrentSegment;
    public int PathLength;

    public PathCheckData(SectionSegment segment) : this(segment, 0)
    {

    }

    public PathCheckData(SectionSegment segment, int pathLength)
    {
        CurrentSegment = segment;
        PathLength = pathLength;
    }
}

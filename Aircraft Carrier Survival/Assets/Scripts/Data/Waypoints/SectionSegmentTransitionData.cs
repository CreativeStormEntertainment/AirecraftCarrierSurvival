using System;

[Serializable]
public struct SectionSegmentTransitionData
{
    public int SectionID;
    public string Section;
    public int Subsection;
    public int Segment;

    public void Reset(string value)
    {
        SectionID = 0;
        Section = value;
        Subsection = 0;
        Segment = 0;
    }
}

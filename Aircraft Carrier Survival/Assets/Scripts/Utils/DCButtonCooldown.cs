public class DCButtonCooldown
{
    public SectionSegment SectionSegment;
    public DcButtons DcButtons;
    public bool Destruction;

    public DCButtonCooldown(SectionSegment sectionSegment, DcButtons dcButtons, bool destruction)
    {
        SectionSegment = sectionSegment;
        DcButtons = dcButtons;
        Destruction = destruction;
    }
}

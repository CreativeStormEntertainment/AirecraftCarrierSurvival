using System;

[Serializable]
public class RadialButtonData
{
    public EDCType Type;
    public RadialSubbutton Button;
    public Action ButtonAction;
    public Func<bool> MetRequirements;
    public Func<SubSectionRoom> GetTargetSubsection;
}

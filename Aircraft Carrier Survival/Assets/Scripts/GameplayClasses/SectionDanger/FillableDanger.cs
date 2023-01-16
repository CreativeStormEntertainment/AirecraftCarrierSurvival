
public class FillableDanger : EventableDanger
{
    public FillPercentageData FillData;
    public bool ActiveFill;
    public bool DisableFill;
    public bool RepairMirrorsFill;

    public bool Unfillable;

    public override bool Repair
    {
        get => base.Repair;
        set
        {
            base.Repair = value;
            if (RepairMirrorsFill)
            {
                if (value)
                {
                    RepairData.Percent = 1f - FillData.Percent;
                }
            }
            else
            {
                FillData.Current = 0f;
            }
        }
    }

    public FillableDanger(bool repairMirrorsFill)
    {
        FillData = new FillPercentageData();
        ShouldClearOnRepair = false;
        RepairMirrorsFill = repairMirrorsFill;
    }

    public override void Update()
    {
        base.Update();
        if (Repair && RepairMirrorsFill)
        {
            FillData.Percent = 1f - RepairData.Percent;
        }
        else if (!Repair && ActiveFill && !DisableFill)
        {
            if (!Unfillable || (FillData.Current + 2f) < FillData.Max)
            {
                FillData.Update();
            }
        }
    }
}


public class RepairableDanger
{
    public PercentageData RepairData;
    public bool Exists;
    public bool ShouldClearOnRepair;

    public virtual bool Repair
    {
        get => repair;
        set
        {
            repair = value;
            if (!isMajor)
            {
                RepairData.Current = 0f;
            }
        }
    }
    private bool repair;

    public float RepairPower;
    public float RepairDivisor;

    public bool isMajor = false;

    public RepairableDanger()
    {
        ShouldClearOnRepair = true;
        RepairData = new PercentageData();
        RepairData.ReachedMax += OnRepairReachedMax;
        RepairDivisor = 1f;
    }

    public virtual void Update()
    {
        if (repair)
        {
            RepairData.Update(RepairPower / RepairDivisor);
        }
    }

    private void OnRepairReachedMax()
    {
        Repair = false;
        if (ShouldClearOnRepair)
        {
            Exists = false;
        }
    }
}

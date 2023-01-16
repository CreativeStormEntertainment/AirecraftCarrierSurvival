
public class SpreadableDanger : EventableDanger
{
    public PercentageData SpreadData;
    public override bool Repair
    {
        get => base.Repair;
        set
        {
            base.Repair = value;
            SpreadData.Current = 0f;
        }
    }

    public float SpreadPower
    {
        get;
        set;
    } = 1f;

    public SpreadableDanger()
    {
        SpreadData = new PercentageData();
        SpreadData.ReachedMax += OnSpreadReachedMax;
    }

    public override void Update()
    {
        base.Update();
        if (!Repair)
        {
            SpreadData.Update(SpreadPower);
        }
    }

    private void OnSpreadReachedMax()
    {
        SpreadData.Current = 0;
    }
}

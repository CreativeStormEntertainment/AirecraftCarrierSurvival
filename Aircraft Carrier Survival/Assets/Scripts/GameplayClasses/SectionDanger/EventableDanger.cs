
public class EventableDanger : RepairableDanger
{
    public PercentageData EventData;
    public override bool Repair
    {
        get => base.Repair;
        set
        {
            base.Repair = value;
            EventData.Current = 0f;
        }
    }

    public EventableDanger()
    {
        EventData = new PercentageData();
        EventData.ReachedMax += OnEventReachedMax;
    }

    public override void Update()
    {
        base.Update();
        if (!Repair)
        {
            EventData.Update();
        }
    }

    private void OnEventReachedMax()
    {
        EventData.Current = 0;
    }
}

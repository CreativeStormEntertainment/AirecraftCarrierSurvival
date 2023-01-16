
public class RepairProgresser : TwoWayProgresser
{
    public override bool Positive 
    { 
        get => base.Positive;
        set
        {
            var current = value ? positiveWay : negativeWay;
            getProgress = current.GetProgress;
            setProgress = current.SetProgress;

            if (value != base.Positive)
            {
                positiveWay.SetProgress(0f);
                negativeWay.SetProgress(1f);
            }
            base.Positive = value;
        }
    }

    public override bool IsWorking 
    { 
        get => base.IsWorking;
        set
        {
            base.IsWorking = value;
            if (!value)
            {
                negativeWay.SetProgress(1f);
            }
        }
    }

    new readonly RepairOneWayData positiveWay;
    new readonly RepairOneWayData negativeWay;

    public RepairProgresser(RepairOneWayData positiveWay, RepairOneWayData negativeWay) : base(positiveWay.GetProgress, positiveWay.SetProgress, positiveWay, negativeWay)
    {
        this.positiveWay = positiveWay;
        this.negativeWay = negativeWay;
    }
}

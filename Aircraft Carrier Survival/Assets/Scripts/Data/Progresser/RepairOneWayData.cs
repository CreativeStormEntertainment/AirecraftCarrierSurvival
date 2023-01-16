using System;

public class RepairOneWayData : OneWayData
{
    public Func<float> GetProgress;
    public Action<float> SetProgress;

    public RepairOneWayData(Func<float> progressStep, Action onProgressed, Func<float> getProgress, Action<float> setProgress) : base(progressStep, onProgressed)
    {
        GetProgress = getProgress;
        SetProgress = setProgress;
    }
}

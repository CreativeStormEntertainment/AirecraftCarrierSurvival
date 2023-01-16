using System;

public class OneWayData
{
    public Func<float> ProgressStep;
    public Action OnProgressed;

    public OneWayData(Func<float> progressStep, Action onProgressed)
    {
        ProgressStep = progressStep;
        OnProgressed = onProgressed;
    }
}
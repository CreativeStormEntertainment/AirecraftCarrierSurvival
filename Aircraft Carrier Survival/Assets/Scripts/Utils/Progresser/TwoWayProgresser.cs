using System;
using UnityEngine;

public class TwoWayProgresser : BaseProgresser
{
    protected OneWayData positiveWay;
    protected OneWayData negativeWay;
    private readonly Action onProgress;

    bool positive = true;
    public virtual bool Positive
    {
        get => positive;
        set
        {
            positive = value;
            var current = value ? positiveWay : negativeWay;
            progressStep = current.ProgressStep;
            onProgressed = current.OnProgressed;
        }
    }

    protected TwoWayProgresser(Func<float> getProgress, Action<float> setProgress, OneWayData positive, OneWayData negative) : base(false, 1)
    {
        Setup(getProgress, setProgress, positive, negative);
    }

    public TwoWayProgresser(Func<float> getProgress, Action<float> setProgress, Action onProgress, OneWayData positive, OneWayData negative) : base(false, 1)
    {
        Setup(getProgress, setProgress, positive, negative);
        this.onProgress = onProgress;
    }

    private void Setup(Func<float> getProgress, Action<float> setProgress, OneWayData positiveWay, OneWayData negativeWay)
    {
        this.getProgress = getProgress;
        this.setProgress = setProgress;
        this.positiveWay = positiveWay;
        this.negativeWay = negativeWay;
        positive = true;
    }

    protected override void DoProgressInner()
    {
        if (positive)
        {
            float progress = getProgress() - 1f;
            if ((getProgress() - 1f) > Mathf.Epsilon)
            {
                IsWorking = false;
                onProgressed();
                return;
            }
        }
        else
        {
            if (getProgress() < Mathf.Epsilon)
            {
                IsWorking = false;
                onProgressed();
                return;
            }
        }
        onProgress?.Invoke();
    }
}

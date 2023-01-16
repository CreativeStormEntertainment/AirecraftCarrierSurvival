using System;
using UnityEngine;

public class PercentageData
{
    public event Action ReachedMax = delegate { };

    public float TemplateMax
    {
        get => templateMax;
        set
        {
            templateMax = value;
            Max = value;
        }
    }
    public float Percent
    {
        get => Mathf.Clamp01(Current / Max);
        set
        {
            float oldCurrent = Current;
            Current = Mathf.Clamp01(value) * Max;
            CheckEvents(oldCurrent);
        }
    }
    public float Current;
    public float Max;

    private float templateMax;

    public void FullFilled()
    {
        //to make sure it will fill up
        Update(Max * 2f + 10f);
    }

    public void Update(float power = 1f)
    {
        float oldCurrent = Current;
        Current += power;
        CheckEvents(oldCurrent);
    }

    protected virtual void CheckEvents(float oldCurrent)
    {
        if (Current >= Max)
        {
            Current = Max;
            ReachedMax();
        }
    }
}

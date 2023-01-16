using System;

public class FillPercentageData : PercentageData
{
    public event Action ReachedTenth = delegate { };
    public event Action ReachedAfterTenth = delegate { };
    public event Action DroppedQuarter = delegate { };
    public event Action ReachedQuarter = delegate { };
    public event Action DroppedHalf = delegate { };
    public event Action ReachedHalf = delegate { };
    public event Action ReachedAfterHalf = delegate { };
    public event Action DroppedMax = delegate { };

    private bool reachedTenth;
    private float reachedTenthStep;

    private bool reachedHalf;
    private float reachedHalfStep;

    protected override void CheckEvents(float oldCurrent)
    {
        if (reachedTenth && reachedTenthStep == oldCurrent && Current > oldCurrent)
        {
            ReachedAfterTenth();
        }
        if (oldCurrent < (Max / 10f) && Current >= (Max / 10f))
        {
            reachedTenth = true;
            reachedTenthStep = Current;
            ReachedTenth();
        }
        else
        {
            reachedTenth = false;
        }

        if (oldCurrent < (Max / 4f) && Current >= (Max / 4f))
        {
            ReachedQuarter();
        }
        else if (oldCurrent >= (Max / 4f) && Current < (Max / 4f))
        {
            DroppedQuarter();
        }

        if (reachedHalf && reachedHalfStep == oldCurrent && Current > oldCurrent)
        {
            ReachedAfterHalf();
        }
        if (oldCurrent < (Max / 2f) && Current >= (Max / 2f))
        {
            reachedHalf = true;
            reachedHalfStep = Current;
            ReachedHalf();
        }
        else
        {
            if (oldCurrent >= (Max / 2f) && Current < (Max / 2f))
            {
                DroppedHalf();
            }
            reachedHalf = false;
        }
        if (oldCurrent >= Max && Current < Max)
        {
            DroppedMax();
        }
        base.CheckEvents(oldCurrent);
    }
}

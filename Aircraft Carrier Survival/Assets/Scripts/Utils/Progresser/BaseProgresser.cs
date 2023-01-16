using System;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseProgresser
{
    protected Func<float> getProgress;
    protected Action<float> setProgress;
    private bool invoked = false;
    private readonly int step = 1;

    protected Func<float> progressStep;
    protected Action onProgressed;

    private bool isWorking = false;
    public virtual bool IsWorking
    {
        get => isWorking;
        set
        {
            isWorking = value;
        }
    }
    protected BaseProgresser(bool isWorking, int step)
    {
        this.isWorking = isWorking;
        this.step = step;
    }

    protected BaseProgresser(Func<float> progressStep, Action onProgressed, bool isWorking, int step) : this(isWorking, step)
    {
        Assert.IsNotNull(progressStep);
        Assert.IsNotNull(onProgressed);
        this.progressStep = progressStep;
        this.onProgressed = onProgressed;
    }

    public void InvokeProgress()
    {
        if (IsWorking && !invoked)
        {
            invoked = true;
            TimeManager.Instance.Invoke(DoProgress, step);
        }
    }

    private void DoProgress()
    {
        setProgress(getProgress() + progressStep());

        DoProgressInner();
        if (IsWorking)
        {
            TimeManager.Instance.Invoke(DoProgress, step);
        }
        else
        {
            invoked = false;
        }
    }

    protected virtual void DoProgressInner()
    {
        float progress = getProgress() - 1f;
        if (progress > Mathf.Epsilon)
        {
            setProgress(progress);
            onProgressed();
        }
    }
}

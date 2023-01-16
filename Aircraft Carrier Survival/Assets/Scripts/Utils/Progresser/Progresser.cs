using System;

public class Progresser : BaseProgresser
{
    public float Progress = 0f;
    public override bool IsWorking
    {
        get => base.IsWorking;
        set
        {
            if (base.IsWorking != value)
            {
                Progress = 0f;
            }
            base.IsWorking = value;
        }
    }
    private readonly Action onProgress;

    public Progresser(Func<float> progressStep, Action onProgressed, bool isWorking) :  base(progressStep, onProgressed, isWorking, 1)
    {
        getProgress = () => Progress;
        setProgress = (float value) => Progress = value;
    }

    public Progresser(Func<float> progressStep, Action onProgress, Action onProgressed, int step) : base(progressStep, onProgressed, true, step)
    {
        getProgress = () => Progress;
        setProgress = (float value) => Progress = value;
        this.onProgress = onProgress;
    }

    protected override void DoProgressInner()
    {
        base.DoProgressInner();
        onProgress?.Invoke();
    }
}

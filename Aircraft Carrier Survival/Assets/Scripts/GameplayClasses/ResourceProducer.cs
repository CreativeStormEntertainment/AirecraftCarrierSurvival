using UnityEngine;
using UnityEngine.Assertions;

public class ResourceProducer
{
    public int CurrentAmount;
    public int ProduceCount;
    public int ProduceTime;

    int enabledProduction;
    public int EnabledProduction
    {
        get => enabledProduction;
        set
        {
            enabledProduction = value;
            progresser.IsWorking = value != 0;
            progresser.InvokeProgress();
        }
    }
    private readonly Progresser progresser;

    public ResourceProducer()
    {
        enabledProduction = 0;
        ProduceCount = 1;
        progresser = new Progresser(() =>  1f / this.ProduceTime, OnProgressed, false);
    }

    protected virtual void OnProgressed()
    {
        var globResMan = GlobalResourceManager.Instance;
        int produced = Mathf.Min(LocalResourceManager.Instance.GetCurrentStorageSpaceLeft(), ProduceCount, globResMan.CurrentSuppliesAmount);
        Assert.IsFalse(produced < 0);
        CurrentAmount += produced;
        globResMan.CurrentSuppliesAmount -= produced;
    }
}

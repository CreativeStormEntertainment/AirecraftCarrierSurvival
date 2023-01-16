using System;
using UnityEngine;
using UnityEngine.Assertions;

public class LocalResourceManager : MonoBehaviour
{
    public static LocalResourceManager Instance;

    public event Action<float> AmmoAmountChanged = delegate { };
    public event Action<float> ToolsAmountChanged = delegate { };

    [SerializeField]
    float storageCapacity;

    private readonly ResourceProducer ammoProducer = new ResourceProducer();
    private readonly ResourceProducer toolsProducer = new ResourceProducer();

    public IntTooltip AmmoTooltip;
    public IntTooltip ToolsTooltip;

    public int AmmoCurrentAmount
    {
        get => ammoProducer.CurrentAmount;
        set
        {
            ammoProducer.CurrentAmount = Mathf.Max(0, value);
            //AmmoTooltip.SetInt(ammoProducer.CurrentAmount);
            //AmmoAmountChanged(Mathf.Clamp01(ammoProducer.CurrentAmount / StorageCapacity));
        }
    }

    public int ToolsCurrentAmount
    {
        get => toolsProducer.CurrentAmount;
        set
        {
            toolsProducer.CurrentAmount = Mathf.Max(0, value);
            //ToolsTooltip.SetInt(toolsProducer.CurrentAmount);
            //ToolsAmountChanged(Mathf.Clamp01(toolsProducer.CurrentAmount / StorageCapacity));
        }
    }

    public int AmmoProduceCount
    {
        get => ammoProducer.ProduceCount;
        set
        {
            Assert.IsTrue(value >= 0);
            ammoProducer.ProduceCount = value;
        }
    }
    public int ToolsProduceCount
    {
        get => toolsProducer.ProduceCount;
        set
        {
            Assert.IsTrue(value >= 0);
            toolsProducer.ProduceCount = value;
        }
    }

    public int AmmoEnabledProduction
    {
        get => ammoProducer.EnabledProduction;
        set => ammoProducer.EnabledProduction = value;
    }

    public int ToolsEnabledProduction
    {
        get => toolsProducer.EnabledProduction;
        set => toolsProducer.EnabledProduction = value;
    }

    public int ProduceLocalResourceTime
    {
        get => ammoProducer.ProduceTime;
        set
        {
            Assert.IsTrue(value > 0);
            ammoProducer.ProduceTime = toolsProducer.ProduceTime = value;
        }
    }

    public float StorageCapacity
    {
        get => storageCapacity;
        set
        {
            Assert.IsTrue(value > Mathf.Epsilon);
            storageCapacity = value;
        }
    }

    void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    public int GetCurrentStorageSpaceLeft()
    {
        return Mathf.CeilToInt(StorageCapacity) - (AmmoCurrentAmount + ToolsCurrentAmount);
    }
}

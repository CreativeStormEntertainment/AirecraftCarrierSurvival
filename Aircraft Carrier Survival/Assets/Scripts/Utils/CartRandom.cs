using System;
using UnityEngine;

[Serializable]
public class CartRandom
{
    [SerializeField]
    private int successCount = 1;
    [SerializeField]
    private int failCount = 0;

    private int currentSuccesses;
    private int currentFails;
    private int value;

    public void Init()
    {
        currentSuccesses = successCount;
        currentFails = failCount;
        value = currentSuccesses + currentFails;
    }

    public void LoadData(ref RandomData data)
    {
        currentFails = (int)data.CurrentValue;
        currentSuccesses = (int)data.SuccessValue;
        value = currentSuccesses + currentFails;
    }

    public void SaveData(ref RandomData data)
    {
        data.CurrentValue = currentFails;
        data.SuccessValue = currentSuccesses;
    }

    public bool Check()
    {
        if (UnityEngine.Random.Range(0, value) <= currentSuccesses)
        {
            currentSuccesses--;
            UpdateValue();
            return true;
        }
        else
        {
            currentFails--;
            UpdateValue();
            return false;
        }
    }

    private void UpdateValue()
    {
        if (--value <= 0)
        {
            Init();
        }
    }
}

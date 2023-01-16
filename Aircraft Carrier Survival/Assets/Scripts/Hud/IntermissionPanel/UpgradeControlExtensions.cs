using System.Collections.Generic;
using UnityEngine.Assertions;

public static class UpgradeControlExtensions
{
    public static void Setup(this List<UpgradeControl> controls, int data, int currentData)
    {
        int dataSize = controls[0].GetDataSize();
        int leftBit = 0;
        foreach (var control in controls)
        {
            leftBit += dataSize;
            control.Setup(BinUtils.GetBits(data, leftBit - 1, leftBit - dataSize), currentData);
        }
        Assert.IsTrue((controls.Count * dataSize) < 32);
    }

    public static int Save(this List<UpgradeControl> controls)
    {
        int result = 0;
        int dataSize = controls[0].GetDataSize();
        for (int i = controls.Count - 1; i >= 0; i--)
        {
            result <<= dataSize;
            result |= controls[i].Save();
        }
        return result;
    }

    public static void Refresh(this List<UpgradeControl> controls)
    {
        foreach (var control in controls)
        {
            control.Refresh();
        }
    }

    public static void SetData(this List<UpgradeControl> controls, int data)
    {
        foreach (var control in controls)
        {
            control.SetData(data);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UpgradeControl : MonoBehaviour
{
    public event Action Upgraded = delegate { };

    private const int DataSize = 3;

    public int Current
    {
        get;
        private set;
    }

    [SerializeField]
    private bool commandPoints = false;
    [SerializeField]
    private List<UpgradeControlData> controlDatas = null;

    [SerializeField]
    private List<UpgradeUIData> uiDatas = null;
    [SerializeField]
    private UpgradeButton button = null;

    public int GetDataSize()
    {
        return DataSize * controlDatas.Count;
    }

    public int GetUpgrade(int carrier)
    {
        return controlDatas[carrier].Current;
    }

    public int GetCurrentUpgrade()
    {
        return GetUpgrade(Current);
    }

    public void Setup(int data, int currentData)
    {
        button.Setup(commandPoints, 100, Upgrade, Highlight);

        int leftBit = 0;
        Current = 0;
        Assert.IsTrue(controlDatas.Count == (int)ECarrierType.Count);
        foreach (var controlData in controlDatas)
        {
            Assert.IsTrue(controlData.UpgradeDatas.Count < (1 << DataSize));
            leftBit += DataSize;
            int value = BinUtils.GetBits(data, leftBit - 1, leftBit - DataSize);
            while (controlData.Current < value)
            {
                Upgrade();
            }
            Current++;
        }
        Current = -1;
        SetData(currentData);
    }

    public int Save()
    {
        int result = 0;
        for (int i = controlDatas.Count - 1; i >= 0; i--)
        {
            Assert.IsTrue(controlDatas[i].Current >= 0 && controlDatas[i].Current < (1 << DataSize));
            result <<= DataSize;
            result |= controlDatas[i].Current;
        }
        return result;
    }

    public void SetData(int index)
    {
        if (Current > -1)
        {
            var oldData = controlDatas[Current];
            var oldUpgradeData = oldData.UpgradeDatas[oldData.Current];
            foreach (var obj in oldUpgradeData.ShowCurrent)
            {
                obj.SetActive(false);
            }
        }
        Current = index;
        var data = controlDatas[Current];
        button.MaxUpgrades = data.UpgradeDatas.Count - 1;
        var upgradeData = data.UpgradeDatas[data.Current];
        foreach (var obj in upgradeData.ShowCurrent)
        {
            obj.SetActive(true);
        }
        button.SetUpgrade(data.Current, upgradeData.Cost, false);
        for (int i = 0; i < data.UpgradeDatas.Count; i++)
        {
            var uiData = uiDatas[i];
            uiData.Set(i <= data.Current);
            uiData.Text.text = data.UpgradeDatas[i].TextOnUI;
        }
    }

    public void Refresh()
    {
        button.Refresh();
    }

    private void Upgrade()
    {
        var data = controlDatas[Current];

        var upgradeData = data.UpgradeDatas[data.Current];
        foreach (var obj in upgradeData.ShowCurrent)
        {
            obj.SetActive(false);
        }
        Highlight(false);
        data.Current++;
        uiDatas[data.Current].Set(true);
        upgradeData = data.UpgradeDatas[data.Current];
        foreach (var obj in upgradeData.ShowCurrent)
        {
            obj.SetActive(true);
        }
        button.SetUpgrade(data.Current, upgradeData.Cost, false);

        Upgraded();
    }

    private void Highlight(bool highlight)
    {
        var data = controlDatas[Current];
        foreach (var obj in data.UpgradeDatas[data.Current].ShowOnHighlight)
        {
            obj.SetActive(highlight);
        }
    }
}

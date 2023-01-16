using System;
using System.Collections.Generic;
using UnityEngine;

public class AircraftIntermissionData : IShopButtonData
{
    public List<int> BuyCosts => buyCosts;

    public Action<bool> HighlightAction
    {
        get;
    }

    public EPlaneType Type
    {
        get;
    }

    public string BuyTextID
    {
        get;
    }

    public int UnlockCost => UpgradeData.Cost;
    public string UnlockTextID => UpgradeData.TextOnUI;

    public bool Unlocked
    {
        get;
        set;
    }

    public int Count
    {
        get;
        set;
    }

    public int ShopCount
    {
        get;
        set;
    }

    public int CurrentCount
    {
        get;
        set;
    }

    public int BuyCost
    {
        get;
        set;
    }

    public SingleUpgradeData UpgradeData
    {
        get;
        set;
    }

    public int ChosenSkin
    {
        get;
        set;
    }

    public int CurrentTier
    {
        get;
        set;
    }

    public List<AircraftIntermission> Squadrons
    {
        get;
        private set;
    }

    private readonly List<int> buyCosts;

    public AircraftIntermissionData(EPlaneType type, int currentCount, List<int> buyCost, string buyTextID, int chosenSkin, SingleUpgradeData data, int tier, List<AircraftIntermission> squadronObjects)
    {
        CurrentCount = currentCount;

        CurrentTier = tier;

        HighlightAction = (highlight) =>
        {
            foreach (var obj in UpgradeData.ShowOnHighlight)
            {
                obj.SetActive(highlight);
            }
        };

        Type = type;
        BuyTextID = buyTextID;

        ChosenSkin = chosenSkin;

        buyCosts = buyCost;

        Count = 1;

        UpgradeData = data;
        Squadrons = squadronObjects;
        UpdateSquadronObjects();
    }

    public bool CheckCount(int additionalCount)
    {
        return (CurrentCount + additionalCount) < IntermissionData.MaxCountCount;
    }

    public void AddCurrent(int value)
    {
        CurrentCount += value;
        UpdateSquadronObjects();
    }

    public void ClearCurrent()
    {
        AddCurrent(-CurrentCount);
    }

    public void UpdateSquadronObjects()
    {
        for (int i = 0; i < Squadrons.Count; i++)
        {
            Squadrons[i].gameObject.SetActive(i < CurrentCount);
        }
    }
}

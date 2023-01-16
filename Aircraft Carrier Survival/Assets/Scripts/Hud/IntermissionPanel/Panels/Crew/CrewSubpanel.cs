using System;
using System.Collections.Generic;
using UnityEngine;

using UnityRandom = UnityEngine.Random;

[Serializable]
public class CrewSubpanel : PeopleSubpanel<CrewIntermissionData, CrewUpgradeSaveData, CrewPeopleSaveData, CrewShopButton, CrewInventoryItem, CrewDropSlot>
{
    [SerializeField]
    private CrewShopInventory shopInventory = null;
    [SerializeField]
    private CrewDraggableInventory draggableInventory = null;
    [SerializeField]
    private CrewInventory activeInventory = null;

    [SerializeField]
    private int crewsInShopCount = 3;
    [SerializeField]
    private List<int> crewCosts = null;
    [SerializeField]
    private CrewDataList crewDataList = null;
    [SerializeField]
    private string crewBuyTextID = "BuyCrewID";

    [SerializeField]
    private List<GameObject> crewsModels = null;

    [SerializeField]
    private int minCrews = 3;

    private float tier3Chance;
    private float tier2Chance;
    private UpgradeControl control;
    private int carrier;

    public void Setup(UpgradeControl control, ref CrewPeopleSaveData data, int carrier)
    {
        var ownedCrewsSave = SaveManager.Instance.Data.MissionRewards.CrewsUpgrades;

        int admiralLevel = SaveManager.Instance.Data.MissionRewards.GetAdmiralLevel();
        tier3Chance = admiralLevel * 0.017f;
        tier2Chance = admiralLevel * 0.023f;

        subpanelData = new PeopleSubpanelData<CrewIntermissionData, CrewUpgradeSaveData, CrewShopButton, CrewInventoryItem, CrewDropSlot>(shopInventory, draggableInventory, activeInventory, CreateDataFromSave, CreateSaveFromData, crewsModels);

        foreach (var model in subpanelData.Models)
        {
            model.SetActive(false);
        }

        shop = new List<CrewIntermissionData>();
        owned = new List<CrewIntermissionData>();
        selected = new List<CrewIntermissionData>();

        subpanelData.DraggableInventory.DoubleClicked += OnDoubleClicked;
        subpanelData.ActiveInventory.ItemChanged += OnItemChanged;

        subpanelData.ActiveInventory.Init(root, canvas);
        subpanelData.ActiveInventory.Setup(0);

        this.carrier = carrier;
        this.control = control;

        control.Upgraded += OnLoadUpgrade;
        control.Setup(data.Upgrade, carrier);

        control.Upgraded -= OnLoadUpgrade;
        control.Upgraded += () => OnSlotsUpgraded(upgradeSlotIncrease);

        if (data.Shop.Count < crewsInShopCount)
        {
            shop.AddRange(GetShopItems());
        }
        else
        {
            foreach (var shopData in data.Shop)
            {
                shop.Add(subpanelData.DataConstructor(shopData));
            }
        }
        subpanelData.ShopInventory.Setup(shop, shop.Count, OnPersonBought);

        foreach (var savedData in ownedCrewsSave)
        {
            var newData = subpanelData.DataConstructor(savedData);
            owned.Add(newData);
        }
        subpanelData.DraggableInventory.Setup(owned, owned.Count);

#if !UNITY_EDITOR
        Debug.Log($"[HHH]{carrier};{startSlots.Count}");
#endif
        SetCarrier((ECarrierType)carrier, control.GetCurrentUpgrade());
        for (int i = 0; i < data.Selected.Count; i++)
        {
#if !UNITY_EDITOR
            Debug.Log($"[HHH]{i};{data.Selected.Count}");
            Debug.Log($"[HHH]{data.Selected[i]};{owned.Count}");
#endif
            if (data.Selected[i] >= 0)
            {
                SetSelectedPerson(i, owned[data.Selected[i]]);
            }
        }
    }

    public void Save(ref CrewPeopleSaveData data)
    {
        var ownedCrewsSave = SaveManager.Instance.Data.MissionRewards.CrewsUpgrades;
        data.Upgrade = control.Save();

        ownedCrewsSave.Clear();
        foreach (var personData in owned)
        {
            ownedCrewsSave.Add(personData.Data);
        }

        data.Selected.Clear();
        foreach (var personData in selected)
        {
            data.Selected.Add(owned.IndexOf(personData));
        }

        data.Shop.Clear();
        foreach (var personData in shop)
        {
            data.Shop.Add(subpanelData.SaveConstructor(personData));
        }
    }

    public void SetCarrier(ECarrierType type, int upgrade)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            OnItemChanged(i, null);
        }
        selected.Clear();
        subpanelData.ActiveInventory.Setup(0);
        int count = startSlots[(int)type] + upgrade * upgradeSlotIncrease;
        OnSlotsUpgraded(count);
    }

    private CrewIntermissionData CreateDataFromSave(CrewUpgradeSaveData saveData)
    {
        CrewIntermissionData newCrew = new CrewIntermissionData(canvas, root, saveData.Cost, crewBuyTextID);
        newCrew.Data = saveData;
        return newCrew;
    }

    private CrewUpgradeSaveData CreateSaveFromData(CrewIntermissionData data)
    {
        CrewUpgradeSaveData newSaveData = data.Data;
        return newSaveData;
    }

    protected override CrewIntermissionData GetShopItem()
    {
        CrewIntermissionData newCrew = new CrewIntermissionData(canvas, root, 1, crewBuyTextID);
        newCrew.Data.CrewDataIndex = UnityRandom.Range(0, crewDataList.List.Count);
        newCrew.BuyCost = newCrew.Data.Cost = crewCosts[0];
        return newCrew;
    }

    protected override List<CrewIntermissionData> GetShopItems()
    {
        var shopItems = new List<CrewIntermissionData>();
        for (int i = 0; i < crewsInShopCount; i++)
        {
            CrewIntermissionData newCrew = GetShopItem();
            shopItems.Add(newCrew);
        }
        return shopItems;
    }

    protected override void OnItemChanged(int slot, CrewIntermissionData newSelected)
    {
        base.OnItemChanged(slot, newSelected);
        IntermissionManager.Instance.SetActiveCrewWarning(GetSelectedUnitsCount() < minCrews);
    }

    private void OnLoadUpgrade()
    {
        if (control.Current == carrier)
        {
            OnSlotsUpgraded(upgradeSlotIncrease);
        }
    }
}

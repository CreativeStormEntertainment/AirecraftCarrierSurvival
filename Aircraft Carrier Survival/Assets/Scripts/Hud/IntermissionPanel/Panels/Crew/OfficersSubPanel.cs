using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OfficersSubPanel : PeopleSubpanel<OfficerIntermissionData, OfficerUpgrades, OfficerPeopleSaveData, OfficerShopButton, OfficerInventoryItem, OfficerDropSlot>
{
    public static readonly List<int> OfficersGreenBasket = new List<int>() { 2, 3 };
    public static readonly List<int> OfficersOrangeBasket = new List<int>() { 4, 5 };
    public static readonly List<int> OfficersRedBasket = new List<int>() { 6, 7, 8 };

    [SerializeField]
    private OfficersShopInventory shopInventory = null;
    [SerializeField]
    private OfficersDraggableInventory draggableInventory = null;
    [SerializeField]
    private OfficersInventory activeInventory = null;

    [SerializeField]
    private OfficerList officerList = null;

    [SerializeField]
    private int officersInShopCount = 2;
    [SerializeField]
    private string officerBuyTextID = "BuyOfficerID";

    [SerializeField]
    private List<GameObject> officersModels = null;

    [SerializeField]
    private int minOfficers = 2;

    private List<int> availableOfficers = new List<int>();
    private List<int> greenBasket = new List<int>();
    private List<int> orangeBasket = new List<int>();
    private List<int> redBasket = new List<int>();

    private UpgradeControl control;
    private int carrier;

    public void Setup(UpgradeControl control, ref OfficerPeopleSaveData data, int carrier)
    {
        foreach (var officer in officerList.Officers)
        {
            officer.Init();
        }

        var officersSave = SaveManager.Instance.Data.MissionRewards.OfficersUpgrades;
        SetInitAvailableOfficers();
        subpanelData = new PeopleSubpanelData<OfficerIntermissionData, OfficerUpgrades, OfficerShopButton, OfficerInventoryItem, OfficerDropSlot>(shopInventory, draggableInventory, activeInventory, CreateDataFromSave, CreateSaveFromData, officersModels);

        foreach (var model in subpanelData.Models)
        {
            model.SetActive(false);
        }

        shop = new List<OfficerIntermissionData>();
        owned = new List<OfficerIntermissionData>();
        selected = new List<OfficerIntermissionData>();

        subpanelData.DraggableInventory.DoubleClicked += OnDoubleClicked;
        subpanelData.ActiveInventory.ItemChanged += OnItemChanged;

        this.carrier = carrier;
        this.control = control;

        subpanelData.ActiveInventory.Init(root, canvas);
        subpanelData.ActiveInventory.Setup(0);
        control.Upgraded += OnLoadUpgrade;
        control.Setup(data.Upgrade, carrier);

        control.Upgraded -= OnLoadUpgrade;
        control.Upgraded += () => OnSlotsUpgraded(upgradeSlotIncrease);

        if (data.Shop.Count < officersInShopCount)
        {
            shop.AddRange(GetShopItems());
        }
        else
        {
            foreach (var shopData in data.Shop)
            {
                if (shopData != -1)
                {
                    var newData = subpanelData.DataConstructor(officersSave[shopData]);
                    newData.Index = shopData;
                    newData.BuyCost = officerList.Officers[shopData].Cost;
                    shop.Add(newData);
                }
            }
        }
        subpanelData.ShopInventory.Setup(shop, shop.Count, OnPersonBought);
        SetAllAvailableOfficers();

        foreach (var savedData in data.Owned)
        {
            if (savedData != -1)
            {
                var newData = subpanelData.DataConstructor(officersSave[savedData]);
                newData.Index = savedData;
                newData.BuyCost = officerList.Officers[savedData].Cost;
                owned.Add(newData);
            }
        }

        SetCarrier((ECarrierType)carrier, control.GetCurrentUpgrade());
        for (int i = 0; i < data.Selected.Count; i++)
        {
            if (data.Selected[i] >= 0)
            {
                int index = data.Selected[i];
                SetSelectedPerson(i, owned.Find((x) => x.Index == index));
            }
        }
        subpanelData.DraggableInventory.Setup(owned, owned.Count);
        IntermissionManager.Instance.SetActiveOfficersWarning(GetSelectedUnitsCount() < minOfficers);
    }

    public void Save(ref OfficerPeopleSaveData data)
    {
        data.Upgrade = control.Save();
        data.Owned.Clear();
        foreach (var personData in owned)
        {
            data.Owned.Add(personData == null ? -1 : personData.Index);
        }

        data.Selected.Clear();
        foreach (var personData in selected)
        {
            data.Selected.Add(personData == null ? -1 : personData.Index);
        }

        data.Shop.Clear();
        foreach (var personData in shop)
        {
            data.Shop.Add(personData == null ? -1 : personData.Index);
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

    protected override OfficerIntermissionData GetShopItem()
    {
        if (availableOfficers.Count > 0)
        {
            int random = RandomUtils.GetRandom(availableOfficers);
            availableOfficers.Remove(random);
            var officerSave = SaveManager.Instance.Data.MissionRewards.OfficersUpgrades[random];
            OfficerIntermissionData newOfficer = new OfficerIntermissionData(canvas, root, officerBuyTextID);
            newOfficer.Data = officerSave;
            newOfficer.Index = random;
            newOfficer.BuyCost = officerList.Officers[random].Cost;
            return newOfficer;
        }
        return null;
    }

    protected override List<OfficerIntermissionData> GetShopItems()
    {
        var shopItems = new List<OfficerIntermissionData>();

        int officersCount = Mathf.Min(availableOfficers.Count, officersInShopCount);
        for (int i = 0; i < officersCount; i++)
        {
            OfficerIntermissionData newCrew = GetShopItem();
            shopItems.Add(newCrew);
        }
        return shopItems;
    }

    protected override void OnItemChanged(int slot, OfficerIntermissionData newSelected)
    {
        base.OnItemChanged(slot, newSelected);
        IntermissionManager.Instance.SetActiveOfficersWarning(GetSelectedUnitsCount() < minOfficers);
    }

    private OfficerIntermissionData CreateDataFromSave(OfficerUpgrades saveData)
    {
        OfficerIntermissionData newCrew = new OfficerIntermissionData(canvas, root, officerBuyTextID);
        newCrew.Data = saveData;
        return newCrew;
    }

    private OfficerUpgrades CreateSaveFromData(OfficerIntermissionData data)
    {
        OfficerUpgrades newSaveData = data.Data;
        return newSaveData;
    }

    private void SetAllAvailableOfficers()
    {
        availableOfficers.Clear();

        GetAvailableOfficersFromBasket(OfficersGreenBasket, greenBasket);
        availableOfficers.AddRange(greenBasket);
        GetAvailableOfficersFromBasket(OfficersOrangeBasket, orangeBasket);
        availableOfficers.AddRange(orangeBasket);
        GetAvailableOfficersFromBasket(OfficersRedBasket, redBasket);
        availableOfficers.AddRange(redBasket);
    }

    private void SetInitAvailableOfficers()
    {
        availableOfficers.Clear();

        GetAvailableOfficersFromBasket(OfficersGreenBasket, greenBasket);
        GetAvailableOfficersFromBasket(OfficersOrangeBasket, orangeBasket);
        GetAvailableOfficersFromBasket(OfficersRedBasket, redBasket);

        int index = GetRandomOfficer(greenBasket, orangeBasket, redBasket);
        if (index == -1)
        {
            return;
        }
        availableOfficers.Add(index);
        index = GetRandomOfficer(orangeBasket, redBasket, greenBasket);
        if (index == -1)
        {
            return;
        }
        availableOfficers.Add(index);
        index = GetRandomOfficer(redBasket, greenBasket, orangeBasket);
        if (index == -1)
        {
            return;
        }
        availableOfficers.Add(index);
    }

    private void GetAvailableOfficersFromBasket(List<int> basket, List<int> list)
    {
        var saveData = SaveManager.Instance.Data;
        list.Clear();
        foreach (var officer in basket)
        {
            if (!saveData.IntermissionData.OfficerData.Owned.Contains(officer) && owned.Find(o => o.Index == officer) == null && !availableOfficers.Contains(officer) && shop.Find(o => o.Index == officer) == null)
            {
                list.Add(officer);
            }
        }
    }

    private int GetRandomOfficerFromBasket(List<int> basket)
    {
        if (basket.Count > 0)
        {
            int rand = UnityEngine.Random.Range(0, basket.Count);
            int officer = basket[rand];
            basket.RemoveAt(rand);
            return officer;
        }
        return -1;
    }

    private int GetRandomOfficer(List<int> basketA, List<int> basketB, List<int> basketC)
    {
        int index = GetRandomOfficerFromBasket(basketA);
        if (index == -1)
        {
            index = GetRandomOfficerFromBasket(basketB);
            if (index == -1)
            {
                index = GetRandomOfficerFromBasket(basketC);
            }
        }
        return index;
    }

    private void OnLoadUpgrade()
    {
        if (control.Current == carrier)
        {
            OnSlotsUpgraded(upgradeSlotIncrease);
        }
    }
}

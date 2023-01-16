using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class EscortPanel : Panel
{
    [SerializeField]
    private Canvas canvas = null;

    [SerializeField]
    private List<EscortBonusData> bonuses = null;

    [SerializeField]
    private List<Button> tabButtons = null;

    [SerializeField]
    private List<Text> tabTexts = null;

    [SerializeField]
    private List<Transform> activeInventoryModelsSlots = null;

    [SerializeField]
    private List<RectTransform> activeInventoryUISlots = null;

    [SerializeField]
    private RectTransform rootContainer = null;

    [SerializeField]
    private EscortShopInventory shopInventory = null;

    [SerializeField]
    private EscortInventory inventory = null;

    [SerializeField]
    private EscortActiveInventory activeInventory = null;

    [SerializeField]
    private StrikeGroupData strikeGroups = null;

    [SerializeField]
    private GameObject warning1 = null;
    [SerializeField]
    private GameObject warning2 = null;

    [SerializeField]
    private int startEscortCount = 3;

    [SerializeField]
    private EscortTooltip tooltip = null;

    [SerializeField]
    private Sprite activeTab = null;
    [SerializeField]
    private Sprite inactiveTab = null;

    [SerializeField]
    private Color bonusesDefaultColor = Color.white;
    [SerializeField]
    private Color bonusesGreenColor = Color.green;
    [SerializeField]
    private Color bonusesRedColor = Color.red;

    [SerializeField]
    private StudioEventEmitter hoverShipTypeEvent;

    [SerializeField]
    private ScrollRect scrollRect = null;

    private Dictionary<int, EscortTierData> tierDict;
    private List<EscortItemData> owned;
    private List<EscortItemData> selected;

    private int currentTier;

    private Dictionary<EStrikeGroupPassiveSkill, EscortBonusData> bonusesDict;
    private Dictionary<EStrikeGroupPassiveSkill, int> bonusesDifferenceDict;

    private List<EscortShopButtonData> currentTabList = new List<EscortShopButtonData>();

    private bool currentTab;

    private Color color1;
    private Color color2;
    private Image image1;
    private Image image2;

    public override void Setup(NewSaveData data)
    {
        base.Setup(data);

        scrollRect.verticalScrollbar.onValueChanged.AddListener((_) =>
        {
            scrollRect.verticalScrollbar.value = Mathf.Clamp01(scrollRect.verticalScrollbar.value);
        });

        tierDict = new Dictionary<int, EscortTierData>();
        owned = new List<EscortItemData>();
        selected = new List<EscortItemData>();
        bonusesDict = new Dictionary<EStrikeGroupPassiveSkill, EscortBonusData>();
        bonusesDifferenceDict = new Dictionary<EStrikeGroupPassiveSkill, int>();
        foreach (var bonus in bonuses)
        {
            bonusesDict[bonus.Category] = bonus;
        }
        tabButtons[0].onClick.AddListener(() => ChangeTab(true));
        tabButtons[1].onClick.AddListener(() => ChangeTab(false));

        color1 = tabTexts[0].color;
        color2 = tabTexts[1].color;
        image1 = tabButtons[0].GetComponent<Image>();
        image2 = tabButtons[1].GetComponent<Image>();

        //for (int i = 0; i < tierButtons.Count; i++)
        //{
        //    int index = i;
        //    tierButtons[index].Button.onClick.AddListener(() => ChangeTier(index));
        //}
        shopInventory.SetShowTooltip += OnSetShowTooltip;
        inventory.SetShowTooltip += OnSetShowTooltip;
        inventory.RaycastResultsChanged += ShowBonusesDifference;
        inventory.Setup(0);
        activeInventory.Init(rootContainer, canvas);
        activeInventory.SetShowTooltip += OnSetShowTooltip;
        for (int i = 0; i < startEscortCount; i++)
        {
            OnSlotsUpgraded();
        }
        controls[0].Upgraded += OnSlotsUpgraded;
        controls.Setup(data.IntermissionData.SelectedEscort.Count - startEscortCount, 0);

        for (int i = 0; i < 3; i++)
        {
            tierDict.Add(i, new EscortTierData(strikeGroups));
        }
        Assert.IsTrue(strikeGroups.Data.Count < 33);
        for (int i = 0; i < strikeGroups.Data.Count; i++)
        {
            var tierData = tierDict[strikeGroups.Data[i].Tier - 1];
            int id = 1 << i;
            tierData.Escorts |= id;
            tierData.EscortCount++;
            if ((data.IntermissionData.UnlockedEscorts & (1 << i)) != 0)
            {
                tierData.Unlocked |= id;
            }
        }

        inventory.Setup(0);
        for (int i = 0; i < data.IntermissionData.OwnedEscorts.Count; i++)
        {
            int escortID = data.IntermissionData.OwnedEscorts[i];
            OnEscortBought(escortID, tierDict[strikeGroups.Data[escortID].Tier - 1]);
        }

        currentTier = -1;

        foreach (var escort in owned)
        {
            if (escort != null)
            {
                escort.Repair = false;
            }
        }
        foreach (var broken in data.IntermissionData.Broken)
        {
            if (broken != -1)
            {
                var escortItem = owned[broken];
                if (escortItem != null)
                {
                    escortItem.Repair = true;
                }
            }
        }

        for (int i = 0; i < data.IntermissionData.SelectedEscort.Count; i++)
        {
            if (data.IntermissionData.SelectedEscort[i] != -1)
            {
                var escortItem = owned[data.IntermissionData.SelectedEscort[i]];
                if (escortItem.Repair)
                {
                    SetShowWarnings(true);
                }
                else
                {
                    SetActiveEscort(i, escortItem);
                }
            }
        }

        inventory.DoubleClicked += OnDoubleClicked;
        activeInventory.ItemChanged += OnItemChanged;

        currentTier = 0;

        ChangeTab(true);
        RecalculateBonuses();
    }

    public override void Save(NewSaveData data)
    {
        data.IntermissionData.OwnedEscorts.Clear();
        data.IntermissionData.Broken.Clear();
        for (int i = 0; i < owned.Count; i++)
        {
            data.IntermissionData.OwnedEscorts.Add(owned[i].EscortID);
            if (owned[i].Repair)
            {
                data.IntermissionData.Broken.Add(i);
            }
        }
        data.IntermissionData.SelectedEscort.Clear();
        foreach (var escort in selected)
        {
            data.IntermissionData.SelectedEscort.Add(escort == null ? -1 : escort.InventoryID);
        }
        foreach (var tierData in tierDict.Values)
        {
            data.IntermissionData.UnlockedEscorts |= tierData.Unlocked;
        }
    }

    public void SetShowWarnings(bool show)
    {
        warning1.SetActive(show);
        warning2.SetActive(show);
    }

    protected override void InnerRefresh(int prevCarrier)
    {
        controls.Refresh();
        shopInventory.Refresh();
    }

    private void ChangeTab(bool buyTab)
    {
        currentTab = buyTab;
        tabButtons[0].interactable = !buyTab;
        tabButtons[1].interactable = buyTab;

        image1.sprite = buyTab ? activeTab : inactiveTab;
        image2.sprite = buyTab ? inactiveTab : activeTab;

        color1.a = buyTab ? 1f : 0.5f;
        color2.a = buyTab ? 0.5f : 1f;
        tabTexts[0].color = color1;
        tabTexts[1].color = color2;

        tabTexts[0].rectTransform.anchoredPosition = new Vector2(0f, buyTab ? 0f : -5f);
        tabTexts[1].rectTransform.anchoredPosition = new Vector2(0f, buyTab ? -5f : 0f);

        shopInventory.Setup(GetShopButtons(buyTab), currentTabList.Count, OnItemChanged);
        inventory.Setup(owned, owned.Count);
        scrollRect.verticalNormalizedPosition = 1f;
        scrollRect.Rebuild(CanvasUpdate.PostLayout);
    }

    private void ChangeTier(int tier)
    {
        //var tierButtonData = tabButtons[currentTier];
        //tierButtonData.Button.interactable = true;
        //tierButtonData.RectTransform.sizeDelta = baseSize;

        //currentTier = tier;

        //tierButtonData = tabButtons[currentTier];
        //tierButtonData.Button.interactable = false;
        //tierButtonData.RectTransform.sizeDelta = selectedSize;

        //var tierData = tierDict[currentTier];
        //shopInventory.Setup(GetTierShopButtons(tierData), tierData.EscortCount, OnItemChanged);
        //inventory.Setup(tierData.Owned, tierData.Owned.Count);
    }

    private void OnSlotsUpgraded()
    {
        activeInventory.Add(activeInventoryUISlots[selected.Count].anchoredPosition);
        selected.Add(null);
    }

    private void OnItemChanged(int index, bool bought)
    {
        var tierData = tierDict[currentTier];
        if (bought)
        {
            OnEscortBought(currentTabList[index].Index, tierData);
        }
        else
        {
            tierData.Unlocked |= 1 << currentTabList[index].Index;
            shopInventory.Setup(GetShopButtons(currentTab), currentTabList.Count);
        }
    }

    private void OnItemChanged(int slot, EscortItemData newEscort)
    {
        bool refresh = false;
        var oldEscort = selected[slot];
        if (oldEscort != null)
        {
            Destroy(activeInventoryModelsSlots[slot].GetChild(0).gameObject);

            oldEscort.Selected = false;
            refresh = true;
        }
        if (newEscort == null)
        {
            selected[slot] = null;
        }
        else
        {
            selected[slot] = newEscort;
            newEscort.Selected = true;

            var data = newEscort.Member;
            refresh = refresh || data.Tier == currentTier;

            Instantiate(data.IntermissionPrefab, activeInventoryModelsSlots[slot]);
        }
        RecalculateBonuses();
        if (true)
        {
            ChangeTab(currentTab);
        }
    }

    private void OnDoubleClicked(EscortInventoryItem item)
    {
        for (int index = 0; index < selected.Count; index++)
        {
            if (selected[index] == null)
            {
                SetActiveEscort(index, item.Data);
                item.gameObject.SetActive(false);
                BackgroundAudio.Instance.PlayEvent(ECrewUIState.DragSuccess);
                return;
            }
        }
        BackgroundAudio.Instance.PlayEvent(EIntermissionClick.InactiveClick);
    }

    private List<EscortShopButtonData> GetTierShopButtons(EscortTierData tierData)
    {
        int i = 0;
        List<EscortShopButtonData> list = new List<EscortShopButtonData>();
        for (int id = 0; id < tierData.EscortCount;)
        {
            Assert.IsTrue(i < 31);
            if ((tierData.Escorts & (1 << i++)) != 0)
            {
                id++;
                list.Add(new EscortShopButtonData(tierData, i - 1));
            }
        }
        return list;
    }

    private List<EscortShopButtonData> GetShopButtons(bool buyTab)
    {
        currentTabList.Clear();
        int j = 0;
        foreach (var tierData in tierDict.Values)
        {
            int i = 0;
            for (int id = 0; id < tierData.EscortCount;)
            {
                Assert.IsTrue(i < 31);
                if ((tierData.Escorts & (1 << i++)) != 0)
                {
                    id++;
                    var data = new EscortShopButtonData(tierData, i - 1);
                    if (data.Unlocked == buyTab)
                    {
                        currentTabList.Add(data);
                    }
                }
            }
            j++;
        }
        return currentTabList;
    }

    private void OnEscortBought(int escortID, EscortTierData tierData)
    {
        var newEscort = new EscortItemData(strikeGroups, escortID, inventory.ItemsCount, canvas, rootContainer);
        tierData.Owned.Add(newEscort);
        owned.Add(newEscort);
        inventory.Add(newEscort);
    }

    private void SetActiveEscort(int slot, EscortItemData item)
    {
        OnItemChanged(slot, item);
        activeInventory.Set(slot, item);
    }

    private void ShowBonusesDifference(StrikeGroupMemberData oldData, StrikeGroupMemberData newData)
    {
        if (oldData == null || newData == null)
        {
            RecalculateBonuses();
            return;
        }
        bonusesDifferenceDict.Clear();
        foreach (var skill in oldData.PassiveSkills)
        {
            bonusesDifferenceDict[skill.Skill] = -skill.Param;
        }
        foreach (var skill in newData.PassiveSkills)
        {
            if (bonusesDifferenceDict.ContainsKey(skill.Skill))
            {
                bonusesDifferenceDict[skill.Skill] += skill.Param;
            }
            else
            {
                bonusesDifferenceDict[skill.Skill] = skill.Param;
            }
        }

        foreach (var difference in bonusesDifferenceDict)
        {
            var bonus = bonusesDict[difference.Key];
            if (difference.Value > 0)
            {
                bonus.Text.color = bonusesGreenColor;
            }
            else if (difference.Value < 0)
            {
                bonus.Text.color = bonusesRedColor;
            }
            bonus.Text.text = (bonus.CurrentValue + difference.Value).ToString();
            switch (bonus.Category)
            {
                case EStrikeGroupPassiveSkill.MaxSupplies:
                case EStrikeGroupPassiveSkill.FasterResupply:
                case EStrikeGroupPassiveSkill.DcRepairSpeed:
                    bonus.Text.text += "%";
                    break;
                default:
                    break;
            }
        }
    }

    private void RecalculateBonuses()
    {
        foreach (var bonus in bonuses)
        {
            bonus.CurrentValue = 0;
            bonus.Text.color = bonusesDefaultColor;
        }
        foreach (var escortData in selected)
        {
            if (escortData != null)
            {
                var escortShipData = strikeGroups.Data[escortData.EscortID];
                foreach (var skillData in escortShipData.PassiveSkills)
                {
                    bonusesDict[skillData.Skill].CurrentValue += skillData.Param;
                }
            }
        }
        foreach (var bonus in bonuses)
        {
            switch (bonus.Category)
            {
                case EStrikeGroupPassiveSkill.MaxSupplies:
                case EStrikeGroupPassiveSkill.FasterResupply:
                case EStrikeGroupPassiveSkill.DcRepairSpeed:
                    bonus.Text.text = bonus.CurrentValue + "%";
                    break;
                default:
                    bonus.Text.text = bonus.CurrentValue.ToString();
                    break;
            }
        }
    }

    private void OnSetShowTooltip(StrikeGroupMemberData data, bool show)
    {
        if (show)
        {
            tooltip.Show(data);
            hoverShipTypeEvent.Play();
        }
        else
        {
            tooltip.Hide();
        }
    }
}

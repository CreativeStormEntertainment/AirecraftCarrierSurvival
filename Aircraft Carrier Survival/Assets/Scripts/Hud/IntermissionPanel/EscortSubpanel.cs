using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscortSubpanel : IntermissionSubpanel
{
    public StrikeGroupData Data;

    public List<SlotShipButton> Slots => slots;

    public Transform shipModelsRoot = null;
    public List<List<GameObject>> shipModels;


    [SerializeField]
    private List<SlotShipButton> slots = null;

    [SerializeField]
    private ShipButton shipButtonPrefab = null;
    [SerializeField]
    private GameObject shipButtonsPanel = null;
    [SerializeField]
    private Transform shipButtonsRoot = null;

    private List<ShipButton> shipButtons;

    [SerializeField]
    private Text DefenseBonusStat = null;
    [SerializeField]
    private Text EscortBonus = null;
    [SerializeField]
    private Text MaxSupply = null;
    [SerializeField]
    private Text MaxSquadrons = null;
    private List<string> currentStats = new List<string>();

    [SerializeField]
    private UpgradeEscortSlot upgradeButton = null;

    [SerializeField]
    private Image upgradeButtonIcon = null;

    [SerializeField]
    private int baseUpgradeCost = 1;
    private int upgradeCost = 0;

    private int upgradeIconPtr = 0;
    [SerializeField]
    List<GameObject> upgradeLevelImages = new List<GameObject>();

    [SerializeField]
    private string textConfirmWindow = "";

    [SerializeField]
    private int startSlots = 3;
    [SerializeField]
    private int maxSlots = 5;

    private IntermissionPanel mainPanel;
    private ConfirmWindow confirmWindow;

    public void Setup(IntermissionPanel mainPanel, ConfirmWindow confirmWindow)
    {
        this.mainPanel = mainPanel;
        this.confirmWindow = confirmWindow;

        upgradeIconPtr = 0;
        upgradeCost = baseUpgradeCost;
        shipModels = new List<List<GameObject>>();
        for (int i = 0; i < shipModelsRoot.childCount; ++i)
        {
            var slot = shipModelsRoot.GetChild(i);
            List<GameObject> list = new List<GameObject>();
            for (int j = 0; j < Data.Data.Count; ++j)
            {
                var spawned = Instantiate(Data.Data[j].IntermissionPrefab, slot);
                spawned.SetActive(false);
                list.Add(spawned);
            }
            shipModels.Add(list);
        }
        for (int i = 0; i < slots.Count; ++i)
        {
            slots[i].Setup(i);
        }
        shipButtons = new List<ShipButton>();
        for (int i = 0; i < Data.Data.Count; ++i)
        {
            var button = Instantiate(shipButtonPrefab, shipButtonsRoot);
            button.Setup(this, shipButtonsPanel, i, Data.Data[i]);
            shipButtons.Add(button);
        }

        LoadFromSave();

        ShowStats();
        IntermissionPanel.Instance.UpdateStatisticsWindow();

        upgradeButton.button.onClick.AddListener(ClickUpgrade);

        RefreshUpgradeButton();
    }

    public override void Hide()
    {
        base.Hide();
        shipButtonsPanel.SetActive(false);
        if (ShipButton.CurrentChosenSlot != null)
        {
            ShipButton.CurrentChosenSlot.Deselect();
        }
    }

    public void SetTooltipsPosition(bool last)
    {
        foreach (var button in shipButtons)
        {
            button.SwitchTooltip(last);
        }
    }

    public void ShowStats()
    {
        int stat1 = 0;
        int stat2 = 0;
        int stat3 = 0;
        int stat4 = 0;

        int max = 0;/*SaveManager.Instance.Data.ConvoySlots;*/
        for (int index = 0; index < max; index++)
        {
            var data = Data.Data[slots[index].ShipIndex];
            for (int i = 0; i < data.PassiveSkills.Count; ++i)
            {
                switch (data.PassiveSkills[i].Skill)
                {
                    case EStrikeGroupPassiveSkill.MaxSupplies:
                        stat3 += data.PassiveSkills[i].Param;
                        break;
                    case EStrikeGroupPassiveSkill.Escort:
                        stat2 += data.PassiveSkills[i].Param;
                        break;
                    case EStrikeGroupPassiveSkill.Defense:
                        stat1 += data.PassiveSkills[i].Param;
                        break;
                    case EStrikeGroupPassiveSkill.MaxSquadrons:
                        stat4 += data.PassiveSkills[i].Param;
                        break;
                }
            }
        }

        DefenseBonusStat.text = "+" + stat1;
        EscortBonus.text = "+" + stat2;
        MaxSupply.text = stat3 + "%";
        MaxSquadrons.text = "+" + stat4;
    }

    public void SaveCurrentStats()
    {
        currentStats.Clear();
        currentStats.Add(DefenseBonusStat.text);
        currentStats.Add(EscortBonus.text);
        currentStats.Add(MaxSupply.text);
        currentStats.Add(MaxSquadrons.text);
    }

    public void BackToCurrentStats()
    {
        DefenseBonusStat.text = currentStats[0];
        EscortBonus.text = currentStats[1];
        MaxSupply.text = currentStats[2];
        MaxSquadrons.text = currentStats[3];
    }

    public void SaveEscortSlotsState()
    {
        var data = SaveManager.Instance.Data;
        data.IntermissionData.SelectedEscort.Clear();
        foreach (SlotShipButton slot in Slots)
        {
            if (slot.IsUnlock)
            {
                data.IntermissionData.SelectedEscort.Add(slot.ShipIndex);
            }
        }
    }

    public void LoadFromSave()
    {
        var saveData = SaveManager.Instance.Data;
        int diffSlots = /*saveData.ConvoySlots - */startSlots;
        if (diffSlots > 0)
        {
            upgradeIconPtr = diffSlots;
            upgradeCost += diffSlots * baseUpgradeCost;

            if (upgradeIconPtr >= mainPanel.UpgradeSprites.Count)
            {
                upgradeIconPtr = mainPanel.UpgradeSprites.Count - 1;
            }
            upgradeButtonIcon.sprite = mainPanel.UpgradeSprites[upgradeIconPtr];
            SetUpgradeImages();
            //if (saveData.ConvoySlots == maxSlots)
            //{
            //    upgradeButton.gameObject.SetActive(false);
            //}
        }
        var locMan = LocalizationManager.Instance;

        for (int i = 0; i < saveData.IntermissionData.SelectedEscort.Count; i++)
        {
            EnableSlot(i, false);
            if (saveData.IntermissionData.SelectedEscort[i] != -1)
            {
                var escortData = Data.Data[saveData.IntermissionData.SelectedEscort[i]];

                slots[i].ShipIndex = saveData.IntermissionData.SelectedEscort[i];
                slots[i].LastChosenIndex = slots[i].ShipIndex;

                shipModels[i][saveData.IntermissionData.SelectedEscort[i]].SetActive(true);
                slots[i].ShipIcon.sprite = escortData.Icon;

                slots[i].ShipName.text = locMan.GetText(escortData.NameID);
                slots[i].Deselect();
            }
        }
    }

    public override void RefreshUpgradeButton()
    {
        base.RefreshUpgradeButton();
        if (SaveManager.Instance.Data.IntermissionMissionData.UpgradePoints < upgradeCost)
        {
            upgradeButton.Disable();
            var icon = upgradeButton.transform.GetChild(0);
            icon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
        }
    }

    public void EnableSlot(int index, bool showShipAndIcon)
    {
        slots[index].IsUnlock = true;
        if (showShipAndIcon)
        {
            slots[index].ShowShipAndIcon();
        }
        slots[index].ActivateSlot();
        slots[index].Button.interactable = true;
    }

    public override void ConfirmUpgrade()
    {
        base.ConfirmUpgrade();
        shipButtonsPanel.SetActive(false);
        var data = SaveManager.Instance.Data;

        int diff = maxSlots;/* - data.ConvoySlots;*/
        if (diff > 0)
        {
            //EnableSlot(data.ConvoySlots, true);

            //data.ConvoySlots++;
            ShowStats();
            upgradeIconPtr++;
        }
        if (diff < 2)
        {
            upgradeButton.gameObject.SetActive(false);
        }

        if (upgradeIconPtr >= mainPanel.UpgradeSprites.Count)
        {
            upgradeIconPtr = mainPanel.UpgradeSprites.Count - 1;
        }
        upgradeButtonIcon.sprite = mainPanel.UpgradeSprites[upgradeIconPtr];

        IntermissionPanel.Instance.ReduceUpgradePoints(upgradeCost);
        ++upgradeCost;
        SetUpgradeImages();
    }

    private void ClickUpgrade()
    {
        confirmWindow.Setup(EDeckUpgradeType.Escort, textConfirmWindow, mainPanel.UpgradeSprites[upgradeIconPtr]);
        mainPanel.ActivateClickBlocker(true);
        shipButtonsPanel.SetActive(false);
        if (ShipButton.CurrentChosenSlot != null)
        {
            ShipButton.CurrentChosenSlot.Deselect();
        }
    }

    private void SetUpgradeImages()
    {
        int diffSlots =/* SaveManager.Instance.Data.ConvoySlots - */startSlots;
        for (int i = 0; i < upgradeLevelImages.Count / 2; ++i)
        {
            if (diffSlots > i)
            {
                upgradeLevelImages[i].SetActive(true);
                upgradeLevelImages[i + 2].SetActive(false);
            }
            else
            {
                upgradeLevelImages[i].SetActive(false);
                upgradeLevelImages[i + 2].SetActive(true);
            }
        }
    }
}

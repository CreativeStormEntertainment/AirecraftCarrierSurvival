using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarrierSubpanel : IntermissionSubpanel
{
    [SerializeField]
    private InputField shipName = null;

    [SerializeField]
    private List<UpgradeAircraftCarrier> upgradeButtons = null;
    [SerializeField]
    private List<Image> upgradeButtonIcons = null;
    private List<int> upgradeButtonIconIndexes;

    [SerializeField]
    private List<int> upgradeCosts = new List<int>();

    [SerializeField]
    private AircraftCarrierTypeChoose carrierTypeChoose = null;


    private IntermissionPanel mainPanel;
    private ConfirmWindow confirmWindow;
    private int selectedUpgradeButton = 0;

    public void Setup(IntermissionPanel mainPanel, ConfirmWindow confirmWindow)
    {
        this.mainPanel = mainPanel;
        this.confirmWindow = confirmWindow;
        upgradeButtonIconIndexes = new List<int>();
        var data = SaveManager.Instance.Data;
        for (int i = 0; i < upgradeButtons.Count; ++i)
        {
            int j = i;
            upgradeButtons[i].Setup(mainPanel.UpgradeSprites);
            upgradeButtons[i].button.onClick.AddListener(() => selectedUpgradeButton = j);
            upgradeButtonIconIndexes.Add(0);
            while (upgradeButtons[i].UpgradeTier < data.CarrierUpgrades[i + 1])
            {
                SetupUpgrade(i);
            }
        }
        shipName.text = data.CarrierName;
        RefreshUpgradeButton();
    }

    public override void Hide()
    {
        base.Hide();
        carrierTypeChoose.SetImageAndPopUp(false);
    }

    public override void RefreshUpgradeButton()
    {
        base.RefreshUpgradeButton();
        int upgradePoints = SaveManager.Instance.Data.IntermissionMissionData.UpgradePoints;
        for (int i = 0; i < upgradeButtons.Count; ++i)
        {
            if (upgradePoints < upgradeCosts[i])
            {
                upgradeButtons[i].Disable();
                var icon = upgradeButtons[i].transform.GetChild(0);
                icon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
            }
        }
    }

    public override void ConfirmUpgrade()
    {
        base.ConfirmUpgrade();
        upgradeButtons[selectedUpgradeButton].ConfirmUpgrade();
        IntermissionPanel.Instance.ReduceUpgradePoints(upgradeCosts[selectedUpgradeButton]);

        ++upgradeButtonIconIndexes[selectedUpgradeButton];
        upgradeButtonIcons[selectedUpgradeButton].sprite = mainPanel.UpgradeSprites[upgradeButtonIconIndexes[selectedUpgradeButton]];

        ++upgradeCosts[selectedUpgradeButton];
    }

    void SetupUpgrade(int n)
    {
        upgradeButtons[n].ConfirmUpgrade();

        ++upgradeButtonIconIndexes[n];
        upgradeButtonIcons[n].sprite = mainPanel.UpgradeSprites[upgradeButtonIconIndexes[n]];

        ++upgradeCosts[n];
    }

    public override void StopUpgrade()
    {
        base.StopUpgrade();
        upgradeButtons[selectedUpgradeButton].StopUpgrade();
    }

    public void SaveUpgrades()
    {
        var data = SaveManager.Instance.Data;
        data.CarrierName = shipName.text;

        data.CarrierUpgrades.Clear();
        data.CarrierUpgrades.Add(3);
        for (int i = 0; i < upgradeButtons.Count; ++i)
        {
            data.CarrierUpgrades.Add(upgradeButtons[i].UpgradeTier);
        }
        data.CarrierUpgrades.Add(1);
    }
}

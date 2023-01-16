using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AircraftSubpanel : IntermissionSubpanel
{
    [SerializeField]
    private UpgradeShipButton upgradeSlotButton = null;
    [SerializeField]
    private List<UpgradePlanesButton> upgradeButtons = new List<UpgradePlanesButton>();

    [SerializeField]
    List<GameObject> upgradeLevelImages = new List<GameObject>();

    [SerializeField]
    private List<int> upgradeCosts = new List<int>();

    [SerializeField]
    private List<SetPlaneColor> planeColors = new List<SetPlaneColor>();

    private IntermissionPanel mainPanel;
    private ConfirmWindow confirmWindow;

    private int selectedUpgradeButton = 0;

    private List<PlaneTypeIntWrapper> planeMaterialsIndices;

    public void Setup(IntermissionPanel mainPanel, ConfirmWindow confirmWindow)
    {
        planeMaterialsIndices = new List<PlaneTypeIntWrapper>();
        this.mainPanel = mainPanel;
        this.confirmWindow = confirmWindow;
        upgradeSlotButton.Setup(mainPanel.UpgradeSprites);
        upgradeSlotButton.button.onClick.AddListener(() => selectedUpgradeButton = 0);
        for (int i = 0; i < upgradeButtons.Count; ++i)
        {
            int j = i+1;
            upgradeButtons[i].button.onClick.AddListener(() => selectedUpgradeButton = j);
            upgradeButtons[i].Setup(mainPanel.UpgradeSprites);
        }
        for (int i = 0; i < planeColors.Count; ++i)
        {
            planeColors[i].Setup(this);
        }
        RefreshUpgradeButton();
    }

    public void LoadFromSave()
    {
        SetUpgradeImages();
    }
    /*
    [SerializeField] private List<ButtonPlane> planesIcons = null;
    //[SerializeField] private ButtonIntermissionPanel planeButton = null;
    //[SerializeField] private Text numberOfSelected = null;

    private void Start()
    {
        SetAirPlaneIcons();
        ShowSelectedPlanes();
    }


    public void SetAirPlaneIcons()
    {
        var planesList = IntermissionManager.Instance.GetPlaneList();

        int planeIndex = 0;
        foreach(ButtonPlane plane in planesIcons)
        {
            var currentPlane = planesList[planeIndex];

            plane.PlaneIcon.sprite = currentPlane.GetImageIcon();

            if ((SaveManager.Instance.Data.LockedPlaneTypes & (1 << (planeIndex))) == (1 << (planeIndex))) // if plane is locked
            {
                plane.LockedIcon.SetActive(true);
                plane.LockedIcon.GetComponent<Image>().sprite = currentPlane.GetLockedIcon();

                plane.PriceText.SetActive(true);
                plane.Price.SetActive(true);
                plane.Price.GetComponent<Text>().text = currentPlane.GetUnlockCost().ToString();
            }
            planeIndex++;
        }

    }

    public void ShowSelectedPlanes()
    {
        //var chosenPlanes = SaveManager.Instance.Data.Planes;
        //foreach (ButtonPlane plane in planesIcons)
        //{
        //    if(chosenPlanes.Contains(plane.GetPlaneType()))
        //    {
        //        plane.IsChosen = true;

        //        plane.GetComponent<Image>().color = new Color32(255, 255, 0, 255);
        //    }
        //}
        //planeButton.choicesNumber = chosenPlanes.Count;
        //numberOfSelected.text = planeButton.choicesNumber.ToString();
    }
    */

    public override void RefreshUpgradeButton()
    {
        base.RefreshUpgradeButton();
        var data = SaveManager.Instance.Data;
        if (data.IntermissionMissionData.UpgradePoints < upgradeCosts[0])
        {
            upgradeSlotButton.Disable();
            var icon = upgradeSlotButton.transform.GetChild(0);
            icon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
        }
        for (int i = 0; i < upgradeButtons.Count; ++i)
        {
            if (data.IntermissionMissionData.UpgradePoints < upgradeCosts[i + 1])
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
        if (selectedUpgradeButton == 0)
        {
            upgradeSlotButton.ConfirmUpgrade();
        }
        else
        {
            upgradeButtons[selectedUpgradeButton-1].ConfirmUpgrade();
        }
        IntermissionPanel.Instance.ReduceUpgradePoints(upgradeCosts[selectedUpgradeButton]);
        ++upgradeCosts[selectedUpgradeButton];
        SetUpgradeImages();
    }

    public override void StopUpgrade()
    {
        base.StopUpgrade();
        if (selectedUpgradeButton == 0)
        {

        }
        else
        {
            upgradeButtons[selectedUpgradeButton-1].StopUpgrade();
        }
    }

    public void UpdatePlaneColor(EPlaneType planeType, int colorIndex)
    {
        PlaneTypeIntWrapper wrapper = planeMaterialsIndices.Find(x => x.Type == planeType);

        if (wrapper == null)
        {
            wrapper = new PlaneTypeIntWrapper(planeType, 0);
            planeMaterialsIndices.Add(wrapper);
        }

        wrapper.Value = colorIndex;
    }

    public void SaveUpgrades()
    {
        SaveManager.Instance.Data.PlaneColorIndices = planeMaterialsIndices;
    }

    private void SetUpgradeImages()
    {
        //for (int i = 0; i < upgradeLevelImages.Count / 2; ++i)
        //{
        //    if (saveData.ConvoyUpgradeLevel > i)
        //    {
        //        upgradeLevelImages[i].SetActive(true);
        //        upgradeLevelImages[i + 2].SetActive(false);
        //    }
        //    else
        //    {
        //        upgradeLevelImages[i].SetActive(false);
        //        upgradeLevelImages[i + 2].SetActive(true);
        //    }
        //}
    }
}

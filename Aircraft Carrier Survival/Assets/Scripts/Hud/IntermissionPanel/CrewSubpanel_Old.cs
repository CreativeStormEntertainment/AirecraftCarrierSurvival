using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class CrewSubpanel_Old : IntermissionSubpanel
{
    public List<CrewData> CurrentCrew;

    public int SpecialityCost => specialityCost;

    [SerializeField]
    private CrewDataList crews = null;

    [SerializeField]
    private OfficerList officers = null;

    [SerializeField]
    private List<SlotCrewButton> officerButtons = new List<SlotCrewButton>();
    [SerializeField]
    private List<SlotCrewButton> crewButtons = new List<SlotCrewButton>();

    [SerializeField]
    private int crewUpgradeSlotCost = 0;
    [SerializeField]
    private int specialityCost = 1;


    [SerializeField]
    private Button upgradeOfficerButton = null;
    [SerializeField]
    private Image upgradeOfficerButtonIcon;
    [SerializeField]
    private ShowTooltip officerButtonTooltip = null;
    [SerializeField]
    private Button upgradeCrewButton = null;
    [SerializeField]
    private Image upgradeCrewButtonIcon = null;
    [SerializeField]
    private Sprite buttonBackgroundDisabled = null;
    [SerializeField]
    private ShowTooltip crewButtonTooltip = null;
    /*
    [SerializeField]
    private Sprite lockedButtonImage = null;
    */

    private bool isUpgradingCrewSlot = false;

    [SerializeField]
    private List<GameObject> officerUpgrade = null;

    [SerializeField]
    private List<GameObject> officerEmpty = null;

    [SerializeField]
    private List<GameObject> officerUnlockedPortrait = null;

    [SerializeField]
    private List<GameObject> officerLockedPortrait = null;

    [SerializeField]
    private List<GameObject> crewUpgrade = null;

    [SerializeField]
    private List<GameObject> crewEmpty = null;

    [SerializeField]
    private List<GameObject> crewUnlockedPortrait = null;

    [SerializeField]
    private List<GameObject> crewLockedPortrait = null;

    [SerializeField]
    private int visualizationBaseCount = 100;
    [SerializeField]
    private int visualizationUpgradeCount = 50;
    [SerializeField]
    private List<GameObject> visualization = null;

    //[SerializeField]
    //private Text supplyText = null;
    //[SerializeField]
    //private Text radarText = null;
    //[SerializeField]
    //private Text defenseText = null;

    private int officerSlotLevel = 0;
    private int crewSlotLevel = 0;
    private int crewButtonIconIndex = 0;

    [SerializeField]
    private string officerTextPopup = null;
    [SerializeField]
    private string crewTextPopup = null;
    private List<Animator> visualizationAnimators = new List<Animator>();

    private IntermissionPanel mainPanel;
    private ConfirmWindow confirmWindow;

    public void Setup(IntermissionPanel mainPanel, ConfirmWindow confirmWindow)
    {
        this.mainPanel = mainPanel;
        this.confirmWindow = confirmWindow;

        CurrentCrew = new List<CrewData>(crews.List);

        var data = SaveManager.Instance.Data;

        for (int i = 0; i < data.CrewmenSpecialty.Count; i++)
        {
            var crewData = new CrewData();
            crewData.Portrait = CurrentCrew[i].Portrait;
            crewData.GreySprite = CurrentCrew[i].GreySprite;
            crewData.Rank = CurrentCrew[i].Rank;
            crewData.Specialty = data.CrewmenSpecialty[i];
            CurrentCrew[i] = crewData;
        }

        upgradeOfficerButton.onClick.AddListener(BuyOfficerSlot);
        upgradeCrewButton.onClick.AddListener(BuyCrewSlot);

        InitVisualization();

        if (data.UpgradedCrews > 8)
        {
            UpgradeCrew();
            ++crewUpgradeSlotCost;
            if (data.UpgradedCrews > 10)
            {
                UpgradeCrew();
            }
        }

        var officersSetup = new List<OfficerSetup>(officers.Officers);

        var admiralSkills = new List<OfficerSkill>() { new OfficerSkill(EOfficerSkills.CommandingAirForce, data.AdmiralAirLevel), new OfficerSkill(EOfficerSkills.CommandingNavy, data.AdmiralNavyLevel) };
        for (int i = 0; i < 3; ++i)
        {
            officerButtons[i].Setup(this, i, officers.Portraits[officersSetup[i].PortraitNumber].Square, null, officersSetup[i]);
        }
        for (int i = 0; i < crewButtons.Count; ++i)
        {
            crewButtons[i].Setup(this, i, CurrentCrew[i].Portrait, crews.Ranks[CurrentCrew[i].Rank]);
        }
    }

    public void BuyCrewSlot()
    {
        isUpgradingCrewSlot = true;
        confirmWindow.Setup(EDeckUpgradeType.Crew, crewTextPopup, mainPanel.UpgradeSprites[0]);
        IntermissionPanel.Instance.ActivateClickBlocker(true);
    }

    public void BuyOfficerSlot()
    {
        isUpgradingCrewSlot = false;
        confirmWindow.Setup(EDeckUpgradeType.Crew, officerTextPopup, mainPanel.UpgradeSprites[0]);
        IntermissionPanel.Instance.ActivateClickBlocker(true);
    }

    public override void ConfirmUpgrade()
    {
        base.ConfirmUpgrade();
        if (isUpgradingCrewSlot)
        {
            SaveManager.Instance.Data.UpgradedCrews += 2;
            UpgradeCrew();
            IntermissionPanel.Instance.ReduceUpgradePoints(crewUpgradeSlotCost);
            ++crewUpgradeSlotCost;
        }
        else
        {
            SetOfficerUpgrade(officerSlotLevel + 1);
        }
        confirmWindow.gameObject.SetActive(false);
    }

    public void SetOfficerUpgrade(int level)
    {
        if (level > officerUpgrade.Count)
        {
            return;
        }
        officerSlotLevel = level;
        for (int i = 0; i < officerUpgrade.Count; ++i)
        {
            officerUpgrade[i].SetActive(level > i);
            officerUnlockedPortrait[i].SetActive(level > i);
        }

        for (int i = 0; i < officerEmpty.Count; ++i)
        {
            officerEmpty[i].SetActive(level <= i);
            officerLockedPortrait[i].SetActive(officerEmpty[i].activeSelf);
        }
    }

    public void SetCrewUpgrade(int level)
    {
        if (level > crewUpgrade.Count)
        {
            return;
        }
        crewSlotLevel = level;
        for (int i = 0; i < crewUpgrade.Count; ++i)
        {
            crewUpgrade[i].SetActive(level > i);
            crewUnlockedPortrait[i].SetActive(level > i);
        }

        for (int i = 0; i < crewEmpty.Count; ++i)
        {
            crewEmpty[i].SetActive(level <= i);
            crewLockedPortrait[i].SetActive(crewEmpty[i].activeSelf);
        }
        int visualizationRange = visualizationBaseCount + level * visualizationUpgradeCount;
        for (int i = 0; i < visualization.Count; ++i)
        {
            bool wasEnabled = visualization[i].activeSelf;
            visualization[i].SetActive(i <= visualizationRange);
            if (!wasEnabled)
            {
                visualizationAnimators[i].SetTrigger("NextAnim");
                visualizationAnimators[i].SetInteger("CurrentIdle", Random.Range(0, 9));
            }
        }
    }

    public void InitVisualization()
    {
        visualizationAnimators.Clear();
        foreach (var vis in visualization)
        {
            visualizationAnimators.Add(vis ? vis.GetComponent<Animator>() : null);
        }

        for (int i = 0; i < visualization.Count; ++i)
        {
            //if (!visualization[i])
            //    continue;

            if (i >= visualizationBaseCount)
            {
                visualization[i].SetActive(false);
            }
            else
            {
                visualization[i].SetActive(true);
                visualizationAnimators[i].SetTrigger("NextAnim");
                visualizationAnimators[i].SetInteger("CurrentIdle", Random.Range(0, 9));
            }
        }
    }


    public void SaveCrewmenWithSpecialty()
    {
        List<ECrewmanSpecialty> crewmanSpecialties = new List<ECrewmanSpecialty>();

        //var crewmen = IntermissionPanel.Instance.Crew.Crewmen;

        var specialties = SaveManager.Instance.Data.CrewmenSpecialty;
        specialties.Clear();
        foreach (var crew in CurrentCrew)
        {
            specialties.Add(crew.Specialty);
        }
    }

    public override void RefreshUpgradeButton()
    {
        base.RefreshUpgradeButton();
        //if (saveData.UpgradePoints < officerUpgradeSlotCost)
        //{
        //    //upgradeOfficerButton.image.sprite = buttonBackgroundDisabled;
        upgradeOfficerButton.interactable = false;
        upgradeOfficerButtonIcon.color = new Color(1f, 1f, 1f, 0.25f);
        ++officerButtonTooltip.IndexTooltip;
        //}

        if (SaveManager.Instance.Data.IntermissionMissionData.UpgradePoints < crewUpgradeSlotCost)
        {
            //upgradeCrewButton.image.sprite = buttonBackgroundDisabled;
            upgradeCrewButton.interactable = false;
            upgradeCrewButtonIcon.color = new Color(1f, 1f, 1f, 0.25f);
            ++crewButtonTooltip.IndexTooltip;
        }
    }

    private void UpgradeCrew()
    {
        SetCrewUpgrade(crewSlotLevel + 2);
        ++crewButtonIconIndex;
        if (crewSlotLevel >= crewUpgrade.Count)
        {
            upgradeCrewButton.gameObject.SetActive(false);
        }
        else
        {
            upgradeCrewButtonIcon.sprite = mainPanel.UpgradeSprites[crewButtonIconIndex];
        }
    }
}

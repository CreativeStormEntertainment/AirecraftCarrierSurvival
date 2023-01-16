using GambitUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntermissionPanel : MonoBehaviour
{
    public static IntermissionPanel Instance;

    public DockPanel Dock;
    //public CrewSubpanel_Old Crew;
    //public AircraftSubpanel Aircraft;
    //public EscortSubpanel Escort;

    public ConfirmWindow ConfirmWindow;

    public List<Sprite> UpgradeSprites = new List<Sprite>();
    public List<List<IntermissionMissionBtn>> IntermissionMissionBtns { get; private set; }

    [SerializeField]
    private Text commandPoints = null;
    [SerializeField]
    private Text upgradePoints = null;

    [SerializeField]
    private List<MissionDescription> availableMissions;
    [SerializeField]
    private ButtonAircraftCarrierType firstAircraftTypeToSet = null;

    //[SerializeField]
    //private CrewSubpanel_Old crewSubpanel = null;

    [SerializeField]
    private GameObject clickBlocker = null;

    [SerializeField]
    private Transform chaptersContainer = null;
    [SerializeField]
    private Button debugButton = null;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ConfirmWindow.Setup();
        //Crew.Setup(this, ConfirmWindow);
        //Aircraft.Setup(this, ConfirmWindow);
        //Escort.Setup(this, ConfirmWindow);

        var data = SaveManager.Instance.Data;
        commandPoints.text = data.IntermissionMissionData.CommandsPoints.ToString();
        upgradePoints.text = data.IntermissionMissionData.UpgradePoints.ToString();

        firstAircraftTypeToSet.SetNameAndType();

        CreateMissions();
        RefreshUpgradeButtons();

        if (debugButton != null)
        {
            debugButton.onClick.AddListener(Unlock);
        }
    }

    public void CreateMissions()
    {
        //Create list of missions
        var data = SaveManager.Instance.Data;
        //if (sMan.GameMode == EGameMode.Fabular)
        {
            IntermissionMissionBtns = new List<List<IntermissionMissionBtn>>();
            foreach (Transform chapter in chaptersContainer)
            {
                var pomList = new List<IntermissionMissionBtn>();
                foreach (Transform mission in chapter)
                {
                    if (mission.TryGetComponent(out IntermissionMissionBtn btn))
                    {
                        pomList.Add(btn);
                    }
                    else
                    {
                        Debug.LogError("Thrashy object");
                    }
                }
                chapter.gameObject.SetActive(pomList[0].ChapterID == data.CurrentChapter);
                IntermissionMissionBtns.Add(pomList);
            }
        }
    }

    public void Unlock()
    {
        debugButton.onClick.RemoveListener(Unlock);
        debugButton.interactable = false;
        foreach (var list in IntermissionMissionBtns)
        {
            list[0].transform.parent.gameObject.SetActive(true);
        }
        this.StartCoroutineActionAfterFrames(() => 
            {
                foreach (var list in IntermissionMissionBtns)
                {
                    foreach (var button in list)
                    {
                        button.Unlock();
                    }
                }
            }, 3);
    }

    public void ReduceCommandPoints(int cost)
    {
        var data = SaveManager.Instance.Data;
        data.IntermissionMissionData.CommandsPoints -= cost;

        commandPoints.text = data.IntermissionMissionData.CommandsPoints.ToString();
    }

    public void ReduceUpgradePoints(int cost)
    {
        var data = SaveManager.Instance.Data;
        data.IntermissionMissionData.UpgradePoints -= cost;

        upgradePoints.text = data.IntermissionMissionData.UpgradePoints.ToString();
    }

    public bool CheckUpgradePoints(int cost)
    {
        return SaveManager.Instance.Data.IntermissionMissionData.UpgradePoints >= cost;
    }

    public void RefreshUpgradeButtons()
    {
        //Crew.RefreshUpgradeButton();
        //Aircraft.RefreshUpgradeButton();
        //Escort.RefreshUpgradeButton();
    }

    public void ActivateClickBlocker(bool isActive)
    {
        clickBlocker.SetActive(isActive);
    }



    public void UpdateStatisticsWindow()
    {
        //supplyStorageText.text = supplyStorageTexts[upgradeAircraftCarrier[0].UpgradeTier - 1];
        //radarRangeText.text = radarRangeTexts[upgradeAircraftCarrier[1].UpgradeTier - 1];

        //int defence = upgradeAircraftCarrier[2].UpgradeTier;
        //for (int index = 0; index < saveData.ConvoyUnlockedSlotsNumber; index++)
        //{
            //var shipInSlot = EscortSubpanel.Instance.Slots[index].ShipType;
            //var shipStats = IntermissionManager.Instance.GetShipList()[(int)shipInSlot];
            //defence += shipStats.GetDefenceBonus();
        //}
        //defenceBonusText.text = defence.ToString();
    }

    public void ConfirmUpgrade(EDeckUpgradeType upgrade)
    {
        switch (upgrade)
        {
            case EDeckUpgradeType.Aircraft:
                //Aircraft.ConfirmUpgrade();
                break;


            case EDeckUpgradeType.Crew:
                //Crew.ConfirmUpgrade();
                break;

            case EDeckUpgradeType.Escort:
                //Escort.ConfirmUpgrade();
                break;
        }
        RefreshUpgradeButtons();
    }

    public void StopUpgrade(EDeckUpgradeType upgrade)
    {
        switch (upgrade)
        {
            case EDeckUpgradeType.Aircraft:
                //Aircraft.StopUpgrade();
                break;

            case EDeckUpgradeType.Crew:
            case EDeckUpgradeType.Escort:
                break;
        }
    }

    public void SaveUpgrades()
    {
        //Escort.SaveEscortSlotsState();
        //Crew.SaveCrewmenWithSpecialty();
        //Aircraft.SaveUpgrades();
    }
}

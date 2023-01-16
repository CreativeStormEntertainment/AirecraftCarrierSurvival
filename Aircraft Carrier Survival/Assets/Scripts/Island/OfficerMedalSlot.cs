using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OfficerMedalSlot : DraggableMedalSlot
{
    [SerializeField]
    private Image officerImage = null;
    [SerializeField]
    private GameObject navyImage = null;
    [SerializeField]
    private GameObject airImage = null;
    [SerializeField]
    private Text navyText = null;
    [SerializeField]
    private Text airText = null;
    [SerializeField]
    private List<OfficerLevelImage> levelImages = null;

    private int officer;
    private OfficerUpgradeWindow officerUpgradeWindow;
    private MedalsWindow medalsWindow;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        var officersUpgrades = GameStateManager.Instance.MissionSuccessPopup.MissionRewards.OfficersUpgrades;
        var upgrade = officersUpgrades[officer];
        medalsWindow.CrewInspector.Setup(upgrade, officer);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        medalsWindow.CrewInspector.Hide();
    }

    public void SetupOfficer(int index, OfficerUpgradeWindow upgradeWindow, MedalsWindow medalsWindow)
    {
        medalImage.SetActive(false);
        officer = index;
        officerUpgradeWindow = upgradeWindow;
        this.medalsWindow = medalsWindow;
        var officerList = IslandsAndOfficersManager.Instance.OfficerList;
        var data = officerList.Portraits[officerList.Officers[officer].PortraitNumber];
        officerImage.sprite = data.Square;
        UpdateLevelVisualization();
        Blocked = false;
    }

    public override void AssignMedal(DraggableMedal medal)
    {
        if (!Blocked)
        {
            var officerList = IslandsAndOfficersManager.Instance.OfficerList;
            base.AssignMedal(medal);
            Blocked = true;
            var stateMan = GameStateManager.Instance;
            var officersUpgrades = stateMan.MissionSuccessPopup.MissionRewards.OfficersUpgrades;
            var upgrade = officersUpgrades[officer];
            var threshold = officerList.OfficerLevelThreshold;
            int level = upgrade.GetLevel(threshold);

            stateMan.FireOfficerGainedMedal(++upgrade.Medals);

            int newLevel = upgrade.GetLevel(threshold);

            if (level != newLevel)
            {
                if (newLevel == 1)
                {
                    officerUpgradeWindow.SetupManeuver(officer);
                }
                else
                {
                    officerUpgradeWindow.SetupUpgrades(officer);
                }
                medalsWindow.LevelUpSound.Play();
            }
            else
            {
                medalsWindow.MedalSound.Play();
            }

            officersUpgrades[officer] = upgrade;
            UpdateLevelVisualization();
        }
    }

    public void UpdateLevelVisualization()
    {
        var officersUpgrades = GameStateManager.Instance.MissionSuccessPopup.MissionRewards.OfficersUpgrades;
        var thresholds = IslandsAndOfficersManager.Instance.OfficerList.OfficerLevelThreshold;
        var upgrade = officersUpgrades[officer];
        int level = upgrade.GetLevel(thresholds);
        levelImage.sprite = levelFrames[level];
        for (int i = 0; i < levelImages.Count; i++)
        {
            levelImages[i].SetFillAmount(0f);
        }
        int medals = upgrade.Medals;
        int lowerThreshold = 0;
        for (int i = 0; i <= level; i++)
        {
            if (levelImages.Count > i)
            {
                levelImages[i].SetFillAmount((float)(medals - lowerThreshold) / (thresholds[i] - lowerThreshold));
                lowerThreshold = thresholds[i];
            }
        }
        medalsCount.text = upgrade.Medals.ToString();
        var officerData = IslandsAndOfficersManager.Instance.OfficerList.Officers[officer];
        bool showNavy = IslandsAndOfficersManager.Instance.OfficerList.Officers[officer].HasNavy;
        navyImage.gameObject.SetActive(showNavy);
        airImage.gameObject.SetActive(!showNavy);
        airText.text = (officerData.BaseAirLvl + officersUpgrades[officer].UpgradedAirPoints).ToString();
        navyText.text = (officerData.BaseNavyLvl + officersUpgrades[officer].UpgradedNavyPoints).ToString();
    }

    //private List<int> RandomManeuvers()
    //{
    //    var tacMan = TacticManager.Instance;
    //    var availableManeuvers = new List<int>();
    //    for (int i = tacMan.PlayerManeuversList.AdmiralManeuversCount - 1; i < tacMan.AllPlayerManeuvers.Count; i++)
    //    {
    //        if (SaveManager.Instance.Data.MissionRewards.OfficersUpgrades.FindIndex(item => item.SelectedManeuver == i) == -1)
    //        {
    //            availableManeuvers.Add(i);
    //        }
    //    }
    //    while (availableManeuvers.Count > 2)
    //    {
    //        availableManeuvers.RemoveAt(Random.Range(0, availableManeuvers.Count));
    //    }
    //    return availableManeuvers;
    //}
}

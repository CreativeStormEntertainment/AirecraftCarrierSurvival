using UnityEngine;
using UnityEngine.UI;

public class OfficerUpgradeWindow : MonoBehaviour
{
    [SerializeField]
    private ManeuversCard cardA = null;
    [SerializeField]
    private ManeuversCard cardB = null;
    [SerializeField]
    private ManeuversCard card = null;
    [SerializeField]
    private Button navyPoint = null;
    [SerializeField]
    private Button airPoint = null;

    private int officerIndex;
    private MedalsWindow medalsWindow;

    private void Start()
    {
        navyPoint.onClick.AddListener(() => AddPoint(true));
        airPoint.onClick.AddListener(() => AddPoint(false));
    }
    public void Init(MedalsWindow medalsWindow)
    {
        this.medalsWindow = medalsWindow;
    }

    public void SetupUpgrades(int officerIndex)
    {
        HideAll();
        this.officerIndex = officerIndex;
        var tacMan = TacticManager.Instance;
        var upgrades = SaveManager.Instance.Data.MissionRewards.OfficersUpgrades[officerIndex];
        var maneuver = tacMan.AllPlayerManeuvers[IslandsAndOfficersManager.Instance.OfficerList.Officers[officerIndex].ManeuverIndex];
        var nextLevelManeuver = maneuver;
        switch (upgrades.ManeuverLevel)
        {
            case 1:
                nextLevelManeuver = maneuver.Level2;
                break;
            case 2:
                nextLevelManeuver = maneuver.Level3;
                break;
        }
        bool showNavy = IslandsAndOfficersManager.Instance.OfficerList.Officers[officerIndex].HasNavy;
        navyPoint.gameObject.SetActive(showNavy);
        airPoint.gameObject.SetActive(!showNavy);

        cardA.Setup(nextLevelManeuver, upgrades.ManeuverLevel, this);
        SetShow(true);
    }

    public void SetupManeuver(int officerIndex)
    {
        HideAll();
        var tacMan = TacticManager.Instance;
        card.Setup(tacMan.AllPlayerManeuvers[IslandsAndOfficersManager.Instance.OfficerList.Officers[officerIndex].ManeuverIndex], 1, this);
        this.officerIndex = officerIndex;
        SetShow(true);
    }

    public void SelectManeuver()
    {
        var officersUpgrades = GameStateManager.Instance.MissionSuccessPopup.MissionRewards.OfficersUpgrades;
        var changed = officersUpgrades[officerIndex];

        changed.ManeuverLevel++;
        officersUpgrades[officerIndex] = changed;
        SetShow(false);
    }

    private void AddPoint(bool navy)
    {
        var officersUpgrades = GameStateManager.Instance.MissionSuccessPopup.MissionRewards.OfficersUpgrades;
        var changed = officersUpgrades[officerIndex];
        if (navy)
        {
            changed.UpgradedNavyPoints++;
        }
        else
        {
            changed.UpgradedAirPoints++;
        }
        officersUpgrades[officerIndex] = changed;
        SetShow(false);
    }

    private void HideAll()
    {
        navyPoint.gameObject.SetActive(false);
        airPoint.gameObject.SetActive(false);
        cardA.gameObject.SetActive(false);
        cardB.gameObject.SetActive(false);
        card.gameObject.SetActive(false);
    }

    private void SetShow(bool show)
    {
        gameObject.SetActive(show);
        if (!show)
        {
            medalsWindow.SetContinueButtonEnabled();
            medalsWindow.UpdateVisualization();
        }
    }
}

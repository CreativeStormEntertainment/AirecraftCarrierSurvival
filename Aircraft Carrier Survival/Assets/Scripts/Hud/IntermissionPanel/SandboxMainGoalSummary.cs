using UnityEngine;
using UnityEngine.UI;

public class SandboxMainGoalSummary : MonoBehaviour
{
    public ref MissionRewards MissionRewards => ref missionRewards;

    [SerializeField]
    private GameObject mainPanel = null;
    [SerializeField]
    private GameObject winText = null;
    [SerializeField]
    private GameObject failText = null;
    [SerializeField]
    private MissionSuccessLeftPanel leftPanel = null;
    [SerializeField]
    private Text expPoints = null;
    [SerializeField]
    private Text commandPoints = null;
    [SerializeField]
    private Text upgradePoints = null;
    [SerializeField]
    private Text altExpPoints = null;
    [SerializeField]
    private Text altCommandPoints = null;
    [SerializeField]
    private Text altUpgradePoints = null;
    [SerializeField]
    private Button continueButton = null;
    [SerializeField]
    private float alternativeHeight = 563f;
    [SerializeField]
    private GameObject leftPanelContainer = null;
    [SerializeField]
    private GameObject middlePanel = null;
    [SerializeField]
    private GameObject altMiddlePanel = null;
    [SerializeField]
    private RectTransform mainPanelRect = null;

    private MissionRewards missionRewards;

    private void Start()
    {
        continueButton.onClick.AddListener(Continue);
    }

    public void Setup()
    {
        var saveData = SaveManager.Instance.Data;
        missionRewards = saveData.MissionRewards;
        var mainGoal = saveData.SandboxData.MainGoal;
        int rewardCommandPoints = 0;
        int rewardExp = 0;
        int rewardUpgradePoints = 0;
        bool win = mainGoal.Points >= mainGoal.PointsToComplete;
        if (mainGoal.DaysToFinish == 0)
        {
            rewardCommandPoints = (int)(mainGoal.CommandPoints * (win ? 1f : 0.5f));
            rewardExp = (int)(mainGoal.Exp * (win ? 1f : 0.5f));
            rewardUpgradePoints = (int)(mainGoal.UpgradePoints * (win ? 1f : 0f));
        }
        saveData.IntermissionData.CommandPoints += rewardCommandPoints;
        saveData.IntermissionData.UpgradePoints += rewardUpgradePoints;
        missionRewards.SandboxAdmiralExp += rewardExp;

        IntermissionManager.Instance.UpdatePoints();

        winText.SetActive(win);
        failText.SetActive(!win);

        leftPanel.Setup();

        if (leftPanel.GetRewardsCount() == 0)
        {
            leftPanelContainer.SetActive(false);
            middlePanel.SetActive(false);
            altMiddlePanel.SetActive(true);
            altCommandPoints.text = rewardCommandPoints.ToString();
            altUpgradePoints.text = rewardUpgradePoints.ToString();
            altExpPoints.text = rewardExp.ToString();
            mainPanelRect.sizeDelta = new Vector2(mainPanelRect.sizeDelta.x, alternativeHeight);
        }
        else
        {
            commandPoints.text = rewardCommandPoints.ToString();
            upgradePoints.text = rewardUpgradePoints.ToString();
            expPoints.text = rewardExp.ToString();
        }
        gameObject.SetActive(true);
    }

    private void Continue()
    {
        gameObject.SetActive(false);
        SaveManager.Instance.Data.MissionRewards = missionRewards;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SandboxMainGoalDescription : MonoBehaviour
{
    public event Action LaunchClicked = delegate { };

    [SerializeField]
    private Button launchButton = null;
    [SerializeField]
    private Text mainGoalType = null;
    [SerializeField]
    private Text desc = null;
    [SerializeField]
    private Text time = null;
    [SerializeField]
    private Text rewardsDesc = null;
    [SerializeField]
    private Text commandPoints = null;
    [SerializeField]
    private Text upgradePoints = null;
    [SerializeField]
    private Text exp = null;
    [SerializeField]
    private PlannedOperationsSctiptable plannedOperations = null;
    [SerializeField]
    private RectTransform objectiveParent = null;
    [SerializeField]
    private List<Text> objectives = null;

    private void Awake()
    {
        if (launchButton != null)
        {
            launchButton.onClick.AddListener(LaunchGame);
        }
    }

    public void Setup(MainGoalData mainGoal)
    {
        var locMan = LocalizationManager.Instance;
        mainGoalType.text = locMan.GetText(mainGoal.Type + "Title");
        desc.text = mainGoal.Type == EMainGoalType.PlannedOperations ? 
            locMan.GetText(plannedOperations.PlannedOperationsMissions[mainGoal.PlannedOperationsMapsIndex].DescriptionId) : locMan.GetText(mainGoal.Type + "_" + mainGoal.DescriptionIndex.ToString("00"));
        time.text = mainGoal.DaysToFinish.ToString();
        commandPoints.text = mainGoal.CommandPoints.ToString();
        upgradePoints.text = mainGoal.UpgradePoints.ToString();
        exp.text = mainGoal.Exp.ToString();
        foreach (var obj in objectives)
        {
            obj.gameObject.SetActive(false);
        }
        if (mainGoal.Type == EMainGoalType.PlannedOperations)
        {
            for (int i = 0; i < mainGoal.PointsToComplete; i++)
            {
                objectives[i].text = "       " + LocalizationManager.Instance.GetText(plannedOperations.PlannedOperationsMissions[mainGoal.PlannedOperationsMapsIndex].SandboxObjectiveTypes[i].ToString() + "_" + mainGoal.ObjectiveIdIndexes[i].ToString("00"));
                objectives[i].gameObject.SetActive(true);
            }
        }
        else
        {
            objectives[0].text = "       " + LocalizationManager.Instance.GetText(mainGoal.ObjectiveType.ToString() + "_" + mainGoal.ObjectiveIdIndexes[0].ToString("00"));
            objectives[0].gameObject.SetActive(true);
        }
        gameObject.SetActive(true);
    }

    private void LaunchGame()
    {
        LaunchClicked();
    }
}

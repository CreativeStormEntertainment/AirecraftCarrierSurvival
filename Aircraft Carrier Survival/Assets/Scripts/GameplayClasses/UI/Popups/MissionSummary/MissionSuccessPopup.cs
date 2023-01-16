using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionSuccessPopup : MissionSummaryPopup
{
    public ref MissionRewards MissionRewards => ref missionRewards;
    public GameObject MainPanel => mainPanel;
    public MissionSuccessRightPanel RightPanel => rightPanel;

    [SerializeField]
    private GameObject mainPanel = null;
    [SerializeField]
    private MissionSuccessLeftPanel leftPanel = null;
    [SerializeField]
    private MissionSuccessMiddlePanel middlePanel = null;
    [SerializeField]
    private MissionSuccessRightPanel rightPanel = null;
    [SerializeField]
    private MedalsWindow medalsWindow = null;
    [SerializeField]
    private Button awardMedalsButton = null;

    private MissionRewards missionRewards;

    private bool allOptional;
    private bool init;

    public override void Setup(EMissionLoseCause cause)
    {
        base.Setup(cause);
        if (!init)
        {
            rightPanel.CountFinished += () =>
            {
                leftPanel.NextUpgrade();
                awardMedalsButton.interactable = true;
                restartMissionButton.interactable = true;
            };
            awardMedalsButton.onClick.AddListener(OnAwardsButtonClicked);
            init = true;
        }
        var saveData = SaveManager.Instance.Data;
        if (saveData.GameMode == EGameMode.Sandbox)
        {
            UpdateSandbox();
            restartMissionButton.gameObject.SetActive(false);
        }
        awardMedalsButton.interactable = false;
        restartMissionButton.interactable = false;
        awardMedalsButton.gameObject.SetActive(true);
        MissionRewards = SaveManager.Instance.Data.MissionRewards.Duplicate();
        MissionRewards.ActiveBuff = ETemporaryBuff.None;

        var selectedOfficersIndices = saveData.IntermissionData.OfficerData.Selected;
        var upgradesList = MissionRewards.OfficersUpgrades;
        for (int i = 0; i < selectedOfficersIndices.Count; i++)
        {
            if (selectedOfficersIndices[i] >= 0)
            {
                var officerSave = upgradesList[selectedOfficersIndices[i]];
                officerSave.MissionsPlayed++;
                upgradesList[selectedOfficersIndices[i]] = officerSave;
            }
        }

        leftPanel.Setup();
        middlePanel.Setup();
        rightPanel.Setup();
        mainPanel.SetActive(true);
        medalsWindow.gameObject.SetActive(false);

        int index = 0;
        var objMan = ObjectivesManager.Instance;
        allOptional = true;
        foreach (var obj in objMan.Objectives)
        {
            if (obj.Data.ObjectiveCategory == EObjectiveCategory.Optional)
            {
                if (!obj.ObjectiveValidState || obj.Success == obj.Data.InverseFinishStateInSummary)
                {
                    allOptional = false;
                }
                if (objectives.Count <= index)
                {
                    objectives.Add(Instantiate(objectives[0]));
                }
                objectives[index].Setup(obj);
                index++;
            }
        }
    }

    public void SaveRewards()
    {
        var saveMan = SaveManager.Instance;
        var data = saveMan.Data;
        ref var intermissionData = ref data.IntermissionData;

        data.MissionRewards = MissionRewards;

        ref var rewards = ref data.IntermissionMissionData;
        intermissionData.CommandPoints += rewards.CommandsPoints;
        intermissionData.UpgradePoints += rewards.UpgradePoints;
        if (rewards.EscortType >= 0)
        {
            intermissionData.OwnedEscorts.Add(rewards.EscortType);
        }
        data.ManeuversLevels = leftPanel.ManeuversLevels;
        data.MissionRewards.ActiveBuff = rewards.Buff;
        data.MissionInProgress.InProgress = false;
        if (saveMan.Data.GameMode == EGameMode.Fabular)
        {
            CrewManager.Instance.SaveLosses(ref data.MissionRewards, ref intermissionData);
            StrikeGroupManager.Instance.SaveLosses(ref intermissionData);
            TacticManager.Instance.SaveLosses(ref intermissionData);
            intermissionData.Broken.Clear();
            intermissionData.Broken.AddRange(intermissionData.SelectedEscort);
            if (saveMan.TransientData.LastMission)
            {
                saveMan.Data.CurrentChapter++;
                saveMan.Data.FinishedMissions.Clear();
            }
            else
            {
                if (!saveMan.Data.FinishedMissions.Contains(saveMan.Data.CurrentMission) && saveMan.Data.CurrentMission != 0)
                {
                    saveMan.Data.FinishedMissions.Add(saveMan.Data.CurrentMission);
                }
            }
        }
        else if (saveMan.Data.GameMode == EGameMode.Sandbox)
        {

        }
        data.SavedInIntermission = false;
    }

    private void OnAwardsButtonClicked()
    {
        medalsWindow.Setup(rightPanel.MedalsAwarded, allOptional);
        mainPanel.SetActive(false);
        awardMedalsButton.gameObject.SetActive(false);
    }

    private void UpdateSandbox()
    {
        var saveData = SaveManager.Instance.Data;
        var sandMan = SandboxManager.Instance;
        sandMan.SandboxTerritoryManager.ConquerTerritory(true);
        var missionData = saveData.SandboxData.CurrentMissionSaveData;
        if (missionData.MapInstanceInProgress)
        {
            if (missionData.ObjectiveType == ESandboxObjectiveType.EnemyFleetInstance)
            {
                SandboxManager.Instance.WorldMapFleetsManager.DespawnClosestFleet();
            }
        }
        if (missionData.PoiType == EPoiType.MainObjective)
        {
            var mainGoal = sandMan.SandboxGoalsManager.MainGoal;
            mainGoal.Points++;
            var timeMan = TimeManager.Instance;
            if (sandMan.SandboxGoalsManager.MainGoal.Type != EMainGoalType.PlannedOperations || mainGoal.UnlockedDay.Month <= timeMan.CurrentMonth && mainGoal.UnlockedDay.Day <= timeMan.CurrentDay)
            {
                sandMan.SandboxGoalsManager.FinishMainGoal();
            }
        }
    }
}

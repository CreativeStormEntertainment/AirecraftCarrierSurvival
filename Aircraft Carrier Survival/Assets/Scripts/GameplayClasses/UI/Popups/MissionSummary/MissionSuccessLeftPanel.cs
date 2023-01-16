using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionSuccessLeftPanel : MonoBehaviour
{
    public List<int> ManeuversLevels => maneuversLevels;

    [SerializeField]
    private MissionSuccessRightPanel rightPanel = null;
    [SerializeField]
    private ChoseBuffWindow choseOrderWindow = null;
    [SerializeField]
    private Image admiralImage = null;
    [SerializeField]
    private Image expFillImage = null;
    [SerializeField]
    private Text renownLevel = null;
    [SerializeField]
    private Image admiralImage2 = null;
    [SerializeField]
    private Text renownLevel2 = null;
    [SerializeField]
    private List<RewardTab> rewardTabs = null;
    [SerializeField]
    private SandboxAdmiralLevels admiralLevels = null;
    [SerializeField]
    private OfficerList officerList = null;
    [SerializeField]
    private float timeToFillExperienceBar = 2f;

    private RewardTab currentTab;

    private int ordersUpgrades;
    private int pointsUpgrades;

    private List<int> maneuversLevels = new List<int>();
    private List<SandboxAdmiralLevel> rewards = new List<SandboxAdmiralLevel>();

    private float expFillAmount;
    private float lastFillAmount;
    private float timer;
    private float helperTimer;

    private void Start()
    {
        foreach (var tab in rewardTabs)
        {
            tab.Init();
        }
    }

    private void Update()
    {
        if (timer < 1)
        {
            timer += Time.unscaledDeltaTime;
            expFillImage.fillAmount = Mathf.Lerp(0f, expFillAmount, timer);
        }
    }

    public void Setup()
    {
        foreach (var tab in rewardTabs)
        {
            tab.gameObject.SetActive(false);
            tab.SetShowButton(false);
        }
        var saveMan = SaveManager.Instance;
        var islMan = IslandsAndOfficersManager.Instance;
        int optionalObjectivesCompleted = 0;
        var objMan = ObjectivesManager.Instance;
        if (objMan != null)
        {
            foreach (var objective in objMan.Objectives)
            {
                if (objective.Category == EObjectiveCategory.Optional && objective.ObjectiveValidState && objective.Success != objective.Data.InverseFinishStateInSummary)
                {
                    optionalObjectivesCompleted++;
                }
            }
        }
        maneuversLevels = new List<int>(saveMan.Data.ManeuversLevels);

        int admiralRenownLevel = 0;
        if (saveMan.Data.GameMode == EGameMode.Fabular)
        {
            var missionPopup = GameStateManager.Instance.MissionSuccessPopup;
            int oldRenown = saveMan.Data.MissionRewards.AdmiralRenownLevel;
            int oldLevel = saveMan.Data.MissionRewards.GetAdmiralLevel();
            missionPopup.MissionRewards.AdmiralRenownLevel = oldRenown + 2 + optionalObjectivesCompleted;
            admiralRenownLevel = missionPopup.MissionRewards.GetAdmiralLevel();
            for (int i = oldLevel + 1; i <= missionPopup.MissionRewards.GetAdmiralLevel(); i++)
            {
                var tab = GetNextFreeTab();
                if (islMan.RenownRewards.Count > i - 2)
                {
                    var reward = islMan.RenownRewards[i - 2];
                    if (reward.Maneuver != null)
                    {
                        maneuversLevels[TacticManager.Instance.PlayerManeuversList.Maneuvers.IndexOf(reward.Maneuver)] = reward.ManeuverLevel;
                        tab.SetupManeuversCard(reward.Maneuver, reward.ManeuverLevel);
                    }
                    else
                    {
                        tab.SetupUpgradePoints(reward.UpgradePoints);
                    }
                    SelectTab(tab);
                    foreach (var level in islMan.AdmiralOrderChoiseRewards)
                    {
                        if (i == level)
                        {
                            ordersUpgrades++;
                        }
                    }
                    if (i % 2 > 0)
                    {
                        pointsUpgrades++;
                    }
                }
            }
            expFillImage.transform.parent.gameObject.SetActive(false);
        }
        else if (saveMan.Data.GameMode == EGameMode.Sandbox)
        {
            var missionRewards = GameStateManager.Instance != null ? GameStateManager.Instance.MissionSuccessPopup.MissionRewards : IntermissionManager.Instance.SandboxMainGoalSummary.MissionRewards;
            var saveData = saveMan.Data;
            int oldExp = saveData.MissionRewards.SandboxAdmiralExp;
            if (saveData.SandboxData.CurrentMissionSaveData.MapInstanceInProgress)
            {
                missionRewards.SandboxAdmiralExp += saveData.SandboxData.CurrentMissionSaveData.SandboxMissionRewards.AdmiralExp;
            }

            admiralRenownLevel = 1;
            int i = 0;
            for (; i < admiralLevels.Levels.Count; i++)
            {
                if (oldExp < admiralLevels.Levels[i].RequiredExp)
                {
                    break;
                }
                else
                {
                    admiralRenownLevel = admiralLevels.Levels[i].Level;
                }
            }
            //if (i > 0)
            //{
            //    lastFillAmount = (float)(oldExp - admiralLevels.Levels[i - 1].RequiredExp) / (admiralLevels.Levels[i].RequiredExp - admiralLevels.Levels[i - 1].RequiredExp);
            //}
            //else
            //{
            //    lastFillAmount = (float)oldExp / admiralLevels.Levels[i].RequiredExp;
            //}
            for (; i < admiralLevels.Levels.Count; i++)
            {
                if (admiralLevels.Levels[i].RequiredExp <= missionRewards.SandboxAdmiralExp)
                {
                    rewards.Add(admiralLevels.Levels[i]);
                    admiralRenownLevel = admiralLevels.Levels[i].Level;
                }
                else
                {
                    break;
                }
            }
            expFillImage.transform.parent.gameObject.SetActive(true);
            timer = 0f;
            if (i > 0)
            {
                expFillAmount = (float)(missionRewards.SandboxAdmiralExp - admiralLevels.Levels[i - 1].RequiredExp) / (admiralLevels.Levels[i].RequiredExp - admiralLevels.Levels[i - 1].RequiredExp);
            }
            else
            {
                expFillAmount = (float)missionRewards.SandboxAdmiralExp / admiralLevels.Levels[i].RequiredExp;
            }
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.MissionSuccessPopup.MissionRewards = missionRewards;
            }
            else
            {
                IntermissionManager.Instance.SandboxMainGoalSummary.MissionRewards = missionRewards;
            }
            saveData.IntermissionMissionData.UpgradePoints = 0;
            saveData.IntermissionMissionData.CommandsPoints = SaveManager.Instance.Data.SandboxData.CurrentMissionSaveData.SandboxMissionRewards.CommandPoints;
            saveData.IntermissionMissionData.EscortType = -1;
            saveData.IntermissionMissionData.EnemyBlocksDestroyed = 9999;
            saveData.IntermissionMissionData.SquadronsToLose = -1;
            saveData.IntermissionMissionData.HoursToFinishMission = -1;
            saveData.IntermissionMissionData.Buff = ETemporaryBuff.None;
        }

        admiralImage2.sprite = admiralImage.sprite = officerList.Portraits[saveMan.Data.AdmiralPortrait].Square;
        renownLevel2.text = renownLevel.text = admiralRenownLevel.ToString();
        if (rightPanel != null && saveMan.Data.GameMode != EGameMode.Sandbox)
        {
            rightPanel.Play = true;
        }
        else
        {
            NextUpgrade();
        }
    }

    public void SelectTab(RewardTab tab)
    {
        if (currentTab != null)
        {
            currentTab.SelectTab(false);
        }
        currentTab = tab;
        currentTab.SelectTab(true);
    }

    public RewardTab GetNextFreeTab()
    {
        foreach (var tab in rewardTabs)
        {
            if (!tab.Used)
            {
                return tab;
            }
        }
        return null;
    }

    public void ChooseOrder(IslandBuff buff)
    {
        GetNextFreeTab().SetupBuffCard(buff);
        ordersUpgrades--;
        NextUpgrade();
    }

    public void ChoosePoints(bool air)
    {
        GetNextFreeTab().SetupPoints(air);
        pointsUpgrades--;
        NextUpgrade();
    }

    public void ChooseManeuver(PlayerManeuverData maneuver, int level)
    {
        maneuversLevels[TacticManager.Instance.PlayerManeuversList.Maneuvers.IndexOf(maneuver)] = level;
        GetNextFreeTab().SetupManeuversCard(maneuver, level);
        NextUpgrade();
    }

    public void NextUpgrade()
    {
        var saveMan = SaveManager.Instance;
        if (saveMan.Data.GameMode == EGameMode.Fabular)
        {
            if (ordersUpgrades > 0)
            {
                choseOrderWindow.SetupBuffs();
            }
            else if (pointsUpgrades > 0)
            {
                choseOrderWindow.SetupPoints();
            }
        }
        else if (saveMan.Data.GameMode == EGameMode.Sandbox)
        {
            if (rewards.Count > 0)
            {
                var reward = rewards[0];
                rewards.RemoveAt(0);
                choseOrderWindow.Setup(reward);
            }
            else if (rewardTabs[0].Used)
            {
                SelectTab(rewardTabs[0]);
            }
        }
    }

    public int GetRewardsCount()
    {
        return rewards.Count;
    }
}

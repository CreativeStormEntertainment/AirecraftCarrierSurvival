using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionLostPopup : MissionSummaryPopup
{
    private HashSet<EMissionLoseCause> totalLoseCauses = new HashSet<EMissionLoseCause> { EMissionLoseCause.CrewMembersDead, EMissionLoseCause.SectionsDamaged,
        EMissionLoseCause.SectionsDestroyed, EMissionLoseCause.SectionsFired, EMissionLoseCause.SectionsFlooded};

    [SerializeField]
    private Text causeOfDefeat = null;
    [SerializeField]
    private Text strategyTip = null;

    [SerializeField]
    private Button pearlHarborButton = null;
    [SerializeField]
    private Button loadLastSaveButton = null;
    [SerializeField]
    private Button mainMenuButton = null;

    private bool totalLose;

    protected void Start()
    {
        pearlHarborButton.onClick.AddListener(OnPearlHarborButton);
        loadLastSaveButton.onClick.AddListener(LoadLastSave);
        mainMenuButton.onClick.AddListener(MainMenu);
    }

    public override void Setup(EMissionLoseCause cause)
    {
        base.Setup(cause);
        bool sandbox = SaveManager.Instance.Data.GameMode == EGameMode.Sandbox;
        restartMissionButton.gameObject.SetActive(!sandbox);
        if (sandbox)
        {
            totalLose = totalLoseCauses.Contains(cause);
            //loadLastSaveButton.gameObject.SetActive(totalLose);
            mainMenuButton.gameObject.SetActive(totalLose);
            //pearlHarborButton.gameObject.SetActive(!totalLose);
            UpdateSandbox();
        }
        var locMan = LocalizationManager.Instance;
        var summaryTexts = summaryData.SummaryTypeData.Find(data => data.ID == cause);
        if (summaryTexts != null)
        {
            causeOfDefeat.text = locMan.GetText(summaryTexts.LoseCauseID);
            int rand = Random.Range(0, summaryTexts.TipID.Count);
            strategyTip.text = locMan.GetText(summaryTexts.TipID[rand]);
        }
        else
        {
            Debug.LogError("There is no summary texts for LoseCause : " + cause);
        }

        int index = 0;
        var objMan = ObjectivesManager.Instance;
        foreach (var obj in objMan.Objectives)
        {
            if (obj.ObjectiveValidState && obj.Data.ObjectiveCategory != EObjectiveCategory.None)
            {
                if (objectives.Count > index)
                {
                    objectives[index].Setup(obj);
                }
                else
                {
                    var newObj = Instantiate(objectives[0]);
                    newObj.Setup(obj);
                    objectives.Add(newObj);
                }
                index++;
            }
        }
    }

    private void UpdateSandbox()
    {
        var saveData = SaveManager.Instance.Data;
        var sandMan = SandboxManager.Instance;
        sandMan.SandboxTerritoryManager.ConquerTerritory(false);
        var missionData = saveData.SandboxData.CurrentMissionSaveData;
        if (missionData.MapInstanceInProgress)
        {
            if (missionData.PoiType == EPoiType.MainObjective)
            {
                var mainGoal = sandMan.SandboxGoalsManager.MainGoal;
                var timeMan = TimeManager.Instance;
                if (sandMan.SandboxGoalsManager.MainGoal.Type != EMainGoalType.PlannedOperations || mainGoal.UnlockedDay.Month <= timeMan.CurrentMonth && mainGoal.UnlockedDay.Day <= timeMan.CurrentDay)
                {
                    sandMan.SandboxGoalsManager.FinishMainGoal();
                }
            }
        }
    }

    public void OnPearlHarborButton()
    {
        var mode = SaveManager.Instance.Data.GameMode;
        if (mode == EGameMode.Fabular || totalLose)
        {
            if (totalLose)
            {
                var data = SaveManager.Instance.Data;
                ref var intermissionData = ref data.IntermissionData;
                CrewManager.Instance.SaveLosses(ref data.MissionRewards, ref intermissionData);
                StrikeGroupManager.Instance.SaveLosses(ref intermissionData);
                TacticManager.Instance.SaveLosses(ref intermissionData);
            }
            BackToPearlHarbor();
        }
        else if (mode == EGameMode.Sandbox)
        {
            BackToWorldMap();
        }
    }

    private void LoadLastSave()
    {
        SaveManager.Instance.LoadLastSave();
    }

    private void MainMenu()
    {
        HudManager.Instance.Settings.GameplayMenu.MainMenu();
    }
}

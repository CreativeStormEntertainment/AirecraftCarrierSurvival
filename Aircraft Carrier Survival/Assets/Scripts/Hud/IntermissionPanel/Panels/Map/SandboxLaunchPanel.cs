using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SandboxLaunchPanel : Panel
{
    public event Action LaunchGame = delegate { };

    [SerializeField]
    private RectTransform descriptionMapTransform = null;
    [SerializeField]
    private RectTransform mapTransform = null;
    [SerializeField]
    private SandboxMainGoalDescription sandboxMainGoalDescription = null;
    [SerializeField]
    private List<SectorButton> sectorButtons = null;
    [SerializeField]
    private List<MainGoalSetupDataList> mainGoalSetupDatas = null; //EMainGoalType order
    [SerializeField]
    private PlannedOperationsSctiptable plannedOperations = null;

    private Dictionary<SectorButton, MainGoalData> mainGoals = new Dictionary<SectorButton, MainGoalData>();
    private List<EMainGoalType> mainGoalTypesBasket = new List<EMainGoalType>();
    private List<EMissionLength> missionLengthsBasket = new List<EMissionLength>();
    private List<SectorButton> sectorButtonsBasket = new List<SectorButton>();
    private List<SectorButton> tempList = new List<SectorButton>();
    private SectorButton selectedSector;
    private List<int> plannedOperationsBasket = new List<int>();
    private SandboxObjectiveTypes sandboxObjectiveTypes;

    public void Awake()
    {
        sandboxObjectiveTypes = new SandboxObjectiveTypes();
        sandboxObjectiveTypes.Init();
        plannedOperationsBasket = new List<int>(SaveManager.Instance.Data.IntermissionData.PlannedOperationsBasket);
        RefillLengthBasket();
        CheckRefillPlannedOperationsBasket();
        for (int i = 0; i < (int)EMainGoalType.Count; i++)
        {
            mainGoalTypesBasket.Add((EMainGoalType)i);
            if ((EMainGoalType)i == EMainGoalType.PlannedOperations)
            {
                for (int j = 0; j < 3; j++)
                {
                    mainGoalTypesBasket.Add((EMainGoalType)i);
                }
            }
        }
        for (int j = 0; j < sectorButtons.Count; j++)
        {
            sectorButtonsBasket.Add(sectorButtons[j]);
            sectorButtons[j].Hide();
        }
        int index = 0;
        while (mainGoalTypesBasket.Count > 0)
        {
            var randomType = RandomUtils.GetRandom(mainGoalTypesBasket);
            mainGoalTypesBasket.Remove(randomType);
            var randomLength = RandomUtils.GetRandom(missionLengthsBasket);
            missionLengthsBasket.Remove(randomLength);
            if (missionLengthsBasket.Count == 0)
            {
                RefillLengthBasket();
            }
            var randomButton = RandomUtils.GetRandom(sectorButtonsBasket);
            if (index < 3)
            {
                randomButton = GetRandomButtonOfSector((ESectorType)index);
            }
            sectorButtonsBasket.Remove(randomButton);
            MainGoalSetupData setupData = mainGoalSetupDatas[(int)randomType].Datas[(int)randomLength];
            int plannedOperationsMapsIndex = -1;
            var objectiveType = ESandboxObjectiveType.Count;
            if (randomType == EMainGoalType.PlannedOperations)
            {
                plannedOperationsMapsIndex = UnityEngine.Random.Range(0, plannedOperations.PlannedOperationsMissions.Count);
                plannedOperationsBasket.Remove(plannedOperationsMapsIndex);
                CheckRefillPlannedOperationsBasket();
            }
            else
            {
                objectiveType = RandomUtils.GetRandom(sandboxObjectiveTypes.MainGoalObjectivesDictionary[randomType]);
            }

            Vector2 pinPositionProportion = new Vector2(randomButton.RectTransform.anchoredPosition.x / mapTransform.sizeDelta.x, randomButton.RectTransform.anchoredPosition.y / mapTransform.sizeDelta.y);
            MainGoalData data = new MainGoalData(randomType, randomLength, randomButton.SectorType, setupData, pinPositionProportion, plannedOperationsMapsIndex, UnityEngine.Random.Range(1, 6), objectiveType);
            data.SetupObjectiveIds();
            randomButton.Setup(data);
            mainGoals.Add(randomButton, data);
            index++;
        }
        SelectSector(RandomUtils.GetRandom(mainGoals).Key);
    }

    public override void Setup(NewSaveData data)
    {
        Assert.IsTrue(SaveManager.Instance.Data.GameMode == EGameMode.Sandbox);
        base.Setup(data);

        foreach (var button in sectorButtons)
        {
            button.Clicked += SelectSector;
        }
        sandboxMainGoalDescription.LaunchClicked += FireLaunchGame;
    }

    public override void Save(NewSaveData data)
    {

    }

    private void FireLaunchGame()
    {
        var saveData = SaveManager.Instance.Data;
        if (saveData.GameMode == EGameMode.Sandbox)
        {
            saveData.SandboxData.IsSaved = false;
            saveData.SandboxData.MainGoal = mainGoals[selectedSector];
            saveData.SandboxData.SpawnedPois.Clear();
            saveData.SandboxData.MainObjectivesBasket.Clear();
            saveData.SandboxData.OptionalObjectivesBasket.Clear();
            saveData.SandboxData.QuestObjectivesBasket.Clear();
            saveData.SandboxData.DifficultyBasket.Clear();
            saveData.SandboxData.TicksPassedToGameplaySetter = 0;
            saveData.SandboxData.CurrentMissionSaveData.MapInstanceInProgress = false;
        }
        LaunchGame();
    }

    private void SelectSector(SectorButton button)
    {
        if (selectedSector != null)
        {
            selectedSector.SetSelected(false);
        }
        selectedSector = button;
        selectedSector.SetSelected(true);
        sandboxMainGoalDescription.Setup(mainGoals[selectedSector]);
        mapTransform.anchoredPosition = descriptionMapTransform.anchoredPosition;
    }

    private void RefillLengthBasket()
    {
        for (int i = 0; i < (int)EMissionLength.Count; i++)
        {
            missionLengthsBasket.Add((EMissionLength)i);
        }
    }

    private void CheckRefillPlannedOperationsBasket()
    {
        if (plannedOperationsBasket.Count != 0)
        {
            return;
        }
        for (int i = 0; i < plannedOperations.PlannedOperationsMissions.Count; i++)
        {
            plannedOperationsBasket.Add(i);
        }
    }

    private SectorButton GetRandomButtonOfSector(ESectorType sector)
    {
        tempList.Clear();
        foreach (var button in sectorButtonsBasket)
        {
            if (button.SectorType == sector)
            {
                tempList.Add(button);
            }
        }
        return RandomUtils.GetRandom(tempList);
    }
}

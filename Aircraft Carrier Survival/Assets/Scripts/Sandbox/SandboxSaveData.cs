using System;
using System.Collections.Generic;

[Serializable]
public struct SandboxSaveData
{
    public bool IsSaved;
    public MainGoalData MainGoal;
    public List<SandboxPoiData> SpawnedPois; 
    public SandboxCurrentMissionSaveData CurrentMissionSaveData;
    public TimeSaveData MissionStartTime;
    public List<ESandboxObjectiveType> MainObjectivesBasket;
    public List<ESandboxObjectiveType> OptionalObjectivesBasket;
    public List<ESandboxObjectiveType> QuestObjectivesBasket;
    public List<int> DifficultyBasket;
    public List<ETerritoryType> TerritoryNodes;
    public int TicksPassedToGameplaySetter;
    public int TicksPassedToRedWatersSetter;
    public int TicksPassedToRedWatersPopup;
    public int TicksToSpawnPOMainPoi;
    public int SelectedBuffs;
    public int ChosenConsequence;
    public int DrawnEvent;
    public int EventDrawHoursPassed;
    public WorldMapFleetsSaveData WorldMapFleetsSaveData;
    public SandboxSpawnMapData SandboxSpawnMapData;

    public SandboxSaveData Duplicate()
    {
        var result = new SandboxSaveData();
        result.IsSaved = IsSaved;
        result.MainGoal = MainGoal.Duplicate();
        result.SpawnedPois = new List<SandboxPoiData>(SpawnedPois);
        result.MainObjectivesBasket = new List<ESandboxObjectiveType>(MainObjectivesBasket);
        result.OptionalObjectivesBasket = new List<ESandboxObjectiveType>(OptionalObjectivesBasket);
        result.QuestObjectivesBasket = new List<ESandboxObjectiveType>(QuestObjectivesBasket);
        result.DifficultyBasket = new List<int>(DifficultyBasket);
        result.TerritoryNodes = new List<ETerritoryType>(TerritoryNodes);
        result.CurrentMissionSaveData = CurrentMissionSaveData.Duplicate();
        result.MissionStartTime = MissionStartTime;
        result.TicksPassedToGameplaySetter = TicksPassedToGameplaySetter;
        result.TicksPassedToRedWatersSetter = TicksPassedToRedWatersSetter;
        result.TicksPassedToRedWatersPopup = TicksPassedToRedWatersPopup;
        result.TicksToSpawnPOMainPoi = TicksToSpawnPOMainPoi;
        result.SelectedBuffs = SelectedBuffs;
        result.ChosenConsequence = ChosenConsequence;
        result.DrawnEvent = DrawnEvent;
        result.EventDrawHoursPassed = EventDrawHoursPassed;
        result.WorldMapFleetsSaveData = WorldMapFleetsSaveData.Duplicate();
        result.SandboxSpawnMapData = SandboxSpawnMapData.Duplicate();
        return result;
    }
}
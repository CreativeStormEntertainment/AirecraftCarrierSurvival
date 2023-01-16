using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MainGoalData
{
    public EMainGoalType Type;
    public EMissionLength MissionLength;
    public ESectorType Sector;
    public int PointsToComplete;
    public int CommandPoints;
    public int UpgradePoints;
    public int Exp;
    public int DaysToFinish;
    public int TicksToUpdateDays;
    public int Points;
    public int PlannedOperationsMapsIndex;
    public int SpawnedPOMainPoiIndex;
    public int CompletePOMainPoi;
    public int DescriptionIndex;
    public MyVector2 PinPositionProportion;
    public ESandboxObjectiveType ObjectiveType;
    public List<int> ObjectiveIdIndexes;
    public DayTime UnlockedDay;

    public MainGoalData()
    {

    }

    public MainGoalData(EMainGoalType type, EMissionLength missionLength, ESectorType sector, MainGoalSetupData setupData, Vector2 pinPositionProportion, int plannedOperationsMapsIndex, int descriptionIndex, ESandboxObjectiveType objectiveType)
    {
        Type = type;
        PointsToComplete = setupData.PointsToComplete;
        MissionLength = missionLength;
        Sector = sector;
        CommandPoints = setupData.CommandPoints;
        UpgradePoints = setupData.UpgradePoints;
        Exp = setupData.Experience;
        DaysToFinish = setupData.DaysToFinish;
        Points = 0;
        PlannedOperationsMapsIndex = plannedOperationsMapsIndex;
        PinPositionProportion = pinPositionProportion;
        SpawnedPOMainPoiIndex = -1;
        DescriptionIndex = descriptionIndex;
        ObjectiveType = objectiveType;
        ObjectiveIdIndexes = new List<int>();
    }

    public MainGoalData Duplicate()
    {
        var result = new MainGoalData();
        result.Type = Type;
        result.PointsToComplete = PointsToComplete;
        result.MissionLength = MissionLength;
        result.Sector = Sector;
        result.CommandPoints = CommandPoints;
        result.UpgradePoints = UpgradePoints;
        result.Exp = Exp;
        result.DaysToFinish = DaysToFinish;
        result.Points = Points;
        result.PlannedOperationsMapsIndex = PlannedOperationsMapsIndex;
        result.PinPositionProportion = PinPositionProportion;
        result.SpawnedPOMainPoiIndex = SpawnedPOMainPoiIndex;
        result.DescriptionIndex = DescriptionIndex;
        result.ObjectiveIdIndexes = new List<int>(ObjectiveIdIndexes);
        result.UnlockedDay = UnlockedDay;
        return result;
    }

    public void SetupObjectiveIds()
    {
        if (Type != EMainGoalType.PlannedOperations)
        {
            ObjectiveIdIndexes.Add(UnityEngine.Random.Range(1, 6));
        }
        else
        {
            for (int i = 0; i < PointsToComplete; i++)
            {
                ObjectiveIdIndexes.Add(UnityEngine.Random.Range(1, 6));
            }
        }
    }
}
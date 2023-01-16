using System;

[Serializable]
public class SandboxPoiData
{
    public EPoiType PoiType;
    public int NodeIndex;
    public int RegionIndex;
    public EMissionDifficulty Difficulty;
    //public Difficulty;
    public int TicksToObsolete;
    public SandboxMissionRewards MissionRewards;
    public ESandboxObjectiveType ObjectiveType;
    public int DescriptionIndex;
    public int ObjectiveDescriptionIndex;
    public int EnemyForcesDescriptionIndex;
    public int MapIndex;
    public int AdditionalEnemiesCount;
    public DayTime? PoUnlockDay;

    public SandboxPoiData(EPoiType type, int nodeIndex, EMissionDifficulty difficulty, int hoursToObsolete, ESandboxObjectiveType objectiveType = ESandboxObjectiveType.None)
    {
        PoiType = type;
        NodeIndex = nodeIndex;
        Difficulty = difficulty;
        TicksToObsolete = hoursToObsolete * TimeManager.Instance.TicksForHour;
        ObjectiveType = objectiveType;
        AdditionalEnemiesCount = -1;
        ObjectiveDescriptionIndex = -1;
    }
}

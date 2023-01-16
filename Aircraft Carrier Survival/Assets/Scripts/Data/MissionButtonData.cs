using System;
using System.Collections.Generic;

[Serializable]
public class MissionButtonData
{
    public int ChapterID;
    public int MissionID;
    public bool IsFinalMission;
    public string MissionNameID;
    public string MissionDescriptionID;
    public List<string> MissionObjectiveDescriptionIDs;
    public EMissionDifficulty MissionDifficulty;
    public IntermissionMissionData MissionData;

    [NonSerialized]
    public DayTime? ForcedDate;

    [NonSerialized]
    public ELevelMissionState State;
}

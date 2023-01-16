using System;
using UnityEngine;

[Serializable]
public class MissionData
{
    [SerializeField] private string missionName = "";
    [SerializeField] private string missionDescription = "";
    [SerializeField] private string objective1 = "";
    [SerializeField] private string objective2 = "";
    [SerializeField] private string objective3 = "";
    [SerializeField] private int difficultyLvl = 0;
    [SerializeField] private int commandPoints = 0;

    public string GetMissionName()
    {
        return missionName;
    }

    public string GetMissionDescription()
    {
        return missionDescription;
    }

    public string GetFirstObjective()
    {
        return objective1;
    }

    public string GetSecondObjective()
    {
        return objective2;
    }

    public string GetThirdObjective()
    {
        return objective3;
    }

    public int GetDifficultyLvl()
    {
        return difficultyLvl;
    }

    public int GetCommandPoints()
    {
        return commandPoints;
    }


}



using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MissionInstanceRewardsData
{
    public ESandboxObjectiveType ObjectiveType;
    public List<SandboxMissionRewards> Rewards;

    public SandboxMissionRewards GetRewardsData(EMissionDifficulty difficulty)
    {
        if (Rewards.Count <= (int)difficulty)
        {
            Debug.LogError("RewardsScriptable doesn't have RewardsData for objective : " + ObjectiveType + " with difficulty : " + difficulty);
            return new SandboxMissionRewards(0, 0);
        }
        return Rewards[(int)difficulty];
    }
}

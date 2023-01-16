using System;
using UnityEngine;

[Serializable]
public class SandboxCurrentMissionSaveData
{
    public bool MapInstanceInProgress;
    public int NodeIndex;
    public SandboxMissionRewards SandboxMissionRewards;
    public ESandboxObjectiveType ObjectiveType;
    public EPoiType PoiType;

    public SandboxCurrentMissionSaveData()
    {

    }

    public SandboxCurrentMissionSaveData(int nodeIndex, SandboxMissionRewards rewards, ESandboxObjectiveType objectiveType, EPoiType poiType)
    {
        MapInstanceInProgress = true;
        NodeIndex = nodeIndex;
        SandboxMissionRewards = rewards;
        ObjectiveType = objectiveType;
        PoiType = poiType;
    }

    public SandboxCurrentMissionSaveData Duplicate()
    {
        var result = new SandboxCurrentMissionSaveData();
        result.NodeIndex = NodeIndex;
        result.SandboxMissionRewards = SandboxMissionRewards.Duplicate();
        result.ObjectiveType = ObjectiveType;
        result.PoiType = PoiType;
        return result;
    }
}

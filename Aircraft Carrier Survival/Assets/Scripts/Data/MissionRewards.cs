using System;
using System.Collections.Generic;

[Serializable]
public struct MissionRewards
{
    public int SandboxAdmiralExp;
    public int AdmiralRenownLevel;
    public int AdmiralUnlockedOrders;
    public List<OfficerUpgrades> OfficersUpgrades;
    public List<CrewUpgradeSaveData> CrewsUpgrades;
    public ETemporaryBuff ActiveBuff;

    public MissionRewards Duplicate()
    {
        MissionRewards result = this;
        result.OfficersUpgrades = new List<OfficerUpgrades>(OfficersUpgrades);
        result.CrewsUpgrades = new List<CrewUpgradeSaveData>(CrewsUpgrades);
        return result;
    }

    public int GetAdmiralLevel()
    {
        return AdmiralRenownLevel / 2;
    }

    public int GetAdmiralLevelRewardIndex()
    {
        return GetAdmiralLevel() - 2;
    }
}

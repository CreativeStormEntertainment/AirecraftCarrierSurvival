using System;
using System.Collections.Generic;

[Serializable]
public struct IslandBuffSaveData
{
    public EIslandBuff CurrentBuff;
    public int BuffTimer;
    public List<int> AssignedOfficers;
    public int OfficersCooldown;

    public IslandBuffSaveData Duplicate()
    {
        var result = this;
        result.AssignedOfficers = new List<int>(AssignedOfficers);
        return result;
    }
}

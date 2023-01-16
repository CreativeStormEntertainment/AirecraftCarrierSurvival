using System;
using System.Collections.Generic;

[Serializable]
public struct OfficerUpgrades
{
    public int ManeuverLevel;
    public int UpgradedAirPoints;
    public int UpgradedNavyPoints;
    public int Medals;
    public int MissionsPlayed;

    public int GetLevel(List<int> levels)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            if (Medals < levels[i])
            {
                return i;
            }
        }
        return levels.Count;
    }
}

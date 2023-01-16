using System;
using System.Collections.Generic;

[Serializable]
public class MissionPrepareData
{
    public ECameraView? View
    {
        get
        {
            if (EnableView)
            {
                return OverridenView;
            }
            return null;
        }
    }
    public List<MissionCrewData> CrewData => EnableCrewData ? OverridenCrewData : null;
    public List<EIslandRoomType> OfficersData => EnableOfficersData ? OverridenOfficersData : null;
    public List<PlayerManeuverData> AvailableManeuvers => EnableAvailableManeuvers ? OverridenAvailableManeuvers : null;
    public List<int> DefaultSwitchesValues => EnableDefaultSwitchesValues ? OverridenDefaultSwitchesValues : null;
    public List<OverrideBuffData> Buffs => EnableBuffs ? OverridenBuffs : null;

    public List<int> Escort => EnableEscort ? OverridenEscort : null;

    public bool EnableView;
    public ECameraView OverridenView;
    public bool EnableCrewData;
    public List<MissionCrewData> OverridenCrewData;
    public bool EnableOfficersData;
    public List<EIslandRoomType> OverridenOfficersData;
    public bool EnableAvailableManeuvers;
    public List<PlayerManeuverData> OverridenAvailableManeuvers;
    public bool EnableDefaultSwitchesValues;
    public List<int> OverridenDefaultSwitchesValues;
    public bool EnableBuffs;
    public List<OverrideBuffData> OverridenBuffs;
    public bool EnableEscort;
    public List<int> OverridenEscort;
    public bool EnableDCNoCategory;
    public bool EnableNoAACasualties;
    public bool EnableNextMission;
    public SOTacticMap NextMission;
    public bool EnableNoSegmentDangers;
}

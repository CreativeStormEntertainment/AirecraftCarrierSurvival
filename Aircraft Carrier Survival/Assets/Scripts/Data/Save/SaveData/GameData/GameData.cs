using System;
using System.Collections.Generic;

[Serializable]
public struct GameData
{
    public bool InProgress;
    public bool LastMission;

    //chosen map
    public int MissionMap;

    public TimeSaveData Time;
    public bool HasMissionStartTime;
    public DayTime MissionStartTimeB;

    public CrewManagerSaveData CrewManager;
    public StrikeGroupSaveData StrikeGroup;
    public List<IslandBuffSaveData> IslandBuffs;

    public SuppliesSaveData Supplies;
    //every segment status
    public List<SegmentSaveData> Segments;
    public DamageControlSaveData DamageControl;

    public TacticalMapSaveData TacticalMap;
    //current missions, missions in progress & time, squadron state
    public List<MissionSaveData> Missions;

    public DeckSaveData Deck;
    public EnemyAttacksSaveData EnemyAttacks;
    public CameraSaveData Camera;

    //finished objectives
    //half finished objectives
    public List<ObjectiveSaveData> Objectives;
    //shown events
    public List<EventSaveData> EventsData;
    public TimerSaveData MissionTimer;

    //world map
    public WorldMapSaveData WorldMap;

    public List<int> ForcesBarShips;
    public DamageRangeSaveData DamageRange;
    public DamageRangeSaveData DamageRange2;
    public List<int> List;
    public VisualsSaveData VisualsData;

    public GameData Duplicate()
    {
        var result = this;
        result.CrewManager = CrewManager.Duplicate();
        result.StrikeGroup = StrikeGroup.Duplicate();
        result.DamageControl = DamageControl.Duplicate();
        result.TacticalMap = TacticalMap.Duplicate();
        result.Deck = Deck.Duplicate();
        result.EnemyAttacks = EnemyAttacks.Duplicate();
        result.WorldMap = WorldMap.Duplicate();
        result.VisualsData = VisualsData.Duplicate();

        result.Segments = new List<SegmentSaveData>(Segments);

        result.Missions = new List<MissionSaveData>();
        for (int i = 0; i < Missions.Count; i++)
        {
            result.Missions.Add(Missions[i].Duplicate());
        }
        result.Objectives = new List<ObjectiveSaveData>();
        for (int i = 0; i < Objectives.Count; i++)
        {
            result.Objectives.Add(Objectives[i].Duplicate());
        }
        result.EventsData = new List<EventSaveData>();
        for (int i = 0; i < EventsData.Count; i++)
        {
            result.EventsData.Add(EventsData[i].Duplicate());
        }
        result.IslandBuffs = new List<IslandBuffSaveData>();
        for (int i = 0; i < IslandBuffs.Count; i++)
        {
            result.IslandBuffs.Add(IslandBuffs[i].Duplicate());
        }

        if (ForcesBarShips != null)
        {
            result.ForcesBarShips = new List<int>(ForcesBarShips);
        }
        if (List != null)
        {
            result.List = new List<int>();
        }

        return result;
    }
}

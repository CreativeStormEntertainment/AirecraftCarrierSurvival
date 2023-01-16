using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

[Serializable]
public class SaveData
{
    public int SaveVersion;

    public int CommandPoints;
    public int UpgradePoints;
    public int PlaneTypeSlots;

    public int CrewMaxSlots;

    public int CrewUnlockedSlotsNumber;
    public List<string> CrewmenPortraits;
    public List<List<ECrewmanSpecialty>> CrewmenSpecialties;
    public List<int> CrewmenRanks;
    public List<int> CrewmenLastSlots;

    public int ConvoyMaxSlots;
    public int ConvoyUnlockedSlotsNumber;
    public int ConvoyUpgradeLevel;
	public List<int> Convoy;
    public string ShipName;

    public List<int> AllUpgradesLv;
    public int ChosenCrew;
    public int ChosenPlanes;

    //public int CrewSlotsUnlocked;
    public int PlaneSlotsUnlocked;
    //public int ConvoySlotsUnlocked;

    public List<CrewmanData> AvailableCrewmen;

    public int LockedPlaneTypes;
    //public int LockedCrewman;
    public int LockedUpgrades;

    public List<int> AvailableMissionsID;
    public int ChosenMissionID;
    public List<PlaneTypeIntWrapper> PlaneColorIndices;

    public string AdmiralName;
    public string AdmiralDescription;
    public int AdmiralAirLevel;
    public int AdmiralShipLevel;
    public int AdmiralEnduranceLevel;
    public List<int> AdmiralVisual;
    public int AdmiralPortrait;
    public int AdmiralVoice;

    public int PlaneAMaterialIndex = 0;
    public int PlaneBMaterialIndex = 0;
    public int PlaneCMaterialIndex = 0;

    public int ChapterIndex = 0;
    public int MissionIndex = 0;

    public List<OfficerSetup> OfficerSetups;

    public List<int> ShipUpgrades;

    public bool TutorialFinished;

    public List<DestructionSave> Destructions = new List<DestructionSave>();

    public EGameMode GameMode = EGameMode.Tutorial;
    public bool DayTimeToChange = true;

    public List<int> FinishedMissions = new List<int>();
    public List<EIslandRoomType> OfficersLastRooms = new List<EIslandRoomType>();
    public List<int> LastSwitches = new List<int>();
    public int OfficersUnlocked = 2;

    public bool UnlockChapter;
    public bool FinishedGame;

    [NonSerialized]
    public bool ShowTutorialWindow = false;
    [NonSerialized]
    public bool IsFinalMission = false;
    [NonSerialized]
    public SOTacticMap TacticMap = null;
    [NonSerialized]
    public DayTime CurrentDayTime;
    [NonSerialized]
    public int UpgradeRewards;
    [NonSerialized]
    public int CommandsRewards;

    public PersistentData PersistentData = new PersistentData();

    public SaveData Duplicate()
    {
        var result = new SaveData();

        result.SaveVersion = SaveVersion;

        result.CommandPoints = CommandPoints;
        result.UpgradePoints = UpgradePoints;
        result.PlaneTypeSlots = PlaneTypeSlots;

        result.CrewMaxSlots = CrewMaxSlots;

        result.CrewUnlockedSlotsNumber = CrewUnlockedSlotsNumber;
        result.CrewmenPortraits = new List<string>(CrewmenPortraits);
        result.CrewmenSpecialties = new List<List<ECrewmanSpecialty>>(CrewmenSpecialties);
        result.CrewmenRanks = new List<int>(CrewmenRanks);
        result.CrewmenLastSlots = new List<int>(CrewmenLastSlots);

        result.ConvoyMaxSlots = ConvoyMaxSlots;
        result.ConvoyUnlockedSlotsNumber = ConvoyUnlockedSlotsNumber;
        result.ConvoyUpgradeLevel = ConvoyUpgradeLevel;
        result.ShipName = ShipName;

        //result.CrewSlotsUnlocked = CrewSlotsUnlocked;
        result.PlaneSlotsUnlocked = PlaneSlotsUnlocked;
        //result.ConvoySlotsUnlocked = ConvoySlotsUnlocked;

        result.Convoy = new List<int>(Convoy);
        result.AllUpgradesLv = new List<int>(AllUpgradesLv);
        result.ChosenCrew = ChosenCrew;
        result.ChosenPlanes = ChosenPlanes;

        result.AvailableCrewmen = new List<CrewmanData>(AvailableCrewmen);

        result.LockedPlaneTypes = LockedPlaneTypes;
        //result.LockedCrewman = LockedCrewman;
        //result.LockedConvoyTypes = LockedConvoyTypes;
        result.LockedUpgrades = LockedUpgrades;

        result.AvailableMissionsID = AvailableMissionsID;
        result.ChosenMissionID = ChosenMissionID;

        result.AdmiralName = AdmiralName;
        result.AdmiralDescription = AdmiralDescription;
        result.AdmiralAirLevel = AdmiralAirLevel;
        result.AdmiralShipLevel = AdmiralShipLevel;
        result.AdmiralEnduranceLevel = AdmiralEnduranceLevel;
        result.AdmiralVisual = new List<int>(AdmiralVisual);
        result.AdmiralPortrait = AdmiralPortrait;
        result.AdmiralVoice = AdmiralVoice;

        result.ChapterIndex = ChapterIndex;
        result.MissionIndex = MissionIndex;

        result.FinishedMissions = new List<int>(FinishedMissions);
        result.UnlockChapter = UnlockChapter;
        result.FinishedGame = FinishedGame;

        result.PlaneColorIndices = new List<PlaneTypeIntWrapper>(PlaneColorIndices);
        result.OfficerSetups = new List<OfficerSetup>(OfficerSetups);
        result.ShipUpgrades = new List<int>(ShipUpgrades);
        result.Destructions = new List<DestructionSave>(Destructions);
        result.OfficersLastRooms = new List<EIslandRoomType>(OfficersLastRooms);
        result.OfficersUnlocked = OfficersUnlocked;
        result.LastSwitches = new List<int>(LastSwitches);

        result.GameMode = GameMode;

        result.PersistentData = PersistentData.Duplicate();

        return result;
    }

    public void UpdateAdmiralData(SaveData data)
    {
        AdmiralName = data.AdmiralName;
        AdmiralDescription = data.AdmiralDescription;
        AdmiralAirLevel = data.AdmiralAirLevel;
        AdmiralShipLevel = data.AdmiralShipLevel;
        AdmiralEnduranceLevel = data.AdmiralEnduranceLevel;
        AdmiralVisual = new List<int>(data.AdmiralVisual);
        AdmiralPortrait = data.AdmiralPortrait;
        AdmiralVoice = data.AdmiralVoice;
        TutorialFinished = data.TutorialFinished;
        ChapterIndex = data.ChapterIndex;
        MissionIndex = data.MissionIndex;

        //  TODO    Marek   -   add rewards saving
    }

    public int GetPlaneLv(EPlaneType type)
    {
        return (ChosenPlanes >> ((int)type * 2)) % 4;
    }

    public void SetPlaneLv(EPlaneType type, int lv)
    {
        Assert.IsTrue(lv >= 0);
        Assert.IsTrue(lv < 4);
        ChosenPlanes &= ~(3 << ((int)type * 2));
        ChosenPlanes |= (lv << ((int)type * 2));
    }
}

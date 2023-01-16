using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class NewSaveData : BaseSaveData
{
    private const string PathExt = ".sav";
    private const int Version = 85;

    private const int IdleSlots = 14;

    private static readonly List<EDepartments> OldDepartmentsOrder = new List<EDepartments>() { EDepartments.AA, EDepartments.Deck, EDepartments.Air, EDepartments.Medical, EDepartments.Engineering, EDepartments.Navigation };
    private static readonly Dictionary<EDepartments, int> OldDepartmentsSlotCount = new Dictionary<EDepartments, int>() { { EDepartments.AA, 3 }, { EDepartments.Deck, 5 }, { EDepartments.Air, 5 },
        { EDepartments.Medical, 5 }, { EDepartments.Engineering, 5 }, { EDepartments.Navigation, 3 } };
    private static readonly Dictionary<EDepartments, int> NewDepartmentSlots = new Dictionary<EDepartments, int>() { { EDepartments.AA, 3 }, { EDepartments.Deck, 5 }, { EDepartments.Air, 3 },
        { EDepartments.Medical, 5 }, { EDepartments.Engineering, 5 }, { EDepartments.Navigation, 3 } };

    private static readonly NewSaveDataWrapper<NewSaveData> StaticWrapper = new NewSaveDataWrapper<NewSaveData>();
    private static StartSaveData StartSaveData;

    public override string LocalPath => PathExt;

    protected override BaseSaveDataWrapper Wrapper => StaticWrapper;

    protected override int MinSaveVersion => 82;

    protected override int CurrentSaveVersion => saveVersion;

    protected override int MaxSaveVersion => Version;

    public string ProfileName;
    public bool FreshSave = true;

    public EGameMode GameMode;
    public bool IsTutorial;

    [Space(10f)]
    public string AdmiralName;
    public int AdmiralAirLevel;
    public int AdmiralNavyLevel;
    public int AdmiralPortrait;
    public EVoiceType AdmiralVoice;
    public List<int> AdmiralVisual;
    public MissionRewards MissionRewards;

    [Space(10f)]
    public List<ECrewmanSpecialty> CrewmenSpecialty;
    public int UpgradedCrews;

    [Space(10f)]
    public string CarrierName;
    public List<int> CarrierUpgrades;

    [Space(10f)]
    public List<PlaneTypeIntWrapper> PlaneColorIndices;

    [HideInInspector]
    public int CurrentMission = -1;
    [HideInInspector]
    public int CurrentChapter;
    [HideInInspector]
    public bool SavedInIntermission;

    public DayTime SavedSandboxTime;

    [Space(10f)]
    public List<CrewSlotData> CrewsSlots;
    public List<EIslandRoomType> OfficersLastRooms;
    public List<EIslandRoomType> OfficersPrevRooms;
    public List<int> LastSwitches;

    [Space(10f)]
    public List<int> FinishedMissions;

    [Space(10f)]
    public ECarrierType SelectedAircraftCarrier = ECarrierType.CV3;

    [Space(10f)]
    public List<int> ManeuversLevels = new List<int>();

    [Space(10f)]
    public IntermissionData IntermissionData;

    [Space(10f)]
    public SandboxSaveData SandboxData;

    [Space(10f)]
    public EDifficulty Difficulty;

    [HideInInspector]
    public GameData MissionInProgress;

    [HideInInspector]
    public IntermissionMissionData IntermissionMissionData;

    [NonSerialized]
    public long Timestamp;

    [HideInInspector]
    public bool FinishedGame;

    [HideInInspector]
    public bool ShowCongratulations;

    [HideInInspector]
    public bool FinishedTutorial;

    [HideInInspector]
    [SerializeField]
    private int saveVersion = LegacySaveVersion;

    [HideInInspector]
    [SerializeField]
    private int planesUpgrades = default;

    public static NewSaveData GetStartData()
    {
        if (StartSaveData == null)
        {
            StartSaveData = Resources.Load<StartSaveData>("Save/StartSaveData");
        }
        var data = (NewSaveData)StartSaveData.Data.Duplicate();
        data.saveVersion = Version;
        return data;
    }

    public int GetPlaneLv(EPlaneType type)
    {
        return (planesUpgrades >> ((int)type * 2)) % 4;
    }

    public void SetPlaneLv(EPlaneType type, int lv)
    {
        Assert.IsTrue(lv >= 0);
        Assert.IsTrue(lv < 4);
        planesUpgrades &= ~(3 << ((int)type * 2));
        planesUpgrades |= (lv << ((int)type * 2));
    }

    public override BaseSaveData Duplicate()
    {
        var result = new NewSaveData();
        result.saveVersion = saveVersion;

        result.Path = Path;
        result.TempPath = TempPath;

        result.ProfileName = ProfileName;
        result.FreshSave = FreshSave;

        result.GameMode = GameMode;

        result.AdmiralName = AdmiralName;
        result.AdmiralAirLevel = AdmiralAirLevel;
        result.AdmiralNavyLevel = AdmiralNavyLevel;
        result.AdmiralPortrait = AdmiralPortrait;
        result.AdmiralVoice = AdmiralVoice;
        result.AdmiralVisual = new List<int>(AdmiralVisual);
        result.MissionRewards = MissionRewards.Duplicate();

        result.IntermissionMissionData = IntermissionMissionData;

        result.CrewmenSpecialty = new List<ECrewmanSpecialty>(CrewmenSpecialty);
        result.UpgradedCrews = UpgradedCrews;

        result.CarrierName = CarrierName;
        result.CarrierUpgrades = new List<int>(CarrierUpgrades);

        result.planesUpgrades = planesUpgrades;

        result.PlaneColorIndices = new List<PlaneTypeIntWrapper>();
        foreach (var wrapper in PlaneColorIndices)
        {
            result.PlaneColorIndices.Add(wrapper.Duplicate());
        }

        result.CurrentMission = CurrentMission;
        result.SavedSandboxTime = SavedSandboxTime;

        result.CrewsSlots = new List<CrewSlotData>(CrewsSlots);
        result.OfficersLastRooms = new List<EIslandRoomType>(OfficersLastRooms);
        if (OfficersPrevRooms == null)
        {
            result.OfficersPrevRooms = new List<EIslandRoomType>();
        }
        else
        {
            result.OfficersPrevRooms = new List<EIslandRoomType>(OfficersPrevRooms);
        }
        result.LastSwitches = new List<int>(LastSwitches);

        result.FinishedMissions = new List<int>(FinishedMissions);

        result.IntermissionData = IntermissionData.Duplicate();
        result.SandboxData = SandboxData.Duplicate();
        result.MissionInProgress = MissionInProgress.Duplicate();

        result.ManeuversLevels = new List<int>(ManeuversLevels);

        result.IsTutorial = IsTutorial;

        return result;
    }

    protected override void RefreshVersion()
    {
        if (saveVersion != Version)
        {
            saveVersion = Version;
            FreshSave = false;
        }
    }

    protected override void UpgradeSaveFrom(BaseSaveData data)
    {
        var saveData = (NewSaveData)data;

        int version = LegacySaveVersion;
        if (saveVersion == version++)
        {
            saveVersion++;
            ProfileName = saveData.ProfileName;
            SavedSandboxTime = saveData.SavedSandboxTime;
        }
        if (saveVersion == version++) //74
        {
            saveVersion++;
            MissionInProgress = saveData.MissionInProgress.Duplicate();
            MissionInProgress.InProgress = false;
        }
        if (saveVersion == version++) //75
        {
            saveVersion++;
            FinishedGame = saveData.FinishedMissions.Count > 2;
        }
        if (saveVersion == version++) //76
        {
            saveVersion++;
            MissionInProgress.InProgress = false;
        }
        if (saveVersion == version++) //77
        {
            saveVersion++;
            ManeuversLevels = new List<int>(StartSaveData.Data.ManeuversLevels);
        }
        if (saveVersion == version++) //78
        {
            saveVersion++;
        }
        if (saveVersion == version++) //79
        {
            saveVersion++;
            MissionRewards = StartSaveData.Data.MissionRewards.Duplicate();
            IntermissionMissionData = StartSaveData.Data.IntermissionMissionData;
        }
        if (saveVersion < 83)
        {
            if (MissionInProgress.InProgress)
            {
                var ships = MissionInProgress.TacticalMap.EnemyShips;
                for (int i = 0; i < ships.Count; i++)
                {
                    var shipData = ships[i];
                    shipData.BlocksData = new List<EnemyBlockSaveData>();
                    for (int j = 0; j < 9; j++)
                    {
                        shipData.BlocksData.Add(new EnemyBlockSaveData() { Revealed = false, Durability = 9, Alternative = 0 });
                    }
                    ships[i] = shipData;
                }
            }
            saveVersion = 83;
        }
        if (saveVersion < 84)
        {
            Difficulty = StartSaveData.Data.Difficulty;
            saveVersion = 84;
        }
        if (saveVersion < 85)
        {
            var missions = MissionInProgress.Missions;
            if (missions != null)
            {
                for (int i = 0; i < missions.Count; i++)
                {
                    var missionData = missions[i];
                    missionData.SentSquadronsLeft = new List<SquadronsSaveData>();
                    missionData.RecoveryDirections = new List<int>();
                    if (missionData.SentSquadronsDirections != null)
                    {
                        foreach (var dir in missionData.SentSquadronsDirections)
                        {
                            missionData.RecoveryDirections.Add(dir ? 2 : 0);
                        }
                    }
                    missions[i] = missionData;
                }
            }
            saveVersion = 85;
        }
    }

    protected override void UpgradeSaveFromLegacy(SaveData legacyData)
    {
        saveVersion = LegacySaveVersion;

        FreshSave = false;

        GameMode = legacyData.GameMode;

        AdmiralName = legacyData.AdmiralName;
        AdmiralAirLevel = legacyData.AdmiralAirLevel;
        AdmiralNavyLevel = legacyData.AdmiralShipLevel;
        AdmiralPortrait = legacyData.AdmiralPortrait;
        AdmiralVoice = (EVoiceType)legacyData.AdmiralVoice;
        AdmiralVisual = new List<int>(legacyData.AdmiralVisual);

        IntermissionMissionData.CommandsPoints = legacyData.CommandPoints;
        IntermissionMissionData.UpgradePoints = legacyData.UpgradePoints;

        CrewmenSpecialty = new List<ECrewmanSpecialty>();
        foreach (var list in legacyData.CrewmenSpecialties)
        {
            CrewmenSpecialty.Add((list == null || list.Count == 0) ? ECrewmanSpecialty.None : list[0]);
        }
        UpgradedCrews = legacyData.CrewUnlockedSlotsNumber;

        CarrierName = legacyData.ShipName;
        CarrierUpgrades = new List<int>(legacyData.ShipUpgrades);

        planesUpgrades = legacyData.ChosenPlanes;
        PlaneColorIndices = new List<PlaneTypeIntWrapper>(legacyData.PlaneColorIndices);

        CrewsSlots = new List<CrewSlotData>();
        foreach (var slotIndex in legacyData.CrewmenLastSlots)
        {
            var data = new CrewSlotData();
            data.Slot = -1;
            int slot = slotIndex;
            if (slotIndex < IdleSlots)
            {
                data.Department = EDepartments.Count;
                data.Slot = slotIndex;
            }
            slot -= IdleSlots;
            foreach (var department in OldDepartmentsOrder)
            {
                int count = OldDepartmentsSlotCount[department];
                if (slot < count)
                {
                    if (slot < NewDepartmentSlots[department])
                    {
                        data.Department = department;
                        data.Slot = slot;
                    }
                    else
                    {
                        data.Slot = -1;
                    }
                    break;
                }
                slot -= count;
            }
        }

        OfficersLastRooms = new List<EIslandRoomType>(legacyData.OfficersLastRooms);
        LastSwitches = new List<int>(legacyData.LastSwitches);

        FinishedMissions = new List<int>(legacyData.FinishedMissions);
    }
}

using FMODUnity;
#if STEAM_BUILD
using Steamworks;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Achievements : MonoBehaviour
{
    public event Action<EAchievementType> Achievement = delegate { };

    public static Achievements Instance;

    [SerializeField]
    private List<AchievementData> achievements = null;
    [SerializeField]
    [EventRef]
    private string basicAchievementEvent = null;
    [SerializeField]
    [EventRef]
    private string hiddenAchievementEvent = null;

    [SerializeField]
    private List<string> hiddenAchievements = null;

    private Dictionary<EAchievementType, List<AchievementData>> all;
    private Dictionary<EAchievementType, List<AchievementData>> uncompleted;

    private bool steamOK;
    private bool load;
    private bool save;

    private bool setuped;
    private bool intermission;

#if STEAM_BUILD
    private ulong gameID;
    private Callback<UserStatsReceived_t> statsReceived;
    private Callback<UserStatsStored_t> statsStored;
    private Callback<UserAchievementStored_t> achievementStored;
#endif

    private HashSet<EAchievementType> helper;

    private FMODEvent basicAchievementSound;
    private FMODEvent hiddenAchievementSound;
    private HashSet<string> hiddenAchievementsSet;

    private void Awake()
    {
        Instance = this;

        UnityEngine.Cursor.lockState = CursorLockMode.Confined;

        all = new Dictionary<EAchievementType, List<AchievementData>>();
        uncompleted = new Dictionary<EAchievementType, List<AchievementData>>();
        helper = new HashSet<EAchievementType>();
        foreach (var data in achievements)
        {
            if (!all.TryGetValue(data.Type, out var list))
            {
                list = new List<AchievementData>();
                all.Add(data.Type, list);
            }

            if (data.Type == EAchievementType.CompleteChapter)
            {
                while (list.Count <= data.Data)
                {
                    list.Add(null);
                }
                list[data.Data] = data;
            }
            else
            {
                list.Add(data);
            }
        }
        foreach (var pair in all)
        {
            uncompleted.Add(pair.Key, new List<AchievementData>(pair.Value));
        }

        basicAchievementSound = new FMODEvent(basicAchievementEvent);
        hiddenAchievementSound = new FMODEvent(hiddenAchievementEvent);
        hiddenAchievementsSet = new HashSet<string>();
        foreach (var id in hiddenAchievements)
        {
            hiddenAchievementsSet.Add(id);
        }
    }

    private void Start()
    {
#if STEAM_BUILD
        if (!SteamManager.Initialized)
#endif
        {
            return;
        }
#if STEAM_BUILD
        gameID = new CGameID(SteamUtils.GetAppID()).m_GameID;
        statsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        statsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        achievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
#endif

        load = true;
    }

    private void Update()
    {
#if STEAM_BUILD
        if (!SteamManager.Initialized)
        {
            return;
        }
#endif
        if (load)
        {
#if STEAM_BUILD
            load = !SteamUserStats.RequestCurrentStats();
#endif
        }
        if (save)
        {
#if STEAM_BUILD
            save = !SteamUserStats.StoreStats();
#endif
        }
    }

    public void SetupIntermission()
    {
        var interMan = IntermissionManager.Instance;
        if (interMan == null)
        {
            setuped = false;
            return;
        }

        setuped = true;
        intermission = true;

        if (!steamOK)
        {
            return;
        }

        foreach (var data in GetAllDatas(all))
        {
            switch (data.Type)
            {
                case EAchievementType.UnlockCV9:
                    interMan.CarrierBought -= OnCarrierBought;
                    break;
                case EAchievementType.SpentUpgrades:
                    interMan.UsedUpgradePoints -= OnUsedUpgradePoints;
                    break;
            }
        }
        helper.Clear();
        foreach (var data in GetAllDatas(uncompleted))
        {
            switch (data.Type)
            {
                case EAchievementType.UnlockCV9:
                    interMan.CarrierBought += OnCarrierBought;
                    break;
                case EAchievementType.SpentUpgrades:
                    interMan.UsedUpgradePoints += OnUsedUpgradePoints;
                    break;
            }
        }
        helper.Clear();
    }

    public void SetupGameplay()
    {
        if (GameSceneManager.Instance == null)
        {
            setuped = false;
            return;
        }

        setuped = true;
        intermission = false;

        if (!steamOK)
        {
            return;
        }

        var attacksMan = EnemyAttacksManager.Instance;
        var crewMan = CrewManager.Instance;
        var stateMan = GameStateManager.Instance;
        var sectionMan = SectionRoomManager.Instance;
        var tacMan = TacticManager.Instance;
        foreach (var data in GetAllDatas(all))
        {
            switch (data.Type)
            {
                case EAchievementType.TacticMapMove:
                    tacMan.Carrier.ShipPositionChanged -= OnShipPositionChanged;
                    break;
                case EAchievementType.SurviveAttack:
                    attacksMan.EnemyAttacked -= OnEnemyAttacked;
                    break;
                case EAchievementType.SinkEnemy:
                    tacMan.ObjectDestroyed -= OnObjectDestroyed;
                    break;
                case EAchievementType.OfficerMedals:
                    stateMan.OfficerGainedMedal -= OnOfficerGainedMedal;
                    break;
                case EAchievementType.CompleteTutorial:
                    stateMan.TutorialFinished -= OnTutorialFinished;
                    break;
                case EAchievementType.CompleteChapter:
                    stateMan.ChapterFinished -= OnChapterFinished;
                    break;
                case EAchievementType.CompleteAll:
                    helper.Add(EAchievementType.CompleteAll);
                    stateMan.MissionFinished -= OnMissionFinished;
                    break;
                case EAchievementType.SinkBlocks:
                    tacMan.EnemyBlockDestroyed -= OnEnemyBlockDestroyed;
                    break;
                case EAchievementType.KillCrews:
                    crewMan.CrewDead -= OnCrewDead;
                    break;
                case EAchievementType.BlindKamikaze:
                    tacMan.BlindKamikaze -= OnBlindKamikaze;
                    break;
                case EAchievementType.SinkAfterWar:
                    tacMan.EnemyBlockDestroyedAfterWar -= OnEnemyBlockDestroyedAfterWar;
                    break;
                case EAchievementType.Die:
                    sectionMan.CarrierDestroyed -= OnCarrierDestroyed;
                    break;
                case EAchievementType.Travel:
                    tacMan.SwimMile -= OnSwimMile;
                    break;
            }
        }
        helper.Clear();
        foreach (var data in GetAllDatas(uncompleted))
        {
            switch (data.Type)
            {
                case EAchievementType.TacticMapMove:
                    tacMan.Carrier.ShipPositionChanged += OnShipPositionChanged;
                    break;
                case EAchievementType.SurviveAttack:
                    attacksMan.EnemyAttacked += OnEnemyAttacked;
                    break;
                case EAchievementType.SinkEnemy:
                    tacMan.ObjectDestroyed += OnObjectDestroyed;
                    break;
                case EAchievementType.OfficerMedals:
                    stateMan.OfficerGainedMedal += OnOfficerGainedMedal;
                    break;
                case EAchievementType.CompleteTutorial:
                    stateMan.TutorialFinished += OnTutorialFinished;
                    break;
                case EAchievementType.CompleteChapter:
                    stateMan.ChapterFinished += OnChapterFinished;
                    break;
                case EAchievementType.CompleteAll:
                    helper.Add(EAchievementType.CompleteAll);
                    stateMan.MissionFinished += OnMissionFinished;
                    break;
                case EAchievementType.SinkBlocks:
                    tacMan.EnemyBlockDestroyed += OnEnemyBlockDestroyed;
                    break;
                case EAchievementType.KillCrews:
                    crewMan.CrewDead += OnCrewDead;
                    break;
                case EAchievementType.BlindKamikaze:
                    tacMan.BlindKamikaze += OnBlindKamikaze;
                    break;
                case EAchievementType.SinkAfterWar:
                    tacMan.EnemyBlockDestroyedAfterWar += OnEnemyBlockDestroyedAfterWar;
                    break;
                case EAchievementType.Die:
                    sectionMan.CarrierDestroyed += OnCarrierDestroyed;
                    break;
                case EAchievementType.Travel:
                    tacMan.SwimMile += OnSwimMile;
                    break;
            }
        }
        helper.Clear();
    }

    public void ForceSave(Action onFinish)
    {
#if STEAM_BUILD
        if (!SteamManager.Initialized)
#endif
        {
            onFinish?.Invoke();
            return;
        }

        StartCoroutine(SaveCoroutine(onFinish));
    }

    public void ResetAchievements()
    {
#if STEAM_BUILD
        SteamUserStats.ResetAllStats(true);
#endif
        foreach (var pair in all)
        {
            uncompleted[pair.Key] = new List<AchievementData>(pair.Value);
        }
        if (setuped)
        {
            if (intermission)
            {
                SetupIntermission();
            }
            else
            {
                SetupGameplay();
            }
        }
    }

    private IEnumerator SaveCoroutine(Action action)
    {
        int count = 0;
        while (save)
        {
            if (++count > 1000)
            {
                Debug.LogError("error saving achievements");
                break;
            }
            Update();
            yield return null;
        }
        action?.Invoke();
    }

    private IEnumerable<AchievementData> GetAllDatas(Dictionary<EAchievementType, List<AchievementData>> dict)
    {
        foreach (var list in dict.Values)
        {
            foreach (var data in list)
            {
                if (data == null || !helper.Add(data.Type))
                {
                    continue;
                }
                yield return data;
                break;
            }
        }
    }

    private void OnCarrierBought(ECarrierType type)
    {
        if (!uncompleted.TryGetValue(EAchievementType.UnlockCV9, out var list))
        {
            return;
        }
        var data = list[0];
        if (type == ECarrierType.CV9)
        {
            IntermissionManager.Instance.CarrierBought -= OnCarrierBought;

            uncompleted.Remove(EAchievementType.UnlockCV9);
            FireAchievement(EAchievementType.UnlockCV9, data.Id);
        }
    }

    private void OnUsedUpgradePoints(int points)
    {
        if (!uncompleted.TryGetValue(EAchievementType.SpentUpgrades, out var list) || points < 1)
        {
            return;
        }
        Increase(list[0], points);
    }

    private void OnShipPositionChanged()
    {
        TacticManager.Instance.Carrier.ShipPositionChanged -= OnShipPositionChanged;
        SingleAchievement(EAchievementType.TacticMapMove);
    }

    private void OnEnemyAttacked()
    {
        EnemyAttacksManager.Instance.EnemyAttacked -= OnEnemyAttacked;
        SingleAchievement(EAchievementType.SurviveAttack);
    }

    private void OnObjectDestroyed(int id, bool ok)
    {
        var tacMan = TacticManager.Instance;
        var ship = tacMan.GetShip(id);
        if (ok && ship.Side == ETacticalObjectSide.Enemy && ship.Dead && !ship.NotByPlayer)
        {
            tacMan.ObjectDestroyed -= OnObjectDestroyed;
            SingleAchievement(EAchievementType.SinkEnemy);
        }
    }

    private void OnOfficerGainedMedal(int medalCount)
    {
        if (!uncompleted.TryGetValue(EAchievementType.OfficerMedals, out var list))
        {
            return;
        }
        var data = list[0];
        if (medalCount >= data.Data)
        {
            GameStateManager.Instance.OfficerGainedMedal -= OnOfficerGainedMedal;

            uncompleted.Remove(EAchievementType.OfficerMedals);
            FireAchievement(EAchievementType.OfficerMedals, data.Id);
        }
    }

    private void OnTutorialFinished(int tutorial)
    {
        if (!uncompleted.TryGetValue(EAchievementType.CompleteTutorial, out var list))
        {
            return;
        }
        CheckBit(list[0], 1 << tutorial);
    }

    private void OnChapterFinished(int id)
    {
        if (!uncompleted.TryGetValue(EAchievementType.CompleteChapter, out var list))
        {
            return;
        }
        if (list.Count > id && list[id] != null)
        {
            FireAchievement(EAchievementType.CompleteChapter, list[id].Id);
            list[id] = null;
            if (CheckRemove(EAchievementType.CompleteChapter, list))
            {
                GameStateManager.Instance.ChapterFinished -= OnChapterFinished;
            }
        }
    }

    private void OnMissionFinished(int id, bool completed)
    {
        if (!completed || !uncompleted.TryGetValue(EAchievementType.CompleteAll, out var list))
        {
            return;
        }
        CheckBit(list[0], 1 << (SaveManager.Instance.Data.CurrentChapter * 4 + id));
        Log(list[0].StoredData);
    }

    private void OnEnemyBlockDestroyed(EnemyManeuverData data)
    {
        if (!uncompleted.TryGetValue(EAchievementType.SinkBlocks, out var list))
        {
            return;
        }
        Increase(list[0], 1);
    }

    private void OnCrewDead()
    {
        if (!uncompleted.TryGetValue(EAchievementType.KillCrews, out var list))
        {
            return;
        }
        Increase(list[0], 1);
    }

    private void OnBlindKamikaze()
    {
        TacticManager.Instance.BlindKamikaze -= OnBlindKamikaze;
        SingleAchievement(EAchievementType.BlindKamikaze);
    }

    private void OnEnemyBlockDestroyedAfterWar()
    {
        TacticManager.Instance.EnemyBlockDestroyedAfterWar -= OnEnemyBlockDestroyedAfterWar;
        SingleAchievement(EAchievementType.SinkAfterWar);
    }

    private void OnCarrierDestroyed()
    {
        if (!uncompleted.TryGetValue(EAchievementType.Die, out var list))
        {
            return;
        }
        Increase(list[0], 1);
    }

    private void OnSwimMile()
    {
        if (!uncompleted.TryGetValue(EAchievementType.Travel, out var list))
        {
            return;
        }
        Increase(list[0], 1);
    }

    private void SingleAchievement(EAchievementType type)
    {
        if (!uncompleted.TryGetValue(type, out var list))
        {
            return;
        }
        uncompleted.Remove(type);
        FireAchievement(type, list[0].Id);
    }

    private void CheckBit(AchievementData data, int bit)
    {
        if ((data.StoredData & bit) == bit)
        {
            return;
        }
        data.StoredData |= bit;
#if STEAM_BUILD
        SteamUserStats.SetStat(data.StatID, data.StoredData);
        SteamUserStats.SetStat(data.Stat2ID, BinUtils.CountBits(data.StoredData));
#endif
        save = true;
    }

    private void Increase(AchievementData data, int count)
    {
        data.StoredData += count;
#if STEAM_BUILD
        SteamUserStats.SetStat(data.StatID, data.StoredData);
#endif
        save = true;
    }

    private void FireAchievement(EAchievementType type, string id)
    {
#if STEAM_BUILD
        SteamUserStats.SetAchievement(id);
#endif
        Achievement(type);
        save = true;
    }

#if STEAM_BUILD
    private void OnUserStatsReceived(UserStatsReceived_t steamData)
    {
        if (steamData.m_nGameID != gameID || steamData.m_eResult != EResult.k_EResultOK)
        {
            return;
        }
        steamOK = true;
        foreach (var pair in all)
        {
            if (pair.Key == EAchievementType.SinkBlocks)
            {
                pair.ToString();
            }
            uncompleted.TryGetValue(pair.Key, out var list);
            for (int i = 0; i < pair.Value.Count; i++)
            {
                var data = pair.Value[i];
                if (data == null)
                {
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(data.StatID))
                {
                    SteamUserStats.GetStat(data.StatID, out data.StoredData);
                }
                if (SteamUserStats.GetAchievement(data.Id, out bool achieved))
                {
                    if (achieved)
                    {
                        if (list != null)
                        {
                            list[i] = null;
                            if (CheckRemove(pair.Key, list))
                            {
                                list = null;
                            }
                        }
                    }
                    else if (list == null || list[i] == null)
                    {
                        SteamUserStats.SetAchievement(data.Id);
                        save = true;
                    }
                }

            }
        }
        if (setuped)
        {
            if (intermission)
            {
                SetupIntermission();
            }
            else
            {
                SetupGameplay();
            }
        }
    }

    private void OnUserStatsStored(UserStatsStored_t steamData)
    {
        if (steamData.m_nGameID != gameID)
        {
            return;
        }
        switch (steamData.m_eResult)
        {
            case EResult.k_EResultOK:
                CheckAchievementProgress(EAchievementType.SinkBlocks);
                CheckAchievementProgress(EAchievementType.SpentUpgrades);
                CheckAchievementProgress(EAchievementType.KillCrews);
                CheckAchievementProgress(EAchievementType.Die);
                CheckAchievementProgress(EAchievementType.Travel);

                CheckAchievementBitProgress(EAchievementType.CompleteTutorial);
                CheckAchievementBitProgress(EAchievementType.CompleteAll);
                if (SteamUserStats.GetStat("NEW_STAT_1_9a", out int bits))
                {
                    Log(bits);
                }
                else
                {
                    Debug.Log("[ACHi]Cannot get stats");
                }
                if (save)
                {
                    OnUserStatsReceived();
                }
                break;
            case EResult.k_EResultInvalidParam:
                OnUserStatsReceived();
                break;
        }
    }

    private void OnAchievementStored(UserAchievementStored_t steamData)
    {
        if (steamData.m_nGameID != gameID)
        {
            return;
        }
        if (hiddenAchievementsSet.Contains(steamData.m_rgchAchievementName))
        {
            hiddenAchievementSound.Play();
        }
        else
        {
            basicAchievementSound.Play();
        }
    }

    private void OnUserStatsReceived()
    {
        OnUserStatsReceived(new UserStatsReceived_t() { m_nGameID = gameID, m_eResult = EResult.k_EResultOK });
    }
#endif

    private bool CheckRemove(EAchievementType type, List<AchievementData> datas)
    {
        foreach (var data in datas)
        {
            if (data != null)
            {
                return false;
            }
        }
        uncompleted.Remove(type);
        return true;
    }

    private void CheckAchievementProgress(EAchievementType type)
    {
        switch (type)
        {
            case EAchievementType.SinkBlocks:
            case EAchievementType.SpentUpgrades:
            case EAchievementType.KillCrews:
            case EAchievementType.Die:
            case EAchievementType.Travel:
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
        if (uncompleted.TryGetValue(type, out var list))
        {
            var data = list[0];

#if STEAM_BUILD
            if (SteamUserStats.GetStat(data.StatID, out data.StoredData) && data.StoredData >= data.Data)
#endif
            {
                FireAchievement(type, data.Id);
                uncompleted.Remove(type);
            }
        }
    }

    private void CheckAchievementBitProgress(EAchievementType type)
    {
        Assert.IsTrue(type == EAchievementType.CompleteTutorial || type == EAchievementType.CompleteAll);
        if (uncompleted.TryGetValue(type, out var list))
        {
            var data = list[0];
            int value = (1 << data.Data) - 1;
#if STEAM_BUILD
            if (SteamUserStats.GetStat(data.StatID, out data.StoredData) && (data.StoredData & value) == value)
#endif
            {
                FireAchievement(type, data.Id);
                uncompleted.Remove(type);
            }
        }
    }

    private void Log(int bits)
    {
        Debug.Log("[ACHi]Finished missions:");
        for (int i = 0; i < 32; i++)
        {
            if ((bits & (1 << i)) != 0)
            {
                Debug.Log($"C{(i / 4) + 1}M{(i % 4) + 1}");
            }
        }
    }
}

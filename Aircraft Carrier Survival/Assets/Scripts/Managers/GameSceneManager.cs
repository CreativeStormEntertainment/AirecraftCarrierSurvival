using GambitUtils;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Assertions;

public class GameSceneManager : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Data/Load difficulties", false, 209)]
    private static void LoadDifficulties()
    {
        var lines = TSVUtils.LoadData(@"Assets\Data\TSV\Difficulties.tsv");
        var difficulties = new List<DifficultyData>();
        foreach (var line in lines)
        {
            difficulties.Add(new DifficultyData(line));
        }

        var gameMan = GameObject.Find("Managers").GetComponent<GameSceneManager>();
        Undo.RecordObject(gameMan, "Loaded difficulties");
        gameMan.Difficulties = difficulties;
    }

    [MenuItem("Tools/TacticMaps/Cache maps", false, 203)]
    private static void CacheMaps()
    {
        var gameMan = GameObject.Find("Managers").GetComponent<GameSceneManager>();
        Undo.RecordObject(gameMan, "Cached new maps");

        foreach (var folder in Directory.GetDirectories(@"Assets\GameplayAssets\ScriptableData\TacticMaps"))
        {
            foreach (var file in Directory.GetFiles(folder, "*.asset"))
            {
                var map = AssetDatabase.LoadAssetAtPath<SOTacticMap>(file);
                if (!gameMan.maps.Contains(map))
                {
                    gameMan.maps.Add(map);
                }
            }
        }
    }

#endif
    public static GameSceneManager Instance;

    public bool IsLoading
    {
        get;
        private set;
    }

    public DemoMissionGame Demo;

    // change to find by get component
    public Canvas AircraftCanvas;
    public GameObject TacticalFightModule;

    public List<DifficultyData> Difficulties;
    public ScenarioData GameScenario;

    [SerializeField]
    private bool forceGameMode = false;
    [SerializeField]
    private EGameMode gameMode = EGameMode.Fabular;

    [SerializeField]
    private GameObject temporaryTutorial = null;

    //[HideInInspector]
    [SerializeField]
    private List<SOTacticMap> maps = null;

    [SerializeField]
    private ForcesBar forcesBar = null;
    [SerializeField]
    private DamageRange damageRange = null;
    [SerializeField]
    private DamageRange damageRange2 = null;
    [SerializeField]
    private UIManager uiManager = null;

    private LocalResourceManager localResourceManager;
    private GlobalResourceManager globalResourceManager;
    private AirCraftCarrier aircraftCarrier;
    private EffectManager effectManager;
    private TimeManager timeManager;
    private SectionRoomManager sectionRoomManager;
    private PortraitManager portraitManager;
    private IslandsAndOfficersManager islandsAndOfficersManager;
    private WorkerInstancesManager workerInstancesManager;
    private EventManager eventManager;
    private WeatherManager weatherManager;
    private DamageControlManager dcManager;
    private PlaneMovementManager planeMovementManager;
    private AircraftCarrierDeckManager deckManager;
    private CrewManager crewManager;
    private StrikeGroupManager strikeGroupManager;
    private CameraManager cameraManager;
    private ReportManager reportManager;
    private ObjectivesManager objectivesManager;
    private VoiceSoundsManager voiceManager;
    private HudManager hudManager;
    private ResourceManager resourceManager;
    private EnemyAttacksManager enemyAttacksManager;
    private TacticManager tacticManager;
    private SimpleTimerLabel timer;
    private WorldMap worldMap;

    private void Awake()
    {
        Instance = this;

        BasicInput.Instance.Disable();
    }

    private void Start()
    {
        Setup();
    }

    public void StartDemo()
    {
        Demo.gameObject.SetActive(false);
    }

    private void Setup()
    {
        GameScenario = ScenarioManager.Instance.Scenarios[0];
        eventManager = GetComponent<EventManager>();
        objectivesManager = GetComponent<ObjectivesManager>();

        var saveMan = SaveManager.Instance;
        var input = BasicInput.Instance;
        if (!input.WasLoaded)
        {
            input.Load(ref saveMan.PersistentData.InputData);
        }
        input.Enable();

#if UNITY_EDITOR
        if (forceGameMode)
        {
            saveMan.Data.MissionInProgress.InProgress = saveMan.Data.MissionInProgress.InProgress && saveMan.Data.GameMode == gameMode;
            saveMan.Data.GameMode = gameMode;
        }
#endif
        if (saveMan.Data.MissionInProgress.InProgress)
        {
            eventManager.LoadStart();
            objectivesManager.LoadStart();
        }
        //GameScenario.GameplayInit(eventManager);
        MapMarkersManager.Instance.Setup();

        temporaryTutorial.SetActive(saveMan.TransientData.HighlightHelp);

        aircraftCarrier = FindObjectOfType<AirCraftCarrier>();
        aircraftCarrier.SetCarrierName(saveMan.Data.CarrierName);

        effectManager = GetComponent<EffectManager>();

        timeManager = GetComponent<TimeManager>();

        tacticManager = GetComponent<TacticManager>();

        var fabularMap = tacticManager.EmptyMap;
        if (saveMan.Data.GameMode == EGameMode.Fabular)
        {
            if (saveMan.Data.MissionInProgress.InProgress)
            {
                int map = saveMan.Data.MissionInProgress.MissionMap;
                fabularMap = maps[map];
                saveMan.TransientData.FabularTacticMap = fabularMap;
                saveMan.TransientData.LastMission = saveMan.Data.MissionInProgress.LastMission;
            }
            else
            {
                fabularMap = saveMan.TransientData.FabularTacticMap == null ? tacticManager.DefaultMap : saveMan.TransientData.FabularTacticMap;
#if UNITY_EDITOR
                saveMan.TransientData.FabularTacticMap = fabularMap;
#endif
            }
        }

        GameStateManager.Instance.Tutorial = fabularMap.Overrides.OfficersData != null;

        deckManager = GetComponent<AircraftCarrierDeckManager>();
        deckManager.PreSetup();

        tacticManager.PreSetup(saveMan.Data.MissionRewards.ActiveBuff);

        islandsAndOfficersManager = GetComponent<IslandsAndOfficersManager>();
        islandsAndOfficersManager.Setup(fabularMap);

        crewManager = GetComponentInChildren<CrewManager>();
        crewManager.Setup(fabularMap);

        enemyAttacksManager = GetComponent<EnemyAttacksManager>();
        enemyAttacksManager.Setup(saveMan.Data.MissionRewards.ActiveBuff, saveMan.Data.GameMode);

        portraitManager = GetComponent<PortraitManager>();
        portraitManager.Setup(islandsAndOfficersManager);

        sectionRoomManager = GetComponent<SectionRoomManager>();
        sectionRoomManager.Setup(fabularMap);

        planeMovementManager = GetComponent<PlaneMovementManager>();
        planeMovementManager.Setup();

        dcManager = GetComponent<DamageControlManager>();
        dcManager.Setup(7);

        workerInstancesManager = GetComponent<WorkerInstancesManager>();
        workerInstancesManager.Setup();

        sectionRoomManager.PostSetup();

        weatherManager = GetComponent<WeatherManager>();

        resourceManager = GetComponent<ResourceManager>();
        resourceManager.Setup();

        deckManager.Setup();

        strikeGroupManager = GetComponent<StrikeGroupManager>();
        strikeGroupManager.Setup(fabularMap);

        cameraManager = GetComponent<CameraManager>();
        cameraManager.Setup(fabularMap);

        reportManager = GetComponent<ReportManager>();
        reportManager.Setup(deckManager.BomberLv, deckManager.FighterLv, deckManager.TorpedoLv);

        voiceManager = GetComponent<VoiceSoundsManager>();
        voiceManager.Setup(saveMan.Data.AdmiralVoice, 2);

        hudManager = GetComponent<HudManager>();
        hudManager.Setup(saveMan.Data.GameMode);

        tacticManager.Setup(fabularMap, saveMan.Data.MissionRewards.ActiveBuff);

        worldMap = WorldMap.Instance;

        switch (saveMan.Data.GameMode)
        {
            case EGameMode.Tutorial:
                Demo.Setup();
                break;
            case EGameMode.Fabular:
                if (saveMan.Data.MissionInProgress.InProgress)
                {
                    tacticManager.ChangeMapLayout(fabularMap, false, false);
                    saveMan.Data.MissionInProgress.HasMissionStartTime = true;
                }
                else
                {
                    StartDemo();
                    tacticManager.ChangeMapLayout(fabularMap, false, true);
                }
                break;
            case EGameMode.Sandbox:
                timeManager.SetTime(saveMan.Data.SavedSandboxTime);
                SandboxManager.Instance.Setup();
                if (!saveMan.Data.SandboxData.CurrentMissionSaveData.MapInstanceInProgress)
                {
                    hudManager.ShowWorldMap();
                }
                Assert.IsTrue(saveMan.Data.SandboxData.CurrentMissionSaveData.MapInstanceInProgress == saveMan.Data.MissionInProgress.InProgress);
                break;
        }

        objectivesManager.Setup();

        eventManager.Setup();

        //dcManager.SpawnDC();
        dcManager.PostSetup();

        timer = SceneUtils.FindObjectOfType<SimpleTimerLabel>();
        Assert.IsNotNull(timer);
        ref var missionInProgress = ref saveMan.Data.MissionInProgress;
        if (missionInProgress.InProgress)
        {
            eventManager.LoadStart();
            enemyAttacksManager.LoadStart();

            timeManager.LoadData(ref missionInProgress.Time);
            crewManager.LoadData(ref missionInProgress.CrewManager);
            strikeGroupManager.LoadData(ref missionInProgress.StrikeGroup, ref missionInProgress.VisualsData);
            islandsAndOfficersManager.LoadData(missionInProgress.IslandBuffs);

            resourceManager.LoadData(ref missionInProgress.Supplies);
            sectionRoomManager.LoadData(missionInProgress.Segments);
            dcManager.LoadData(ref missionInProgress.DamageControl);

            IsLoading = true;
            if (saveMan.Data.GameMode == EGameMode.Sandbox)
            {
                this.StartCoroutineActionAfterFrames(() => LoadOther(), 5);
                StartCoroutine(ResetTimeManager(true));
            }
            else
            {
                LoadOther();

                StartCoroutine(ResetTimeManager(false));
            }
        }

        var achievements = Achievements.Instance;
        if (achievements != null)
        {
            achievements.SetupGameplay();
        }
    }

    public void UpdateSave()
    {
        var saveMan = SaveManager.Instance;

        SaveContemporary();

        ref var missionInProgress = ref saveMan.Data.MissionInProgress;

        missionInProgress.InProgress = saveMan.Data.GameMode != EGameMode.Sandbox || saveMan.Data.SandboxData.CurrentMissionSaveData.MapInstanceInProgress;
        missionInProgress.LastMission = saveMan.TransientData.LastMission;

        missionInProgress.MissionMap = maps.IndexOf(saveMan.TransientData.FabularTacticMap);
        if (missionInProgress.MissionMap == -1)
        {
#if !UNITY_EDITOR
            if (saveMan.TransientData.FabularTacticMap != null)
#endif
            {
                Debug.LogError("Map not exist");
            }
        }

        timeManager.SaveData(ref missionInProgress.Time);
        crewManager.SaveData(ref missionInProgress.CrewManager);
        strikeGroupManager.SaveData(ref missionInProgress.StrikeGroup, ref missionInProgress.VisualsData);
        islandsAndOfficersManager.SaveData(missionInProgress.IslandBuffs);

        resourceManager.SaveData(ref missionInProgress.Supplies);
        sectionRoomManager.SaveData(missionInProgress.Segments);
        dcManager.SaveData(ref missionInProgress.DamageControl);

        tacticManager.SaveData(ref missionInProgress.TacticalMap, missionInProgress.Missions);

        deckManager.SaveData(ref missionInProgress.Deck);
        enemyAttacksManager.SaveData(ref missionInProgress.EnemyAttacks);
        cameraManager.SaveData(ref missionInProgress.Camera);

        objectivesManager.SaveData(missionInProgress.Objectives);
        eventManager.SaveData(missionInProgress.EventsData);
        timer.SaveData(ref missionInProgress.MissionTimer);

        var list = forcesBar.SaveData();
        if (list == null)
        {
            list = new List<int>();
        }
        else
        {
            list = new List<int>(list);
        }
        missionInProgress.ForcesBarShips = list;
        missionInProgress.DamageRange = damageRange.SaveData();
        missionInProgress.DamageRange2 = damageRange2.SaveData();
        missionInProgress.List = new List<int>(uiManager.SaveData());

        uiManager.SaveShowable(ref missionInProgress.VisualsData);

        if (saveMan.Data.GameMode == EGameMode.Sandbox)
        {
            worldMap.SaveData(ref saveMan.Data.MissionInProgress.WorldMap);
            SandboxManager.Instance.Save();
        }
        saveMan.Data.SavedInIntermission = false;
    }

    public void SaveContemporary()
    {
        islandsAndOfficersManager.SaveOfficersAndSwitches();
        crewManager.SaveCrewmenPositions();
    }

    public void ChangeToAircraftModule()
    {
    }

    private void LoadOther()
    {
        IsLoading = false;
        ref var missionInProgress = ref SaveManager.Instance.Data.MissionInProgress;
        tacticManager.LoadData(ref missionInProgress.TacticalMap, missionInProgress.Missions);
        strikeGroupManager.LateLoadData(ref missionInProgress.StrikeGroup);

        deckManager.LoadData(ref missionInProgress.Deck);
        enemyAttacksManager.LoadData(ref missionInProgress.EnemyAttacks);
        cameraManager.LoadData(ref missionInProgress.Camera);

        objectivesManager.LoadData(missionInProgress.Objectives);
        eventManager.LoadData(missionInProgress.EventsData);
        timer.LoadData(ref missionInProgress.MissionTimer);

        forcesBar.LoadData(missionInProgress.ForcesBarShips);
        damageRange.LoadData(ref missionInProgress.DamageRange);
        damageRange2.LoadData(ref missionInProgress.DamageRange2);
        uiManager.Load(missionInProgress.List);
        uiManager.LoadShowable(ref missionInProgress.VisualsData);

        DeckOrderPanelManager.Instance.UpdateOrders();
    }

    private IEnumerator ResetTimeManager(bool waitMore)
    {
        var timeMan = TimeManager.Instance;
        timeMan.enabled = false;

        int count = waitMore ? 0 : 10;
        var time = DateTime.Now;
        do
        {
            count++;
            yield return null;
        }
        while ((DateTime.Now - time).TotalSeconds < 1.5d || count < 5);

        timeMan.enabled = true;
        timeMan.LoadTime();
    }
}

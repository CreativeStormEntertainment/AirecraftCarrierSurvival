using FMODUnity;
using GambitUtils;
using UnityEngine;
using System;

public class SandboxManager : MonoBehaviour, ITickable
{
    public static SandboxManager Instance;

    public event Action MissionInstanceFinished = delegate { };

    public PoiManager PoiManager => poiManager;
    public SandboxGoalsManager SandboxGoalsManager => sandboxGoalsManager;
    public SandboxTerritoryManager SandboxTerritoryManager => sandboxTerritoryManager;
    public SandboxAdmiralLevels SandboxAdmiralLevels => admiralLevels;
    public WorldMapFleetsManager WorldMapFleetsManager => worldMapFleetsManager;
    public MissionInstanceRewardsScriptable RewardsScriptable => rewardsScriptable;
    public SandboxEventsManager SandboxEventsManager => sandboxEventsManager;

    [SerializeField]
    private GameObject sandboxManagers = null;
    [SerializeField]
    private int hoursToDraw = 6;
    [SerializeField]
    private float chanceToSpawnEnemy = 0.25f;
    [SerializeField]
    private PoiManager poiManager = null;
    [SerializeField]
    private SandboxGoalsManager sandboxGoalsManager = null;
    [SerializeField]
    private SandboxTerritoryManager sandboxTerritoryManager = null;
    [SerializeField]
    private SandboxEventsManager sandboxEventsManager = null;
    [SerializeField]
    private WorldMapFleetsManager worldMapFleetsManager = null;
    [SerializeField]
    private SandboxAdmiralLevels admiralLevels = null;
    [SerializeField]
    private MissionInstanceRewardsScriptable rewardsScriptable = null;
    [SerializeField]
    private SandboxMapTexturesCreator sandboxMapTexturesCreator = null;
    [SerializeField]
    private StudioEventEmitter mapEnteranceSound = null;

    private UIManager uiManager;
    private WorldMap worldMap;
    private int ticks;

    private void Awake()
    {
        if (SaveManager.Instance.Data.GameMode == EGameMode.Sandbox)
        {
            Instance = this;
        }
    }

    public void Setup()
    {
        var saveData = SaveManager.Instance.Data;
        if (saveData.GameMode == EGameMode.Sandbox)
        {
            uiManager = UIManager.Instance;
            worldMap = WorldMap.Instance;
            worldMap.Preinit();
            worldMap.Setup();
            if (saveData.SandboxData.IsSaved)
            {
                ticks = saveData.SandboxData.TicksPassedToRedWatersPopup;
                worldMap.Load(ref saveData.MissionInProgress.WorldMap);
            }
            sandboxManagers.SetActive(true);
            sandboxGoalsManager.Setup();
            poiManager.Setup();
            sandboxTerritoryManager.Setup();
            sandboxEventsManager.Setup();
            worldMapFleetsManager.Setup();
            var data = saveData.SandboxData.CurrentMissionSaveData;
            if (data.MapInstanceInProgress)
            {
                HudManager.Instance.HideWorldMap();
                var map = worldMap.SandboxMapSpawner.Load(ref saveData.SandboxData.SandboxSpawnMapData);
                TacticManager.Instance.ChangeMapLayout(map, true, false);
                sandboxMapTexturesCreator.ClearTexture();
                StartCoroutine(sandboxMapTexturesCreator.
                    SetMapTextures(TacticalMapCreator.TransformWorldMapPointTo16kMapPoint(PoiManager.GetNode(saveData.SandboxData.CurrentMissionSaveData.NodeIndex).Position
                    + new Vector2(0f, (TacticalMapCreator.TacticMapHeight / 2f) - TacticalMapCreator.VerticalOffset * TacticalMapCreator.TacticToWorldMapScale)), OnLandTextureChanged));
                //SetMission(map, PoiManager.GetNode(saveData.SandboxData.CurrentMissionSaveData.NodeIndex));
            }
            TimeManager.Instance.AddTickable(this);
        }
    }

    public void Save()
    {
        var saveData = SaveManager.Instance.Data;
        saveData.SandboxData.IsSaved = true;
        saveData.SandboxData.TicksPassedToRedWatersPopup = ticks;
        poiManager.Save(ref saveData.SandboxData);
        sandboxGoalsManager.Save(ref saveData.SandboxData);
        sandboxTerritoryManager.Save(ref saveData.SandboxData);
        sandboxEventsManager.Save(ref saveData.SandboxData);
        worldMapFleetsManager.Save(ref saveData.SandboxData.WorldMapFleetsSaveData);
        if (saveData.SandboxData.CurrentMissionSaveData.MapInstanceInProgress)
        {
            worldMap.SandboxMapSpawner.Save(ref saveData.SandboxData.SandboxSpawnMapData);
        }
    }

    public void Tick()
    {
        poiManager.Tick();
        sandboxEventsManager.Tick();
        if (worldMap.gameObject.activeInHierarchy && (worldMap.MapShip as WorldMapShip).CurrentNode.TerritoryType == ETerritoryType.RedWaters)
        {
            var timeMan = TimeManager.Instance;
            ticks += timeMan.WorldMapTickQuotient;
            if (ticks >= hoursToDraw * timeMan.TicksForHour)
            {
                ticks = 0;
                if (UnityEngine.Random.value <= chanceToSpawnEnemy)
                {
                    ShowEnemyInstancePopup(PoiManager.GetClosestObjectiveNode(worldMap.MapShip.Position, ESandboxObjectiveType.EnemyFleetInstance), EMissionDifficulty.Medium);
                }
            }
        }
        else
        {
            ticks = 0;
        }

        sandboxGoalsManager.Tick();
        worldMapFleetsManager.Tick();
    }

    public void FireMissionInstanceFinished()
    {
        MissionInstanceFinished();
    }

    public void SetMission(SOTacticMap mission, SandboxNode node)
    {
        ticks = 0;

        StartCoroutine(sandboxMapTexturesCreator.SetMapTextures(TacticalMapCreator.TransformWorldMapPointTo16kMapPoint(node.Position +
            new Vector2(0f, (TacticalMapCreator.TacticMapHeight / 2f) - TacticalMapCreator.VerticalOffset * TacticalMapCreator.TacticToWorldMapScale)), OnLandTextureChanged));
        var timeMan = TimeManager.Instance;
        timeMan.SetTime(timeMan.GetCurrentTime());
        this.StartCoroutineActionAfterFrames(() =>
        {
            TacticManager.Instance.ChangeMapLayout(mission, true, false);
            sandboxEventsManager.MapInstanceEntranceConsequence();
        }, 5);
        HudManager.Instance.HideWorldMap();
        mapEnteranceSound.Play();
    }

    public void ShowMissionInstancePopup(SandboxPoi poi)
    {
        uiManager.MissionInstancePopup.Show(poi);
    }

    public void ShowRepairSpotPopup(SandboxPoi poi)
    {
        uiManager.RepairSpotPopup.Show(poi);
    }

    public void ShowSandboxQuestPopup(SandboxPoi poi)
    {
        uiManager.SandboxQuestPopup.Show(poi);
    }

    public void ShowSandboxPearlHarbourPopup(SandboxPoi poi)
    {
        uiManager.PearlHarbourPopup.Show(poi);
    }

    public void ShowEnemyInstancePopup(SandboxNode node, EMissionDifficulty difficulty, int buildingBlocks = -1, WorldMapFleet fleet = null)
    {
        if (!SaveManager.Instance.Data.SandboxData.CurrentMissionSaveData.MapInstanceInProgress)
        {
            uiManager.EnemyInstancePopup.Show(node, difficulty, buildingBlocks, fleet);
        }
    }

    private void ShowAbortMissionPopup()
    {
        uiManager.AbortMissionPopup.Show();
    }

    private void OnLandTextureChanged()
    {
        TacticManager.Instance.FireNodesChanged();
    }
}

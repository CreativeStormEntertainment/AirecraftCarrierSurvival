using UnityEngine;
using UnityEngine.UI;

public class EnemyInstancePopup : SandboxPopup
{
    [SerializeField]
    private Button allyAssistanceButton = null;
    [SerializeField]
    private int commandPointsForAlly = 20;

    private SOTacticMap tacticMap;
    private SandboxNode node;
    private MapSpawnData spawnData;
    private int buildingBlocks;
    private WorldMapFleet fleet;

    private void Awake()
    {
        allyAssistanceButton.onClick.AddListener(AllyAssistance);
    }

    public void Show(SandboxNode node, EMissionDifficulty difficulty, int buildingBlocks, WorldMapFleet fleet)
    {
        base.Show(null);
        var sandMan = SandboxManager.Instance;
        var worldMap = WorldMap.Instance;
        var spawner = worldMap.SandboxMapSpawner;
        spawnData = worldMap.NodeMaps.NodeDatas[sandMan.PoiManager.GetNodeIndex(node)].Maps.Find(map => map.Type == ESandboxObjectiveType.EnemyFleetInstance);
        spawner.Init(spawnData, difficulty);
        this.node = node;
        this.buildingBlocks = buildingBlocks;
        this.fleet = fleet;
        buttonB.interactable = StrikeGroupManager.Instance.AliveMembers.Count > 0;
        allyAssistanceButton.interactable = SaveManager.Instance.Data.IntermissionData.CommandPoints >= commandPointsForAlly;
    }

    protected override void OnClickA()
    {
        base.OnClickA();
        var sandMan = SandboxManager.Instance;
        ref var sandboxSave = ref SaveManager.Instance.Data.SandboxData;
        var spawner = WorldMap.Instance.SandboxMapSpawner;
        var name = fleet != null ? fleet.ShipName : "";
        tacticMap = spawner.SpawnEnemyFleet(spawnData, ref buildingBlocks, ref name);
        if (fleet != null)
        {
            fleet.BuildingBlocks = buildingBlocks;
            fleet.ShipName = name;
        }
        var rewards = sandMan.RewardsScriptable.RewardsList.Find(item => item.ObjectiveType == ESandboxObjectiveType.EnemyFleetInstance).GetRewardsData(EMissionDifficulty.Medium);
        sandboxSave.CurrentMissionSaveData = new SandboxCurrentMissionSaveData(sandMan.PoiManager.GetNodeIndex(node), rewards, ESandboxObjectiveType.EnemyFleetInstance, EPoiType.EnemyPatrolFleet);
        TimeManager.Instance.SaveData(ref sandboxSave.MissionStartTime);
        HudManager.Instance.HideWorldMap();
        sandMan.SetMission(tacticMap, node);
    }

    protected override void OnClickB()
    {
        base.OnClickB();
        StrikeGroupManager.Instance.DamageRandom(1);
        if (fleet != null)
        {
            fleet.Despawn();
        }
    }

    private void AllyAssistance()
    {
        SaveManager.Instance.Data.IntermissionData.CommandPoints += -commandPointsForAlly;
        allyAssistanceButton.interactable = false;
        Hide();
        if (fleet != null)
        {
            fleet.Despawn();
        }
    }
}

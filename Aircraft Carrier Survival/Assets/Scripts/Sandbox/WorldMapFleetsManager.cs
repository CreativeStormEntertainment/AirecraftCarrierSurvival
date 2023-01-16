using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapFleetsManager : MonoBehaviour
{
    public int DespawnRangeSqr
    {
        get;
        private set;
    }

    public int AttackRangeSqr
    {
        get;
        private set;
    }

    [SerializeField]
    private WorldMapFleet fleetPrefab = null;
    [SerializeField]
    private List<EWorldMapFleetType> fleetTypesBasketDefault = null;
    [SerializeField]
    private int minHoursToSpawnFleet = 15;
    [SerializeField]
    private int maxHoursToSpawnFleet = 24;
    [SerializeField]
    private int destinationPointPreferedDistance = 800;
    [SerializeField]
    private int smallSpawnRange = 20;
    [SerializeField]
    private int bigSpawnRange = 100;
    [SerializeField]
    private int despawnRange = 400;
    [SerializeField]
    private int attackRange = 200;
    [SerializeField]
    private int instanceFinishedHours = 48;
    [SerializeField]
    private int maxNeutralFleets = 5;
    [SerializeField]
    private int maxEnemyFleets = 5;

    private List<EWorldMapFleetType> fleetTypesBasket = new List<EWorldMapFleetType>();

    private int ticksToSpawnFleet;

    private List<WorldMapFleet> spawnedFleets = new List<WorldMapFleet>();
    private List<SandboxNode> tempList = new List<SandboxNode>();
    private int index;

    private int instanceFinishedTimer;

    private void Init()
    {
        DespawnRangeSqr = despawnRange * despawnRange;
        AttackRangeSqr = attackRange * attackRange;
        SandboxManager.Instance.MissionInstanceFinished += OnMissionInstanceFinished;
    }

    public void Setup()
    {
        Init();
        fleetTypesBasket.Clear();
        CheckFillTypesBasket();
        var data = SaveManager.Instance.Data.SandboxData.WorldMapFleetsSaveData;
        var savedFleets = data.Fleets;
        ticksToSpawnFleet = data.TicksToSpawnFleet;
        instanceFinishedTimer = data.InstanceFinishedTimer;
        if (savedFleets != null)
        {
            foreach (var fleet in savedFleets)
            {
                LoadFleet(fleet);
            }
        }
    }

    public void Save(ref WorldMapFleetsSaveData data)
    {
        foreach (var fleet in spawnedFleets)
        {
            if (fleet != null)
            {
                data.Fleets.Add(fleet.Save());
            }
        }
        data.TicksToSpawnFleet = ticksToSpawnFleet;
        data.InstanceFinishedTimer = instanceFinishedTimer;
    }

    private void LoadFleet(FleetSaveData saveData)
    {
        bool isEnemy = saveData.FleetType != EWorldMapFleetType.Neutral;
        if (!isEnemy)
        {
            if (spawnedFleets.FindAll(obj => obj.Type == EWorldMapFleetType.Neutral).Count >= maxNeutralFleets)
            {
                return;
            }
        }
        else
        {
            if (spawnedFleets.FindAll(obj => obj.Type == EWorldMapFleetType.EnemyAggressive || obj.Type == EWorldMapFleetType.EnemyFleeing).Count >= maxEnemyFleets)
            {
                return;
            }
        }
        var worldMap = WorldMap.Instance;
        var fleet = Instantiate(fleetPrefab, worldMap.FleetsParent);
        fleet.Despawned += OnFleetDespawned;
        spawnedFleets.Add(fleet);
        fleet.Load(saveData, worldMap.MarkersParent);
    }

    public void Tick()
    {
        if (SaveManager.Instance.Data.SandboxData.CurrentMissionSaveData.MapInstanceInProgress)
        {
            return;
        }
        if (instanceFinishedTimer > 0)
        {
            if ((instanceFinishedTimer -= TimeManager.Instance.WorldMapTickQuotient) <= 0)
            {
                InstanceTimerExpired();
            }
            else
            {
                return;
            }
        }
        if ((ticksToSpawnFleet -= TimeManager.Instance.WorldMapTickQuotient) <= 0)
        {
            SpawnFleet();
            ticksToSpawnFleet = Random.Range(minHoursToSpawnFleet, maxHoursToSpawnFleet) * TimeManager.Instance.TicksForHour;
        }
        for (index = 0; index < spawnedFleets.Count; index++)
        {
            spawnedFleets[index].Tick();
        }
    }

    public SandboxNode GetDestinationPointNode(Vector2 startPoint)
    {
        return GetRandomNodeInRange(startPoint, destinationPointPreferedDistance - 50, destinationPointPreferedDistance + 50);
    }

    public void SpawnFleet(bool forceEnemy = false)
    {
        var type = forceEnemy ? EWorldMapFleetType.EnemyAggressive : RandomUtils.GetRandom(fleetTypesBasket);
        bool isEnemy = type != EWorldMapFleetType.Neutral;
        if (!isEnemy)
        {
            if (spawnedFleets.FindAll(obj => obj.Type == EWorldMapFleetType.Neutral).Count >= maxNeutralFleets)
            {
                return;
            }
        }
        else
        {
            if (spawnedFleets.FindAll(obj => obj.Type == EWorldMapFleetType.EnemyAggressive || obj.Type == EWorldMapFleetType.EnemyFleeing).Count >= maxEnemyFleets)
            {
                return;
            }
        }
        fleetTypesBasket.Remove(type);
        CheckFillTypesBasket();
        var difficulty = EMissionDifficulty.Medium;
        if (type != EWorldMapFleetType.Neutral)
        {
            difficulty = (EMissionDifficulty)Random.Range(0, (int)EMissionDifficulty.VeryHard);
        }
        var worldMap = WorldMap.Instance;
        var mapShip = WorldMap.Instance.MapShip as WorldMapShip;
        var spawnPoint = GetRandomNodeInRange(mapShip.Position, mapShip.FieldOfView + smallSpawnRange, mapShip.FieldOfView + bigSpawnRange);
        var destinationPoint = GetDestinationPointNode(spawnPoint.Position);

        var fleet = Instantiate(fleetPrefab, worldMap.FleetsParent);
        fleet.Setup(difficulty, type, worldMap.MarkersParent, spawnPoint, destinationPoint);
        fleet.Despawned += OnFleetDespawned;
        spawnedFleets.Add(fleet);
    }

    public void DespawnClosestFleet()
    {
        var mapShip = WorldMap.Instance.MapShip;
        WorldMapFleet closestFleet = null;
        var minDist = float.MaxValue;
        foreach (var fleet in spawnedFleets)
        {
            float distance = (fleet.RectTransform.anchoredPosition - mapShip.Rect.anchoredPosition).sqrMagnitude;
            if (minDist > distance)
            {
                minDist = distance;
                closestFleet = fleet;
            }
        }
        if (closestFleet != null)
        {
            closestFleet.Despawn();
        }
    }

    private void InstanceTimerExpired()
    {
        foreach (var fleet in spawnedFleets)
        {
            fleet.SetNewDestinationPoint(GetDestinationPointNode(fleet.RectTransform.anchoredPosition).Position);
        }
    }

    private SandboxNode GetRandomNodeInRange(Vector2 startPosition, float smallRange, float bigRange)
    {
        tempList.Clear();

        float smallRangeSqr = smallRange;
        smallRangeSqr *= smallRangeSqr;
        float bigRangeSqr = bigRange;
        bigRangeSqr *= bigRangeSqr;
        foreach (var node in WorldMap.Instance.EnemyFleetsNodes)
        {
            float distance = (startPosition - node.Position).sqrMagnitude;
            if (distance < bigRangeSqr && distance > smallRangeSqr)
            {
                tempList.Add(node);
            }
        }
        return RandomUtils.GetRandom(tempList);
    }

    private void CheckFillTypesBasket()
    {
        if (fleetTypesBasket.Count == 0)
        {
            foreach (var type in fleetTypesBasketDefault)
            {
                fleetTypesBasket.Add(type);
            }
        }
    }

    private void OnFleetDespawned(WorldMapFleet fleet)
    {
        spawnedFleets.Remove(fleet);
        index--;
        fleet.Despawned -= OnFleetDespawned;
    }

    private void OnMissionInstanceFinished()
    {
        instanceFinishedTimer = instanceFinishedHours * TimeManager.Instance.TicksForHour;
    }
}

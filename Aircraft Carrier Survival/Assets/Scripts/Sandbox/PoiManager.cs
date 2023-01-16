using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PoiManager : MonoBehaviour
{
    public List<ESandboxObjectiveType> MainObjectivesBasket
    {
        get;
        private set;
    } = new List<ESandboxObjectiveType>();

    public List<ESandboxObjectiveType> OptionalObjectivesBasket
    {
        get;
        private set;
    } = new List<ESandboxObjectiveType>();

    public List<ESandboxObjectiveType> QuestObjectivesBasket
    {
        get;
        private set;
    } = new List<ESandboxObjectiveType>();

    public List<ESandboxObjectiveType> RemovedOptionalObjectives
    {
        get;
        private set;
    } = new List<ESandboxObjectiveType>();

    public List<ESandboxObjectiveType> RemovedMainObjectives
    {
        get;
        private set;
    } = new List<ESandboxObjectiveType>();

    public int PoiRadiousSqr
    {
        get;
        private set;
    }

    [SerializeField]
    private int hoursToPoisAmountTest = 6;
    [SerializeField]
    private PoiPool mainObjectivesPool = null;
    [SerializeField]
    private PoiPool optionalObjectivesPool = null;
    [SerializeField]
    private PoiPool enemyPatrolPool = null;
    [SerializeField]
    private PoiPool repairSpotPool = null;
    [SerializeField]
    private PoiPool questPool = null;
    [SerializeField]
    private PoiPool questMapPool = null;
    [SerializeField]
    private List<PoiSetterData> initialPoiSetterData = null;
    [SerializeField]
    private List<PoiTimeToObsolete> initialTimesToObsolete = null;
    [SerializeField]
    private List<PoiTimeToObsolete> gameplayTimesToObsolete = null;
    [SerializeField]
    private int optionalObjectiveInitialMinDistance = 300;
    [SerializeField]
    private int poiRadious = 125;
    [SerializeField]
    private List<int> poiSpawnRanges = null;
    [SerializeField]
    private List<int> maxPoiTypeCounts = null; ///EPoiType order
    [SerializeField]
    private int maxPoisSpawnAtOnce = 3;
    [SerializeField]
    private Vector2 questMapPoiSpawnRange = new Vector2(150f, 250f);
    [SerializeField]
    private PlannedOperationsSctiptable plannedOperations = null;
    [SerializeField]
    private int hoursToObsoleteMainGoalPoi = 168;
    [SerializeField]
    private bool debugBlockSpawningOptionalPOI = false;

    private Dictionary<EPoiType, List<SandboxPoi>> pois = new Dictionary<EPoiType, List<SandboxPoi>>();
    private Dictionary<EPoiType, PoiPool> poiPools = new Dictionary<EPoiType, PoiPool>();

    //private Dictionary<ESectorType, List<SandboxNode>> poiNodesBySector = new Dictionary<ESectorType, List<SandboxNode>>();

    //private Dictionary<ESectorType, HashSet<SandboxNode>> availablePoiNodesBySector = new Dictionary<ESectorType, HashSet<SandboxNode>>();

    private int ticksPassedToGameplaySetter;

    private MainGoalData mainGoal;
    private PoiSetterData currentData;

    private List<int> difficultyBasket = new List<int>();
    private List<EPoiType> poiBasket = new List<EPoiType>();

    private HashSet<int> usedHours = new HashSet<int>();
    private List<SandboxNode> filteredByPosition = new List<SandboxNode>();
    private List<SandboxNode> filteredByObjective = new List<SandboxNode>();
    private List<SandboxNode> filteredByGameplaySetter = new List<SandboxNode>();
    private List<SandboxNode> gameplaySetterTempList = new List<SandboxNode>();
    private List<SandboxNode> repairSpotGameplaySetterTempList = new List<SandboxNode>();

    private List<SandboxNode> availablePoiNodes = new List<SandboxNode>();
    private List<SandboxNode> availableRepairSpotNodes = new List<SandboxNode>();

    private List<SandboxNode> allSandboxNodes = new List<SandboxNode>();
    private List<SandboxNode> fleetsSandboxNodes = new List<SandboxNode>();
    private List<SandboxNode> basesSandboxNodes = new List<SandboxNode>();
    private List<SandboxNode> allPoiNodes = new List<SandboxNode>();
    private List<SandboxNode> poiNodes = new List<SandboxNode>();
    private List<SandboxNode> repairSpotPoiNodes = new List<SandboxNode>();
    private PearlHarbourPoi pearlHarbour;

    private SandboxObjectiveTypes sandboxObjectiveTypes;

    private int spawnRange;
    private int ticksPassedToRedWatersSetter;
    private int ticksToSpawnPOMainPoi;

    public void Save(ref SandboxSaveData data)
    {
        data.DifficultyBasket = new List<int>(difficultyBasket);
        data.MainObjectivesBasket = new List<ESandboxObjectiveType>(MainObjectivesBasket);
        data.OptionalObjectivesBasket = new List<ESandboxObjectiveType>(OptionalObjectivesBasket);
        data.QuestObjectivesBasket = new List<ESandboxObjectiveType>(QuestObjectivesBasket);
        data.SpawnedPois.Clear();
        foreach (var list in pois.Values)
        {
            foreach (var poi in list)
            {
                data.SpawnedPois.Add(poi.Data);
            }
        }
        data.TicksPassedToGameplaySetter = ticksPassedToGameplaySetter;
        data.TicksPassedToRedWatersSetter = ticksPassedToRedWatersSetter;
        data.TicksToSpawnPOMainPoi = ticksToSpawnPOMainPoi;
    }

    public void Init()
    {
        sandboxObjectiveTypes = new SandboxObjectiveTypes();
        sandboxObjectiveTypes.Init();
        PoiRadiousSqr = poiRadious * poiRadious;
        mainGoal = SandboxManager.Instance.SandboxGoalsManager.MainGoal;
        currentData = initialPoiSetterData[(int)mainGoal.MissionLength];
        var uiMan = UIManager.Instance;
        mainObjectivesPool.Init(uiMan.SandboxPoiParent);
        optionalObjectivesPool.Init(uiMan.SandboxPoiParent);
        repairSpotPool.Init(uiMan.SandboxPoiParent);
        questPool.Init(uiMan.SandboxPoiParent);
        questMapPool.Init(uiMan.SandboxPoiParent);
        enemyPatrolPool.Init(uiMan.SandboxPoiParent);
        var worldMap = WorldMap.Instance;
        poiNodes = worldMap.PoiNodes;
        allPoiNodes = new List<SandboxNode>(poiNodes);
        fleetsSandboxNodes = worldMap.EnemyFleetsNodes;
        basesSandboxNodes = worldMap.EnemyBasesNodes;
        repairSpotPoiNodes = worldMap.RepairSpotNodes;
        allPoiNodes.AddRange(repairSpotPoiNodes);

        for (int i = 0; i < (int)EPoiType.Count; i++)
        {
            List<SandboxPoi> list = new List<SandboxPoi>();
            pois.Add((EPoiType)i, list);
        }

        poiPools.Add(EPoiType.MainObjective, mainObjectivesPool);
        poiPools.Add(EPoiType.OptionalObjective, optionalObjectivesPool);
        poiPools.Add(EPoiType.EnemyPatrolFleet, enemyPatrolPool);
        poiPools.Add(EPoiType.RepairSpot, repairSpotPool);
        poiPools.Add(EPoiType.Quest, questPool);
        poiPools.Add(EPoiType.QuestMap, questMapPool);

        foreach (var data in initialTimesToObsolete)
        {
            data.MinDistance *= data.MinDistance;
        }
        pearlHarbour = worldMap.PearlHarbour;
        spawnRange = poiSpawnRanges[(int)SaveManager.Instance.Data.SelectedAircraftCarrier];
        RefillPoiBasket();
        FillAvailableNodesLists();
    }

    public void Setup()
    {
        Init();

        var saveData = SaveManager.Instance.Data;
        if (saveData.SandboxData.IsSaved)
        {
            ticksPassedToGameplaySetter = saveData.SandboxData.TicksPassedToGameplaySetter;
            ticksPassedToRedWatersSetter = saveData.SandboxData.TicksPassedToRedWatersSetter;
            ticksToSpawnPOMainPoi = saveData.SandboxData.TicksToSpawnPOMainPoi;
            difficultyBasket = new List<int>(saveData.SandboxData.DifficultyBasket);
            MainObjectivesBasket = new List<ESandboxObjectiveType>(saveData.SandboxData.MainObjectivesBasket);
            OptionalObjectivesBasket = new List<ESandboxObjectiveType>(saveData.SandboxData.OptionalObjectivesBasket);
            QuestObjectivesBasket = new List<ESandboxObjectiveType>(saveData.SandboxData.QuestObjectivesBasket);
            foreach (var poi in saveData.SandboxData.SpawnedPois)
            {
                LoadPoi(poi);
            }
        }
        else
        {
            RefillDifficultyBasket();
            RefillSandBoxObjectiveBasket(ESandboxObjectiveBasket.Main);
            RefillSandBoxObjectiveBasket(ESandboxObjectiveBasket.Optional);
            RefillSandBoxObjectiveBasket(ESandboxObjectiveBasket.Quest);
            var worldMapRectTransform = WorldMap.Instance.RectTransform;
            int nodeIndex;
            if (mainGoal.Type != EMainGoalType.PlannedOperations)
            {
                nodeIndex = GetClosestNodeIndex(FilterNodesListByObjective(availablePoiNodes, filteredByObjective, mainGoal.ObjectiveType), new Vector2(mainGoal.PinPositionProportion.X * worldMapRectTransform.sizeDelta.x,
                    mainGoal.PinPositionProportion.Y * worldMapRectTransform.sizeDelta.y));
                SpawnPoi(new SandboxPoiData(EPoiType.MainObjective, nodeIndex, EMissionDifficulty.Medium, hoursToObsoleteMainGoalPoi, mainGoal.ObjectiveType));
            }
            else
            {
                if (pois[EPoiType.MainObjective].Count == 0)
                {
                    var objectives = plannedOperations.PlannedOperationsMissions[mainGoal.PlannedOperationsMapsIndex].SandboxObjectiveTypes;
                    nodeIndex = GetClosestNodeIndex(FilterNodesListByObjective(availablePoiNodes, filteredByObjective, objectives[0]),
                        new Vector2(mainGoal.PinPositionProportion.X * worldMapRectTransform.sizeDelta.x, mainGoal.PinPositionProportion.Y * worldMapRectTransform.sizeDelta.y));
                    mainGoal.SpawnedPOMainPoiIndex = 0;
                    var missionObjective = objectives[mainGoal.SpawnedPOMainPoiIndex];
                    SpawnPoi(new SandboxPoiData(EPoiType.MainObjective, nodeIndex, GetDifficulty(), GetPoiTimeToObsolete(nodeIndex, initialTimesToObsolete, true), missionObjective));
                    var date = TimeManager.Instance.GetDateAfterDays(2);
                    (pois[EPoiType.MainObjective][pois[EPoiType.MainObjective].Count - 1] as MainObjectivePOI).SetPlannedOperationsDateToActivate(new DayTime(date.Year, date.Month, date.Day, 0, 0));
                    while (mainGoal.SpawnedPOMainPoiIndex < mainGoal.PointsToComplete - 1)
                    {
                        SpawnNextPlannedOperationsPoi();
                    }
                }
            }
            if (!debugBlockSpawningOptionalPOI)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (FilterNodesListByPosition(availablePoiNodes, filteredByPosition, pois[EPoiType.MainObjective][0].RectTransform.anchoredPosition, optionalObjectiveInitialMinDistance).Count > 0)
                    {
                        nodeIndex = GetRandomPoiNodeIndex(filteredByPosition);
                        SpawnPoi(new SandboxPoiData(EPoiType.OptionalObjective, nodeIndex, GetDifficulty(), GetPoiTimeToObsolete(nodeIndex, initialTimesToObsolete, true)));
                    }
                }
            }
            //nodeIndex = GetRandomPoiNodeIndex(FilterNodesListByPosition(availablePoiNodes, filteredByPosition, pearlHarbour.RectTransform.anchoredPosition, questAndRepairInitialMinDistance));
            //SpawnPoi(new SandboxPoiData(EPoiType.Quest, nodeIndex, EMissionDifficulty.Medium, GetPoiTimeToObsolete(nodeIndex, initialTimesToObsolete, true)));

            var rand = Random.value;
            if (rand <= currentData.ChanceToSpawnRepairPoint)
            {
                if (FilterNodesListByPosition(availableRepairSpotNodes, filteredByPosition, pois[EPoiType.MainObjective][0].RectTransform.anchoredPosition, optionalObjectiveInitialMinDistance * 2f).Count > 0)
                {
                    nodeIndex = GetRandomPoiNodeIndex(filteredByPosition);
                    SpawnPoi(new SandboxPoiData(EPoiType.RepairSpot, nodeIndex, EMissionDifficulty.Medium, GetPoiTimeToObsolete(nodeIndex, initialTimesToObsolete, true)));
                }
            }
        }
    }

    public void RemovePoi(SandboxPoi poi)
    {
        pois[poi.Data.PoiType].Remove(poi);
        allPoiNodes[poi.Data.NodeIndex].Occupied = false;
        if (poi.Data.PoiType != EPoiType.MainObjective)
        {
            poiBasket.Add(poi.Data.PoiType);
        }
        poi.RemovePoi();
    }

    public void Tick()
    {
        if (mainGoal.DaysToFinish > 0 && (ticksPassedToGameplaySetter += TimeManager.Instance.WorldMapTickQuotient) > hoursToPoisAmountTest * TimeManager.Instance.TicksForHour)
        {
            ticksPassedToGameplaySetter = 0;
            GameplaySetter();
        }
        foreach (var type in pois.Values)
        {
            for (int i = 0; i < type.Count; i++)
            {
                if (type[i].Tick())
                {
                    i--;
                }
            }
        }
    }

    public void SpawnNextPlannedOperationsPoi()
    {
        if (mainGoal.PointsToComplete > mainGoal.SpawnedPOMainPoiIndex + 1)
        {
            var objectives = plannedOperations.PlannedOperationsMissions[mainGoal.PlannedOperationsMapsIndex].SandboxObjectiveTypes;
            mainGoal.SpawnedPOMainPoiIndex++;
            var missionObjective = objectives[mainGoal.SpawnedPOMainPoiIndex];
            FilterNodesListByObjective(availablePoiNodes, filteredByObjective, missionObjective);
            var node = GetRandomPoiNodeIndex(filteredByObjective);
            //var node = GetClosestObjectiveNode(WorldMap.Instance.MapShip.Position, missionObjective);
            var data = new SandboxPoiData(EPoiType.MainObjective, node, GetDifficulty(), 0, missionObjective);
            SpawnPoi(data);
            var date = TimeManager.Instance.GetDateAfterDays(7 * mainGoal.SpawnedPOMainPoiIndex);
            (pois[EPoiType.MainObjective][pois[EPoiType.MainObjective].Count - 1] as MainObjectivePOI).SetPlannedOperationsDateToActivate(new DayTime(date.Year, date.Month, date.Day, 0, 0));
        }
    }

    public void SetPoisInteractable(bool interactable)
    {
        foreach (var poiList in pois.Values)
        {
            foreach (var poi in poiList)
            {
                poi.SetInteractable(interactable);
            }
        }
        pearlHarbour.SetInteractable(interactable);
    }

    public SandboxNode GetNode(int index)
    {
        return allPoiNodes[index];
    }

    public int GetNodeIndex(SandboxNode node)
    {
        return allPoiNodes.IndexOf(node);
    }

    public List<ESandboxObjectiveType> GetSandboxObjectiveBasket(EPoiType poiType)
    {
        List<ESandboxObjectiveType> basket = null;
        switch (poiType)
        {
            case EPoiType.OptionalObjective:
                basket = OptionalObjectivesBasket;
                break;
            case EPoiType.QuestMap:
                basket = QuestObjectivesBasket;
                break;
            case EPoiType.MainObjective:
                basket = MainObjectivesBasket;
                break;
        }
        return basket;
    }

    public ESandboxObjectiveType GetSandboxObjectiveType(ESandboxObjectiveBasket basketType)
    {
        var basket = MainObjectivesBasket;
        switch (basketType)
        {
            case ESandboxObjectiveBasket.Main:
                basket = MainObjectivesBasket;
                break;
            case ESandboxObjectiveBasket.Optional:
                basket = OptionalObjectivesBasket;
                break;
            case ESandboxObjectiveBasket.Quest:
                basket = QuestObjectivesBasket;
                break;
        }
        var type = RandomUtils.GetRandom(basket);
        basket.Remove(type);
        switch (basketType)
        {
            case ESandboxObjectiveBasket.Main:
                RemovedMainObjectives.Add(type);
                if (RemovedMainObjectives.Count == 3)
                {
                    basket.Add(RemovedMainObjectives[0]);
                    RemovedMainObjectives.RemoveAt(0);
                }
                break;
            case ESandboxObjectiveBasket.Optional:
                RemovedOptionalObjectives.Add(type);
                if (RemovedOptionalObjectives.Count == 4)
                {
                    basket.Add(RemovedOptionalObjectives[0]);
                    RemovedOptionalObjectives.RemoveAt(0);
                }
                break;
            case ESandboxObjectiveBasket.Quest:
                break;
        }
        if (basket.Count == 0)
        {
            RefillSandBoxObjectiveBasket(basketType);
        }
        return type;
    }

    public void SpawnQuestMapPoi()
    {
        FilterHashSetByGameplaySetter(availablePoiNodes, gameplaySetterTempList, questMapPoiSpawnRange.x, questMapPoiSpawnRange.y);
        int nodeIndex = GetRandomPoiNodeIndex(gameplaySetterTempList);
        if (nodeIndex == -1)
        {
            GetClosestNodeIndex(availablePoiNodes, WorldMap.Instance.MapShip.Rect.anchoredPosition);
        }
        var data = new SandboxPoiData(EPoiType.QuestMap, nodeIndex, EMissionDifficulty.Hard, GetPoiTimeToObsolete(nodeIndex, initialTimesToObsolete, false));
        SpawnPoi(data);
    }

    public void UpdateLists(bool add, List<SandboxNode> nodes)
    {
        if (add)
        {
            foreach (var node in nodes)
            {
                if (poiNodes.Contains(node) && IsNodeAvailable(node))
                {
                    availablePoiNodes.Add(node);
                }
                if (repairSpotPoiNodes.Contains(node) && IsNodeAvailable(node))
                {
                    availableRepairSpotNodes.Add(node);
                }
            }
        }
        else
        {
            foreach (var node in nodes)
            {
                availablePoiNodes.Remove(node);
                availableRepairSpotNodes.Remove(node);
            }
        }
    }

    public SandboxNode GetClosestObjectiveNode(Vector2 position, ESandboxObjectiveType objectiveType)
    {
        float dist = float.MaxValue;
        SandboxNode closestNode = null;
        foreach (var node in poiNodes)
        {
            var newDist = (position - node.Position).sqrMagnitude;
            if (newDist < dist && WorldMap.Instance.NodeMaps.NodeDatas[GetNodeIndex(node)].Maps.Find(map => map.Type == objectiveType) != null)
            {
                closestNode = node;
                dist = newDist;
            }
        }
        if (closestNode == null)
        {
            Debug.LogError("There is no nodes with objectiveType = " + objectiveType.ToString());
        }
        return closestNode;
    }

    public void DebugSpawnPoi(ESandboxObjectiveType objectiveType, SandboxNode node, EMissionDifficulty difficulty)
    {
        if (objectiveType == ESandboxObjectiveType.EnemyFleetInstance)
        {
            SandboxManager.Instance.WorldMapFleetsManager.SpawnFleet();
            return;
        }
        int nodeIndex;
        if (node != null && !node.Occupied)
        {
            nodeIndex = allPoiNodes.IndexOf(node);
        }
        else
        {
            nodeIndex = allPoiNodes.IndexOf(GetClosestObjectiveNode(WorldMap.Instance.MapShip.Position, objectiveType));
        }
        Debug.LogError(nodeIndex);
        SpawnPoi(new SandboxPoiData(EPoiType.OptionalObjective, nodeIndex, difficulty, GetPoiTimeToObsolete(nodeIndex, initialTimesToObsolete, true), objectiveType), true);
    }

    private void RefillSandBoxObjectiveBasket(ESandboxObjectiveBasket basketType)
    {
        switch (basketType)
        {
            case ESandboxObjectiveBasket.Main:
                if (mainGoal.Type != EMainGoalType.PlannedOperations)
                {
                    Assert.IsTrue(MainObjectivesBasket.Count == 0);
                    foreach (var obj in sandboxObjectiveTypes.MainGoalObjectivesDictionary[mainGoal.Type])
                    {
                        MainObjectivesBasket.Add(obj);
                    }
                }
                break;
            case ESandboxObjectiveBasket.Optional:
                Assert.IsTrue(OptionalObjectivesBasket.Count == 0);
                foreach (var obj in SandboxObjectiveTypes.OptionalObjectiveTypes)
                {
                    OptionalObjectivesBasket.Add(obj);
                }
                break;
            case ESandboxObjectiveBasket.Quest:
                Assert.IsTrue(QuestObjectivesBasket.Count == 0);
                foreach (var obj in SandboxObjectiveTypes.QuestObjectiveTypes)
                {
                    QuestObjectivesBasket.Add(obj);
                }
                break;
        }
    }

    private void SpawnPoi(SandboxPoiData data, bool debug = false)
    {
        if (debugBlockSpawningOptionalPOI && data.PoiType == EPoiType.OptionalObjective && !debug)
        {
            return;
        }
        Assert.IsTrue(data.PoiType == EPoiType.EnemyPatrolFleet || data.PoiType == EPoiType.QuestMap || poiBasket.Contains(data.PoiType) || data.PoiType == EPoiType.MainObjective || debug);
        var newPoi = poiPools[data.PoiType].Get();

        var node = allPoiNodes[data.NodeIndex];


        newPoi.Setup(data);
        //newPoi.Setup(data, RandomUtils.GetRandom(node.Maps));


        newPoi.RectTransform.anchoredPosition = node.Position;

        pois[data.PoiType].Add(newPoi);
        poiBasket.Remove(data.PoiType);
    }

    private void LoadPoi(SandboxPoiData data)
    {
        //Assert.IsTrue(data.PoiType == EPoiType.QuestMap || poiBasket.Contains(data.PoiType) || (data.PoiType == EPoiType.MainObjective));
        var newPoi = poiPools[data.PoiType].Get();
        newPoi.Load(data);
        newPoi.RectTransform.anchoredPosition = allPoiNodes[data.NodeIndex].Position;
        pois[data.PoiType].Add(newPoi);
        poiBasket.Remove(data.PoiType);
    }

    private void GameplaySetter()
    {
        usedHours.Clear();
        FilterHashSetByGameplaySetter(availablePoiNodes, gameplaySetterTempList, spawnRange - poiRadious, spawnRange + poiRadious);
        FilterHashSetByGameplaySetter(availableRepairSpotNodes, repairSpotGameplaySetterTempList, spawnRange - poiRadious, spawnRange + poiRadious);
        for (int i = 0; i < maxPoisSpawnAtOnce; i++)
        {
            if (poiBasket.Count == 0)
            {
                return;
            }
            EPoiType poiType = RandomUtils.GetRandom(poiBasket);
            if (debugBlockSpawningOptionalPOI && poiType == EPoiType.OptionalObjective)
            {
                return;
            }
            int nodeIndex = poiType == EPoiType.RepairSpot ? GetRandomPoiNodeIndex(repairSpotGameplaySetterTempList) : GetRandomPoiNodeIndex(gameplaySetterTempList);
            if (poiType == EPoiType.OptionalObjective && nodeIndex >= poiNodes.Count)
            {
                Debug.Log("wtf? | " + nodeIndex);
            }
            if (nodeIndex == -1)
            {
                break;
            }
            int timeToObsolete = GetPoiTimeToObsolete(nodeIndex, gameplayTimesToObsolete, false);
            EMissionDifficulty difficulty = EMissionDifficulty.Medium;
            if (poiType == EPoiType.Quest || poiType == EPoiType.RepairSpot)
            {
                timeToObsolete += 24;
            }
            else
            {
                difficulty = GetDifficulty();
            }
            var data = new SandboxPoiData(poiType, nodeIndex, difficulty, timeToObsolete);
            SpawnPoi(data);
        }
    }

    private EMissionDifficulty GetDifficulty()
    {
        if (difficultyBasket.Count == 0)
        {
            RefillDifficultyBasket();
        }
        else
        {
            var difficulty = RandomUtils.GetRandom(difficultyBasket);
            difficultyBasket.Remove(difficulty);
            return (EMissionDifficulty)difficulty;
        }
        return EMissionDifficulty.Easy;
    }

    private List<SandboxNode> FilterHashSetByGameplaySetter(List<SandboxNode> inputList, List<SandboxNode> outputList, float smallRange, float bigRange)
    {
        var ship = WorldMap.Instance.MapShip;
        outputList.Clear();

        float smallRangeSqr = smallRange;
        smallRangeSqr *= smallRangeSqr;
        float bigRangeSqr = bigRange;
        bigRangeSqr *= bigRangeSqr;
        foreach (var node in inputList)
        {
            float distance = (ship.Rect.anchoredPosition - node.Position).sqrMagnitude;
            if (distance < bigRangeSqr && distance > smallRangeSqr)
            {
                outputList.Add(node);
            }
        }
        return outputList;
    }

    private List<SandboxNode> FilterNodesListByPosition(List<SandboxNode> inputList, List<SandboxNode> outputList, Vector2 position, float range)
    {
        outputList.Clear();
        var rangeSqr = range * range;
        foreach (var node in inputList)
        {
            if (Vector2.SqrMagnitude(node.Position - position) <= rangeSqr)
            {
                outputList.Add(node);
            }
        }
        return outputList;
    }

    private List<SandboxNode> FilterNodesListByObjective(List<SandboxNode> inputList, List<SandboxNode> outputList, ESandboxObjectiveType objectiveType)
    {
        outputList.Clear();
        foreach (var node in inputList)
        {
            if (WorldMap.Instance.NodeMaps.NodeDatas[poiNodes.IndexOf(node)].Maps.Find(map => map.Type == objectiveType) != null)
            {
                outputList.Add(node);
            }
        }
        return outputList;
    }

    private int GetRandomPoiNodeIndex(ICollection<SandboxNode> nodeList)
    {
        if (nodeList.Count == 0)
        {
            //Debug.LogError("Empty node list");
            return -1;
        }
        var chosenNode = RandomUtils.GetRandom(nodeList);
        chosenNode.Occupied = true;

        return allPoiNodes.IndexOf(chosenNode);
    }

    private int GetClosestNodeIndex(ICollection<SandboxNode> nodeList, Vector2 position)
    {
        SandboxNode closestNode = null;
        var minDist = float.MaxValue;
        foreach (var node in nodeList)
        {
            float distance = (position - node.Position).sqrMagnitude;
            if (minDist > distance)
            {
                minDist = distance;
                closestNode = node;
            }
        }
        Assert.IsNotNull(closestNode);
        closestNode.Occupied = true;
        return allPoiNodes.IndexOf(closestNode);
    }

    private void FillAvailableNodesLists()
    {
        FillAvailableNodesList(availablePoiNodes, poiNodes);
        FillAvailableNodesList(availableRepairSpotNodes, repairSpotPoiNodes);
    }

    private void FillAvailableNodesList(List<SandboxNode> list, List<SandboxNode> sourceList)
    {
        list.Clear();
        foreach (var node in sourceList)
        {
            if (IsNodeAvailable(node))
            {
                list.Add(node);
            }
        }
    }

    private bool IsNodeAvailable(SandboxNode node)
    {
        return !node.Occupied && !node.BlockedByDistance;
    }

    private int GetPoiTimeToObsolete(int nodeIndex, List<PoiTimeToObsolete> list, bool distanceFromPearlHarbour)
    {
        Vector2 position = distanceFromPearlHarbour ? pearlHarbour.RectTransform.anchoredPosition : WorldMap.Instance.MapShip.Rect.anchoredPosition;
        float sqrMagnitude = (allPoiNodes[nodeIndex].Position - position).sqrMagnitude;
        for (int i = 0; i < list.Count; i++)
        {
            if (sqrMagnitude < list[i].MinDistance || i == list.Count - 1)
            {
                int time = Random.Range(list[i].MinDays, list[i].MaxDays);
                time *= 24;
                while (usedHours.Contains(time))
                {
                    int fixTime = Random.Range(-4, 4);
                    fixTime = fixTime == 0 ? 4 : fixTime;
                    time += fixTime;
                }
                usedHours.Add(time);
                return time;
            }
        }
        return 0;
    }

    private void RefillDifficultyBasket()
    {
        for (int i = 0; i < currentData.DifficultyMinimumPois.Count; i++)
        {
            for (int j = 0; j < currentData.DifficultyMinimumPois[i]; j++)
            {
                difficultyBasket.Add(i);
            }
        }
    }

    private void RefillPoiBasket()
    {
        poiBasket.Clear();
        for (int i = 0; i < maxPoiTypeCounts.Count; i++)
        {
            var type = (EPoiType)i;
            if (type != EPoiType.MainObjective && type != EPoiType.EnemyPatrolFleet)
            {
                for (int j = 0; j < maxPoiTypeCounts[i]; j++)
                {
                    poiBasket.Add(type);
                }
            }
        }
    }
}

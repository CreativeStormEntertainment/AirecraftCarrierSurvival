using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

public class WorldMapFleet : MonoBehaviour
{
    public event Action<WorldMapFleet> Despawned = delegate { };

    public RectTransform RectTransform => rectTransform;

    public bool Visible
    {
        get;
        set;
    }

    public int BuildingBlocks
    {
        get;
        set;
    } = -1;

    public string ShipName
    {
        get;
        set;
    }

    public EWorldMapFleetType Type
    {
        get;
        private set;
    }

    [SerializeField]
    private WorldMapMarker markerPrefab = null;
    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private float neutralFleetSpeedMultiplier = 0.5f;
    [SerializeField]
    private float firstEnemyFleeingFleetSpeedMultiplier = 0.75f;
    [SerializeField]
    private float secondEnemyFleeingFleetSpeedMultiplier = 0.5f;
    [SerializeField]
    private List<int> enemyAggressiveFleetSpeeds = null;

    private WorldMapMarker marker;
    private Vector2 destination;

    private int ticksToChangeSpeed;
    private float fleetSpeed;
    private float lastFleetSpeed;
    private bool isChasing;
    private bool isRetreating;
    private List<PathNode> openList = new List<PathNode>();
    private HashSet<PathNode> closeList = new HashSet<PathNode>();
    private Vector2 pos = Vector2.zero;
    private List<PathNode> path = new List<PathNode>();
    private int pathIndex;
    private WorldMapFleetsManager fleetManager;
    private EMissionDifficulty difficulty;

    private float startTime;
    private float destinationReachTime;
    private Vector2 startPosition;

    private float visualTimer;
    protected Vector2 lastTickPos;
    protected Vector2 currentTickPos;

    public void Update()
    {
        visualTimer += Time.deltaTime;
        float percent = visualTimer / TimeManager.Instance.TickTime;
        rectTransform.anchoredPosition = Vector2.Lerp(lastTickPos, currentTickPos, percent);
        if (percent >= 1)
        {
            visualTimer = 0f;
        }
    }

    public void Setup(EMissionDifficulty difficulty, EWorldMapFleetType fleetType, Transform markerParent, SandboxNode spawnPoint, SandboxNode destinationPoint)
    {
        this.difficulty = difficulty;
        Type = fleetType;
        fleetManager = SandboxManager.Instance.WorldMapFleetsManager;
        if (Type == EWorldMapFleetType.EnemyFleeing)
        {
            ticksToChangeSpeed = 48 * TimeManager.Instance.TicksForHour;
        }
        destination = destinationPoint.Position;
        rectTransform.anchoredPosition = spawnPoint.Position;
        lastTickPos = rectTransform.anchoredPosition;
        currentTickPos = rectTransform.anchoredPosition;
        marker = Instantiate(markerPrefab, markerParent);
        marker.Setup(fleetType);
        marker.UpdatePositionAndRotation(rectTransform.anchoredPosition, Quaternion.identity);
        SetSpeed();
    }

    public void Load(FleetSaveData saveData, Transform markerParent)
    {
        fleetManager = SandboxManager.Instance.WorldMapFleetsManager;
        difficulty = saveData.Difficulty;
        Type = saveData.FleetType;
        destination = saveData.Destination;
        rectTransform.anchoredPosition = saveData.Position;
        lastTickPos = rectTransform.anchoredPosition;
        currentTickPos = rectTransform.anchoredPosition;
        ticksToChangeSpeed = saveData.TicksToChangeSpeed;
        BuildingBlocks = saveData.BuildingBlocks;
        ShipName = saveData.ShipName;
        marker = Instantiate(markerPrefab, markerParent);
        marker.Setup(Type);
        marker.UpdatePositionAndRotation(rectTransform.anchoredPosition, Quaternion.identity);
        Tick();
    }

    public FleetSaveData Save()
    {
        var save = new FleetSaveData
        {
            Position = rectTransform.anchoredPosition,
            Destination = destination,
            FleetType = Type,
            Difficulty = difficulty
        };
        return save;
    }

    public void SetNewDestinationPoint(Vector2 destinationPoint)
    {
        destination = destinationPoint;
    }

    public void Tick()
    {
        var playerShip = WorldMap.Instance.MapShip as WorldMapShip;
        bool oldVisible = Visible;
        Visible = Vector2.SqrMagnitude(playerShip.Position - rectTransform.anchoredPosition) < playerShip.FieldOfViewSqr;
        var dist = (rectTransform.anchoredPosition - WorldMap.Instance.MapShip.Position).sqrMagnitude;
        if (dist > fleetManager.DespawnRangeSqr && !Visible)
        {
            Despawn();
            return;
        }
        isChasing = Type == EWorldMapFleetType.EnemyAggressive && Visible;
        isRetreating = Type == EWorldMapFleetType.EnemyFleeing && Visible;
        if (oldVisible != Visible || Visible || pathIndex >= path.Count || pathIndex < 0)
        {
            UpdatePath();
        }
        if (Type == EWorldMapFleetType.EnemyFleeing)
        {
            if ((ticksToChangeSpeed -= TimeManager.Instance.WorldMapTickQuotient) <= 0)
            {
                SetSpeed();
            }
        }
        pos = GetNewShipPos();
        pos = Vector2.MoveTowards(pos, path[pathIndex].Position, fleetSpeed);
        if ((pos - path[pathIndex].Position).sqrMagnitude < 0.1f)
        {
            pathIndex++;
            SetTimeToReachDestination();
        }

        if (pathIndex == path.Count && !isChasing && !isRetreating)
        {
            if (Visible)
            {
                destination = fleetManager.GetDestinationPointNode(rectTransform.position).Position;
                UpdatePath();
            }
            else
            {
                Despawn();
                return;
            }
        }
        lastTickPos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = pos;
        currentTickPos = rectTransform.anchoredPosition;
        LookAt();
        if (Visible)
        {
            UpdateMarkerPositionAndRotation();
            marker.SetMarkerSprite(Type, false);
            if (dist < fleetManager.AttackRangeSqr && Type != EWorldMapFleetType.Neutral)
            {
                AttackPlayer();
            }
        }
        else if (Vector2.SqrMagnitude(playerShip.Position - marker.RectTransform.anchoredPosition) < playerShip.FieldOfViewSqr)
        {
            marker.gameObject.SetActive(false);
        }
    }

    public void UpdateMarkerPositionAndRotation()
    {
        marker.UpdatePositionAndRotation(rectTransform.anchoredPosition, rectTransform.rotation);
        gameObject.SetActive(true);
    }

    public void UpdatePath()
    {
        path.Clear();
        pathIndex = 1;
        path = FindPath();
        SetTimeToReachDestination();
        if (path.Count == 0)
        {
            Debug.LogError("Empty path");
        }
    }

    public void Despawn()
    {
        Despawned(this);
        Destroy(marker.gameObject);
        Destroy(gameObject);
    }

    private Vector2 GetNewShipPos()
    {
        if (path.Count <= pathIndex)
        {
            return rectTransform.position;
        }
        if (fleetSpeed != lastFleetSpeed)
        {
            SetTimeToReachDestination();
        }
        lastFleetSpeed = fleetSpeed;
        float percent = (Time.time - startTime) / destinationReachTime;
        return Vector2.Lerp(startPosition, path[pathIndex].Position, percent);
    }

    private void SetTimeToReachDestination()
    {
        if (path.Count > pathIndex)
        {
            startPosition = rectTransform.anchoredPosition;
            startTime = Time.time;
            destinationReachTime = (path[pathIndex].Position - rectTransform.anchoredPosition).magnitude / fleetSpeed;
        }
    }

    private void AttackPlayer()
    {
        var sandMan = SandboxManager.Instance;
        sandMan.ShowEnemyInstancePopup(sandMan.PoiManager.GetClosestObjectiveNode(rectTransform.anchoredPosition, ESandboxObjectiveType.EnemyFleetInstance), difficulty, BuildingBlocks, this);
    }

    private void SetSpeed()
    {
        var ship = WorldMap.Instance.MapShip;
        switch (Type)
        {
            case EWorldMapFleetType.Neutral:
                fleetSpeed = ship.ShipSpeedScaled * neutralFleetSpeedMultiplier;
                break;
            case EWorldMapFleetType.EnemyFleeing:
                if (ticksToChangeSpeed > 0)
                {
                    fleetSpeed = ship.ShipSpeedScaled * firstEnemyFleeingFleetSpeedMultiplier;
                }
                else
                {
                    fleetSpeed = ship.ShipSpeedScaled * secondEnemyFleeingFleetSpeedMultiplier;
                }
                break;
            case EWorldMapFleetType.EnemyAggressive:
                fleetSpeed = enemyAggressiveFleetSpeeds[(int)SaveManager.Instance.Data.SelectedAircraftCarrier];
                break;
        }
    }

    private void LookAt()
    {
        if (pathIndex < path.Count)
        {
            LookAt(path[Visible ? pathIndex : path.Count - 1].Position);
        }
    }

    private void LookAt(Vector2 dest)
    {
        rectTransform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, dest - pos));
    }

    private List<PathNode> FindPath()
    {
        //PrintTime("FindPath", true);

        PathNode startNode = WorldMap.Instance.MapNodes.Find(pos);

        //chaseMode = enemyAttacksManager.IsDetected && canChase;
        PathNode endNode = FindEndNode();
        int attempts = 0;
        while (endNode.IsOnLand)
        {
            endNode = FindEndNode();
            attempts++;

            if (attempts == 1000)
            {
                var error = "Cannot find end node - ";
                Debug.LogError(error);
                break;
            }
        }

        //#QoL, hashSet is better suited if you want to use mainly Contains, 
        //dont use new if you don't have to, why unnecessary garbage?
        openList.Clear();
        openList.Add(startNode);
        closeList.Clear();

        //#QoL
        foreach (var pathNode in WorldMap.Instance.MapNodes.GetNodes())
        {
            pathNode.GCost = int.MaxValue;
            pathNode.CalculateFCost();
            pathNode.CameFromNode = null;
        }

        startNode.GCost = 0;
        startNode.HCost = TacticalMapGrid.CalculateRemainingDistance(startNode.MapSNode, endNode.MapSNode);
        startNode.CalculateFCost();

        bool checkLand = !endNode.IsOnLand;

        while (openList.Count > 0)
        {
            PathNode currentNode = TacticalMapGrid.GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                //PrintTime("FindPath", false);
                return TacticalMapGrid.CalculatePathHelper(endNode);
            }

            openList.Remove(currentNode);
            closeList.Add(currentNode);

            foreach (PathNode nNode in currentNode.GetNeighbourList(WorldMap.Instance.MapNodes))
            {
                if (closeList.Contains(nNode))
                {
                    continue;
                }

                if (checkLand && nNode.IsOnLand)
                {
                    closeList.Add(nNode);
                    continue;
                }

                int tentativeGCost = currentNode.GCost + TacticalMapGrid.CalculateRemainingDistance(currentNode.MapSNode, nNode.MapSNode);
                if (tentativeGCost < nNode.GCost)
                {
                    nNode.CameFromNode = currentNode;
                    nNode.GCost = tentativeGCost;
                    nNode.HCost = TacticalMapGrid.CalculateRemainingDistance(nNode.MapSNode, endNode.MapSNode);
                    nNode.CalculateFCost();

                    if (!openList.Contains(nNode))
                    {
                        openList.Add(nNode);
                    }
                }
            }
        }

        Debug.LogError("Cannot find path to node.");
        //PrintTime("FindPath", false);
        return null;
    }

    private PathNode FindEndNode()
    {
        //dont assign value if you don't use it
        PathNode result;
        if (isChasing)
        {
            result = FindCloseNodeWithOffset();
        }
        else if (isRetreating)
        {
            result = FindRetreatingNode();
        }
        else
        {
            result = FindWithOffset(destination);
        }
        return result;
    }

    //Find node closest to player
    private PathNode FindCloseNodeWithOffset()
    {
        return FindWithOffset(WorldMap.Instance.MapShip.Position);
    }

    private PathNode FindRetreatingNode()
    {
        var pos = rectTransform.anchoredPosition + rectTransform.anchoredPosition - WorldMap.Instance.MapShip.Position;
        var bounds = TacticalMapCreator.HalfGameResolution;
        var position = new Vector2(Mathf.Clamp(pos.x, -bounds.x, bounds.x), Mathf.Clamp(pos.y, -bounds.y, bounds.y));
        return FindWithOffset(position);
    }

    private PathNode FindWithOffset(Vector2 position)
    {
        var point = UnityRandom.insideUnitCircle * 10f;
        point += position;
        return WorldMap.Instance.MapNodes.Find(point);
    }
}

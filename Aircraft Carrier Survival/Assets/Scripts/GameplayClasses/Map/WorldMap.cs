using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldMap : WaypointMap, IPointerClickHandler, ITickable, IPointerDownHandler, IPointerUpHandler
{
    public static WorldMap Instance = null;
    public event Action<bool> Toggled = delegate { };
    public event Action Arrived = delegate { };
    public event Action WaypointAdded = delegate { };

    public Texture2D EnemyZonesTargetTexture;
    public List<TerritoryNode> TerritoryNodes = new List<TerritoryNode>();

    public List<SandboxNode> AllSandboxNodes;
    public List<SandboxNode> PoiNodes;
    public List<SandboxNode> EnemyFleetsNodes;
    public List<SandboxNode> EnemyBasesNodes;
    public List<SandboxNode> RepairSpotNodes;
    public PearlHarbourPoi PearlHarbour => pearlHarbour;
    public Transform FleetsParent => fleetsParent;
    public Transform MarkersParent => markersParent;
    public SandboxMapSpawner SandboxMapSpawner => sandboxMapSpawner;
    public SandboxNodeMaps NodeMaps => nodeMaps;

    public int Mission
    {
        get;
        set;
    } = -1;

    public float StartNodesDistanceSqr => startNodesDistanceSqr;
    public List<Texture2D> LandMasks => landMasks;
    public List<Texture2D> MapImages => mapImages;
    public List<Texture2D> MapMasks => mapMasks;
    public Texture2D EnemyZonesColorsTexture => enemyZonesColorsTexture;
    public int MinDistanceBetweenPois => minDistanceBetweenPois;

    public GameObject Container
    {
        get => mapMainObject;
    }

    [Header("Refs")]
    [SerializeField]
    private SandboxNodeMaps nodeMaps = null;
    [SerializeField]
    private SandboxMapSpawner sandboxMapSpawner = null;
    [SerializeField]
    private WorldMapShip mapShip = null;
    [SerializeField]
    private Image mapImage = null;
    [SerializeField]
    private List<Texture2D> mapImages = null;
    [SerializeField]
    private List<Texture2D> mapMasks = null;
    [SerializeField]
    private List<Texture2D> landMasks = null;
    [SerializeField]
    private Texture2D enemyZonesColorsTexture = null;
    //[SerializeField]
    //private Texture2D enemyZonesTargetTexture = null;

    [SerializeField]
    private WorldMapMovement worldMapMovement = null;

    [SerializeField]
    private MainSceneResolutionResizer resizer = null;

    [SerializeField]
    private int maxWaypointsNumber = 3;
    [SerializeField]
    private PearlHarbourPoi pearlHarbour = null;

    [SerializeField]
    private Transform fleetsParent = null;
    [SerializeField]
    private Transform markersParent = null;
    [SerializeField]
    private int minDistanceBetweenPois = 75;
    [SerializeField]
    private List<WorldMapSectorNode> sectors = null;
    [SerializeField]
    private float sectorRadious = 75f;
    [SerializeField]
    private float startNodesDistanceSqr = 40000f;
    [SerializeField]
    private Transform waypointTargetsParent = null;

    private float sectorRadiousSqr;

    private bool firstTime = true;
    private SectionRoomManager sectionMan;
    private DamageControlManager dcMan;
    private CrewStatusManager crewStatus;

    private float scale;
    private bool loaded;
    private bool addWaypointMode;

    private List<PathNode> tempPath;

    private void Start()
    {
        scale = (resizer.Scale > 1f ? resizer.Scale : 1f);
    }

    private void Update()
    {
        if (!HudManager.Instance.IsSettingsOpened)
        {
            //if (CanAddWaypoint())
            //{
            //    bool inside = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition);
            //    DrawPotentialTrack(waypointList, inside);
            //}
            //else
            //{
            //    SetPreview(false);
            //}        
            if (isPointerDown)
            {
                if (waypointList.Count > 0)
                {
                    waypointList[0].Drag(Input.mousePosition);
                }
                else
                {
                    GetPoints(waypointList, out Vector2 screenPointA, out Vector2 screenPointB);
                    tempPath = GetPath(screenPointA, screenPointB, MapNodes);
                    UpdateTrackPreview(tempPath, null);
                }
            }
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerUp(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            DestroyHoveredWaypoint();
            BackgroundAudio.Instance.PlayEvent(EButtonState.Back);
        }

        //if (eventData.button == PointerEventData.InputButton.Right)
        //{
        //    AddWaypointMode = false;
        //}
    }

    public void PointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
        {
            isPointerDown = false;
            //SandboxManager.Instance.PoiManager.SetPoisInteractable(true);
            if (waypointList.Count > 0)
            {
                waypointList[0].OnEndDrag(eventData);
            }
            OnLeftClick();
        }
    }
    public void PointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
        {
            //SandboxManager.Instance.PoiManager.SetPoisInteractable(false);
            isPointerDown = true;
            if (waypointList.Count > 0)
            {
                waypointList[0].OnBeginDrag(eventData);
            }
        }
    }

    public void OnLeftClick()
    {
        if (waypointList.Count < maxWaypointsNumber)
        {
            GetPoints(waypointList, out Vector2 screenPointA, out Vector2 screenPointB);
            tempPath = GetPath(screenPointA, screenPointB, MapNodes);
            if (tempPath != null)
            {
                AddWaypoint(waypointList, tempPath);
                BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
                tempPath = null;
                HideRoadTrackPreviev();
            }
        }
    }


    public void SaveData(ref WorldMapSaveData saveData)
    {
        saveData.ShipPosition = mapShip.Rect.anchoredPosition;
        saveData.Waypoints = new List<MyVector2>();
        foreach (var waypoint in waypointList)
        {
            saveData.Waypoints.Add(waypoint.RectTransform.anchoredPosition);
        }

        var data = SaveManager.Instance.Data;
        data.SavedSandboxTime = TimeManager.Instance.GetCurrentTime();
    }

    public void Load(ref WorldMapSaveData saveData)
    {
        loaded = true;
        mapShip.SetAnchoredPosition(saveData.ShipPosition);
        var lastWaypoint = MapShip.Position;
        foreach (var waypoint in saveData.Waypoints)
        {
            var path = GetPath(lastWaypoint, waypoint, MapNodes);
            AddWaypoint(waypointList, path);
            lastWaypoint = waypoint;
        }
    }

    public void Setup()
    {
        if (firstTime)
        {
            MapShip = mapShip;
            mapShip.Init();
            firstTime = false;
            dcMan = DamageControlManager.Instance;
            sectionMan = SectionRoomManager.Instance;
            crewStatus = CrewStatusManager.Instance;
            sandboxMapSpawner.Setup();
            sectorRadiousSqr = sectorRadious * sectorRadious;
            if (!loaded)
            {
                mapShip.SetAnchoredPosition(pearlHarbour.RectTransform.anchoredPosition);
            }
            CreateMovementGrid();
            SandboxManager.Instance.MissionInstanceFinished += OnMissionInstanceFinished;
        }
    }

    public void Toggle(bool state)
    {
        if (state)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void RemoveWaypoint(ShipWaypoint waypoint)
    {
        waypointList.Remove(waypoint);
        waypoint.DestroyAllTracks();
        waypoint.DestroyTaskSegments();
        Destroy(waypoint.gameObject);
    }

    public override ShipWaypoint GetNextWaypoint()
    {
        waypointList[0].DestroyAllTracks();
        Destroy(waypointList[0].gameObject);
        waypointList.RemoveAt(0);
        //if(currentObjective != null && waypointList.Count == 0)
        //{
        //    Destroy(currentObjective.gameObject);
        //    currentObjective = null;
        //}

        Arrived();
        return waypointList.Count == 0 ? null : waypointList[0];
    }

    public override bool IsLandOnTheWay(Vector2 a, Vector2 b)
    {
        GetMaskVectors(ref a, ref b);

        return PlotLine(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), Mathf.RoundToInt(b.x), Mathf.RoundToInt(b.y), color => color.r, landMask) != -1f;
    }

    public float IsOnRedWaters(Vector2 a, Vector2 b)
    {
        GetMaskVectors(ref a, ref b);

        return PlotLine(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), Mathf.RoundToInt(b.x), Mathf.RoundToInt(b.y), color => color.r, redWatersMask);
    }

    public int GetClosestSector(Vector2 pos)
    {
        var closest = (int)EWorldMapSector.Generic;
        var closestDist = float.PositiveInfinity;
        foreach (var sector in sectors)
        {
            var dist = Vector2.SqrMagnitude(pos - sector.RectTransform.anchoredPosition);
            if (dist < sectorRadiousSqr && dist < closestDist)
            {
                closestDist = dist;
                closest = (int)sector.Sector;
            }
        }
        return closest;
    }

    private void AddWaypoint(List<ShipWaypoint> wpList, List<PathNode> path)
    {
        var startPoint = GetStartPoint(wpList);
        float distance = Vector2.Distance(startPoint, path[path.Count - 1].Position);
        if (distance < minimumWaypointDistance)
        {
            return;
        }

        ShipWaypoint wp = Instantiate(waypointPrefab, Vector3.zero, Quaternion.identity, waypointTargetsParent).GetComponent<ShipWaypoint>();
        var trans = wp.GetComponent<RectTransform>();
        trans.anchoredPosition = path[path.Count - 1].Position;
        trans.localScale = Vector3.one;
        wpList.Add(wp);
        wp.SetMap(this);
        SpawnRoadTrack(path, wp, roadPrefab, trackParent, road);
    }

    private void DestroyHoveredWaypoint()
    {
        if (waypointList.Count > 0)
        {
            var waypoint = waypointList[waypointList.Count - 1];
            if (waypoint.Hovered)
            {
                RemoveWaypoint(waypoint);
                if (waypointList.Count == 0)
                {
                    TacticManager.Instance.Carrier.StopShip();
                }
            }
        }
    }

    private void Show()
    {
        Container.SetActive(true);
        TimeManager.Instance.AddTickable(this);
        Toggled(true);
    }

    private void Hide()
    {
        TimeManager.Instance.RemoveTickable(this);
        Container.SetActive(false);
        Toggled(false);
        HudManager.Instance.UnsetBlockSpeed();
    }

    private Vector2 GetLastWaypointPosition(List<WorldMapWaypoint> wpList)
    {
        if (wpList.Count < 1)
            return mapShip.Rect.anchoredPosition;
        else
            return wpList[wpList.Count - 1].GetComponent<RectTransform>().anchoredPosition;
    }

    private void GetMaskVectors(ref Vector2 a, ref Vector2 b)
    {
        var scale = landMask.width / rectTransform.sizeDelta.x;
        a *= scale;
        b *= scale;

        float offsetX = landMask.width / 2f;
        float offsetY = landMask.height / 2f;

        a.x += offsetX;
        a.y += offsetY;

        b.x += offsetX;
        b.y += offsetY;
    }

    private void GetPoints(List<ShipWaypoint> wpList, out Vector2 screenPointA, out Vector2 screenPointB)
    {
        screenPointA = GetStartPoint(wpList);
        GetPoint(out screenPointB);
    }

    private bool GetPoint(out Vector2 screenPointB)
    {
        float scaleX = canvasRect.rect.width / Screen.width;
        float scaleY = canvasRect.rect.height / Screen.height;

        screenPointB = Input.mousePosition;
        screenPointB.x *= scaleX;
        screenPointB.x -= canvasRect.rect.width / 2f;
        screenPointB.y *= scaleY;
        screenPointB.y -= canvasRect.rect.height / 2f;
        screenPointB -= worldMapMovement.MapRect.offsetMin;
        screenPointB /= worldMapMovement.CurrentZoom;

        screenPointB *= scale;
        //if(objectiveList.Count > 0)
        //{
        //    var objectivePos = (objectiveList[0] as RectTransform).anchoredPosition;
        //    if (Vector2.SqrMagnitude(objectivePos - screenPointB) < snapDistanceSqr)
        //    {
        //        screenPointB = objectivePos;
        //        return true;
        //    }
        //}
        return true;
    }

    private void GetMissionPoints(out Vector2 screenPointB)
    {
        float scaleX = canvasRect.rect.width / Screen.width;
        float scaleY = canvasRect.rect.height / Screen.height;

        screenPointB = Input.mousePosition;
        screenPointB.x *= scaleX;
        screenPointB.x -= canvasRect.rect.width / 2f;
        screenPointB.y *= scaleY;
        screenPointB.y -= canvasRect.rect.height / 2f;

        screenPointB -= worldMapMovement.MapRect.offsetMin;
        screenPointB /= worldMapMovement.CurrentZoom;
    }

    public void Tick()
    {
        mapShip.Tick();
    }

    private Vector2 GetStartPoint(List<ShipWaypoint> wpList)
    {
        return wpList.Count > 0 ? wpList[wpList.Count - 1].RectTransform.anchoredPosition : mapShip.Rect.anchoredPosition;
    }

    private void CreateMovementGrid()
    {
        if (MapNodes == null)
        {
            MapNodes = new TacticalMapGrid(
                this,
                mapImage.GetComponent<RectTransform>().rect.width,
                mapImage.GetComponent<RectTransform>().rect.height,
                192,
                108);
        }
    }

    private void OnMissionInstanceFinished()
    {
        var halfHeight = TacticalMapCreator.TacticMapHeight / 2f;
        Vector2 offset = new Vector2(0f, halfHeight);
        var nodePos = SandboxManager.Instance.PoiManager.GetNode(SaveManager.Instance.Data.SandboxData.CurrentMissionSaveData.NodeIndex).Position + offset;
        mapShip.Teleport(TacticalMapCreator.TransformTacticMapPointToWorldMapPoint(tacticManager.Map.MapShip.Position, nodePos));
        if (waypointList.Count > 0)
        {
            RemoveWaypoint(waypointList[0]);
        }
        mapShip.StopShip();
    }
}

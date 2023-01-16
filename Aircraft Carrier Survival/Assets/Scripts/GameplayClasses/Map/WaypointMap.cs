using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WaypointMap : Map, IPointerClickHandler
{
    public bool WaypointDraging
    {
        get;
        set;
    }

    public MapShip MapShip
    {
        get;
        protected set;
    }

    public RectTransform MissionWaypointOutlineTransform
    {
        get;
        set;
    }

    public TacticalMapGrid MapNodes
    {
        get;
        set;
    }

    public bool LandOnTheWay
    {
        get;
        private set;
    }

    public int WaypointListCount => waypointList.Count;
    public TacticManager TacticManager => tacticManager;
    public RectTransform RectTransform => rectTransform == null ? (rectTransform = GetComponent<RectTransform>()) : rectTransform;

    [Header("BASE Prefabs")]
    [SerializeField]
    protected GameObject waypointPrefab = null;
    [SerializeField]
    protected GameObject roadPrefab = null;
    [SerializeField]
    protected GameObject dotPrefab = null;

    [Header("BASE Refs")]
    [SerializeField]
    protected Texture2D redWatersMask = null;
    [SerializeField]
    protected RectTransform trackParent = null;
    [SerializeField]
    protected Canvas canvas = null;
    [SerializeField]
    protected GameObject mapMainObject;
    [SerializeField]
    protected TacticManager tacticManager = null;

    [Header("BASE Params")]
    [SerializeField]
    protected float snapDistanceSqr = 1600f;
    [SerializeField]
    protected float minimumWaypointDistance = 5f;
    [SerializeField]
    protected int maxVisibleMissions = 1;

    [Header("BASE Sprites")]
    [SerializeField]
    protected Sprite road = null;
    [SerializeField]
    protected Color planningGreen = new Color32(224, 240, 183, 255);
    [SerializeField]
    protected Color green = new Color32(197, 210, 164, 255);
    [SerializeField]
    protected Color red = new Color32(207, 106, 93, 255);

    protected RectTransform canvasRect = null;

    protected List<RectTransform> previewTrackList = new List<RectTransform>();
    protected List<Image> previewTrackImageList = new List<Image>();
    protected RectTransform rectTransform = null;

    protected List<ShipWaypoint> waypointList = new List<ShipWaypoint>();
    protected bool isPointerDown;
    protected List<RectTransform> roadTrackPreview = new List<RectTransform>();
    protected List<Image> previewRoadTrackImageList = new List<Image>();

    protected bool isTacticalMap = false;

    private List<PathNode> openList = new List<PathNode>();
    private HashSet<PathNode> closeList = new HashSet<PathNode>();
    private PathNodeComparer PathNodeComparer = new PathNodeComparer();

    private bool inited;

    private void Awake()
    {
        if (!inited)
        {
            Preinit();
        }
        minimumWaypointDistance = 0f; ///TO REMOVE
        SpawnTrackPreview();

        OnAwake();
    }

    public virtual Vector2 GetEndPointFromMousePosition()
    {
        return Vector2.zero;
    }

    public virtual bool IsLandOnTheWay(Vector2 a, Vector2 b)
    {
        return true;
    }

    public virtual ShipWaypoint GetNextWaypoint()
    {
        return null;
    }

    public virtual bool CanBeDropped(ShipWaypoint waypoint, Vector2 pos)
    {
        return true;
    }

    public virtual void RedrawPotentialTrack(ShipWaypoint waypoint, bool ship, bool ok, bool plan)
    {

    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {

    }

    public void Preinit()
    {
        rectTransform = transform as RectTransform;
        canvasRect = canvas.transform as RectTransform;
        inited = true;
    }

    public void HideRoadTrackPreviev()
    {
        foreach (var track in roadTrackPreview)
        {
            track.gameObject.SetActive(false);
        }
    }

    public List<PathNode> GetPath(Vector2 startPosition, Vector2 targetPosition, TacticalMapGrid grid)
    {
        if (grid.Find(targetPosition).IsOnLand && !isTacticalMap)
        {
            return null;
        }
        LandOnTheWay = IsLandOnTheWay(startPosition, targetPosition);
        if (!LandOnTheWay || isTacticalMap)
        {
            List<PathNode> list = new List<PathNode>();
            var startNode = grid.Find(startPosition);
            var endNode = grid.Find(targetPosition);
            list.Add(startNode);
            list.Add(endNode);
            return list;
        }
        LandOnTheWay = false;
        return FindPath(startPosition, targetPosition, grid);
    }

    public List<PathNode> FindPath(Vector2 startNodePos, Vector2 endNodePos, TacticalMapGrid grid)
    {
        var startNode = grid.Find(startNodePos);
        var endNode = grid.Find(endNodePos);
        if (endNode.IsOnLand)
        {
            return null;
        }
        openList.Clear();
        openList.Add(startNode);
        openList[0].CameFromNode = null;
        openList[0].StartNodeDistance = 0f;
        closeList.Clear();
        while (openList.Count > 0)
        {
            var currentNode = openList[openList.Count - 1];
            if (endNode == currentNode)
            {
                return TacticalMapGrid.CalculatePathHelper(endNode);
            }
            openList.RemoveAt(openList.Count - 1);
            closeList.Add(currentNode);

            foreach (PathNode newNode in currentNode.GetNeighbourList(grid))
            {
                if (!closeList.Add(newNode))
                {
                    continue;
                }

                if (newNode.IsOnLand && IsLandOnTheWay(currentNode.Position, newNode.Position))
                {
                    continue;
                }

                newNode.CameFromNode = currentNode;
                newNode.GCost = newNode.CameFromNode.GCost + TacticalMapGrid.CalculateRemainingDistance(currentNode.MapSNode, newNode.MapSNode);
                newNode.StartNodeDistance = TacticalMapGrid.CalculateRemainingDistance(newNode.MapSNode, endNode.MapSNode) + newNode.GCost;
                int index = openList.BinarySearch(newNode, PathNodeComparer);
                if (index < 0)
                {
                    openList.Insert(~index, newNode);
                }
                else
                {
                    openList.Insert(index, newNode);
                }
            }
        }
        return null;
    }

    public void SpawnRoadTrack(List<PathNode> path, MapWaypoint waypoint, GameObject roadPrefab, RectTransform trackParent, Sprite sprite)
    {
        if (!LandOnTheWay)
        {
            waypoint.SetPath(path);
        }
        waypoint.SpawnRoadTrack(path, roadPrefab, trackParent, sprite, LandOnTheWay ? red : green);
    }

    public ShipWaypoint GetFirstWaypoint()
    {
        return waypointList.Count > 0 ? waypointList[0] : null;
    }

    public bool IsThereNextWaypoint()
    {
        return waypointList.Count > 0;
    }

    public void RefreshCurrentTrack(Vector2 pos)
    {
        GetFirstWaypoint()?.RefreshRoadTrack(pos);
    }

    public void CreatePreviewTracks()
    {
        for (int i = 0; i < 40; i++)
        {
            RectTransform track = Instantiate(roadPrefab, trackParent).transform as RectTransform;
            previewTrackList.Add(track);
            previewTrackImageList.Add(track.GetComponent<Image>());
            track.gameObject.SetActive(false);
        }
    }

    public bool RedrawRoadTracks(ShipWaypoint waypoint)
    {
        if (MapNodes.Find(waypoint.RectTransform.anchoredPosition).IsOnLand && !isTacticalMap)
        {
            waypoint.SetTracksColor(red);
            return false;
        }
        bool refreshPath = false;
        int index = waypointList.IndexOf(waypoint);
        bool landOnTheWay = false;
        if (!CanRedrawRoadTracks(waypoint, index))
        {
            waypoint.SetTracksColor(red);
            return false;
        }

        if (index > 0)
        {
            var list = GetPath(waypointList[index - 1].RectTransform.anchoredPosition, waypointList[index].RectTransform.anchoredPosition, MapNodes);
            landOnTheWay |= LandOnTheWay;
            if (list == null)
            {
                return false;
            }
            waypointList[index].DestroyAllTracks();
            SpawnRoadTrack(list, waypointList[index], roadPrefab, trackParent, road);
        }
        else
        {
            var list = GetPath(MapShip.Position, waypointList[index].RectTransform.anchoredPosition, MapNodes);
            landOnTheWay |= LandOnTheWay;
            if (list == null)
            {
                return false;
            }
            waypointList[index].DestroyAllTracks();
            SpawnRoadTrack(list, waypointList[index], roadPrefab, trackParent, road);
            refreshPath = true;
        }
        if (index < waypointList.Count - 1)
        {
            var list = GetPath(waypointList[index].RectTransform.anchoredPosition, waypointList[index + 1].RectTransform.anchoredPosition, MapNodes);
            landOnTheWay |= LandOnTheWay;
            if (list == null)
            {
                return false;
            }
            waypointList[index + 1].DestroyAllTracks();
            SpawnRoadTrack(list, waypointList[index + 1], roadPrefab, trackParent, road);
        }
        if (refreshPath)
        {
            MapShip.RefreshPath();
        }
        LandOnTheWay = landOnTheWay;
        return true;
    }

    protected virtual void OnAwake()
    {

    }

    protected virtual void SpawnTrack(Vector2 vFrom, Vector2 vTo, MapWaypoint wp, Sprite sprite1, Sprite sprite2, Color color, bool isPlaneTrack)
    {
        wp.SpawnTrack(vFrom, vTo, roadPrefab, trackParent, sprite1, sprite2, color, isPlaneTrack);
    }

    protected virtual bool CanAddWaypointHere(Vector2 from, Vector2 to)
    {
        return !LandOnTheWay;
    }

    protected virtual bool CanRedrawRoadTracks(ShipWaypoint waypoint, int index)
    {
        return index >= 0;
    }

    protected void SpawnTrackPreview()
    {
        for (int i = 0; i < 400; i++)
        {
            var roadTransform = Instantiate(roadPrefab, trackParent).transform as RectTransform;
            roadTrackPreview.Add(roadTransform);
            previewRoadTrackImageList.Add(roadTransform.GetComponent<Image>());
            previewRoadTrackImageList[i].sprite = road;
            roadTrackPreview[i].gameObject.SetActive(false);
        }
    }

    protected void UpdateTrackPreview(List<PathNode> nodes, Vector2? targetPos)
    {
        if (nodes == null)
        {
            foreach (var image in previewRoadTrackImageList)
            {
                image.color = red;
            }
            return;
        }
        int i = 0;
        if (nodes.Count == 2)
        {
            var target = targetPos != null ? targetPos.Value : nodes[1].Position;
            bool disabled = !CanAddWaypointHere(nodes[0].Position, target);
            Vector2 diff = nodes[0].Position - target;
            float dist = diff.magnitude;
            int segmentsCount = (int)(dist / 30f);
            for (i = 0; i <= segmentsCount; i++)
            {
                roadTrackPreview[i].gameObject.SetActive(true);
                roadTrackPreview[i].anchoredPosition = Vector2.Lerp(nodes[0].Position, target, (float)i / segmentsCount);
                previewRoadTrackImageList[i].color = disabled ? red : green;
            }
        }
        else
        {
            for (; i < nodes.Count; i++)
            {
                if (i % 3 != 0)
                {
                    roadTrackPreview[i].gameObject.SetActive(false);
                    continue;
                }
                roadTrackPreview[i].gameObject.SetActive(true);
                roadTrackPreview[i].anchoredPosition = nodes[i].Position;
                previewRoadTrackImageList[i].color = green;
            }
        }
        for (; i < roadTrackPreview.Count - 1; i++)
        {
            roadTrackPreview[i].gameObject.SetActive(false);
        }
    }

    protected void SetPreview(bool state)
    {
        foreach (RectTransform tr in previewTrackList)
        {
            tr.gameObject.SetActive(state);
        }
        foreach (var road in roadTrackPreview)
        {
            road.gameObject.SetActive(state);
        }
    }

    protected void DrawPotentialTrack(Vector2 screenPointA, Vector2 screenPointB, Sprite roadSprite1, Sprite roadSprite2, float dist, bool setMarksRotation, float normalDist, Color color, bool isPlaneTrack)
    {
        int segmentAmount = Mathf.Min(30, (int)(dist / 30f));
        float xDelta = (screenPointB.x - screenPointA.x) / segmentAmount;
        float yDelta = (screenPointB.y - screenPointA.y) / segmentAmount;

        Vector3 dir = (screenPointA - screenPointB).normalized;
        var rotation = Quaternion.Euler(0f, 0f, Vector3.Angle(Vector3.right, dir) * Mathf.Sign(dir.y));

        for (int i = 0; i < segmentAmount; i++)
        {
            previewTrackList[i].gameObject.SetActive(true);
            previewTrackList[i].anchoredPosition = new Vector3(screenPointA.x + xDelta * (i + 1), screenPointA.y + yDelta * (i + 1));
            previewTrackList[i].rotation = rotation;
            previewTrackImageList[i].sprite = i % 2 == 0 ? roadSprite1 : roadSprite2;

            var size = previewTrackImageList[i].sprite.rect.size;
            if (roadSprite1 != roadSprite2)
            {
                size *= .7f;
            }
            if (isPlaneTrack)
            {
                size.y /= 2f;
            }
            previewTrackList[i].sizeDelta = size;

            previewTrackImageList[i].color = (normalDist >= 0f && Vector2.Distance(screenPointA, previewTrackList[i].anchoredPosition) > normalDist) ? red : color;
        }
        if (setMarksRotation)
        {
            Vector2 destinatedDirection = screenPointB - screenPointA;
            Quaternion markRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, destinatedDirection));

            for (int i = 0; i < segmentAmount; i++)
            {
                previewTrackList[i].rotation = markRotation;
            }
        }
        for (int i = segmentAmount; i < previewTrackList.Count; i++)
        {
            previewTrackList[i].gameObject.SetActive(false);
        }
    }
}

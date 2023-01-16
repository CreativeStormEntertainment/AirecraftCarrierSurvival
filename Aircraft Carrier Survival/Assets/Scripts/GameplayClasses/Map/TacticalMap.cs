using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;

public class TacticalMap : WaypointMap, IPointerClickHandler
{
    public static TacticalMap Instance = null;
    public event Action AirRaidWaypointsSet = delegate { };
    public event Action WaypointAdded = delegate { };
    public event Action<Vector2> WaypointPositionAdded = delegate { };
    public event Action<bool, EMarkerType, List<TacticalEnemyShip>> MarkerHighlightChanged = delegate { };
    public event Action MissionAreasSet = delegate { };
    public event Action CourseSettingModeChanged = delegate { };

    public bool ReconWaypointInUoRange
    {
        get;
        set;
    }

    public bool RecoveryOnCarrier
    {
        get;
        set;
    }

    public bool CanAddRetrievalWaypoint
    {
        get;
        set;
    } = true;

    public bool AirRaidMode
    {
        get;
        set;
    }

    public RectTransform BuoyOutlineTransform
    {
        get;
        set;
    }

    public ShipWaypoint AircraftReconPointerPrefab => aircraftReconPointerPrefab;
    public ShipWaypoint AircraftAttackPointPrefab => aircraftAttackPointPrefab;
    public ShipWaypoint AircraftAttackReturnPointPrefab => aircraftAttackReturnPointPrefab;

    public RectTransform ObjectivesParent => objectivesParent;

    public MapMissionSetup MapMissionSetup => mapMissionSetup;

    public ChooseOrderType ChooseOrderType => attackTypePanel;

    public List<ShipWaypoint> AttackWaypoints => airRaidWaypointList;

    public Image MapImage => mapImage;

    [NonSerialized]
    public List<EnemyUnitData> EnemyUnits = null;
    [NonSerialized]
    public bool CourseSettingMode = false;

    [Header("Prefabs")]
    [SerializeField]
    private ShipWaypoint newWaypointPrefab = null;

    [SerializeField]
    private ShipWaypoint aircraftReconPointerPrefab = null;
    [SerializeField]
    private ShipWaypoint aircraftAttackPointPrefab = null;
    [SerializeField]
    private ShipWaypoint aircraftAttackReturnPointPrefab = null;

    [Header("Refs")]
    [SerializeField]
    private TacticalMapShip mapShip = null;
    [SerializeField]
    private Transform enemyMarkers = null;
    [SerializeField]
    private Transform cloudMarkers = null;
    [SerializeField]
    private ChooseOrderType attackTypePanel = null;
    [SerializeField]
    private Camera unitMaskCamera = null;
    [SerializeField]
    private Transform waypointTargetsParent = null;
    [SerializeField]
    private RectTransform objectivesParent = null;

    [Header("Params")]
    [SerializeField]
    [Range(0f, 1f)]
    private float enemyKnowledge = 0f;
    [SerializeField]
    [Range(0f, 1f)]
    private float weatherKnowledge = 0f;
    [SerializeField]
    private Image mapShadow = null;
    [SerializeField]
    private Image mapOutline = null;
    [SerializeField]
    private Image mapImage = null;

    [SerializeField]
    private IntermissionMapResolutionResizer resizer = null;

    [SerializeField]
    private Sprite planeRoad1 = null;
    [SerializeField]
    private Sprite planeRoad2 = null;

    [Header("Mission Setup")]
    [SerializeField]
    private MapMissionSetup mapMissionSetup = null;
    [SerializeField]
    private RectTransform mouseCircle = null;
    [SerializeField]
    private Text tipText = null;

    [SerializeField]
    private Color okRangeColor = default;
    [SerializeField]
    private Color badRangeColor = default;

    [SerializeField]
    private float customWaypointsSnapDistance = 30f;

    private float percentPerEnemy = 0f;
    private List<MapMarker> enemyList = new List<MapMarker>();

    private float percentPerCloud = 0f;
    private List<MapMarker> cloudList = new List<MapMarker>();
    private Material shaderMaterial = null;

    private List<ShipWaypoint> airRaidWaypointList = new List<ShipWaypoint>();

    private bool tacticalObjectTargetMode = false;

    private bool onlyAttackWaypoint;

    private Vector2? customWaypointPlacement;
    private Vector2? customWaypointPlacement2;
    private float customWaypointsSnapDistanceSqr;

    private bool shipClicked;

    private List<TacticalEnemyShip> enemies = new List<TacticalEnemyShip>();
    private List<TacticalEnemyShip> tempEnemies = new List<TacticalEnemyShip>();

    private Image mouseCircleImage;

    private List<PathNode> tempPathNodes;

    protected override void OnAwake()
    {
        customWaypointsSnapDistanceSqr = customWaypointsSnapDistance * customWaypointsSnapDistance;

        BuoyOutlineTransform = Instantiate(TacticManager.Instance.BuoyOutlinePrefab, transform).GetComponent<RectTransform>();
        BuoyOutlineTransform.gameObject.SetActive(false);

        MissionWaypointOutlineTransform = Instantiate(TacticManager.Instance.MissionWaypointOutlinePrefab, transform).GetComponent<RectTransform>();
        MissionWaypointOutlineTransform.gameObject.SetActive(false);

        mouseCircleImage = mouseCircle.GetComponent<Image>();
    }

    private void Start()
    {
        CreatePreviewTracks();

        unitMaskCamera.aspect = mapImage.sprite.rect.width / mapImage.sprite.rect.height;

        OnRectTransformDimensionsChange();
    }

    private void Update()
    {
        bool canSetupCourse = SectionRoomManager.Instance.Helm.IsWorking;

        if (CourseSettingMode && !canSetupCourse)
        {
            ToggleCourseSetup(false);
        }

        bool inside = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition);
        bool can = HudManager.Instance.HasNo(ETutorialMode.DisableFreeWaypointSet);
        bool special = false;
        if (!can && DemoMissionGame.Instance.EnableWaypointArea != null)
        {
            can = true;
            special = true;
        }
        if (can && CourseSettingMode && waypointList.Count < 3)
        {
            GetPoints(waypointList, out Vector2 screenPointA, out Vector2 screenPointB);
            tempPathNodes = GetPath(screenPointA, screenPointB, tacticManager.MapNodes);
            UpdateTrackPreview(tempPathNodes, screenPointB);
            //DrawPotentialTrack(waypointList, inside, special, road, road, false);
        }
        else if (can && AirRaidMode && (airRaidWaypointList.Count < 1 || (CanAddRetrievalWaypoint && airRaidWaypointList.Count < 2 && !onlyAttackWaypoint)))
        {
            bool inRange = DrawPotentialTrack(airRaidWaypointList, inside, special, planeRoad1, planeRoad2, true);
            //if (airRaidWaypointList.Count > 0)
            //{
            DrawRange(airRaidWaypointList.Count > 0);
            //}

            mouseCircleImage.color = inRange ? okRangeColor : badRangeColor;
        }
        else
        {
            SetPreview(false);
        }
    }

    private void OnEnable()
    {
        if (!mapShip.gameObject.activeSelf)
        {
            this.StartCoroutineActionAfterFrames(() =>
            {
                mapShip.gameObject.SetActive(true);
            }, 1);
        }
        //ToggleCourseSetup();
    }

    private void OnDisable()
    {
        if (mapShip.IsMoving)
        {
            mapShip.gameObject.SetActive(false);
        }
        CancelMissionSetup();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (canvas && rectTransform && canvasRect)
        {
            unitMaskCamera.orthographicSize = rectTransform.rect.size.y * canvas.scaleFactor * 0.5f;
        }
    }

    public void Setup()
    {
        Assert.IsNull(Instance);
        Instance = this;
        MapShip = mapShip;
        isTacticalMap = true;
        mapShip.Init();
    }

    public void LoadWaypoints(List<MyVector2> waypoints)
    {
        waypointList.Clear();
        if (AirRaidMode)
        {
            CancelMissionSetup(false);
        }
        if (waypoints.Count > 0)
        {
            tempPathNodes = GetPath(MapShip.Position, waypoints[0], tacticManager.MapNodes);
            CreateWaypoint(newWaypointPrefab, waypoints[0], mapShip.Rect.anchoredPosition, waypointList, false, road, road, green, false);
            for (int i = 1; i < waypoints.Count; i++)
            {
                tempPathNodes = GetPath(waypoints[i - 1], waypoints[i], tacticManager.MapNodes);
                CreateWaypoint(newWaypointPrefab, waypoints[i], waypoints[i - 1], waypointList, false, road, road, green, false);
            }
        }
    }

    public void SaveWaypoints(List<MyVector2> waypoints)
    {
        waypoints.Clear();
        foreach (var waypoint in waypointList)
        {
            waypoints.Add(waypoint.RectTransform.anchoredPosition);
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        mouseCircle.gameObject.SetActive(false);
        var hudMan = HudManager.Instance;
        var enemy = eventData.pointerPressRaycast.gameObject.transform.parent.TryGetComponent(out MarkerHighlight marker);
        shipClicked = eventData.pointerPressRaycast.gameObject.transform.parent.GetComponentInParent<TacticalMapShip>();
        if (!tacticalObjectTargetMode || enemy)
        {
            if (eventData.button == PointerEventData.InputButton.Left && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
            {
                var tacMan = TacticManager.Instance;
                bool special = false;
                if ((hudMan.HasNo(ETutorialMode.DisableFreeWaypointSet) || (special = DemoMissionGame.Instance.EnableWaypointArea != null)))
                {
                    if (CourseSettingMode && !LandOnTheWay)
                    {
                        tacMan.HideIdentificationPanel();
                        AddWaypoint(waypointList, true, special, false, road, road);
                        HideRoadTrackPreviev();
                        return;
                    }
                    else if (AirRaidMode && (airRaidWaypointList.Count < 1 || (CanAddRetrievalWaypoint && airRaidWaypointList.Count < 2 && !onlyAttackWaypoint)))
                    {
                        tacMan.HideIdentificationPanel();
                        AddWaypoint(airRaidWaypointList, false, special, marker != null && marker.MarkerType == EMarkerType.UO, planeRoad1, planeRoad2);
                        return;
                    }
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right && hudMan.HasNo(ETutorialMode.DisableCancelOnTacticalMap) && !GameStateManager.Instance.Tutorial)
            {
                EndWaypoint();
                CancelMissionSetup();
                DestroyHoveredWaypoint();
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right && hudMan.HasNo(ETutorialMode.DisableCancelOnTacticalMap) && !GameStateManager.Instance.Tutorial)
        {
            CancelMissionSetup();
            DestroyHoveredWaypoint();
        }
    }

    public void EndWaypoint()
    {
        if (CourseSettingMode)
        {
            ToggleCourseSetup(false);
        }
        else if (AirRaidMode)
        {
            ToggleAirRaidMode();
            DestroyAirRaidWaypoints();
        }
        TacticManager.Instance.CloseStrategyPanels();
        TacticManager.Instance.HideIdentificationPanel();
    }

    public void Init()
    {
        shaderMaterial = GetComponent<Image>().material;
        InitEnemyVisibility();
        InitCloudVisibility();
        //InitObjectives();
    }

    public void CustomAddWaypoint(Vector2 position)
    {
        tempPathNodes = GetPath(GetStartPoint(waypointList), position, tacticManager.MapNodes);
        CanAddWaypoint(waypointList, out var startPos, ref position, false);
        CreateWaypoint(waypointList, startPos, position, false, false, road, road);
    }

    public void RemoveWaypoints()
    {
        foreach (var wp in waypointList)
        {
            wp.DestroyGameObjectWithAllTracks();
        }
        waypointList.Clear();
        HideRoadTrackPreviev();

    }

    public void ClearRaidWaypoints(TacticalMission mission = null)
    {
        if (mission != null)
        {
            mission.MissionWaypoints = new GameObject("", typeof(RectTransform)).GetComponent<RectTransform>();
            mission.MissionWaypoints.SetParent(transform);
            foreach (ShipWaypoint awp in airRaidWaypointList)
            {
                //FogOfWarManager.Instance.RemoveUnmaskObject(awp.gameObject);
                awp.transform.SetParent(mission.MissionWaypoints);
                awp.GetAllTracksPoints(mission.MissionWaypoints);
                if (awp is MissionWaypoint mw)
                {
                    mw.enabled = false;
                }
            }
        }
        else
        {
            foreach (ShipWaypoint awp in airRaidWaypointList)
            {
                //FogOfWarManager.Instance.RemoveUnmaskObject(awp.gameObject);
                awp.DestroyGameObjectWithAllTracks();
            }
        }
        airRaidWaypointList.Clear();
    }

    public void AddRaidWaypointsToMission(TacticalMission mission)
    {
        RectTransform go = new RectTransform();
        go.name = "MissionWaypoints";
        Transform t = go.transform;
        go.transform.SetParent(trackParent);
        mission.WaypointsImages = new List<Image>();
        foreach (ShipWaypoint awp in airRaidWaypointList)
        {
            awp.transform.SetParent(t);
            foreach (GameObject o in awp.TracksList)
            {
                o.transform.SetParent(t);
                mission.WaypointsImages.Add(o.GetComponent<Image>());
            }
        }
        go.gameObject.SetActive(false);
        mission.MissionWaypoints = go;
        //attackTypePanel.CloseConfirmPanel();
        airRaidWaypointList.Clear();
    }

    public void ActivateObjective()
    {
        waypointList[0].TriggerMapIndicator();
    }

    public void ClearWaypoint(GameObject wp)
    {
        int lastOne = waypointList.Count - 1;
        if (waypointList[lastOne].gameObject == wp)
        {
            waypointList[lastOne].DestroyAllTracks();
            waypointList.RemoveAt(lastOne);
            Destroy(wp.gameObject);
        }
    }

    public override ShipWaypoint GetNextWaypoint()
    {
        waypointList[0].DestroyAllTracks();
        Destroy(waypointList[0].gameObject);
        waypointList.RemoveAt(0);
        if (waypointList.Count == 0)
        {
            return null;
        }
        return waypointList[0];
    }

    public override bool IsLandOnTheWay(Vector2 a, Vector2 b)
    {
        a *= tacticManager.MapNodes.MaskScale;
        b *= tacticManager.MapNodes.MaskScale;
        //a *= landScale;
        //b *= landScale;
        a.x += landMask.width / 2f;
        a.y += landMask.height / 2f;
        b.x += landMask.width / 2f;
        b.y += landMask.height / 2f;

        return PlotLine(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), Mathf.RoundToInt(b.x), Mathf.RoundToInt(b.y), color => color.r, landMask) != -1f;
    }

    public void SetMap(SOTacticMap data, bool isSandbox)
    {
        landMask = data.LandMask;
        EnemyUnits = data.EnemyUnits;
        Nodes = data.Nodes;
        if (!isSandbox)
        {
            shaderMaterial.SetTexture("_PatternTex", data.Map.texture);
            shaderMaterial.SetTexture("_PatternTex2", data.MapShadow.texture);

            mapShadow.sprite = data.MapShadow;
            mapOutline.sprite = data.Map;
            mapImage.sprite = data.Map;
        }
        else
        {
            shaderMaterial.SetTexture("_PatternTex", data.MapRenderTexture);
            shaderMaterial.SetTexture("_PatternTex2", data.MapShadowRenderTexture);
        }
    }

    public void ToggleCourseSetup(bool popup)
    {
        bool working = SectionRoomManager.Instance.Helm.IsWorking;
        if (AirRaidMode || !CourseSettingMode && !working)
        {
            if (popup && !working)
            {
                EventManager.Instance.WaypointPopup();
            }
            return;
        }

        ChangeCourseSet(!CourseSettingMode);

        //SetPreview(courseSettingMode);
        //if (courseSettingMode)
        //    startCourseButtonText.text = "Quit Setup Mode";
        //else
        //    startCourseButtonText.text = "Setup Course";
    }

    public void ToggleAirRaidMode()
    {
        AirRaidMode = !AirRaidMode;
        foreach (var waypoint in waypointList)
        {
            waypoint.Image.raycastTarget = !AirRaidMode;
        }
        CheckMaxRange();
        //txt.text = !AirRaidMode ? "Air Raid Mode ON" : "Air Raid Mode OFF";
        mapShip.Button.image.raycastTarget = !AirRaidMode;

        if (AirRaidMode)
        {
            ChangeCourseSet(false);
        }
        else
        {
            //attackTypePanel.CloseConfirmPanel();
        }
        ObjectivesManager.Instance.SetInteractable(!AirRaidMode);
        mapShip.RaycastTarget.raycastTarget = !AirRaidMode;
    }

    public void ChangeAirRaidMode(bool value, TacticalMission mission = null)
    {
        if (mapMissionSetup != null)
        {
            if (value && mission)
            {
                mapMissionSetup.Setup(mission);
                mapMissionSetup.Show();
                mapMissionSetup.SetInteractable(false);
            }
            else
            {
                mapMissionSetup.Hide();
            }
        }
        ClearRaidWaypoints();
        CanAddRetrievalWaypoint = true;
        AirRaidMode = value;
        ObjectivesManager.Instance.SetInteractable(!AirRaidMode);
        mapShip.RaycastTarget.raycastTarget = !AirRaidMode;
        TacticManager.Instance.Markers.SetInteractable(true);
        tacticalObjectTargetMode = value;
        onlyAttackWaypoint = false;
        foreach (var waypoint in waypointList)
        {
            waypoint.Image.raycastTarget = !value;
        }
        CheckMaxRange();
        mapShip.Button.image.raycastTarget = !AirRaidMode;
        if (value && mission != null)
        {
            switch (mission.OrderType)
            {
                case EMissionOrderType.Airstrike:
                case EMissionOrderType.NightAirstrike:
                    CanAddRetrievalWaypoint = false;
                    GetAllEnemies(ref enemies, mission);
                    if (mission.PossibleMissionTargets.Count == 0)
                    {
                        foreach (var m in tacticManager.Missions[EMissionOrderType.AttackJapan])
                        {
                            foreach (var target in m.PossibleMissionTargets)
                            {
                                enemies.Remove(target);
                            }
                        }
                    }
                    HighlightMarkers(true, EMarkerType.Enemy, enemies);
                    break;
                case EMissionOrderType.MagicAirstrike:
                    CanAddRetrievalWaypoint = false;
                    GetEnemyBasesList(ref enemies, mission);
                    HighlightMarkers(true, EMarkerType.Enemy, enemies);
                    break;
                case EMissionOrderType.IdentifyTargets:
                    enemies.Clear();
                    GetAllEnemies(ref enemies, mission);
                    RemoveIdentifiedEnemies(ref enemies, mission);
                    HighlightMarkers(true, EMarkerType.Enemy, enemies);
                    HighlightMarkers(true, EMarkerType.UO);
                    break;
                case EMissionOrderType.MagicIdentify:
                    GetEnemyBasesList(ref enemies, mission);
                    RemoveIdentifiedEnemies(ref enemies, mission);
                    HighlightMarkers(true, EMarkerType.Enemy, enemies);
                    break;
                case EMissionOrderType.Recon:
                case EMissionOrderType.NightScouts:
                case EMissionOrderType.MagicNightScouts:
                    tacticalObjectTargetMode = false;
                    TacticManager.Instance.Markers.SetInteractable(false);
                    break;
                case EMissionOrderType.Decoy:
                    tacticalObjectTargetMode = false;
                    //onlyAttackWaypoint = true;
                    TacticManager.Instance.Markers.SetInteractable(false);
                    break;
                case EMissionOrderType.FriendlyFleetCAP:
                case EMissionOrderType.FriendlyCAPMidway:
                    HighlightMarkers(true, EMarkerType.Friend);
                    break;
                case EMissionOrderType.AirstrikeSubmarine:
                    CanAddRetrievalWaypoint = false;
                    List<TacticalEnemyShip> submarines = new List<TacticalEnemyShip>();
                    foreach (var enemy in TacticManager.GetAllShips())
                    {
                        if (enemy.HadGreaterInvisibility)
                        {
                            submarines.Add(enemy);
                        }
                    }
                    HighlightMarkers(true, EMarkerType.Enemy, submarines);
                    break;
                case EMissionOrderType.DetectSubmarine:
                    tacticalObjectTargetMode = false;
                    break;
                case EMissionOrderType.RescueVIP:
                    HighlightMarkers(true, EMarkerType.Enemy, mission.PossibleMissionTargets);
                    break;
                case EMissionOrderType.AttackJapan:
                    HighlightMarkers(true, EMarkerType.Enemy, mission.PossibleMissionTargets);
                    //onlyAttackWaypoint = true;
                    break;
                case EMissionOrderType.MidwayAirstrike:
                    HighlightMarkers(true, EMarkerType.Enemy, mission.PossibleMissionTargets);
                    CanAddRetrievalWaypoint = false;
                    break;

            }
        }
        else
        {
            HighlightMarkers(false, EMarkerType.Enemy);
            HighlightMarkers(false, EMarkerType.Friend);
            HighlightMarkers(false, EMarkerType.UO);
        }
    }

    public void HighlightMarkers(bool value, EMarkerType type, List<TacticalEnemyShip> objectsToHighlight = null)
    {
        MarkerHighlightChanged(value, type, objectsToHighlight);
    }

    public void Teleported()
    {
        if (CourseSettingMode)
        {
            ToggleCourseSetup(false);
        }
        RemoveWaypoints();
        mapShip.FillChaseNodes();
    }

    public override bool CanBeDropped(ShipWaypoint waypoint, Vector2 pos)
    {
        bool inside = Math.Abs(pos.x) < RectTransform.rect.xMax && Math.Abs(pos.y) < RectTransform.rect.yMax;
        bool can = HudManager.Instance.HasNo(ETutorialMode.DisableFreeWaypointSet) && SectionRoomManager.Instance.Helm.IsWorking;

        if (!inside || !can || customWaypointPlacement.HasValue)
        {
            return false;
        }

        float minDistSqr = 100f * 100f;
        List<ShipWaypoint> wpList;
        bool airRaid = false;
        int index = airRaidWaypointList.IndexOf(waypoint);
        if (index == -1)
        {
            wpList = waypointList;
        }
        else
        {
            airRaid = true;
            wpList = airRaidWaypointList;
            if ((index == 0 && ReconWaypointInUoRange) || (index == 1 && RecoveryOnCarrier))
            {
                return false;
            }
            float total = Vector2.Distance(mapShip.Rect.anchoredPosition, airRaidWaypointList[0].RectTransform.anchoredPosition);
            for (int i = 1; i < airRaidWaypointList.Count; i++)
            {
                total += Vector2.Distance(airRaidWaypointList[i].RectTransform.anchoredPosition, airRaidWaypointList[i - 1].RectTransform.anchoredPosition);
            }

            float maxDist = tacticManager.GetMissionMaxRange(tacticManager.CurrentMission.OrderType, tacticManager.CurrentMission.UseTorpedoes);
            if (total > maxDist)
            {
                return false;
            }
        }
        int idx = wpList.IndexOf(waypoint);
        RectTransform waypointTrans = waypoint.RectTransform;
        RectTransform secWaypointTrans;
        if (idx == 0)
        {
            secWaypointTrans = mapShip.Rect;
        }
        else
        {
            secWaypointTrans = wpList[idx - 1].transform as RectTransform;
        }
        float dist = (waypointTrans.anchoredPosition - secWaypointTrans.anchoredPosition).sqrMagnitude;
        bool result = (airRaid || !IsLandOnTheWay(secWaypointTrans.anchoredPosition, waypointTrans.anchoredPosition)) && dist > minDistSqr;
        if (result && wpList.Count > ++idx)
        {
            secWaypointTrans = wpList[idx].transform as RectTransform;
            dist = (secWaypointTrans.anchoredPosition - waypointTrans.anchoredPosition).sqrMagnitude;
            return (airRaid || !IsLandOnTheWay(waypointTrans.anchoredPosition, secWaypointTrans.anchoredPosition)) && dist > minDistSqr;
        }
        return result;
    }

    public override void RedrawPotentialTrack(ShipWaypoint waypoint, bool ship, bool ok, bool plan)
    {
        List<ShipWaypoint> wpList;
        Sprite sprite1;
        Sprite sprite2;
        if (ship)
        {
            wpList = waypointList;
            sprite1 = sprite2 = road;
        }
        else
        {
            wpList = airRaidWaypointList;
            sprite1 = planeRoad1;
            sprite2 = planeRoad2;
        }
        Color color;
        if (ok)
        {
            color = plan ? planningGreen : green;
        }
        else
        {
            color = red;
        }
        for (int i = 1; i < wpList.Count; i++)
        {
            wpList[i].DestroyAllTracksDots();
            wpList[i].DestroyAllTaskSegments();
            SpawnTrack(wpList[i - 1].GetComponent<RectTransform>().anchoredPosition, wpList[i].GetComponent<RectTransform>().anchoredPosition, wpList[i], sprite1, sprite2, color, !ship);
        }
        if (wpList.Count > 0)
        {
            wpList[0].DestroyAllTracks();
            wpList[0].DestroyAllTracksDots();
            wpList[0].DestroyAllTaskSegments();

            SpawnTrack(mapShip.Rect.anchoredPosition, wpList[0].GetComponent<RectTransform>().anchoredPosition, wpList[0], sprite1, sprite2, color, !ship);
        }

        if (wpList.Count > 1)
        {
            SpawnTrack(wpList[wpList.Count - 2].GetComponent<RectTransform>().anchoredPosition, wpList[wpList.Count - 1].GetComponent<RectTransform>().anchoredPosition, wpList[wpList.Count - 1], sprite1, sprite2, color, !ship);
        }
    }

    public void CancelMissionSetup(bool saveWaypoints = false)
    {
        if (mapMissionSetup != null)
        {
            mapMissionSetup.Hide();
        }
        mouseCircle.gameObject.SetActive(false);
        ClearRaidWaypoints(saveWaypoints ? TacticManager.Instance.CurrentMission : null);
        ChangeAirRaidMode(false);
        TacticManager.Instance.CurrentMission = null;
    }

    public void TurnOffMap()
    {
        //ToggleCourseSetup();
        HudManager.Instance.ForceSetTacticMap(false);
        SetPreview(false);
    }

    public void CompleteObjective(Transform objective, int chapterIndex = 0)
    {
        //SaveManager.Instance.Data.chaptersList[chapterIndex].Remove(objective.GetComponent<IntermissionMissionBtn>());
    }

    public void EnableCourse()
    {
        customWaypointPlacement = null;
        customWaypointPlacement2 = null;
    }

    public void EnableCourse(Vector2 pos, Vector2? pos2)
    {
        customWaypointPlacement = pos;
        customWaypointPlacement2 = pos2;
    }

    public void SpawnTrack(Vector2 vFrom, Vector2 vTo, MapWaypoint wp, RectTransform parent)
    {
        wp.SpawnTrack(vFrom, vTo, roadPrefab, parent, planeRoad1, planeRoad2, green, true);
    }

    public bool CustomWaypointNear(Vector2 waypoint, Vector2 customPosition)
    {
        return Vector2.SqrMagnitude(waypoint - customPosition) < customWaypointsSnapDistanceSqr;
    }

    public override Vector2 GetEndPointFromMousePosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out Vector2 endPoint);
        return endPoint;
    }

    protected override bool CanAddWaypointHere(Vector2 from, Vector2 to)
    {
        if (base.CanAddWaypointHere(from, to))
        {
            if (!customWaypointPlacement.HasValue)
            {
                return true;
            }
            if ((waypointList.Count == 0 || (waypointList.Count == 1 && !CustomWaypointNear(waypointList[0].RectTransform.anchoredPosition, customWaypointPlacement.Value)) && CustomWaypointNear(to, customWaypointPlacement.Value)))
            {
                return CustomWaypointNear(to, customWaypointPlacement.Value);
            }
            return customWaypointPlacement2.HasValue && CustomWaypointNear(to, customWaypointPlacement2.Value);
        }
        return false;
    }

    protected override bool CanRedrawRoadTracks(ShipWaypoint waypoint, int index)
    {
        return base.CanRedrawRoadTracks(waypoint, index) && (!customWaypointPlacement.HasValue || (index == 0 && CustomWaypointNear(waypoint.RectTransform.anchoredPosition, customWaypointPlacement.Value)) ||
            (customWaypointPlacement2.HasValue && CustomWaypointNear(waypoint.RectTransform.anchoredPosition, customWaypointPlacement2.Value)));
    }

    private void GetAllEnemies(ref List<TacticalEnemyShip> enemies, TacticalMission mission)
    {
        enemies.Clear();
        if (mission.PossibleMissionTargets.Count > 0)
        {
            enemies.AddRange(mission.PossibleMissionTargets);
        }
        else
        {
            foreach (var enemy in TacticManager.GetAllShips())
            {
                if (!enemy.HadGreaterInvisibility && !enemy.IsDisabled && !enemy.Dead && enemy.Side == ETacticalObjectSide.Enemy)
                {
                    enemies.Add(enemy);
                }
            }
        }
    }

    private void GetEnemyBasesList(ref List<TacticalEnemyShip> enemies, TacticalMission mission)
    {
        enemies.Clear();
        if (mission.PossibleMissionTargets.Count > 0)
        {
            enemies.AddRange(mission.PossibleMissionTargets);
        }
        else
        {
            foreach (var enemy in TacticManager.GetAllShips())
            {
                if (!enemy.HadGreaterInvisibility && !enemy.IsDisabled && !enemy.Dead && enemy.Side == ETacticalObjectSide.Enemy && enemy.Type == ETacticalObjectType.Outpost)
                {
                    enemies.Add(enemy);
                }
            }
        }
    }

    private void RemoveIdentifiedEnemies(ref List<TacticalEnemyShip> enemies, TacticalMission mission)
    {
        tempEnemies.Clear();
        foreach (var enemy in enemies)
        {
            bool remove = true;
            foreach (var block in enemy.Blocks)
            {
                if (!block.Visible)
                {
                    remove = false;
                }
            }
            if (remove)
            {
                tempEnemies.Add(enemy);
            }
        }
        foreach (var enemy in tempEnemies)
        {
            enemies.Remove(enemy);
        }
        tempEnemies.Clear();
    }

    private void AddWaypoint(List<ShipWaypoint> wpList, bool isAnimated, bool special, bool uo, Sprite sprite1, Sprite sprite2)
    {
        AddWaypoint(wpList, GetEndPointFromMousePosition(), isAnimated, special, uo, sprite1, sprite2);
    }

    private void AddWaypoint(List<ShipWaypoint> wpList, Vector2 endPos, bool isAnimated, bool special, bool uo, Sprite sprite1, Sprite sprite2)
    {
        if (!CanAddWaypoint(wpList, out var startPos, ref endPos, special))
        {
            return;
        }

        CreateWaypoint(wpList, startPos, endPos, isAnimated, uo, sprite1, sprite2);

        if (!AirRaidMode && wpList.Count > 2)
        {
            ToggleCourseSetup(false);
        }
    }

    private bool CanAddWaypoint(List<ShipWaypoint> wpList, out Vector2 startPos, ref Vector2 endPos, bool special)
    {
        startPos = GetStartPoint(wpList);
        if (wpList.Count >= (AirRaidMode ? 2 : 3))
        {
            return false;
        }

        if (customWaypointPlacement.HasValue)
        {
            bool can = false;
            if ((wpList.Count == 0 || (wpList.Count == 1 && !CustomWaypointNear(wpList[0].RectTransform.anchoredPosition, customWaypointPlacement.Value))) && CustomWaypointNear(endPos, customWaypointPlacement.Value))
            {
                can = true;
                endPos = customWaypointPlacement.Value;
            }
            if (customWaypointPlacement2.HasValue && CustomWaypointNear(endPos, customWaypointPlacement2.Value))
            {
                can = true;
                endPos = customWaypointPlacement2.Value;
            }
            if (!can)
            {
                return false;
            }
        }
        endPos *= (resizer.Scale > 1f ? resizer.Scale : 1f);
        float dist = Vector2.Distance(startPos, endPos);
        var demo = DemoMissionGame.Instance;
        if (dist < minimumWaypointDistance || (special && Vector2.SqrMagnitude(endPos - demo.EnableWaypointArea.anchoredPosition) > demo.MaxDistWaypointSqr))
        {
            return false;
        }
        if (AirRaidMode)
        {
            float maxDist = tacticManager.GetMissionMaxRange(tacticManager.CurrentMission.OrderType, tacticManager.CurrentMission.UseTorpedoes);
            if (wpList.Count == 0)
            {
                if (dist > maxDist || (ReconWaypointInUoRange && !tacticManager.IsUoInReconRange(endPos)))
                {
                    return false;
                }
            }
            else if (wpList.Count == 1)
            {
                if (RecoveryOnCarrier && !shipClicked)
                {
                    return false;
                }
                float dist2 = Vector2.Distance(TacticManager.Instance.Carrier.Rect.anchoredPosition, startPos);
                if ((dist + dist2) > maxDist)
                {
                    return false;
                }
            }
        }
        else if (tempPathNodes == null)
        {
            return false;
        }
        return true;
    }

    private void CreateWaypoint(List<ShipWaypoint> wpList, Vector2 startPos, Vector2 endPos, bool isAnimated, bool uo, Sprite sprite1, Sprite sprite2)
    {
        ShipWaypoint spawnPrefab;
        bool planeWaypoint = true;
        if (!AirRaidMode)
        {
            spawnPrefab = newWaypointPrefab;
            planeWaypoint = false;
        }
        else if (wpList.Count != 0)
        {
            spawnPrefab = aircraftAttackReturnPointPrefab;
        }
        else
        {
            spawnPrefab = TacticalMission.AirstrikeMissions.Contains(TacticManager.OrderType) ? aircraftAttackPointPrefab : aircraftReconPointerPrefab;
            tacticalObjectTargetMode = false;
            mapShip.RaycastTarget.raycastTarget = true;
        }

        var wp = CreateWaypoint(spawnPrefab, endPos, startPos, wpList, uo, sprite1, sprite2, green, planeWaypoint);

        WaypointAdded();
        WaypointPositionAdded(endPos);
    }

    private void CheckMaxRange()
    {
        if (AirRaidMode)
        {
            float maxDist = tacticManager.GetMissionMaxRange(tacticManager.CurrentMission.OrderType, tacticManager.CurrentMission.UseTorpedoes);
            mapShip.RangeCircle.sizeDelta = new Vector2(maxDist, maxDist);
            mapShip.RangeCircle.gameObject.SetActive(true);
        }
        else
        {
            mapShip.RangeCircle.gameObject.SetActive(false);
        }
    }

    private void DestroyAirRaidWaypoints()
    {
        while (airRaidWaypointList.Count > 0)
        {
            airRaidWaypointList.PickLastElement().DestroyGameObjectWithAllTracks();
        }
    }

    private void DrawRange(bool retrievalWaypoint)
    {
        mouseCircle.gameObject.SetActive(true);
        float startRange = tacticManager.CurrentMission.OrderType == EMissionOrderType.DetectSubmarine ? Parameters.Instance.DetectSubmarineRange : tacticManager.StartRange;
        if (TacticalMission.SwitchPlaneTypeMissions.Contains(tacticManager.CurrentMission.OrderType) && tacticManager.CurrentMission.UseTorpedoes)
        {
            startRange *= 1.25f;
        }
        var range = retrievalWaypoint ? tacticManager.RetrievalRange : startRange;
        mouseCircle.sizeDelta = new Vector2(range * 2f, range * 2f);
        var locMan = LocalizationManager.Instance;
        tipText.text = retrievalWaypoint ? locMan.GetText("RetrievalPoint") : locMan.GetText("TargetWaypoint");
    }

    private void ChangeCourseSet(bool change)
    {
        CourseSettingMode = change;
        CheckMaxRange();

        TacticManager.Instance.Markers.SetInteractable(!change);
        ObjectivesManager.Instance.SetInteractable(!change);
        mapShip.RaycastTarget.raycastTarget = !change;
        CourseSettingModeChanged();
    }

    private bool DrawPotentialTrack(List<ShipWaypoint> wpList, bool inside, bool special, Sprite roadSprite1, Sprite roadSprite2, bool isPlaneTrack)
    {
        GetPoints(wpList, out Vector2 screenPointA, out Vector2 screenPointB);

        var color = planningGreen;

        var demo = DemoMissionGame.Instance;
        inside = inside && (!special || Vector2.SqrMagnitude(screenPointB - demo.EnableWaypointArea.anchoredPosition) < demo.MaxDistWaypointSqr);

        float dist = Vector2.Distance(screenPointA, screenPointB);

        float normalDist = -1f;
        bool result = true;
        if (!inside || dist < minimumWaypointDistance || (!AirRaidMode && IsLandOnTheWay(screenPointA, screenPointB)))
        {
            color = red;
            result = false;
        }
        else if (AirRaidMode)
        {
            float maxDist = tacticManager.GetMissionMaxRange(tacticManager.CurrentMission.OrderType, tacticManager.CurrentMission.UseTorpedoes);
            if (wpList.Count == 0)
            {
                maxDist -= 50f;
                if (dist > maxDist)
                {
                    normalDist = maxDist;
                    result = false;
                }
            }
            else if (wpList.Count == 1)
            {
                float dist2 = Vector2.Distance(TacticManager.Instance.Carrier.Rect.anchoredPosition, screenPointA);
                if ((dist + dist2) > maxDist)
                {
                    normalDist = Mathf.Max(0f, maxDist - dist2);
                    result = false;
                }
            }
        }

        DrawPotentialTrack(screenPointA, screenPointB, roadSprite1, roadSprite2, dist, false, normalDist, color, isPlaneTrack);
        return result;
    }

    private Vector2 GetStartPoint(List<ShipWaypoint> wpList)
    {
        return wpList.Count > 0 ? wpList[wpList.Count - 1].RectTransform.anchoredPosition : mapShip.Rect.anchoredPosition;
    }

    private void GetPoints(List<ShipWaypoint> wpList, out Vector2 screenPointA, out Vector2 screenPointB)
    {
        screenPointA = GetStartPoint(wpList);
        screenPointB = GetEndPointFromMousePosition() * (resizer.Scale > 1f ? resizer.Scale : 1f);
    }

    private void InitEnemyVisibility()
    {
        foreach (Transform t in enemyMarkers.transform)
        {
            enemyList.Add(t.GetComponent<MapMarker>());
        }
        TimeManager.Instance.Invoke(RefreshEnemyVisibility, 1);
        percentPerEnemy = Mathf.Floor((1f / enemyList.Count) * 100f) / 100f;
    }

    private void InitCloudVisibility()
    {
        foreach (Transform t in cloudMarkers.transform)
        {
            cloudList.Add(t.GetComponent<MapMarker>());
        }
        TimeManager.Instance.Invoke(RefreshCloudVisibility, 1);
        percentPerCloud = Mathf.Floor((1f / cloudList.Count) * 100f) / 100f;
    }

    private void RefreshEnemyVisibility()
    {
        int enemiesVisible = Mathf.FloorToInt(enemyKnowledge / percentPerEnemy);
        foreach (MapMarker enemy in enemyList)
        {
            enemy.ToggleVisibility(false);
        }
        for (int i = 0; i < enemiesVisible; i++)
        {
            enemyList[i].ToggleVisibility(true);
        }
        TimeManager.Instance.Invoke(RefreshEnemyVisibility, 1);
    }

    private void RefreshCloudVisibility()
    {
        weatherKnowledge += MapMarkersManager.Instance.WeatherKnowledgeGainSpeed;
        int cloudsVisible = Mathf.FloorToInt(weatherKnowledge / percentPerCloud);
        foreach (MapMarker cloud in cloudList)
        {
            cloud.ToggleVisibility(false);
        }
        for (int i = 0; i < cloudsVisible; i++)
        {
            cloudList[i].ToggleVisibility(true);
        }
        TimeManager.Instance.Invoke(RefreshCloudVisibility, 1);
    }

    private void DestroyHoveredWaypoint()
    {
        if (SectionRoomManager.Instance.Helm.IsWorking && !AirRaidMode && waypointList.Count > 0)
        {
            var waypoint = waypointList[waypointList.Count - 1];
            if (waypoint.Hovered)
            {
                waypoint.DestroyGameObjectWithAllTracks();
                waypointList.RemoveAt(waypointList.Count - 1);
                if (waypointList.Count == 0)
                {
                    TacticManager.Instance.Carrier.StopShip();
                }
            }
        }
    }

    private ShipWaypoint CreateWaypoint(ShipWaypoint prefab, Vector2 position, Vector2 from, List<ShipWaypoint> wpList, bool uo, Sprite sprite1, Sprite sprite2, Color color, bool isPlaneTrack)
    {
        ShipWaypoint wp = Instantiate(prefab, Vector3.zero, Quaternion.identity, waypointTargetsParent).GetComponent<ShipWaypoint>();
        var trans = wp.GetComponent<RectTransform>();
        trans.anchoredPosition = position;
        trans.localScale = Vector3.one;
        if (AirRaidMode)
        {
            SpawnTrack(from, position, wp, sprite1, sprite2, color, isPlaneTrack);
        }
        else
        {
            SpawnRoadTrack(tempPathNodes, wp, roadPrefab, trackParent, sprite1);
        }
        wp.SetMap(this);
        if (AirRaidMode)
        {
            if (wpList.Count < 1)
            {
                tacticManager.StartPosition = mapShip.Rect.anchoredPosition;
                tacticManager.AttackPosition = position;

                var param = Parameters.Instance;
                float startRange = tacticManager.CurrentMission.OrderType == EMissionOrderType.DetectSubmarine ? param.DetectSubmarineRange : tacticManager.StartRange;
                float mult = 1f;
                if (TacticalMission.SwitchPlaneTypeMissions.Contains(tacticManager.CurrentMission.OrderType) && tacticManager.CurrentMission.UseTorpedoes)
                {
                    mult += .25f;
                }
                switch (tacticManager.CurrentMission.OrderType)
                {
                    case EMissionOrderType.IdentifyTargets:
                        if (uo)
                        {
                            mult -= param.IdentifyUORangeModifier;
                        }
                        break;
                    case EMissionOrderType.MagicIdentify:
                        mult = 0f;
                        break;
                }
                mult = Mathf.Clamp(mult, 0f, 10f);
                startRange *= mult;
                wp.DrawRadius(startRange);

                if (onlyAttackWaypoint)
                {
                    mapMissionSetup.SetInteractable(true);
                    tacticManager.ReturnPosition = position;
                }
            }
            else
            {
                tacticManager.ReturnPosition = position;
                wp.DrawRadius(tacticManager.RetrievalRange);
                mapMissionSetup.SetInteractable(true);
                mapShip.RangeCircle.gameObject.SetActive(false);
            }
            TacticManager.Instance.RecalculateDistances();

            //wp.SetTaskPoint(wp.transform.position, GetLastWaypointPosition(wpList), airRaidTaskPrefab);
        }
        else
        {
            //#warning todo dots?
            //wp.SetSegment(trans.anchoredPosition, GetLastWaypointPosition(wpList), dotPrefab);
        }
        wpList.Add(wp);

        if (AirRaidMode)
        {
            bool finished = airRaidWaypointList.Count == 2;
            foreach (var waypoint in airRaidWaypointList)
            {
                (waypoint as MissionWaypoint).SetRaycastTarget(finished);
            }
            if (finished)
            {
                MissionAreasSet();
            }
        }

        return wp;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TacticalMapShip : MapShip, IPointerClickHandler
{
    public event Action ChaseNodesChanged = delegate { };

    public Image RaycastTarget => raycastTarget;

    public float MilesTravelled
    {
        get;
        private set;
    }

    public float CurrentRangeSqr
    {
        get;
        private set;
    }

    public List<Vector2> ChaseNodes
    {
        get;
        private set;
    }

    public HashSet<int> FreeChaseNodes
    {
        get;
        private set;
    }

    public Button Button = null;

    public List<ListInt> IntermissionBonusRanges;
    public float BaseIslandBonus = 2f;
    public float BaseAircraftSpottingBonus = 150f;
    public float BaseSubmarineBonus = 50f;

    [Header("Refs")]
    public RectTransform RangeCircle;
    [SerializeField]
    private TacticalMap tacticalMap = null;
    [SerializeField]
    private RectTransform sightCircle = null;

    [Space(20)]
    [SerializeField]
    private RectTransform enemyMarkers = null;
    [SerializeField]
    private RectTransform weatherMarkers = null;

    [SerializeField]
    private float sqrDistToTrigger = 2500f;
    [SerializeField]
    private List<RectTransform> mapTriggers = null;
    [SerializeField]
    private float triggerRange = 30f;

    [SerializeField]
    private float baseRange = 100f;

    [SerializeField]
    private float distToMile = 2.1f;

    [SerializeField]
    private int minChaseDistance = 35;
    [SerializeField]
    private int maxChaseDistance = 85;

    [SerializeField]
    private Image raycastTarget = null;

    private float intermissionBonusRange;

    private List<RectTransform> enemyMarkerList = new List<RectTransform>();
    private List<RectTransform> weatherMarkerList = new List<RectTransform>();
    private float currentAircraftSpotting;
    private float currentEscortSpottingBonus;
    private float currentEscortPassiveSpottingBonus;
    private float currentSubmarineSpotting;
    private float currentIslandBonus = 1f;

    private RectTransform activeTrigger = null;
    private Action triggerListener = null;

    private Vector2 circleSize;

    private Dictionary<Objective, TriggerData> triggers;
    private Dictionary<Objective, TriggerData> fired;

    private float triggerRangeSqr;

    private float distTravelled;

    private List<int> allToChase;
    private List<int> chaseToAll;
#if ALLOW_CHEATS
    private float bonusRange;
#endif
    public void Init()
    {
        rectTransform = GetComponent<RectTransform>();
        waypointMap = tacticalMap;

        triggerRangeSqr = triggerRange * triggerRange;
        triggers = new Dictionary<Objective, TriggerData>();
        fired = new Dictionary<Objective, TriggerData>();
        SetDestinationWaypoint();
        TimeManager.Instance.Invoke(MoveTick, 1);
        MapUnitMaskManager.Instance.AddUnitObject(gameObject, 10);
        circleSize = sightCircle.sizeDelta;

        ChaseNodes = new List<Vector2>();
        allToChase = new List<int>();
        chaseToAll = new List<int>();

        int count = (2 * maxChaseDistance + 1);
        count *= count;
        for (int i = 0; i < count; i++)
        {
            allToChase.Add(-1);
        }

        FreeChaseNodes = new HashSet<int>();

        TacticManager.Instance.NodesChanged += FillChaseNodes;
    }

    public void InitMarkers()
    {
        foreach (RectTransform t in enemyMarkers.transform)
        {
            enemyMarkerList.Add(t);
        }
        foreach (RectTransform w in weatherMarkers.transform)
        {
            weatherMarkerList.Add(w);
        }
    }

    public void Setup()
    {
        SetIntermissionBonus();
    }

    public void LoadData(ref ShipSaveData data)
    {
        HudManager.Instance.LoadShip(ref data);
        SetPosition(data.ShipPosition);
        (waypointMap as TacticalMap).LoadWaypoints(data.Waypoints);
    }

    public void SaveData(ref ShipSaveData data)
    {
        HudManager.Instance.SaveShip(ref data);
        data.ShipPosition = rectTransform.anchoredPosition;
        (waypointMap as TacticalMap).SaveWaypoints(data.Waypoints);
    }

    public void Tick()
    {
        MoveTick();
    }

    public void SetAircraftSpottingBonus(bool set)
    {
        currentAircraftSpotting = set ? BaseAircraftSpottingBonus : 0f;
        RecalculateShipRange();
    }

    public void SetEscortSpottingBonus(int value)
    {
        currentEscortSpottingBonus += value;
        RecalculateShipRange();
    }

    public void SetEscortPassiveSpottingBonus(int value)
    {
        currentEscortPassiveSpottingBonus = value;
        RecalculateShipRange();
    }

    public void SetSubmaringSpottingBonus(bool set)
    {
        currentSubmarineSpotting = set ? BaseSubmarineBonus : 0f;
        RecalculateShipRange();
    }

    public void SetIslandMultiplierBonus(float value)
    {
        currentIslandBonus = value;
        RecalculateShipRange();
    }

    public void SetPosition(Vector2 pos)
    {
        tacticalMap.RemoveWaypoints();
        rectTransform.anchoredPosition = pos;
        destinationWaypoint = null;
        FillChaseNodes();
    }

    public bool CalculateShipVisibility()
    {
        return TacticalMapClouds.Instance.InClouds(rectTransform);
        //tacticalMap.MapClouds.ForceUpdate();

        //if (cloudsTexture == null)
        //{
        //    cloudsRawImage = tacticalMap.MapClouds.CloudsBright;
        //    cloudsTexture = tacticalMap.MapClouds.CloudsMask;
        //}
        //Rect mapRect = tacticalMap.RectTransform.rect;

        ////	[Optimization] Should this be simplified with the primitive variables?
        //Vector2 normalizedMapPosition = new Vector2 {
        //    x = (rectTransform.rect.x + mapRect.x * 0.5f) / mapRect.width,
        //    y = (rectTransform.rect.y + mapRect.y * 0.5f) / mapRect.height
        //};

        //Vector2Int pixelPosition = new Vector2Int
        //{
        //    x = (int)((normalizedMapPosition.x + cloudsRawImage.uvRect.x) * cloudsTexture.width),
        //    y = (int)((normalizedMapPosition.y + cloudsRawImage.uvRect.y) * cloudsTexture.height)
        //};

        //Color positionColor = cloudsTexture.GetPixel(pixelPosition.x, pixelPosition.y);

        //return positionColor.a > visibilityLimit;
    }

    public void SetTrigger(int id, Action listener)
    {
        activeTrigger = mapTriggers[id];
        triggerListener = listener;
    }

    public void SetupTrigger(Objective data, int id, Action onTriggerReached, float range)
    {
        Assert.IsFalse(triggers.ContainsKey(data));
        Assert.IsNotNull(onTriggerReached);

        triggers[data] = new TriggerData(id, onTriggerReached, range > 0f ? (range * range) : triggerRangeSqr);
    }

    public void SetupTrigger(Objective data, List<int> ids, Action<int> onAnyTriggerReached)
    {
        Assert.IsFalse(triggers.ContainsKey(data));
        Assert.IsNotNull(onAnyTriggerReached);

        triggers[data] = new TriggerData(ids, onAnyTriggerReached, triggerRangeSqr);
    }

    public void DropTrigger(int id)
    {
        if (activeTrigger == mapTriggers[id])
        {
            activeTrigger = null;
            triggerListener = null;
        }
    }

    public void RemoveTriggers(Objective data)
    {
        triggers.Remove(data);
    }

#if ALLOW_CHEATS
    public void Cheat()
    {
        if (bonusRange > 10f)
        {
            Debug.Log("Removed visibility cheat");
            bonusRange = 0f;
        }
        else
        {
            Debug.Log("Visibility cheat");
            bonusRange = 1000f;
        }
        RecalculateShipRange();
    }
#endif

    protected override void MovementTick()
    {
        base.MovementTick();
        if (activeTrigger != null && Vector2.SqrMagnitude(activeTrigger.anchoredPosition - rectTransform.anchoredPosition) < sqrDistToTrigger)
        {
            triggerListener();

            activeTrigger = null;
            triggerListener = null;
        }
    }

    protected override void OnWaypointTargetReached()
    {

    }

    protected override void OnPositionChanged(Vector2 oldPos)
    {
        var tacticMan = TacticManager.Instance;

        FillChaseNodes();

        float dist = Vector2.Distance(rectTransform.anchoredPosition, oldPos);
        distTravelled += dist;
        MilesTravelled += dist;
        for (int i = 0; i < 100; i++)
        {
            if (distTravelled > distToMile)
            {
                distTravelled -= distToMile;
                tacticMan.FireSwimMile();
            }
        }

        bool found;
        do
        {
            found = false;
            foreach (var pair in triggers)
            {
                int i = 0;
                if (fired.TryGetValue(pair.Key, out var data))
                {
                    if (pair.Value == data)
                    {
                        i = data.IdsFired[data.IdsFired.Count - 1] + 1;
                    }
                    else
                    {
                        data.IdsFired.Clear();
                        fired.Remove(pair.Key);
                    }
                }
                for (; i < pair.Value.Triggers.Count; i++)
                {
                    if (Vector2.SqrMagnitude(rectTransform.anchoredPosition - tacticMan.Map.Nodes[pair.Value.Triggers[i]]) <= pair.Value.RangeSqr)
                    {
                        found = true;
                        if (pair.Value.OnAnyTriggerReached == null)
                        {
                            Assert.IsTrue(triggers.Count == 1);
                            pair.Value.OnTriggerReached();
                        }
                        else
                        {
                            pair.Value.OnAnyTriggerReached(i);
                            if (!fired.TryGetValue(pair.Key, out var data2))
                            {
                                data2 = pair.Value;
                                data2.IdsFired.Add(i);
                            }
                        }
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }
        }
        while (found);
        foreach (var value in fired.Values)
        {
            value.IdsFired.Clear();
        }
        fired.Clear();

        var min = tacticMan.MinMapPosition;
        var max = tacticMan.MaxMapPosition;
        var pos = rectTransform.anchoredPosition;
        if (pos.x != Mathf.Clamp(pos.x, min.x, max.x) || pos.y != Mathf.Clamp(pos.y, min.y, max.y))
        {
            tacticMan.FireEdgeMapReached();
            var mapCornerData = tacticMan.MapCornerData;
            if (mapCornerData != null)
            {
                bool right = pos.x > mapCornerData.CornerPositional.x;
                bool top = pos.y > mapCornerData.CornerPositional.y;
                bool can = false;
                switch (mapCornerData.CornerOrientation)
                {
                    case EOrientation.NE:
                        can = top && right;
                        break;
                    case EOrientation.SE:
                        can = !top && right;
                        break;
                    case EOrientation.NW:
                        can = top && !right;
                        break;
                    case EOrientation.SW:
                        can = !top && !right;
                        break;
                }
                if (can)
                {
                    tacticMan.FireMapCornerReached();
                }
            }
        }
    }

    private void SetIntermissionBonus()
    {
        var data = SaveManager.Instance.Data;
        intermissionBonusRange = IntermissionBonusRanges[(int)data.SelectedAircraftCarrier].List[Mathf.Clamp(BinUtils.ExtractData(data.IntermissionData.CarriersUpgrades, 3, (int)data.SelectedAircraftCarrier), 0, IntermissionBonusRanges.Count - 1)];
        RecalculateShipRange();
    }

    private void RecalculateShipRange()
    {
        float island = baseRange * currentIslandBonus;
        CurrentRangeSqr = island + currentAircraftSpotting + currentSubmarineSpotting + currentEscortSpottingBonus + currentEscortPassiveSpottingBonus + intermissionBonusRange;
#if ALLOW_CHEATS
        CurrentRangeSqr += bonusRange;
#endif
        MapUnitMaskManager.Instance.UpdateUnitObject(gameObject, (int)CurrentRangeSqr);
        CurrentRangeSqr *= CurrentRangeSqr;
    }

    public void PositionUpdate()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tacticalMap.OnPointerClick(eventData);
    }

    public int TranslateIndex(int index, bool allToChase)
    {
        return (allToChase ? this.allToChase : chaseToAll)[index];
    }

    public void FillChaseNodes()
    {
        ChaseNodes.Clear();
        chaseToAll.Clear();
        FreeChaseNodes.Clear();

        if (TacticManager.Instance.MapNodes == null)
        {
            return;
        }
        SetNodes(false);
        if (ChaseNodes.Count == 0)
        {
            SetNodes(true);
        }

        ChaseNodesChanged();
    }

    private void SetNodes(bool withLand)
    {
        var pos = rectTransform.anchoredPosition;
        int max = maxChaseDistance * maxChaseDistance;
        int min = minChaseDistance * minChaseDistance;
        var mapNodes = TacticManager.Instance.MapNodes;
        for (int i = -maxChaseDistance; i <= maxChaseDistance; i++)
        {
            int index = i + maxChaseDistance;
            if (Mathf.Abs(i) < minChaseDistance)
            {
                for (int j = 0; j <= 2 * maxChaseDistance; j++)
                {
                    allToChase[index * maxChaseDistance + j] = -1;
                }
                continue;
            }
            for (int j = -maxChaseDistance; j <= maxChaseDistance; j++)
            {
                int index2 = index * maxChaseDistance + j + maxChaseDistance;

                var newPos = pos;
                newPos.x += 2 * i;
                newPos.y += 2 * j;

                int sqr = (i * i + j * j);
                if (sqr < min || sqr > max || Mathf.Abs(newPos.x) > 927f || Mathf.Abs(newPos.y) > 507f || !mapNodes.CanFind(newPos, withLand))
                {
                    allToChase[index2] = -1;
                    continue;
                }

                int newIndex = chaseToAll.Count;
                ChaseNodes.Add(newPos);
                allToChase[index2] = newIndex;
                chaseToAll.Add(index2);
                Assert.IsTrue(ChaseNodes.Count == (newIndex + 1) && chaseToAll.Count == (newIndex + 1) && chaseToAll[newIndex] == index2 && allToChase[index2] == newIndex);
                FreeChaseNodes.Add(newIndex);
            }
        }
    }
}

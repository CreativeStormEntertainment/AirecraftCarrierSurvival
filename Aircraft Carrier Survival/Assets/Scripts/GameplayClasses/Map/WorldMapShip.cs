using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapShip : MapShip
{
    public override float ShipSpeedScaled => shipSpeed * shipSpeeds[(int)SaveManager.Instance.Data.SelectedAircraftCarrier];

    public int FieldOfView => fieldsOfView[(int)SaveManager.Instance.Data.SelectedAircraftCarrier];
    public int FieldOfViewSqr
    {
        get;
        private set;
    }

    public override bool IsShipBlocked => blockedTicks > 0;

    public TerritoryNode CurrentNode
    {
        get;
        set;
    }

    [Header("Refs")]
    [SerializeField]
    private WorldMap worldMap = null;
    [SerializeField]
    private List<int> fieldsOfView = null; //ECarrierType order
    [SerializeField]
    private List<float> shipSpeeds = null; //ECarrierType order
    [Header("Sounds")]
    [SerializeField]
    private StudioEventEmitter sailingStart = null;
    [SerializeField]
    private StudioEventEmitter sailingLoop = null;
    [SerializeField]
    private StudioEventEmitter sailingEnd = null;

    private int blockedTicks;
    private float visualTimer;
    private bool wasMoving;

    public void Start()
    {
        SetDestinationWaypoint();
        TimeManager.Instance.Invoke(MoveTick, 1);
    }

    public void Update()
    {
        visualTimer += Time.deltaTime;
        float percent = visualTimer / TimeManager.Instance.TickTime;
        rectTransform.anchoredPosition = Vector2.Lerp(lastTickPos, currentTickPos, percent);
        bool moving = IsMoving;
        if (moving && !wasMoving)
        {
            sailingStart.Play();
            sailingLoop.enabled = true;
        }
        if (!moving && wasMoving)
        {
            sailingEnd.Play();
            sailingLoop.enabled = false;
        }
        wasMoving = moving;
    }

    public void Init()
    {
        rectTransform = GetComponent<RectTransform>();
        waypointMap = worldMap;
        FieldOfViewSqr = FieldOfView * FieldOfView;
    }

    public void Tick()
    {
        MoveTick();
        visualTimer = 0f;
        if (blockedTicks > 0)
        {
            blockedTicks -= TimeManager.Instance.WorldMapTickQuotient;
            if (blockedTicks <= 0)
            {

            }
        }
    }

    public void SetAnchoredPosition(Vector2 anchoredPosition)
    {
        Teleport(anchoredPosition);
        SetClosestNode();
    }

    public void SetClosestNode()
    {
        CurrentNode = SandboxManager.Instance.SandboxTerritoryManager.GetClosestNodeToShip();
    }

    public void SetBlockedTime(int hours)
    {
        blockedTicks = hours * TimeManager.Instance.TicksForHour;
    }

    protected override void OnPositionChanged(Vector2 oldPos)
    {
        base.OnPositionChanged(oldPos);
        SetClosestNode();
    }

    protected override void OnWaypointTargetReached()
    {
        //if (worldMap.WaypointListCount == 1)
        //{
        //    //var poi = SandboxManager.Instance.PoiManager.GetPoiInRange(rectTransform.anchoredPosition, snapDistanceSqr);
        //    //if (poi != null)
        //    //{
        //    //    poi.OnClick();
        //    //}
        //    var hudMan = HudManager.Instance;
        //    hudMan.UnsetBlockSpeed();
        //    hudMan.SetBlockSpeed(0);
        //}
    }
}

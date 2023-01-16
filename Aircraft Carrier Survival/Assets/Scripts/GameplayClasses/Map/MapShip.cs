using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapShip : MonoBehaviour
{
    public Action ShipPositionChanged = delegate { };
    public virtual float ShipSpeedScaled => shipSpeed * AirCraftCarrier.Instance.MovementSpeedEfficiency * HudManager.Instance.ShipSpeedup;
    public virtual bool IsShipBlocked => false;
    public Vector2 Position => rectTransform.anchoredPosition;

    [Header("BASE Params")]
    [SerializeField]
    protected float shipSpeed = 1f;
    [SerializeField]
    private RectTransform container = null;
    [SerializeField]
    protected RectTransform rectTransform = null;


    protected WaypointMap waypointMap = null;

    protected ShipWaypoint destinationWaypoint = default;
    protected Vector2 destinatedDirection = default;

    private bool isShipStopped = false;

    public RectTransform Rect => rectTransform;

    public bool IsMoving => destinationWaypoint != null && !isShipStopped && ShipSpeedScaled > 0f;
    public bool HasWaypoint => waypointMap.IsThereNextWaypoint();

    protected float startTime;
    protected float destinationReachTime;
    protected float lastSpeed;
    protected Vector2 startPosition;

    protected Vector2 lastTickPos;
    protected Vector2 currentTickPos;
    private bool wasStopped;

    public void RefreshPath()
    {
        SetDestinationWaypoint();
        if (destinationWaypoint != null && destinationWaypoint.Path != null && destinationWaypoint.Path.Count > 1)
        {
            destinationWaypoint.CurrentPathNode = 1;
        }
        SetTimeToReachDestination();
    }

    public void SetDestinationWaypoint()
    {
        if (waypointMap.IsThereNextWaypoint())
        {
            destinationWaypoint = waypointMap.GetFirstWaypoint();
        }
        else
        {
            destinationWaypoint = null;
        }
    }

    public void TeleportToDestination()
    {
        if (destinationWaypoint != null)
        {
            rectTransform.anchoredPosition = destinationWaypoint.RectTransform.anchoredPosition;
            waypointMap.RefreshCurrentTrack(rectTransform.anchoredPosition);
            OnWaypointTargetReached();
            SetNextDestinationWaypoint();
        }
    }

    public void StopShip()
    {
        destinationWaypoint = null;
    }

    public void Teleport(Vector2 pos)
    {
        rectTransform.anchoredPosition = pos;
        lastTickPos = rectTransform.anchoredPosition;
        currentTickPos = rectTransform.anchoredPosition;
        TacticalMap.Instance.Teleported();
    }

    protected void MoveTick()
    {
        MovementTick();
    }

    protected virtual void MovementTick()
    {
        isShipStopped = !ResourceManager.Instance.CanMoveShip() || IsShipBlocked;
        lastTickPos = rectTransform.anchoredPosition;
        if (destinationWaypoint == null)
        {
            SetDestinationWaypoint();
            if (destinationWaypoint == null)
            {
                return;
            }
            else
            {
                SetNextPathNode();
            }
        }
        MoveShip();
        currentTickPos = rectTransform.anchoredPosition;
        //if (movementRoutine == null && !isShipStopped)
        //{
        //    movementRoutine = MapMarkersManager.Instance.StartCoroutine(MovementRoutine());
        //}
    }

    protected virtual void OnWaypointTargetReached()
    {

    }

    protected virtual void OnPositionChanged(Vector2 oldPos)
    {

    }

    private void MoveShip()
    {
        if (destinationWaypoint == null || isShipStopped)
        {
            wasStopped = true;
            return;
        }
        if (wasStopped)
        {
            SetTimeToReachDestination();
            if (destinationWaypoint == null)
            {
                return;
            }
        }
        if (ShipSpeedScaled != lastSpeed)
        {
            SetTimeToReachDestination();
        }
        wasStopped = false;
        lastSpeed = ShipSpeedScaled;
        float percent = destinationReachTime > 0f ? (Time.time - startTime) / destinationReachTime : 1f;
        rectTransform.anchoredPosition = Vector2.Lerp(startPosition, destinationWaypoint.Path[destinationWaypoint.CurrentPathNode].Position, percent);
        OnPositionChanged(rectTransform.anchoredPosition);
        ShipPositionChanged();
        waypointMap.RefreshCurrentTrack(rectTransform.anchoredPosition);
        if (percent >= 1)
        {
            rectTransform.anchoredPosition = destinationWaypoint.Path[destinationWaypoint.CurrentPathNode].Position;
            SetNextPathNode();
        }
    }

    private void SetNextPathNode()
    {
        bool lastNode = destinationWaypoint.SetNextPathNode();
        if (lastNode)
        {
            OnWaypointTargetReached();
            SetNextDestinationWaypoint();
            if (destinationWaypoint != null)
            {
                SetNextPathNode();
            }
        }
        else
        {
            SetTimeToReachDestination();
        }
    }

    private void SetTimeToReachDestination()
    {
        if (destinationWaypoint != null)
        {
            startPosition = rectTransform.anchoredPosition;
            startTime = Time.time;
            destinationReachTime = (destinationWaypoint.Path[destinationWaypoint.CurrentPathNode].Position - rectTransform.anchoredPosition).magnitude / ShipSpeedScaled;
            SetRotation((destinationWaypoint.Path[destinationWaypoint.CurrentPathNode].Position - rectTransform.anchoredPosition).normalized);
        }
    }

    private void SetRotation(Vector2 direction)
    {
        waypointMap.TacticManager.ShipDirection = direction;
        container.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, direction));
    }

    private void SetNextDestinationWaypoint()
    {
        ShipWaypoint nextWaypoint = waypointMap.GetNextWaypoint();
        if (nextWaypoint != null)
        {
            SetDestinationWaypoint();
        }
        else
        {
            destinationWaypoint = null;
        }
    }

}

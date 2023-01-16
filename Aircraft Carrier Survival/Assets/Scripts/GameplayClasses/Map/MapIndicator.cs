using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MapIndicator : MonoBehaviour
{
    [SerializeField]
    private EObjectiveState state = EObjectiveState.IN_PROGRESS;

    private int currentObjective = 0;
    public int CurrontObjective { get => currentObjective; }
    public Vector2 AnchoredPosition { get => GetComponent<RectTransform>().anchoredPosition; }

    public void SetState(EObjectiveState objectiveState)
    {
        state = objectiveState;
    }

    public void FireShipArrivedEvent()
    {
        StopShip();
    }

    private void ReleaseShip()
    {
        AirCraftCarrier.Instance.MovementSpeed = 1;
    }

    private void StopShip()
    {
        AirCraftCarrier.Instance.MovementSpeed = 0;
    }
}

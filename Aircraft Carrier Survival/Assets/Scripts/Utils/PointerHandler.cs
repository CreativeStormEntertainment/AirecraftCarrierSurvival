using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerHandler
{
    public event Action Clicked = delegate { };
    public event Action DoubleClicked = delegate { };
    public event Action<Vector2, PointerEventData> Dragged = delegate { };
    public event Action DragFinished = delegate { };

    public EPointerState State
    {
        get;
        private set;
    }

    private readonly PointerEventData.InputButton pointer;
    private readonly float dragScaleFactor;

    private readonly float maxTapTime;
    private readonly float doubleClickThreshold;
    private readonly float minDragDistanceSqr;

    private float tapTime;
    private float doubleClickTime;
    private float doubleClickTime2;
    private Vector2 distance;
    private bool wasDragging;

    private int pressCount;

    public PointerHandler(PointerEventData.InputButton pointer, float dragScaleFactor, float maxTapTime, float doubleClickThreshold, float minDragDistance)
    {
        this.pointer = pointer;
        this.dragScaleFactor = dragScaleFactor;
        this.maxTapTime = maxTapTime;
        this.doubleClickThreshold = doubleClickThreshold;
        
        minDragDistanceSqr = minDragDistance * minDragDistance;
    }

    public void Press(PointerEventData.InputButton button)
    {
        if (button != pointer)
        {
            return;
        }
        State = EPointerState.Pressed;
        tapTime = maxTapTime;
        if (doubleClickTime > 0f)
        {
            doubleClickTime2 = doubleClickThreshold;
        }
        else
        {
            doubleClickTime = doubleClickThreshold;
            doubleClickTime2 = 0f;
        }
        distance = Vector2.zero;
    }

    public void Release()
    {
        Release(pointer);
    }

    public void Release(PointerEventData.InputButton button)
    {
        if (button != pointer)
        {
            return;
        }
        if (State == EPointerState.Pressed)
        {
            State = EPointerState.SinglePressed;
            if (++pressCount == 2)
            {
                ClearDoubleClick();
                State = EPointerState.None;

                DoubleClicked();
            }
        }
        else
        {
            pressCount = 0;
        }
        if (State == EPointerState.SinglePressed || State == EPointerState.Hold)
        {
            State = EPointerState.None;

            Clicked();
        }
        else
        {
            if (State == EPointerState.Drag)
            {
                if (wasDragging)
                {
                    State = EPointerState.None;
                    DragFinished();
                }
                else
                {
                    wasDragging = true;
                }
            }
            else
            {
                State = EPointerState.None;
            }
        }
    }

    public void Drag(PointerEventData data)
    {
        if (data.button != pointer)
        {
            return;
        }
        if (State == EPointerState.Drag)
        {
            Dragged(data.delta / dragScaleFactor, data);
        }
        else if (State != EPointerState.None)
        {
            distance += data.delta / dragScaleFactor;
            if (distance.sqrMagnitude >= minDragDistanceSqr)
            {
                pressCount = 0;
                State = EPointerState.Drag;

                Dragged(distance, data);
            }
        }
    }

    public void ClearDoubleClick()
    {
        doubleClickTime = 0f;
        doubleClickTime2 = 0f;
        pressCount = 0;
    }

    public void Update()
    {
        if (State == EPointerState.Drag && !Input.GetMouseButton((int)pointer))
        {
            Release();
        }
        if (wasDragging)
        {
            State = EPointerState.None;
            DragFinished();
            wasDragging = false;
        }
        if (State == EPointerState.None || State == EPointerState.Pressed)
        {
            doubleClickTime -= Time.deltaTime;
            doubleClickTime2 -= Time.deltaTime;
            if (doubleClickTime < 0f)
            {
                pressCount = 0;
                if (State == EPointerState.Pressed)
                {
                    if (doubleClickTime2 > 0f)
                    {
                        doubleClickTime = doubleClickTime2;
                    }
                    else
                    {
                        State = EPointerState.SinglePressed;
                    }
                }
            }
        }
        if (State == EPointerState.None || State == EPointerState.Pressed || State == EPointerState.SinglePressed)
        {
            tapTime -= Time.deltaTime;
            if (tapTime < 0f)
            {
                State = EPointerState.Hold;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDrag<T1, T2> : MonoBehaviour, IInventoryItem<T1>, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
    where T1 : class, IInventoryDragData
    where T2 : InventoryDrag<T1, T2>
{
    public event Action<T1> Dropped = delegate { };
    public event Action<T2> Clicked = delegate { };

    public virtual bool CanBeDragged
    {
        get;
    } = true;

    public bool ActiveInventory
    {
        get;
        private set;
    }

    public T1 Data
    {
        get;
        private set;
    }

    public Canvas Canvas
    {
        get;
        private set;
    }

    public RectTransform Root
    {
        get;
        private set;
    }

    protected PointerHandler leftHandler;
    protected PointerHandler rightHandler;

    [SerializeField]
    private RectTransform container = null;
    [SerializeField]
    private float maxTapTime = 0.2f;
    [SerializeField]
    private float doubleClickThreshold = 0.5f;
    [SerializeField]
    private float minDragDistance = 30f;

    private RectTransform trans;

    private void Update()
    {
        if (leftHandler != null)
        {
            leftHandler.Update();
            rightHandler.Update();
        }
    }

    public virtual void OnEnable()
    {
        if (leftHandler != null)
        {
            leftHandler.ClearDoubleClick();
            rightHandler.ClearDoubleClick();
        }
    }

    public void Setup(RectTransform root, Canvas canvas)
    {
        Root = root;
        Canvas = canvas;

        Data = null;

        ActiveInventory = true;
        container.gameObject.SetActive(false);
        Init();
    }

    public void Set(T1 data)
    {
        Setup(data);
        container.gameObject.SetActive(true);
        gameObject.SetActive(true);
        Init();
    }

    public virtual void Setup(T1 data)
    {
        Root = data.Root;
        Canvas = data.Canvas;

        Data = data;
        gameObject.SetActive(!data.Selected);
        Init();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (CanBeDragged)
        {
            leftHandler.Press(eventData.button);
            rightHandler.Press(eventData.button);
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (CanBeDragged)
        {
            leftHandler.Release(eventData.button);
            rightHandler.Release(eventData.button);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (CanBeDragged)
        {
            leftHandler.Drag(eventData);
            rightHandler.Drag(eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CanBeDragged)
        {
            BackgroundAudio.Instance.PlayEvent(ECrewUIState.DragStart);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CanBeDragged)
        {
            if (eventData.button == PointerEventData.InputButton.Left && leftHandler.State == EPointerState.Drag)
            {
                bool found = false;
                if (eventData.pointerCurrentRaycast.gameObject != null)
                {
                    var result = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(eventData, result);
                    foreach (var data in result)
                    {
                        if (data.gameObject != null)
                        {
                            var item = data.gameObject.GetComponentInChildren<InventoryDrag<T1, T2>>(true);
                            if (item != null && item.ActiveInventory)
                            {
                                if (item != this)
                                {
                                    var savedData = Data;
                                    if (ActiveInventory)
                                    {
                                        Dropped(null);
                                    }
                                    else
                                    {
                                        gameObject.SetActive(false);
                                    }
                                    item.Dropped(savedData);
                                }
                                found = true;
                                break;
                            }
                        }
                    }

                }
                if (!found)
                {
                    if (ActiveInventory)
                    {
                        Dropped(null);
                    }
                    else
                    {
                        gameObject.SetActive(true);
                        BackgroundAudio.Instance.PlayEvent(ECrewUIState.DragFail);
                    }
                }
            }
            leftHandler.Release(eventData.button);
            rightHandler.Release(eventData.button);
        }
    }

    protected virtual void OnLeftDragged(Vector2 delta, PointerEventData data)
    {
        if (container.parent != Root)
        {
            container.SetParent(Root);
        }
        container.anchoredPosition += delta;
    }

    protected virtual void OnLeftDragFinished()
    {
        container.SetParent(trans);
        container.anchoredPosition = Vector2.zero;
    }

    protected virtual void OnRightClicked()
    {
        if (leftHandler.State == EPointerState.Drag)
        {
            leftHandler.Release();
        }
        else if (ActiveInventory)
        {
            Dropped(null);
        }
    }

    private void Init()
    {
        if (leftHandler == null)
        {
            trans = GetComponent<RectTransform>();

            leftHandler = new PointerHandler(PointerEventData.InputButton.Left, Canvas.scaleFactor, maxTapTime, doubleClickThreshold, minDragDistance);
            rightHandler = new PointerHandler(PointerEventData.InputButton.Right, Canvas.scaleFactor, maxTapTime, doubleClickThreshold, minDragDistance);

            if (!ActiveInventory)
            {
                leftHandler.DoubleClicked += () =>
                {
                    Clicked((T2)this);
                };
            }
            leftHandler.Dragged += OnLeftDragged;
            leftHandler.DragFinished += OnLeftDragFinished;
            rightHandler.Clicked += OnRightClicked;
        }
    }
}


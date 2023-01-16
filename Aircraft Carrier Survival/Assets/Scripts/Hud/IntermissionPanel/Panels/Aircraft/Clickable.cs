using GambitUtils;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Clickable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action LeftClicked = delegate { };
    public event Action RightClicked = delegate { };

    [SerializeField]
    protected GameObject highlight = null;
    [SerializeField]
    private GameObject pressed = null;

    [SerializeField]
    private float maxTapTime = 0.2f;
    [SerializeField]
    private float doubleClickThreshold = 0.5f;
    [SerializeField]
    private float minDragDistance = 30f;

    private PointerHandler leftHandler;
    private PointerHandler rightHandler;

    private void Update()
    {
        leftHandler.Update();
        rightHandler.Update();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (highlight != null)
        {
            highlight.SetActive(true);
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null)
        {
            highlight.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pressed != null)
        {
            pressed.SetActive(true);
        }
        if (highlight != null)
        {
            highlight.SetActive(false);
        }
        leftHandler.Press(eventData.button);
        rightHandler.Press(eventData.button);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (pressed != null)
        {
            pressed.SetActive(false);
        }
        leftHandler.Release(eventData.button);
        rightHandler.Release(eventData.button);
    }

    public void OnDrag(PointerEventData eventData)
    {
        leftHandler.Drag(eventData);
        rightHandler.Drag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        leftHandler.Release(eventData.button);
        rightHandler.Release(eventData.button);
    }

    public void Setup()
    {
        var canvas = transform.GetComponentInParents<Canvas>();
        leftHandler = new PointerHandler(PointerEventData.InputButton.Left, canvas.scaleFactor, maxTapTime, doubleClickThreshold, minDragDistance);
        rightHandler = new PointerHandler(PointerEventData.InputButton.Right, canvas.scaleFactor, maxTapTime, doubleClickThreshold, minDragDistance);

        leftHandler.Clicked += FireLeftClicked;
        rightHandler.Clicked += FireRightClicked;
    }

    private void FireLeftClicked()
    {
        LeftClicked();
    }

    private void FireRightClicked()
    {
        RightClicked();
    }
}

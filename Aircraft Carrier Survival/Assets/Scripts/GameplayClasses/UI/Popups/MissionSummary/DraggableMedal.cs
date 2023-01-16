using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableMedal : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action MedalAssigned = delegate { };

    public bool Drag
    {
        get;
        private set;
    }

    [SerializeField]
    private RectTransform medalRect = null;
    [SerializeField]
    private GameObject hover = null;
    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    private Transform defaultParent = null;
    [SerializeField]
    private GameObject raycastTarget = null;

    private Canvas canvas;
    private EventSystem eventSystem;

    private bool ignore;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        eventSystem = FindObjectOfType<EventSystem>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            ignore = true;
            return;
        }
        Drag = true;

        medalRect.SetParent(canvas.transform);
        hover.SetActive(false);
        highlight.SetActive(true);
        raycastTarget.SetActive(false);
        BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Drag)
        {
            medalRect.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ignore)
        {
            ignore = false;
            return;
        }
        Drag = false;
        raycastTarget.SetActive(true);
        highlight.SetActive(false);
        BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
        if (eventData != null && eventSystem.IsPointerOverGameObject())
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            DraggableMedalSlot slot = null;
            foreach (var result in results)
            {
                slot = result.gameObject.GetComponentInParent<DraggableMedalSlot>();
                if (slot != null)
                {
                    break;
                }
            }
            if (slot != null && !slot.Blocked)
            {
                AssignToSlot(slot);
                return;
            }
        }
        ReturnToBaseSlot();
    }

    public void AssignToSlot(DraggableMedalSlot slot)
    {
        SetShow(false);
        slot.AssignMedal(this);
        MedalAssigned();
    }

    public void ReturnToBaseSlot()
    {
        medalRect.SetParent(defaultParent);
        medalRect.anchoredPosition = Vector2.zero;
    }

    public void SetShow(bool show)
    {
        ReturnToBaseSlot();
        gameObject.SetActive(show);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Drag || ignore)
        {
            hover.SetActive(true);
            BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hover.SetActive(false);
    }
}

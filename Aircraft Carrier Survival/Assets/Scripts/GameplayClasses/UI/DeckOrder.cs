using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckOrder : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public static DeckOrder CurrentDrag;

    [SerializeField]
    private GameObject activeAnim = null;

    private int index = -1;
    private Image image = null;
    private DeckOrderPanelManager dopManager;
    private RectTransform rectTransform = null;
    private Canvas canvas = null;
    private EventSystem eventSystem = null;

    private ItemSlot currentSlot;

    private ItemSlot lastSlot;

    public bool CanBeGrab
    {
        get => index > 0;
    }
    public ACOrder Order = null;
    private PointerEventData lastPointerData;

    public void OnPointerClick(PointerEventData eventData)
    {
        // On Remove Order
        if (eventData.button == PointerEventData.InputButton.Right && CanBeGrab && CurrentDrag == null)
        {
            BackgroundAudio.Instance.PlayEvent(EMainSceneUI.DeleteOrder);
            var deck = AircraftCarrierDeckManager.Instance;
            if (deck.OrderQueue.Contains(Order))
            {
                deck.OrderQueue.Remove(Order);
                dopManager.UpdateOrders();
            }
        }
    }

    public void Setup(int index, ACOrder order, ItemSlot itemSlot, Canvas canvas)
    {
        if(image == null)
        {
            image = transform.GetChild(0).GetComponent<Image>();
        }

        image.sprite = DeckOrderPanelManager.Instance.GetIcon(order.OrderType);

        this.Order = order;

        if (index > 0)
        {
            rectTransform = GetComponent<RectTransform>();
            dopManager = DeckOrderPanelManager.Instance;
            eventSystem = FindObjectOfType<EventSystem>();
            this.canvas = canvas;

            currentSlot = itemSlot;
            lastSlot = currentSlot;
        }
        else
        {
            Disable();
        }
        activeAnim.SetActive(index == 0);
        this.index = index;
    }

    public void Disable()
    {
        this.enabled = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        lastPointerData = eventData;
        BackgroundAudio.Instance.PlayEvent(EMainSceneUI.DragOrder);
        rectTransform.SetParent(GameSceneManager.Instance.AircraftCanvas.transform);
        CurrentDrag = this;

        DeckOrderPanelManager.Instance.IsDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        HighlightEmptySlots(true);
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        if (eventSystem.IsPointerOverGameObject())
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (var s in results)
            {
                var isOk = s.gameObject.TryGetComponent(out ItemSlot slot);
                if (isOk && currentSlot != slot)
                {
                    currentSlot = slot;
                    break;
                }

                currentSlot = lastSlot;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        // On Change Order Queue
        var deck = AircraftCarrierDeckManager.Instance;
        var success = false;

        Assert.IsNotNull(currentSlot);

        if (deck.OrderQueue.Count > currentSlot.Index && currentSlot.Index != 0 && lastSlot.Index != currentSlot.Index)
        {
            var pomOrder = deck.OrderQueue[currentSlot.Index];
            deck.OrderQueue[currentSlot.Index] = deck.OrderQueue[lastSlot.Index];
            deck.OrderQueue[lastSlot.Index] = pomOrder;
            success = true;
        }
        BackgroundAudio.Instance.PlayEvent(success ? EMainSceneUI.DropOnOrder : EMainSceneUI.DropOffOrder);

        rectTransform.SetParent(currentSlot.transform);
        rectTransform.anchoredPosition = Vector2.zero;
        CurrentDrag = null;
        HighlightEmptySlots(false);

        dopManager.IsDragging = false;
        dopManager.UpdateOrders();
    }

    public void ForceEndDrag()
    {
        OnEndDrag(null);
        if (lastPointerData.pointerDrag != null)
        {
            lastPointerData.pointerDrag = null;
        }
        eventSystem.SetSelectedGameObject(null, null);
    }

    private void HighlightEmptySlots(bool highlight)
    {
        foreach (Transform slot in dopManager.Content)
        {
            slot.GetComponent<ItemSlot>().Highlight(highlight);
        }
    }
}

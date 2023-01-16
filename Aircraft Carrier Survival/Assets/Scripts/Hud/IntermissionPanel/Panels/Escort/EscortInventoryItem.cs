using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EscortInventoryItem : InventoryDrag<EscortItemData, EscortInventoryItem>, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<StrikeGroupMemberData, bool> SetShowTooltip = delegate { };
    public event Action<StrikeGroupMemberData, StrikeGroupMemberData> RaycastResultsChanged = delegate { };

    public override bool CanBeDragged => base.CanBeDragged && Data != null &&!Data.Repair;

    [SerializeField]
    private GameObject repair = null;
    [SerializeField]
    private Image shipImage = null;
    [SerializeField]
    private Text shipName = null;

    private EscortInventoryItem foundItem;

    private List<RaycastResult> result = new List<RaycastResult>();

    public override void Setup(EscortItemData data)
    {
        repair.SetActive(data.Repair);
        
        shipImage.sprite = data.Member.Icon;
        shipName.text = LocalizationManager.Instance.GetText(data.Member.NameID);
        base.Setup(data);

        leftHandler.Dragged += OnDrag;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Data != null)
        {
            SetShowTooltip(Data.Member, true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Data != null)
        {
            SetShowTooltip(Data.Member, false);
        }
    }

    public void OnDrag(Vector2 pos, PointerEventData eventData)
    {
        if (!CanBeDragged)
        {
            return;
        }
        SetShowTooltip(Data.Member, false);
        bool found = false;
        if (!ActiveInventory && eventData.button == PointerEventData.InputButton.Left && leftHandler.State == EPointerState.Drag)
        {
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                result.Clear();
                EventSystem.current.RaycastAll(eventData, result);
                foreach (var data in result)
                {
                    if (data.gameObject != null)
                    {
                        var item = data.gameObject.GetComponentInChildren<EscortInventoryItem>(true);
                        if (item != null && item.ActiveInventory && item != this && item.Data != null)
                        {
                            found = true;
                            if (foundItem == null)
                            {
                                foundItem = item;
                                RaycastResultsChanged(item.Data.Member, Data.Member);
                            }
                            break;
                        }
                    }
                }
            }
        }
        if (!found)
        {
            foundItem = null;
            RaycastResultsChanged(null, null);
        }
    }
}

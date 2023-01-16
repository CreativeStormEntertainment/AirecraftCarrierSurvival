using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class PeopleSubpanel<T1, T2, T3, A1, B1, B2>
    where T1 : PeopleIntermissionData
    where T2 : struct
    where T3 : struct
    where A1 : PeopleShopButton<T1>
    where B1 : PeopleInventoryItem<T1, B1>
    where B2 : MonoBehaviour, IInventoryDropSlot<B1, T1>
{
    public List<T1> Selected => selected;

    [SerializeField]
    protected Canvas canvas = null;
    [SerializeField]
    protected RectTransform root = null;
    [SerializeField]
    protected List<int> startSlots = null;
    [SerializeField]
    protected int upgradeSlotIncrease = 1;

    protected List<T1> shop = new List<T1>();
    protected List<T1> owned = new List<T1>();
    protected List<T1> selected = new List<T1>();

    protected PeopleSubpanelData<T1, T2, A1, B1, B2> subpanelData;

    public void Refresh()
    {
        if (subpanelData.ShopInventory != null)
        {
            subpanelData.ShopInventory.Refresh();
        }
    }

    public void SetShow(bool show)
    {
        subpanelData.ShopInventory.gameObject.SetActive(show);
        subpanelData.DraggableInventory.gameObject.SetActive(show);
    }

    protected abstract List<T1> GetShopItems();

    protected abstract T1 GetShopItem();

    protected void OnSlotsUpgraded(int count)
    {
        for (int i = 0; i < count; i++)
        {
            subpanelData.ActiveInventory.Add();
            selected.Add(null);
        }
    }

    protected void OnPersonBought(int slot, bool _)
    {
        var data = shop[slot];

        owned.Add(data);
        subpanelData.DraggableInventory.Add(data);

        data = GetShopItem();
        shop[slot] = data;
        subpanelData.ShopInventory.Refresh(slot, data);
    }

    protected void OnDoubleClicked(B1 item)
    {
        for (int index = 0; index < selected.Count; index++)
        {
            if (selected[index] == null)
            {
                SetSelectedPerson(index, item.Data);
                item.gameObject.SetActive(false);
                BackgroundAudio.Instance.PlayEvent(ECrewUIState.DragSuccess);
                return;
            }
        }
        BackgroundAudio.Instance.PlayEvent(EIntermissionClick.InactiveClick);
    }

    protected virtual void OnItemChanged(int slot, T1 newSelected)
    {
        var oldSelected = selected[slot];
        if (oldSelected != null)
        {
            oldSelected.Selected = false;
            subpanelData.Models[slot].SetActive(false);
        }
        if (newSelected == null)
        {
            selected[slot] = null;
        }
        else
        {
            selected[slot] = newSelected;
            newSelected.Selected = true;
            subpanelData.Models[slot].SetActive(true);
        }
        subpanelData.DraggableInventory.Setup(owned, owned.Count);
    }

    protected void SetSelectedPerson(int slot, T1 data)
    {
        OnItemChanged(slot, data);
        subpanelData.ActiveInventory.Set(slot, data);
    }

    protected int GetSelectedUnitsCount()
    {
        int count = 0;
        foreach (var unit in selected)
        {
            if (unit != null)
            {
                count++;
            }
        }
        return count;
    }
}

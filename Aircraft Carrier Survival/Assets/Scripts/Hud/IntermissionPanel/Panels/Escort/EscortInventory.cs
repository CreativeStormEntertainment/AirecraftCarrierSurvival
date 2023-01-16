using System;

public class EscortInventory : DraggableInventory<EscortInventoryItem, EscortItemData>
{
    public event Action<StrikeGroupMemberData, bool> SetShowTooltip = delegate { };
    public event Action<StrikeGroupMemberData, StrikeGroupMemberData> RaycastResultsChanged = delegate { };

    protected override void Init(EscortInventoryItem item)
    {
        base.Init(item);
        item.SetShowTooltip += OnSetShowTooltip;
        item.RaycastResultsChanged += OnRaycastResultsChanged;
    }

    public void OnSetShowTooltip(StrikeGroupMemberData data, bool show)
    {
        SetShowTooltip(data, show);
    }

    public void OnRaycastResultsChanged(StrikeGroupMemberData oldData, StrikeGroupMemberData newData)
    {
        RaycastResultsChanged(oldData, newData);
    }
}

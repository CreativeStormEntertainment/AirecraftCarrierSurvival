using System;

public class EscortActiveInventory : ActiveInventory<EscortDropSlot, EscortInventoryItem, EscortItemData>
{
    public event Action<StrikeGroupMemberData, bool> SetShowTooltip = delegate { };

    protected override void Init(EscortDropSlot item)
    {
        base.Init(item);
        item.Item.SetShowTooltip += OnSetShowTooltip;
    }
    public void OnSetShowTooltip(StrikeGroupMemberData data, bool show)
    {
        SetShowTooltip(data, show);
    }
}

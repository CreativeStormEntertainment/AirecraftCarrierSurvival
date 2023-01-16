using System;

public class EscortShopInventory : ShopInventory<EscortShopButton, EscortShopButtonData>
{
    public event Action<StrikeGroupMemberData, bool> SetShowTooltip = delegate { };

    protected override void Setup(EscortShopButton button, EscortShopButtonData data, int index)
    {
        base.Setup(button, data, index);
        button.SetShowTooltip -= OnSetShowTooltip;
        button.SetShowTooltip += OnSetShowTooltip;
    }

    public void OnSetShowTooltip(StrikeGroupMemberData data, bool show)
    {
        SetShowTooltip(data, show);
    }
}

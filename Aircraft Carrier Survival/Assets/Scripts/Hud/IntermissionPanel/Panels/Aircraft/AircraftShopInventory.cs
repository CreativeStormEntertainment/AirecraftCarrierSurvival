using System;

public class AircraftShopInventory : ShopInventory<AircraftShopButton, AircraftIntermissionData>
{
    public event Action<EPlaneType, bool> Highlight = delegate { };

    protected override void Setup(AircraftShopButton button, AircraftIntermissionData data, int index)
    {
        base.Setup(button, data, index);

        button.Highlight += FireHighlight;
    }

    public void SetupFinished()
    {
        items[1].transform.SetAsFirstSibling();
    }

    public void AllowBuy(bool allow)
    {
        foreach (var item in items)
        {
            item.AllowBuy(allow);
        }
    }

    public void SetBuy(bool buy)
    {
        foreach (var item in items)
        {
            item.SetBuy(buy);
        }
    }

    private void FireHighlight(EPlaneType type, bool highlight)
    {
        Highlight(type, highlight);
    }
}

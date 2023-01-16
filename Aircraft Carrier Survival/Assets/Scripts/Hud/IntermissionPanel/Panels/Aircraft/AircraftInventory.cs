using System;

public class AircraftInventory : Inventory<AircraftItem, AircraftIntermissionData>
{
    public event Action<EPlaneType> LeftClicked = delegate { };
    public event Action<EPlaneType> RightClicked = delegate { };
    public event Action<EPlaneType, bool> Highlight = delegate { };

    public void Refresh()
    {
        foreach (var item in items)
        {
            item.Refresh();
        }
    }

    public void HighlightButton(EPlaneType type, bool highlight)
    {
        items[(int)type].SetShowHighlight(highlight);
    }

    protected override void Setup(AircraftItem item, AircraftIntermissionData data, int count)
    {
        base.Setup(item, data, count);

        item.LeftClicked += FireLeftClicked;
        item.RightClicked += FireRightClicked;
        item.Highlight += FireHighlight;
    }

    private void FireLeftClicked(EPlaneType type)
    {
        LeftClicked(type);
    }

    private void FireRightClicked(EPlaneType type)
    {
        RightClicked(type);
    }

    private void FireHighlight(EPlaneType type, bool highlight)
    {
        Highlight(type, highlight);
    }
}

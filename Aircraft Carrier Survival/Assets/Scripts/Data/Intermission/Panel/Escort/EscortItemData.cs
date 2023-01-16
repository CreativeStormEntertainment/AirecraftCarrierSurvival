using UnityEngine;

public class EscortItemData : IInventoryDragData
{
    public Canvas Canvas
    {
        get;
    }
    public RectTransform Root
    {
        get;
    }
    public bool Selected
    {
        get;
        set;
    }

    public StrikeGroupMemberData Member => data.Data[EscortID];

    public bool Repair;

    public int EscortID;
    public int InventoryID;

    private readonly StrikeGroupData data;

    public EscortItemData(StrikeGroupData data, int escortID, int inventoryID, Canvas canvas, RectTransform root)
    {
        this.data = data;

        EscortID = escortID;
        InventoryID = inventoryID;
        Canvas = canvas;
        Root = root;
    }
}

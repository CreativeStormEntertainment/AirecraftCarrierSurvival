using UnityEngine;

public class PeopleIntermissionData : IShopButtonData, IInventoryDragData
{
    public string BuyTextID
    {
        get;
    }
    public Canvas Canvas
    {
        get;
    }
    public RectTransform Root
    {
        get;
    }

    public int UnlockCost => 0;
    public string UnlockTextID => string.Empty;
    public bool Unlocked
    {
        get => true;
        set
        {

        }
    }

    public int BuyCost
    {
        get;
        set;
    }

    public bool Selected
    {
        get;
        set;
    }

    protected PeopleIntermissionData(Canvas canvas, RectTransform root, int cost, string text)
    {
        Canvas = canvas;
        Root = root;

        BuyTextID = text;
        BuyCost = cost;
    }
}

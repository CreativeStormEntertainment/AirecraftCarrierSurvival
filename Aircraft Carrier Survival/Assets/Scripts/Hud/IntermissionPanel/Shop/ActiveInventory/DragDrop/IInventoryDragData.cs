using UnityEngine;

public interface IInventoryDragData
{
    Canvas Canvas
    {
        get;
    }

    RectTransform Root
    {
        get;
    }

    bool Selected
    {
        get;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandBuffDragData : IInventoryDragData
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

    public bool CanBeDragged
    {
        get;
        set;
    }

    public IslandBuff Buff => buff;

    private readonly IslandBuff buff;

    public IslandBuffDragData(IslandBuff data, Canvas canvas, RectTransform root)
    {
        this.buff = data;

        Canvas = canvas;
        Root = root;
    }
}

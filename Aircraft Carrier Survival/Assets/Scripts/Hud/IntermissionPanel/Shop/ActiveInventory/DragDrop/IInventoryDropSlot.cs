using System;
using UnityEngine;

public interface IInventoryDropSlot<T1, T2>
    where T1 : InventoryDrag<T2, T1>
    where T2 : class, IInventoryDragData
{
    event Action<T2> ItemChanged;

    RectTransform RectTransform
    {
        get;
    }

    void Setup(RectTransform root, Canvas canvas);
    void Set(T2 data);
}

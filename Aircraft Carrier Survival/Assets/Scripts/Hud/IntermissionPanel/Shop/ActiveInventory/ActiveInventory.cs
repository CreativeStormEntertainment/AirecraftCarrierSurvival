using System;
using UnityEngine;
using UnityEngine.Assertions;

public class ActiveInventory<T1, T2, T3> : MyList<T1>
    where T1 : MonoBehaviour, IInventoryDropSlot<T2, T3>
    where T2 : InventoryDrag<T3, T2>
    where T3 : class, IInventoryDragData
{
    public event Action<int, T3> ItemChanged = delegate { };

    private Canvas canvas;
    private RectTransform root;

    public void Init(RectTransform root, Canvas canvas)
    {
        this.canvas = canvas;
        this.root = root;

        Setup(0);
    }

    public override T1 Add()
    {
        var result = base.Add();
        result.Setup(root, canvas);
        return result;
    }

    public void Add(Vector2 position)
    {
        Add().RectTransform.anchoredPosition = position;
    }

    public void Set(int slot, T3 data)
    {
        Assert.IsTrue(slot < ItemsCount);
        items[slot].Set(data);
    }

    protected override void Init(T1 item)
    {
        base.Init(item);
        int index = ItemsCount;
        item.ItemChanged += (newElement) => ItemChanged(index, newElement);
    }
}

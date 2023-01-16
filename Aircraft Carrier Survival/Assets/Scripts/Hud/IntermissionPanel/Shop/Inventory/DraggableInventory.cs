using System;

public class DraggableInventory<T1, T2> : Inventory<T1, T2>
    where T1 : InventoryDrag<T2, T1>
    where T2 : class, IInventoryDragData
{
    public event Action<T1> DoubleClicked = delegate { };

    protected override void Init(T1 item)
    {
        base.Init(item);
        item.Clicked += FireDoubleClicked;
    }

    private void FireDoubleClicked(T1 t)
    {
        DoubleClicked(t);
    }
}

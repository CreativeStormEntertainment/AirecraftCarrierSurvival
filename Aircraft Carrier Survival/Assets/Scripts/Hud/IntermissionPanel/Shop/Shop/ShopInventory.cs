using System;
using System.Collections.Generic;

public class ShopInventory<T1,T2> : SetupableList<T1, T2>
    where T1 : ShopButton<T2>
    where T2 : IShopButtonData
{
    protected int index;

    private Action<int, bool> onUpgrade;

    public void Setup(List<T2> datas, int count, Action<int, bool> onUpgrade)
    {
        this.onUpgrade = onUpgrade;
        Setup(datas, count);
    }

    public void Refresh()
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    item.Refresh();
                }
            }
        }
    }

    protected override void Setup(T1 button, T2 data, int index)
    {
        button.Setup(data, index, () => Upgrade(index));
    }

    protected virtual void Upgrade(int index)
    {
        var item = items[index];
        bool bought = item.Unlocked;
        if (!item.Unlocked)
        {
            item.Unlocked = true;
        }
        onUpgrade(index, bought);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class SetupableList<T1,T2> : MyList<T1> where T1 : MonoBehaviour
{
    private List<T2> list;

    public void Setup(List<T2> datas, int count)
    {
        list = datas;
        Setup(count);
        list = null;
    }

    public T1 Add(T2 data)
    {
        var result = Add();
        Setup(result, data, ItemsCount - 1);
        return result;
    }

    public override T1 Add()
    {
        var result = base.Add();
        if (list != null)
        {
            Assert.IsTrue(list.Count >= ItemsCount);
            Setup(result, list[ItemsCount - 1], ItemsCount - 1);
        }
        return result;
    }

    protected abstract void Setup(T1 item, T2 data, int index);
}

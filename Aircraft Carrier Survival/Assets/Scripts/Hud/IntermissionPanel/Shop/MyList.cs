using System.Collections.Generic;
using UnityEngine;

public class MyList<T1> : MonoBehaviour where T1 : MonoBehaviour
{
    public int ItemsCount
    {
        get;
        private set;
    }

    protected List<T1> items;

    [SerializeField]
    private T1 prefab = null;
    [UnityEngine.Serialization.FormerlySerializedAs("root")]
    [SerializeField]
    private RectTransform parent = null;

    public void Setup(int count)
    {
        if (items == null)
        {
            items = new List<T1>();
        }
        ItemsCount = 0;
        for (int i = 0; i < count; i++)
        {
            Add();
        }
        for (int index = ItemsCount; index < items.Count; index++)
        {
            items[index].gameObject.SetActive(false);
        }
    }

    public virtual T1 Add()
    {
        if (items.Count == ItemsCount)
        {
            var item2 = Instantiate(prefab, parent);
            Init(item2);
            items.Add(item2);
        }
        var item = items[ItemsCount];
        item.gameObject.SetActive(true);
        ItemsCount++;
        return item;
    }

    protected virtual void Init(T1 item)
    {

    }
}

using System;
using UnityEngine;

[Serializable]
public class Inventory<T1, T2> : SetupableList<T1, T2>
    where T1 : MonoBehaviour, IInventoryItem<T2>
{
    protected override void Setup(T1 item, T2 data, int _)
    {
        item.Setup(data);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public struct PeopleSubpanelData<T1, T2, A1, B1, B2>
    where T1 : PeopleIntermissionData
    where T2 : struct
    where A1 : PeopleShopButton<T1>
    where B1 : PeopleInventoryItem<T1, B1>
    where B2 : MonoBehaviour, IInventoryDropSlot<B1, T1>
{
    public PeopleShopInventory<A1, T1> ShopInventory;
    public DraggableInventory<B1, T1> DraggableInventory;
    public ActiveInventory<B2, B1, T1> ActiveInventory;
    public Func<T2, T1> DataConstructor;
    public Func<T1, T2> SaveConstructor;

    public List<GameObject> Models;

    public PeopleSubpanelData(PeopleShopInventory<A1, T1> shopInventory, DraggableInventory<B1, T1> inventory, ActiveInventory<B2, B1, T1> activeInventory, Func<T2, T1> dataConstructor, Func<T1, T2> saveConstructor, List<GameObject> models)
    {
        ShopInventory = shopInventory;
        DraggableInventory = inventory;
        ActiveInventory = activeInventory;
        DataConstructor = dataConstructor;
        SaveConstructor = saveConstructor;
        Models = models;
    }
}

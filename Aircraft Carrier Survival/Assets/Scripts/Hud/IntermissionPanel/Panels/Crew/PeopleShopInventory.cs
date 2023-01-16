
public class PeopleShopInventory<T1, T2> : ShopInventory<T1, T2>
    where T1 : PeopleShopButton<T2>
    where T2 : PeopleIntermissionData
{
    public void Refresh(int index, T2 newData)
    {
        items[index].Refresh(newData);
    }
}

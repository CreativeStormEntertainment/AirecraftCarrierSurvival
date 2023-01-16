
public interface IShopButtonData
{
    bool Unlocked
    {
        get;
        set;
    }

    int UnlockCost
    {
        get;
    }

    int BuyCost
    {
        get;
    }

    string UnlockTextID
    {
        get;
    }

    string BuyTextID
    {
        get;
    }
}


public struct EscortShopButtonData : IShopButtonData
{
    public bool Unlocked
    {
        get => (tierData.Unlocked & 1 << index) != 0;
        set
        {
            if (value)
            {
                tierData.Unlocked |= 1 << index;
            }
            else
            {
                tierData.Unlocked &= ~(1 << index);
            }
        }
    }

    public int Index => index;

    public int UnlockCost => Member.UnlockCost;

    public int BuyCost => Member.BuyCost;

    public string UnlockTextID => Member.UnlockText;
    public string BuyTextID => Member.BuyText;

    public StrikeGroupMemberData Member => tierData.Data.Data[index];

    private EscortTierData tierData;
    private int index;

    public EscortShopButtonData(EscortTierData tierData, int index)
    {
        this.tierData = tierData;
        this.index = index;
    }
}

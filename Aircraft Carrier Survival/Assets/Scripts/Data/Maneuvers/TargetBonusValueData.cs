
public struct TargetBonusValueData
{
    public int Order;
    public BonusValueData ValueData;

    public TargetBonusValueData(int order, BonusValueData valueData) : this()
    {
        Order = order;
        ValueData = valueData;
    }
}

using System.Collections.Generic;

public class EscortTierData
{
    public StrikeGroupData Data;
    public int Escorts;
    public int Unlocked;

    public int EscortCount;

    public List<EscortItemData> Owned;

    public EscortTierData(StrikeGroupData data)
    {
        Data = data;
        Owned = new List<EscortItemData>();
    }
}

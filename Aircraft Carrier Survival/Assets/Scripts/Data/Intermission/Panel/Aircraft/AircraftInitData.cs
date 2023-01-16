using System;
using System.Collections.Generic;

[Serializable]
public class AircraftInitData
{
    public EPlaneType Type;
    public List<int> BuyCosts;
    public string BuyTextID;

    public List<SingleUpgradeData> UpgradeDatas;
}

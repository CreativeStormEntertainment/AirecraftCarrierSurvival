using System;
using System.Collections.Generic;

[Serializable]
public struct OfficerPeopleSaveData
{
    public List<int> Shop;
    public List<int> Owned;
    public List<int> Selected;
    public int Upgrade;

    public OfficerPeopleSaveData Duplicate()
    {
        var result = this;

        result.Shop = new List<int>(Shop);
        result.Owned = new List<int>(Owned);
        result.Selected = new List<int>(Selected);

        return result;
    }
}

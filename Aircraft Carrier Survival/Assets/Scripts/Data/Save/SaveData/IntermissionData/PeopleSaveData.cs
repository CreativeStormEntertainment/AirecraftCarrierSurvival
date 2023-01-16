using System;
using System.Collections.Generic;

[Serializable]
public struct PeopleSaveData<T> where T : struct
{
    public List<T> Shop;
    public List<T> Owned;
    public List<int> Selected;
    public int Upgrade;

    public PeopleSaveData<T> Duplicate()
    {
        var result = this;

        result.Shop = new List<T>(Shop);
        result.Owned = new List<T>(Owned);
        result.Selected = new List<int>(Selected);

        return result;
    }
}

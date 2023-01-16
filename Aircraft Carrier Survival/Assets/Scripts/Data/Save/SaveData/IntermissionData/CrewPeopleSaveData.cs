using System;
using System.Collections.Generic;

[Serializable]
public struct CrewPeopleSaveData
{
    public List<CrewUpgradeSaveData> Shop;
    public List<int> Selected;
    public int Upgrade;

    public CrewPeopleSaveData Duplicate()
    {
        var result = this;

        result.Shop = new List<CrewUpgradeSaveData>(Shop);
        result.Selected = new List<int>(Selected);

        return result;
    }
}

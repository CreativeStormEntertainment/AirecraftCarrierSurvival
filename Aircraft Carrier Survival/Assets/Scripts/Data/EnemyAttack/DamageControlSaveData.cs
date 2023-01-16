using System;
using System.Collections.Generic;

[Serializable]
public struct DamageControlSaveData
{
    //dc status
    public List<DCSaveData> DcDatas;

    //dc buttons
    public int Damage1ButtonUsed;
    public int Damage2ButtonUsed;
    public int Water1ButtonUsed;
    public int Water2ButtonUsed;

    public RandomData Fault;
    public RandomData Fire;
    public RandomData Flood;
    public RandomData Injured;
    public int RandomTimer;

    public DamageControlSaveData Duplicate()
    {
        var result = this;

        result.DcDatas = new List<DCSaveData>();
        foreach (var data in DcDatas)
        {
            result.DcDatas.Add(data.Duplicate());
        }

        return result;
    }
}

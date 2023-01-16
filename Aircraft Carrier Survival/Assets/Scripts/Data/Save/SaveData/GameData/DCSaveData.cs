using System;
using System.Collections.Generic;

[Serializable]
public struct DCSaveData
{
    public int Portrait;
    public int CurrentSegment;
    public int FinalSegment;
    public bool PlayerOverriden;
    public List<int> Queue;
    public EDcCategory Category;

    public DCSaveData Duplicate()
    {
        var result = this;

        result.Queue = new List<int>(Queue);

        return this;
    }
}

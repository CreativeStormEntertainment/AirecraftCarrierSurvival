using System;
using System.Collections.Generic;

[Serializable]
public struct StrategyVisualsSaveData
{
    public List<int> Objectives;
    public List<int> Sizes;
    public List<int> Targets;

    public StrategyVisualsSaveData Duplicate()
    {
        var result = this;
        if (Objectives == null)
        {
            result.Objectives = new List<int>();
            result.Sizes = new List<int>();
            result.Targets = new List<int>();
        }
        else
        {
            result.Objectives = new List<int>(Objectives);
            result.Sizes = new List<int>(Sizes);
            result.Targets = new List<int>(Targets);
        }
        return result;
    }
}

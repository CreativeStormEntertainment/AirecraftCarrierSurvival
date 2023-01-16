using System;
using System.Collections.Generic;

[Serializable]
public struct EventSaveData
{
    public EEventType Type;
    public List<int> Params;

    public EventSaveData Duplicate()
    {
        var result = this;
        result.Params = new List<int>(Params);
        return result;
    }
}

using System;
using System.Collections.Generic;

[Serializable]
public struct DeckOrderSaveData
{
    public EDeckOrderType Type;
    public EMissionOrderType MissionType;
    public int Mission;
    public List<int> Params;//to hangar, PlaneType, deck mode, swap
    public List<int> Squadrons;

    public DeckOrderSaveData Duplicate()
    {
        var result = this;

        result.Params = new List<int>(Params);
        result.Squadrons = new List<int>(Squadrons);

        return result;
    }
}

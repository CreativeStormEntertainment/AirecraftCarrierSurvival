using System;
using System.Collections.Generic;
using UnityEngine.UI;

[Serializable]
public class AdmiralTraitOption
{
    public SingleAdmiralTraitOption CurrentOption;
    public List<SingleAdmiralTraitOption> Options;
    public Text ButtonText;
}

[Serializable]
public class SingleAdmiralTraitOption
{
    public string Name;
    public int AirLevel, ShipLevel, EnduranceLevel;
}

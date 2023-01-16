using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SingleUpgradeData
{
    public int Cost;
    public string TextOnUpgrade;
    public string TextOnUI;
    public List<GameObject> ShowCurrent;
    public List<GameObject> ShowOnHighlight;
}

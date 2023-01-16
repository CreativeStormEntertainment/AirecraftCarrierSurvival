using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapRandomFleetData
{
    public List<MapEnemyShip> Fleet;
    public List<RectTransform> FleetPaths;
    public bool ShouldPlayMovie3 = true;
}

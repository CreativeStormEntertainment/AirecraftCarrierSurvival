using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData
{
    public Sprite MapShadow;
    public Sprite Map;
    public Texture2D LandMask;
    public List<MapRandomFleetData> Fleets;
    public List<Vector2> ShipPositions;
}

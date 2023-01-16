using UnityEngine;
using System.Collections;

[System.Flags]
public enum ETacticalFightUnitType
{
    None =  1 << 0,
    Plane =  1 << 1,
    Ship = 1 << 2,
    StationaryUnit = 1 << 3,
    FieldWithLand =  1 << 4,
    FieldWithWater = 1 << 5,
    PlayerPlane = 1 << 6
}

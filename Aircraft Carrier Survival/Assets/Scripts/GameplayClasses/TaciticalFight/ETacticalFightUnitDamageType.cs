using UnityEngine;
using System.Collections;
using System;

[Flags]
public enum ETacticalFightUnitDamageType
{
    None = 1 << 0,
    AirCraftGun = 1 << 1,
    Bomb = 1 << 2,
    Torpedo = 1 << 3
}

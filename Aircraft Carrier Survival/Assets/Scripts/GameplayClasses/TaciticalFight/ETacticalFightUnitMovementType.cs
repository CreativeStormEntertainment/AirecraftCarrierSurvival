using UnityEngine;
using System.Collections;
using System;

[Flags]
public enum ETacticalFightUnitMovementType
{
    None = 1 << 0,
    AboveLandAndSea = 1 << 1,
    BySea = 1 << 2,
    ByLand = 1 << 3
}

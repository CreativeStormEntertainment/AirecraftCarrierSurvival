using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RedWaterTacticSetup
{
    [Range(0f, 1f)]
    public float StrikeGroup2Chance = 0.2f;
    [Range(0f, 1f)]
    public float WarshipChance = 0.8f;
    [Range(0f, 1f)]
    public float AnotherShipChance = 0.5f;
    [Range(0f, 1f)]
    public float ExtraWarshipChance = 0.1f;

    public List<SOTacticMap> Maps = new List<SOTacticMap>();
}

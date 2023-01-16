using System;
using UnityEngine;

[Serializable]
public class ConsequenceData
{
    public ESandboxEventConsequence ConsequenceType;
    [Range(0f, 1f)]
    public float Chance = 1f;
    public int Value = 0;
}

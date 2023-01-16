using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SandboxEventConsequence
{
    public List<ConsequenceData> PossibleCosts = null;
    [NonSerialized]
    public int DrawnCostIndex;
    [NonSerialized]
    public int ConsequenceIndex;
    [NonSerialized]
    public int EventIndex;
}

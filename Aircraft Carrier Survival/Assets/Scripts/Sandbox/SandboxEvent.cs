using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SandboxEvent
{
    public List<SandboxEventConsequence> Consequences = null;
    public Sprite Sprite;
    [NonSerialized]
    public int EventIndex;
}

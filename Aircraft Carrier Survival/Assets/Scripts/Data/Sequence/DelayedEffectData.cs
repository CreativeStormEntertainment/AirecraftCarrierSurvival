using System;
using System.Collections.Generic;

[Serializable]
public class DelayedEffectData
{
    public float DelayTime;
    public List<SequenceEffectData> DelayedEffects;
    [NonSerialized]
    public bool ToFire;
}

using System;
using System.Collections.Generic;

[Serializable]
public class EffectsDataWrapper
{
    public List<int> EffectIndices;
    [NonSerialized]
    public List<EffectData> Effects;

    public EffectsDataWrapper() { }

    public EffectsDataWrapper(List<int> effectIndices)
    {
        EffectIndices = effectIndices;
    }

    public void GameplayInit(EffectManager effMan)
    {
        Effects = new List<EffectData>();
        foreach (var index in EffectIndices)
        {
            Effects.Add(effMan.Effects[index]);
        }
    }
}

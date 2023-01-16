using System.Collections.Generic;

public class EffectsCallData
{
    public List<EffectData> Effects;
    public float Effectiveness;

    public EffectsCallData(List<EffectData> effects, float eff)
    {
        Effects = effects;
        Effectiveness = eff;
    }
}

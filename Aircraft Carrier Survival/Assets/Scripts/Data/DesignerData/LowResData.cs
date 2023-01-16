using System;

[Serializable]
public class LowResData : EffectsDataWrapper
{
    public float ResLevel;
    public bool IsActive;

    public LowResData(float resLevel)
    {
        ResLevel = resLevel;
    }
}

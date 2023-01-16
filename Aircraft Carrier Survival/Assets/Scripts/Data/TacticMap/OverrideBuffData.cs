using System;

[Serializable]
public class OverrideBuffData
{
    public EIslandBuff Buff;
    [Enableable]
    public int OverrideDurationHours = -1;
}

using System;

[Serializable]
public struct SegmentSaveData
{
    public bool Damaged;
    public bool Destroyed;
    public bool Irrepairable;
    public bool Fire;

    public float FloodLevel;
    public bool OriginalFlooded;

    public int CrewInjured;
    public int InjuredAnim;
    public int InjuredTime;
    public float MaxInjuredTime;
}

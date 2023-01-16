using System;

[Serializable]
public struct CrewSaveData
{
    public ECrewUnitState Health;
    public int HealTicks;
    public int DeathTicks;
    public int Cooldown;
}

using System;

[Flags]
public enum ESubSectionRoomState
{
    Damaged             = 1 << EDCType.Repair,
    Fire                = 1 << EDCType.Firefight,
    Flood               = 1 << EDCType.WaterPump,
    HasInjured          = 1 << EDCType.Rescue,
    WasFloodByNeighbour = 1 << 29,
    FloodByNeighbour    = 1 << 30,
    Locked              = 1 << 31,
    ActiveFlood         = Flood | FloodByNeighbour,
    AnyFlood            = Flood | FloodByNeighbour | WasFloodByNeighbour,
}

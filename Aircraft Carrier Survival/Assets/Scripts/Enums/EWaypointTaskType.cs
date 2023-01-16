using System;

[Flags]
public enum EWaypointTaskType
{
    Normal          = 1 << 1,
    Firefighting    = 1 << 2,
    Rescue          = 1 << 3,
    Repair          = 1 << 4,
    RepairDoor      = 1 << 5,
    Waterpump       = 1 << 6,
    Rescue2         = 1 << 7,
    Rescue3         = 1 << 8,
    Rescues         = Rescue | Rescue2 | Rescue3,
    All             = -1
}

using System;

[Flags]
public enum EIslandRoomFlag
{
    FlagPlottingRoom    = 1 << EIslandRoomType.FlagPlottingRoom,
    PilotDebriefingRoom = 1 << EIslandRoomType.PilotDebriefingRoom,
    MeteorologyRoom     = 1 << EIslandRoomType.MeteorologyRoom,
    Bridge              = 1 << EIslandRoomType.Bridge,
    NavigationRoom      = 1 << EIslandRoomType.NavigationRoom,
    CIC                 = 1 << EIslandRoomType.CIC,
    RadioRoom           = 1 << EIslandRoomType.RadioRoom,
    CodingRoom          = 1 << EIslandRoomType.CodingRoom,
    OperationsRoom      = 1 << EIslandRoomType.OperationsRoom,
    OrdersRoom          = 1 << EIslandRoomType.OrdersRoom,
}

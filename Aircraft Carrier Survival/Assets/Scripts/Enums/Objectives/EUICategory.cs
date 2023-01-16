using System;

[Flags]
public enum EUICategory
{
    Camera                  = 1 <<  0,
    Island                  = 1 <<  1,
    TacticalMap             = 1 <<  2,
    CarrierSpeed            = 1 <<  3,
    TimeSpeed               = 1 <<  4,
    StrikeGroup             = 1 <<  5,
    OrderBuffs              = 1 <<  6,
    CrewManagement          = 1 <<  7,
    PlaneManagement         = 1 <<  8,
    DC                      = 1 <<  9,
    DCButtons               = 1 << 10,
    Missions                = 1 << 11,
    Events                  = 1 << 12,
    EventsSpawn             = 1 << 13,
    WreckPlaneDamageSpawn   = 1 << 14,
    IssueSpawn              = 1 << 15,
    SpawnUO                 = 1 << 16,
    DeckOrdersWindow        = 1 << 17,
    Time                    = 1 << 18,
    Supplies                = 1 << 19,
    LandingMode             = 1 << 20,
    CameraButton            = 1 << 21,
    Help                    = 1 << 22,
    CloseWindow             = 1 << 23,
    Objectives              = 1 << 24,
    MissionTooltip          = 1 << 25
}

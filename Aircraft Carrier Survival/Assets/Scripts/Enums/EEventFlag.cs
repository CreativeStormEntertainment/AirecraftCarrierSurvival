using System;

[Flags]
public enum EEventFlag
{
    SegmentDamage           = 1 << EEventType.SegmentDamage,
    SectionShutdown         = 1 << EEventType.SectionShutdown,
    NewIntel                = 1 << EEventType.NewIntel,
    UoIsEnemy               = 1 << EEventType.UoIsEnemy,
    Attack                  = 1 << EEventType.Attack,
    SubmarineHunt           = 1 << EEventType.SubmarineHunt,
    HostileScouts           = 1 << EEventType.HostileScouts,
    AllyUnderAttack         = 1 << EEventType.AllyUnderAttack,
    DetectionChanged        = 1 << EEventType.DetectionChanged,
    ShipSunk                = 1 << EEventType.ShipSunk,
    BuffExpired             = 1 << EEventType.BuffExpired,
    ObjectivesUpdated       = 1 << EEventType.ObjectivesUpdated,
    CarrierHealthCritical   = 1 << EEventType.CarrierHealthCritical,
    CrewInjured             = 1 << EEventType.CrewInjured,
    CrewDead                = 1 << EEventType.CrewDead,
    SquadronDamaged         = 1 << EEventType.SquadronDamaged,
    CarrierImmobile         = 1 << EEventType.CarrierImmobile,
    Wreck                   = 1 << EEventType.Wreck,
    Kamikaze                = 1 << EEventType.Kamikaze,
    BigGun                  = 1 << EEventType.BigGun,
}

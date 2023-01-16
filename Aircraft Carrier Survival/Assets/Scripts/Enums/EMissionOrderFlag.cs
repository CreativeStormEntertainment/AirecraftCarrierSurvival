using System;

[Flags]
public enum EMissionOrderFlag
{
    Airstrike               = 1 << EMissionOrderType.Airstrike,
    Recon                   = 1 << EMissionOrderType.Recon,
    IdentifyTargets         = 1 << EMissionOrderType.IdentifyTargets,
    SubmarineHunt           = 1 << EMissionOrderType.SubmarineHunt,
    CounterHostileScouts    = 1 << EMissionOrderType.CounterHostileScouts,
    FriendlyFleetCAP        = 1 << EMissionOrderType.FriendlyFleetCAP,
    CarriersCAP             = 1 << EMissionOrderType.CarriersCAP,
    Scouting                = 1 << EMissionOrderType.Scouting,
    Decoy                   = 1 << EMissionOrderType.Decoy,
    DetectSubmarine         = 1 << EMissionOrderType.DetectSubmarine,
    AirstrikeSubmarine      = 1 << EMissionOrderType.AirstrikeSubmarine,
    RescueVIP               = 1 << EMissionOrderType.RescueVIP,
    AttackJapan             = 1 << EMissionOrderType.AttackJapan,
    MidwayAirstrike         = 1 << EMissionOrderType.MidwayAirstrike,
    FriendlyCAPMidway       = 1 << EMissionOrderType.FriendlyCAPMidway,
    MagicIdentify           = 1 << EMissionOrderType.MagicIdentify,
    MagicAirstrike          = 1 << EMissionOrderType.MagicAirstrike,
    NightAirstrike          = 1 << EMissionOrderType.NightAirstrike,
    NightScouts             = 1 << EMissionOrderType.NightScouts,
    MagicNightScouts        = 1 << EMissionOrderType.MagicNightScouts,
}

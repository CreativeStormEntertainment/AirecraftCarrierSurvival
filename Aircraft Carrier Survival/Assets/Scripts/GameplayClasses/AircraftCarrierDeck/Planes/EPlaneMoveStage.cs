
public enum EPlaneMoveStage
{
    None                                                    = 0,
    //AirRecoveringToHangar
    //AirRecoveringToLanding                                  = 0,
    //LandingToHangar                                         = 1,

    //2x meh
    //DeckLaunchingToAirLaunching
    //there may be one squadron moving to air launching
    DeckLaunchingToLaunchingWait                            = 0,
    DeckLaunchingToStarting_Loop                            = 1,
    StartingToAirLaunching                                  = 2,
    LaunchingWaitToDeckLaunching                            = 3,

    //DeckLaunchingToDeckRecovering
    //DeckLaunchingToDeckRecovering                           = 0,

    //meh
    //DeckLaunchingToHangar
    DeckLaunchingToLaunchingWait_Hangar                      = 0,
    DeckLaunchingToLiftLaunching                             = 1,
    LiftLaunchingToHangar                                    = 2,
    LauchingWaitToDeckLaunching                              = 3,

    //moving backward
    //DeckRecoveringToDeckLaunching
    //DeckRecoveringToDeckLaunching                           = 0,

    //meh
    //DeckRecoveringToHangar
    //moving backward
    DeckRecoveringToRecoveringWait                          = 0,
    DeckRecoveringToLiftRecovering                          = 1,
    //Wait->Recovering moving forward
    LiftRecoveringToHangar                                  = 2,
    RecoveringWaitToDeckRecovering                          = 3,

    //HangarToDeckLaunching
    HangarToLiftLaunching                                   = 0,
    LiftLaunchingToDeckLaunching                            = 1,

    //HangarToDeckRecovering
    HangarToLiftRecovering                                  = 0,
    LiftRecoveringToDeckRecovering                          = 1,

    //SwapLaunching
    DeckLaunchingXToLaunchingWait                           = 0,
    DeckLaunchingAToSwapLaunching                           = 1,
    DeckLaunchingYToLaunchingWait                           = 2,
    DeckLaunchingBSwap                                      = 3,
    DeckLaunchingASwap                                      = 4,
    LaunchingWaitBToSwapLaunching                           = 5,
    LaunchingWaitYToDeckLaunching                           = 6,
    SwapLaunchingBToDeckLaunching                           = 7,
    LaunchingWaitXToDeckLaunching                           = 8,

    //SwapRecovering
    DeckRecoveringXToRecoveringWait                         = 0,
    DeckRecoveringAToSwapRecovering                         = 1,
    DeckRecoveringYToRecoveringWait                         = 2,
    DeckRecoveringBSwap                                     = 3,
    DeckRecoveringASwap                                     = 4,
    RecoveringWaitBToSwapRecovering                         = 5,
    RecoveringWaitYToDeckRecovering                         = 6,
    SwapRecoveringBToDeckRecovering                         = 7,
    RecoveringWaitXToDeckRecovering                         = 8,

    //SwapFrontLaunching
    DeckLaunchingXToLaunchingWait_Front                     = 0,
    DeckLaunchingAToSwapLaunching_Front                     = 1,
    LaunchingWaitXToDeckLaunchingAll                        = 2,
    SwapLaunchingAToDeckLaunching                           = 3,

    //SwapFrontRecovering
    DeckRecoveringXToRecoveringWait_Front                   = 0,
    DeckRecoveringAToSwapRecovering_Front                   = 1,
    RecoveringWaitXToDeckRecovering_Front                   = 2,
    SwapRecoveringAToDeckRecovering                         = 3
}

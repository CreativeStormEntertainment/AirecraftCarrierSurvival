using UnityEngine;
using UnityEngine.Assertions;

public class LandingOrder : ACOrder
{
    private bool toHangar;

    public LandingOrder(AircraftCarrierDeckManager deck, TacticalMission mission) : base(deck, EOrderType.Landing)
    {
        Mission = mission;
    }

    public LandingOrder(AircraftCarrierDeckManager deck, DeckOrderSaveData data, bool executing) : base(deck, EOrderType.Landing)
    {
        Mission = TacticManager.Instance.GetMission(data.MissionType, data.Mission);

        if (executing)
        {
            toHangar = data.Params[0] > 0;

            var planeMovementMan = PlaneMovementManager.Instance;
            planeMovementMan.SetOverrideTick(true);
            planeMovementMan.MovementFinished -= OnMovementFinished;
            planeMovementMan.MovementFinished += OnMovementFinished;
            deck.FirePlaneCountChanged();
        }
    }

    public override void SaveData(ref DeckOrderSaveData data, bool executing)
    {
        data.MissionType = Mission.OrderType;
        data.Mission = TacticManager.Instance.IndexOf(Mission);

        if (executing)
        {
            data.Params.Clear();
            data.Params.Add(toHangar ? 1 : 0);
        }
    }

    public override bool CanBeDone()
    {
        var eventMan = EventManager.Instance;
        if (deck.BlockOrders)
        {
            eventMan.HeavyManeuversPopup();
        }
        else if (deck.IsRunwayDamaged)
        {
            eventMan.RunwayDamagedPopup();
        }
        else if (deck.DeckMode == EDeckMode.Starting)
        {
            eventMan.DeckModeIncorrectPopup();
        }
        else if (Mission.MissionStage != EMissionStage.ReadyToRetrieve)
        {
            eventMan.MissionStageIncorrectPopup();
        }
        else
        {
            return true;
        }
        return false;
    }

    public override void OnStart()
    {
        Mission.Recovering();
        BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.Landing);

        var planeMovementMan = PlaneMovementManager.Instance;
        planeMovementMan.SetOverrideTick(true);
        planeMovementMan.MovementFinished -= OnMovementFinished;
        planeMovementMan.MovementFinished += OnMovementFinished;
        MovementFinished(true);
    }

    public override void Execute()
    {
        //if (toHangar)
        //{
        //    deck.SendSquadronToHangar(squadron);
        //}
        PlaneMovementManager.Instance.Tick();

        Mission.Recovered();
        AircraftCarrierDeckManager.Instance.FirePlaneCountChanged();
    }

    public override void ForceCancel()
    {
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
    }

    public void SetSquadron(PlaneSquadron squadron)
    {
        this.squadron = squadron;
    }

    private void OnMovementFinished()
    {
        MovementFinished(false);
    }

    private void MovementFinished(bool first)
    {
        var plaMovMan = PlaneMovementManager.Instance;
        try
        {
            int count = first ? 0 : 1;
            if (Mission.SentSquadrons.Count > count)
            {
                try
                {
                    squadron = Mission.SentSquadrons[count];
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError("D03");
                    throw ex;
                }
                try
                {
                    if (!first)
                    {
                        Mission.SentSquadrons.RemoveAt(0);
                        deck.FirePlaneCountChanged();
                    }
                }
                catch (System.Exception ex)
                {
                    //if (Mission != null)
                    //{
                    //    UnityEngine.Debug.Log(
                    //        $"{(Mission.AllRecoverySquadronsDirection == null ? "null" : Mission.AllRecoverySquadronsDirection.Count.ToString())};{(Mission.RecoverySquadronDirection == null ? "null2" : Mission.RecoverySquadronDirection.Count.ToString())};{(Mission.SentSquadrons == null ? "null3" : Mission.SentSquadrons.Count.ToString())}");
                    //}
                    UnityEngine.Debug.LogError("D04");
                    throw ex;
                }
                bool forceToHangar = false;
                bool skip = false;
                try
                {
                    if (Mission.RecoveryDirections.Count > count)
                    {
                        try
                        {
                            switch (Mission.RecoveryDirections[count])
                            {
                                case 0:
                                    forceToHangar = true;
                                    break;
                                case 1:
                                    skip = true;
                                    break;
                                case 2:
                                    break;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            UnityEngine.Debug.LogError("D05");
                            throw ex;
                        }
                        try
                        {
                            if (!first)
                            {
                                Mission.RecoveryDirections.RemoveAt(0);
                            }
                            if (skip)
                            {
                                Mission.SentSquadronsLeft.Add(squadron);
                                deck.FirePlaneCountChanged();
                                OnMovementFinished();
                                return;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            UnityEngine.Debug.LogError("D06");
                            throw ex;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError("D07");
                    throw ex;
                }

                bool wreck = false;
                try
                {
                    if (Mission.SentSquadrons.Count == 1 && !deck.HasWreck && deck.ShouldWreck())
                    {
                        wreck = true;
                        try
                        {
                            squadron.IsDamaged = true;
                        }
                        catch (System.Exception ex)
                        {
                            UnityEngine.Debug.LogError("D08");
                            throw ex;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.Log($"Mission: {(Mission == null ? "null" : "notnull")}; SentSquadrons: {((Mission == null || Mission.SentSquadrons == null) ? "null2" : "notnull")}; deck: {(deck == null ? "null3" : "notnull")}");
                    UnityEngine.Debug.LogError("D09");
                    throw ex;
                }
                try
                {
                    toHangar = forceToHangar || wreck || squadron.IsDamaged || deck.DeckSquadrons.Count >= deck.MaxSlots;
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError("D10");
                    throw ex;
                }
                if (toHangar)
                {
                    try
                    {
                        plaMovMan.FromAirToHangar(squadron, deck.PlaneSpeed, wreck, false);
                        deck.SendSquadronToHangar(squadron);
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError("D11");
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        deck.DeckSquadrons.Add(squadron);
                        deck.FirePlaneCountChanged();
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError("D12");
                        throw ex;
                    }
                    try
                    {
                        plaMovMan.FromAirToRecovering(squadron, deck.PlaneSpeed, wreck, false);
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError("D13");
                        throw ex;
                    }
                    try
                    {
                        squadron.AnimationPlay = false;
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError("D14");
                        throw ex;
                    }
                    try
                    {
                        AircraftCarrierDeckManager.Instance.FireDeckSquadronsCountChanged();
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError("D15");
                        throw ex;
                    }
                }
            }
            else
            {
                try
                {
                    plaMovMan.MovementFinished -= OnMovementFinished;
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError("D16");
                    throw ex;
                }
                try
                {
                    deck.SetOrderTimer(0);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError("D17");
                    throw ex;
                }

                try
                {
                    deck.FirePlaneCountChanged();
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError("D18");
                    throw ex;
                }
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("D19");
            throw ex;
        }
    }
}



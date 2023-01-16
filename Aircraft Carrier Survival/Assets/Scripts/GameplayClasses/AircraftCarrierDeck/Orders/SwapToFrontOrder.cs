
using UnityEngine.Assertions;

public class SwapToFrontOrder : ACOrder
{
    public PlaneSquadron SquadronA
    {
        get;
        private set;
    }
    protected int indexA = -1;
    protected EDeckMode deckMode;
    protected bool lastCan;

    public SwapToFrontOrder(AircraftCarrierDeckManager deck, PlaneSquadron squadronA) : base(deck, EOrderType.Swap)
    {
        SquadronA = squadronA;
        deckMode = deck.DeckMode;
    }

    public SwapToFrontOrder(AircraftCarrierDeckManager deck, DeckOrderSaveData data, bool executing) : base(deck, EOrderType.Swap)
    {
        SquadronA = deck.GetSquadron(data.Squadrons[0]);

        deckMode = (EDeckMode)data.Params[0];
        lastCan = data.Params[1] > 0;
        if (executing)
        {
            indexA = data.Params[2];
            var planeMovementMan = PlaneMovementManager.Instance;
            planeMovementMan.MovementFinished -= OnMovementFinished;
            planeMovementMan.MovementFinished += OnMovementFinished;
        }
    }

    public override void SaveData(ref DeckOrderSaveData data, bool executing)
    {
        data.Squadrons.Clear();
        data.Squadrons.Add(deck.IndexOf(SquadronA));

        data.Params.Clear();
        data.Params.Add((int)deckMode);
        data.Params.Add(lastCan ? 1 : 0);
        if (executing)
        {
            data.Params.Add(indexA);
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
        else
        {
            if (deck.DeckSquadrons.Count == 0 || (!lastCan && deck.DeckSquadrons[deck.DeckSquadrons.Count - 1] == SquadronA))
            {
                eventMan.OrderCannotBeDonePopup();
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    public override void OnStart()
    {
        BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.Swap);

        var plaMovMan = PlaneMovementManager.Instance;
        plaMovMan.MovementFinished -= OnMovementFinished;
        plaMovMan.MovementFinished += OnMovementFinished;
        var from = (deck.DeckMode == EDeckMode.Starting ? EPlaneNodeGroup.DeckLaunching : EPlaneNodeGroup.DeckRecovering);
        Assert.IsFalse(lastCan);
        plaMovMan.SwapToFront(SquadronA, deck.DeckSquadrons, from, deck.PlaneSpeed, out indexA);
    }

    public override void Execute()
    {
        SquadronA.Planes.Reverse();
        deck.DeckSquadrons.RemoveAt(indexA);
        deck.DeckSquadrons.Add(SquadronA);
    }

    public override void ForceCancel()
    {
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
    }

    private void OnMovementFinished()
    {
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
        deck.SetOrderTimer(0);
    }
}

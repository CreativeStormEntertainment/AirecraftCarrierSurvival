
public class SendToHangarOrder : ACOrder
{
    public SendToHangarOrder(AircraftCarrierDeckManager deck, PlaneSquadron squadron) : base(deck, EOrderType.SendToHangar)
    {
        this.squadron = squadron;
    }

    public SendToHangarOrder(AircraftCarrierDeckManager deck, DeckOrderSaveData data, bool executing) : base(deck, EOrderType.SendToHangar)
    {
        squadron = deck.GetSquadron(data.Squadrons[0]);

        if (executing)
        {
            var planeMovementMan = PlaneMovementManager.Instance;
            planeMovementMan.MovementFinished -= OnMovementFinished;
            planeMovementMan.MovementFinished += OnMovementFinished;
        }
    }

    public override void SaveData(ref DeckOrderSaveData data, bool _)
    {
        data.Squadrons.Clear();
        data.Squadrons.Add(deck.IndexOf(squadron));
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
        else if (!deck.DeckSquadrons.Contains(squadron))
        {
            eventMan.SquadronIsNotOnDeckPopup();
        }
        else
        {
            return true;
        }
        return false;
    }

    public override void OnStart()
    {
        var destination = (deck.DeckMode == EDeckMode.Starting ? EPlaneNodeGroup.DeckLaunching : EPlaneNodeGroup.DeckRecovering);
        var plaMovMan = PlaneMovementManager.Instance;
        plaMovMan.MovementFinished -= OnMovementFinished;
        plaMovMan.MovementFinished += OnMovementFinished;
        plaMovMan.ToHangar(squadron, deck.DeckSquadrons, destination, deck.PlaneSpeed);
        switch (squadron.PlaneType)
        {
            case EPlaneType.Fighter:
                BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.FighterToHangar);
                break;
            case EPlaneType.Bomber:
                BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.BomberToHangar);
                break;
            case EPlaneType.TorpedoBomber:
                BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.TorpedoToHangar);
                break;
        }
    }

    public override void Execute()
    {
        deck.SendSquadronToHangar(squadron);
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


public class SquadronCreationOrder : ACOrder
{
    public readonly EPlaneType PlaneType;
    private int sqaIndex = -1;

    public SquadronCreationOrder(AircraftCarrierDeckManager deck, EPlaneType planeType) : base(deck, EOrderType.SquadronCreation)
    {
        PlaneType = planeType;
    }

    public SquadronCreationOrder(AircraftCarrierDeckManager deck, DeckOrderSaveData data, bool executing) : base(deck, EOrderType.SquadronCreation)
    {
        PlaneType = (EPlaneType)data.Params[0];

        if (executing)
        {
            sqaIndex = data.Params[1];

            var planeMovementMan = PlaneMovementManager.Instance;
            planeMovementMan.MovementFinished -= OnMovementFinished;
            planeMovementMan.MovementFinished += OnMovementFinished;
        }
    }

    public override void SaveData(ref DeckOrderSaveData data, bool executing)
    {
        data.Params.Clear();
        data.Params.Add((int)PlaneType);

        if (executing)
        {
            data.Params.Add(sqaIndex);
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
        else if (deck.IsMaxSlotsReached)
        {
            eventMan.NotEnoughSpaceOnDeckPopup();
        }
        else if (!deck.CanGetSquadron(PlaneType))
        {
            eventMan.CannotGetSquadronPopup();
        }
        else
        {
            return true;
        }
        return false;
    }

    public override void OnStart()
    {
        var squadron = deck.CreateNewSquadron(PlaneType, true);
        this.squadron = squadron;
        var destination = (deck.DeckMode == EDeckMode.Starting ? EPlaneNodeGroup.DeckLaunching : EPlaneNodeGroup.DeckRecovering);
        switch (PlaneType)
        {
            case EPlaneType.Fighter:
                BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.FighterToDeck);
                break;
            case EPlaneType.Bomber:
                BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.BomberToDeck);
                break;
            case EPlaneType.TorpedoBomber:
                BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.TorpedoToDeck);
                break;
        }
        var plaMovMan = PlaneMovementManager.Instance;
        plaMovMan.MovementFinished -= OnMovementFinished;
        plaMovMan.MovementFinished += OnMovementFinished;
        plaMovMan.FromHangar(squadron, destination, deck.PlaneSpeed, false);

        sqaIndex = deck.DeckSquadrons.IndexOf(squadron);

        //DragPlanesManager.Instance.UpdateSpots();
    }

    public override void Execute()
    {

    }

    public override void ForceCancel()
    {
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
    }

    private void OnMovementFinished()
    {
        deck.DeckSquadrons[sqaIndex].AnimationPlay = false;
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
        deck.SetOrderTimer(0);
    }
}

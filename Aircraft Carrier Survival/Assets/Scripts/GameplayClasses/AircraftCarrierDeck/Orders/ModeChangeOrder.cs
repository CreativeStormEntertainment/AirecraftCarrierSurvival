
public abstract class ModeChangeOrder : ACOrder
{
    private readonly EDeckMode to;

    public ModeChangeOrder(AircraftCarrierDeckManager deck, EDeckMode deckMode, EOrderType orderType) : base(deck, orderType)
    {
        to = deckMode;
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
        else if (deck.DeckMode == to)
        {
            EventManager.Instance.OrderCannotBeDonePopup();
        }
        else
        {
            return true;
        }
        return false;
    }

    public override void OnStart()
    {
        if (deck.DeckStateChangeSound != null)
        {
            deck.DeckStateChangeSound.Play();
        }

        if (deck.DeckSquadrons.Count == 0)
        {
            deck.SetOrderTimer(1);
        }
        else
        {
            var from = (to == EDeckMode.Starting ? EPlaneNodeGroup.DeckRecovering : EPlaneNodeGroup.DeckLaunching);
            if (from == EPlaneNodeGroup.DeckLaunching)
            {
                BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.Recover);
            }
            else
            {
                BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.Launch);
            }
            var plaMovMan = PlaneMovementManager.Instance;
            plaMovMan.MovementFinished -= OnMovementFinished;
            plaMovMan.MovementFinished += OnMovementFinished;
            plaMovMan.BetweenLaunchingRecovering(deck.DeckSquadrons, from, deck.PlaneSpeed);
        }
    }

    public override void Execute()
    {
        if (deck.DeckStateChangeSound != null)
        {
            deck.DeckStateChangeSound.Stop();
        }
        deck.ChangeDeckMode(to, false);
    }

    public override void ForceCancel()
    {
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
    }

    protected void OnMovementFinished()
    {
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
        deck.SetOrderTimer(0);
    }
}

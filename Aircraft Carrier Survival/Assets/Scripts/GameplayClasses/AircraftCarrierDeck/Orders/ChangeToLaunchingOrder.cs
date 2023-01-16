
public class ChangeToLaunchingOrder : ModeChangeOrder
{
    public ChangeToLaunchingOrder(AircraftCarrierDeckManager deck, bool executing) : base(deck, EDeckMode.Starting, EOrderType.ToLaunching)
    {
        if (executing)
        {
            var planeMovementMan = PlaneMovementManager.Instance;
            planeMovementMan.MovementFinished -= OnMovementFinished;
            planeMovementMan.MovementFinished += OnMovementFinished;
        }
    }

    public override void SaveData(ref DeckOrderSaveData data, bool _)
    {

    }
}

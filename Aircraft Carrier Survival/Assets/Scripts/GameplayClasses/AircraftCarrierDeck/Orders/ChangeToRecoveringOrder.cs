
public class ChangeToRecoveringOrder : ModeChangeOrder
{
    public ChangeToRecoveringOrder(AircraftCarrierDeckManager deck, bool executing) : base(deck, EDeckMode.Landing, EOrderType.ToRecovering)
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

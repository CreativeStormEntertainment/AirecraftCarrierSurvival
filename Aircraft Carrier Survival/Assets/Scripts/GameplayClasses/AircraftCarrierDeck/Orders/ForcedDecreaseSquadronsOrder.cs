
public class ForcedDecreaseSquadronsOrder : SendToHangarOrder
{
    public ForcedDecreaseSquadronsOrder(AircraftCarrierDeckManager deck, PlaneSquadron squadron) : base(deck, squadron)
    {
        this.squadron = squadron;
        OrderType = EOrderType.ForcedDecreaseSquadrons;
    }
}

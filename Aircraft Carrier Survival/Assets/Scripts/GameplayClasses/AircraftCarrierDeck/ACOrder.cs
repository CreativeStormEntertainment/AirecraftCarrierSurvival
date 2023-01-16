using System.Collections.Generic;

public abstract class ACOrder
{
    public int Timer
    {
        get;
        private set;
    }

    public EOrderType OrderType
    {
        get;
        protected set;
    }

    protected AircraftCarrierDeckManager deck;

    protected PlaneSquadron squadron;
    public TacticalMission Mission
    {
        get;
        protected set;
    }

    public ACOrder(AircraftCarrierDeckManager deck, EOrderType type)
    {
        this.deck = deck;
        OrderType = type;
        Timer = 100_000_000;
    }

    public abstract void SaveData(ref DeckOrderSaveData data, bool executing);

    public abstract bool CanBeDone();

    public abstract void OnStart();

    public abstract void Execute();

    public abstract void ForceCancel();

    public virtual IEnumerable<PlaneSquadron> GetSquadrons()
    {
        yield break;
    }

    public PlaneSquadron GetSquadron()
    {
        return squadron;
    }
}

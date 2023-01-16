using System;
using System.Collections.Generic;

[Serializable]
public struct DeckSaveData
{
    public List<PlaneSaveData> PlaneDatas;

    //deck mode
    public bool LaunchingMode;
    //what is repaired right now and time
    public EPlaneType CurrentRepairType;
    public int RepairTimeLeft;

    //elevator to-state & current state
    public float ElevatorState;
    public float ElevatorTimer;

    //what squadrons on deck
    public List<EPlaneType> DeckSquadrons;

    //wreck
    public bool HasWreck;
    public int WreckType;

    public int Kamikaze;

    public int FirstOrderDelay;
    //current deck orders
    public List<DeckOrderSaveData> DeckQueue;

    //in progress anim & squadrons positions
    public PlaneMovementSaveData PlaneMovement;

    public RandomData PlaneDamageRandom;
    public RandomData WreckRandom;

    public bool KamikazeInProgress;

    public DeckSaveData Duplicate()
    {
        var result = this;

        result.PlaneMovement = PlaneMovement.Duplicate();

        result.PlaneDatas = new List<PlaneSaveData>(PlaneDatas);
        result.DeckSquadrons = new List<EPlaneType>(DeckSquadrons);

        result.DeckQueue = new List<DeckOrderSaveData>();
        for (int i = 0; i < DeckQueue.Count; i++)
        {
            result.DeckQueue.Add(DeckQueue[i].Duplicate());
        }

        return result;
    }
}

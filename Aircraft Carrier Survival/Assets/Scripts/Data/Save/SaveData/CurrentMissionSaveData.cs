using System.Collections.Generic;

public struct CurrentMissionSaveData
{
    public bool MissionSucceed;
    public bool ShipsDestroyedReward;
    public bool SquadronsLostReward;
    public bool MissionTimeReward;
    public List<bool> OptionalObjectives;
}

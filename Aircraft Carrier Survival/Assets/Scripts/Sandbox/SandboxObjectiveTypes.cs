using System.Collections.Generic;

public class SandboxObjectiveTypes
{
    public static readonly List<ESandboxObjectiveType> ReconnaissanceObjectiveTypes = new List<ESandboxObjectiveType>() { ESandboxObjectiveType.DestroyFleetUndetected, ESandboxObjectiveType.DefendBaseUndetected,
        ESandboxObjectiveType.PatrolBase, ESandboxObjectiveType.DefendFleetUndetected, ESandboxObjectiveType.Scout, ESandboxObjectiveType.Patrol, ESandboxObjectiveType.PatrolUndetected,
        ESandboxObjectiveType.UndetectedPatrolBase, ESandboxObjectiveType.ScoutBase };

    public static readonly List<ESandboxObjectiveType> SupportingOperationObjectiveTypes = new List<ESandboxObjectiveType>() { ESandboxObjectiveType.RetrieveTimedPlanes, ESandboxObjectiveType.RetrieveTimedSurvivors,
        ESandboxObjectiveType.RetrieveSurvivorsUnknown, ESandboxObjectiveType.DefendBase, ESandboxObjectiveType.DefendMultipleBases, ESandboxObjectiveType.DefendFleet, ESandboxObjectiveType.DefendMultipleFleets,
        ESandboxObjectiveType.DefendAllyTimed, ESandboxObjectiveType.EscortRetreat, ESandboxObjectiveType.EscortAttack, ESandboxObjectiveType.EscortInvasion };

    public static readonly List<ESandboxObjectiveType> PatrolObjectiveTypes = new List<ESandboxObjectiveType>() { ESandboxObjectiveType.DestroyBase, ESandboxObjectiveType.DestroyMultipleBases,
        ESandboxObjectiveType.SeekAndDestroy, ESandboxObjectiveType.SeekAndDestroyMultiple, ESandboxObjectiveType.DestroyTimedFleets,
        ESandboxObjectiveType.DestroyChase, ESandboxObjectiveType.DestroyChaseMultiple };

    public static readonly List<ESandboxObjectiveType> OptionalObjectiveTypes = new List<ESandboxObjectiveType>() { ESandboxObjectiveType.DefendAllyTimed, ESandboxObjectiveType.DefendBase,
        ESandboxObjectiveType.DefendBaseUndetected, ESandboxObjectiveType.DefendFleet, ESandboxObjectiveType.DefendFleetUndetected, ESandboxObjectiveType.DefendMultipleBases, ESandboxObjectiveType.DefendMultipleFleets,
        ESandboxObjectiveType.DestroyBase, ESandboxObjectiveType.DestroyMultipleBases, ESandboxObjectiveType.DestroyChase, ESandboxObjectiveType.DestroyChaseMultiple, ESandboxObjectiveType.DestroyFleetUndetected,
        ESandboxObjectiveType.SeekAndDestroy, ESandboxObjectiveType.SeekAndDestroyMultiple, ESandboxObjectiveType.DestroyTimedFleets,
        ESandboxObjectiveType.EscortAttack, ESandboxObjectiveType.EscortInvasion, ESandboxObjectiveType.EscortRetreat, ESandboxObjectiveType.Patrol,
        ESandboxObjectiveType.RetrieveTimedPlanes, ESandboxObjectiveType.RetrieveTimedSurvivors, ESandboxObjectiveType.RetrieveSurvivorsUnknown, ESandboxObjectiveType.Scout,
        ESandboxObjectiveType.PatrolUndetected, ESandboxObjectiveType.UndetectedPatrolBase, ESandboxObjectiveType.ScoutBase };
    public static readonly List<ESandboxObjectiveType> QuestObjectiveTypes = new List<ESandboxObjectiveType>() { ESandboxObjectiveType.DestroyTimedFleets }; //TODO
    //public static readonly List<ESandboxObjectiveType> OptionalObjectiveTypes = new List<ESandboxObjectiveType>() { ESandboxObjectiveType.ScoutBase }; //TODO
    //public static readonly List<ESandboxObjectiveType> PatrolObjectiveTypes = new List<ESandboxObjectiveType>() { ESandboxObjectiveType.DestroyTimedFleets }; //TODO
    //public static readonly List<ESandboxObjectiveType> SupportingOperationObjectiveTypes = new List<ESandboxObjectiveType>() { ESandboxObjectiveType.DestroyTimedFleets }; //TODO
    //public static readonly List<ESandboxObjectiveType> ReconnaissanceObjectiveTypes = new List<ESandboxObjectiveType>() { ESandboxObjectiveType.DestroyTimedFleets }; //TODO

    public Dictionary<EMainGoalType, List<ESandboxObjectiveType>> MainGoalObjectivesDictionary = new Dictionary<EMainGoalType, List<ESandboxObjectiveType>>();

    public void Init()
    {
        MainGoalObjectivesDictionary.Add(EMainGoalType.Patrol, PatrolObjectiveTypes);
        MainGoalObjectivesDictionary.Add(EMainGoalType.Reconnaissance, ReconnaissanceObjectiveTypes);
        MainGoalObjectivesDictionary.Add(EMainGoalType.SupportingOperations, SupportingOperationObjectiveTypes);
    }
}

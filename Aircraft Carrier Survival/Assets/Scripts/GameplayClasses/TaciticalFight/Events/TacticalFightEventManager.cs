using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TacticalFightEventManager : MonoBehaviour
{
    public static TacticalFightEventManager Instance;

    List<TacticalFightEvent> tacticalFightEvents = new List<TacticalFightEvent>();
    List<TacticalFightObjective> tacticalFightObjectives = new List<TacticalFightObjective>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        TacticalFightManager.OnRoundEnd += CheckEventsAreEnding;
        TacticalFightManager.OnBeforeEnemyMove += CheckIsObjectiveDone;
    }

    private void OnDisable()
    {
        TacticalFightManager.OnRoundEnd -= CheckEventsAreEnding;
        TacticalFightManager.OnBeforeEnemyMove -= CheckIsObjectiveDone;
    }

    public void InitializeEventManager()
    {
        tacticalFightEvents = new List<TacticalFightEvent>();
        tacticalFightObjectives = new List<TacticalFightObjective>();
    }

    public void AddEventToQueue(TacticalFightEvent eventToAdd)
    {
        tacticalFightEvents.Add(eventToAdd);
    }

    public void RemoveEventFromQueue(TacticalFightEvent eventToRemove)
    {
        tacticalFightEvents.Remove(eventToRemove);
    }

    public void AddObjective(TacticalFightObjective objectiveToAdd)
    {
        tacticalFightObjectives.Add(objectiveToAdd);
    }

    public void RemoveObjective(TacticalFightObjective objectiveToRemove)
    {
        tacticalFightObjectives.Remove(objectiveToRemove);
    }

    public void CheckIsObjectiveDone()
    {
        List<TacticalFightObjective> objectivesToEnd = new List<TacticalFightObjective>();

        foreach (TacticalFightObjective tacticalFightObjective in tacticalFightObjectives)
        {
            if (tacticalFightObjective.CheckIsObjectiveComplete())
                objectivesToEnd.Add(tacticalFightObjective);

            if (tacticalFightObjective.GetIsObjectiveFailed())
                objectivesToEnd.Add(tacticalFightObjective);
        }

        foreach (TacticalFightObjective objectiveToEnd in objectivesToEnd)
        {
            if (objectiveToEnd.GetIsObjectiveFailed())
                objectiveToEnd.OnObjectiveNotCompleted();
            else
                objectiveToEnd.OnObjectiveComplete();
            RemoveObjective(objectiveToEnd);
        }
    }

    public void CheckEventsAreEnding()
    {
        List<TacticalFightEvent> eventsToEnd = new List<TacticalFightEvent>();

        foreach (TacticalFightEvent tacticalFightEvent in tacticalFightEvents)
        {
            tacticalFightEvent.DecreaseRoundsToActivate();

            if (tacticalFightEvent.GetCurrentRoundsToActivate() <= 0)
                eventsToEnd.Add(tacticalFightEvent);
        }

        foreach(TacticalFightEvent eventToEnd in eventsToEnd)
        {
            eventToEnd.EndEvent();
            RemoveEventFromQueue(eventToEnd);
        }
    }

    public void EndEventOnPlayerUnit(TacticalFightPlayerUnit playerUnit)
    {
        List<TacticalFightPlayerUnitStayOnMapEvent> listOfStayOnMapEvents = tacticalFightEvents.Where(x => x is TacticalFightPlayerUnitStayOnMapEvent).ToList().Cast<TacticalFightPlayerUnitStayOnMapEvent>().ToList();
        TacticalFightPlayerUnitStayOnMapEvent eventToEnd = listOfStayOnMapEvents.FirstOrDefault(x => ((TacticalFightPlayerUnit)x.GetUnitForEvent()) == playerUnit);
        if(eventToEnd != null)
        {
            eventToEnd.EndEvent();
            RemoveEventFromQueue(eventToEnd);
        }
    }
}

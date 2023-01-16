using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionData
{
    public float Time;
    public EActionType Action;
    public EDynamicEventType EventType;
    public SectionSegment Segment;

    public int PatternID;
    public int AttackInPattern;

    public int FleetID;
    public string Title;
    public string Description;

    public string MissionName;
    public string MissionDesc;
    public EMissionOrderType MissionType;
    public List<EPlaneType> Squadrons;
    public EMissionStage Stage;
    public Vector2 AttackPos;
    public Vector2 RetrievalPos;

    public Func<bool> Trigger;
    public Action OnSuccess;
    public Action OnFailure;

    public Action Fun;

    public ActionData(float time, EDynamicEventType type, SectionSegment segment)
    {
        Time = time;
        Action = EActionType.DynamicEvent;
        EventType = type;
        Segment = segment;
    }

    public ActionData(float time, int fleetID)
    {
        Time = time;
        Action = EActionType.FleetSpotted;
        FleetID = fleetID;
    }

    public ActionData(float time, int patternID, int attackInPattern)
    {
        Time = time;
        Action = EActionType.Attack;
        PatternID = patternID;
        AttackInPattern = attackInPattern;
    }

    public ActionData(float time, string title, string description)
    {
        Time = time;
        Action = EActionType.Event;
        Title = title;
        Description = description;
    }

    public ActionData(float time, string missionName, EMissionOrderType missionType, List<EPlaneType> squadrons, EMissionStage stage, Vector2 attackPos, Vector2 retrievalPos)
    {
        Time = time;
        Action = EActionType.Mission;
        MissionName = missionName;
        MissionType = missionType;
        Squadrons = squadrons;
        Stage = stage;
        AttackPos = attackPos;
        RetrievalPos = retrievalPos;
    }

    public ActionData(float time, Func<bool> trigger, Action onSuccess, Action onFailure)
    {
        Time = time;
        Action = EActionType.Trigger;
        Trigger = trigger;
        OnSuccess = onSuccess;
        OnFailure = onFailure;
    }

    public ActionData(float time, Action action)
    {
        Time = time;
        Action = EActionType.Cuustom;
        Fun = action;
    }
}

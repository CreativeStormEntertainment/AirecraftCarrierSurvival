using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectiveEffectData
{
    public EObjectiveEffect EffectType;
    public bool NotEffect;
    public List<Transform> TargetTranses;
    public List<int> Targets;
    public Transform ObjectiveTransform;
    public int ObjectiveTarget;
    public int Movie;
    public int Minutes;
    public int Hours;
    public int Power;
    public int AbsoluteForce;
    public float Range;
    public int Count;
    public EMissionOrderType MissionType;
    public float HoursToRetrieve;
    public RectTransform RetrievePositionRect;
    public int RetrievePosition;
    public int BombersNeeded;
    public int FightersNeeded;
    public int TorpedoesNeeded;
    public string OverrideWinDescriptionID;
    public string TimerID;
    public string TimerTooltipTitleID;
    public string TimerTooltipDescID;
    public int ParamA;
    public int ParamB;
    public EUICategory UICategories;
    public ECameraInputType CameraInput;
    public EIslandRoomFlag IslandRooms;
    public EDepartmentsFlag Departments;
    public EManeuverSquadronType PlaneType;
    public List<StrategyData> Strategy;
    public ETacticalObjectType UOType;
    public ESections Section;
    public bool FirstSubsection;
    public bool SecondSubsection;
    public EIssue Issue;
    public EDepartments Department;
    public EMissionOrderFlag Missions;
    public EEventFlag Events;
    public EDcCategoryFlag DcCategoryFlag;
    public bool AttackStrikeGroup;
    public Vector2 OverridePosition;
    [EventRef]
    public string SoundEventReference;
    public ECameraView CameraView;
    public int Category;
    public string Highlight;
    public float Time;
    public DayTime Date;
    [NonSerialized]
    public bool Expanded;
}

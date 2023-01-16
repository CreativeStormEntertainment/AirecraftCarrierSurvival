using System;

[Serializable]
public struct WaypointData
{
    public EWaypointAnimType AnimType;
    public string AnimName;
    public int AnimID;
    public int MinRepeat;
    public int MaxRepeat;
    public EWaypointTaskType PossibleTasks;

    public SectionSegment Segment;
    public SectionSegment OverrideSegment;
    public SectionSegment ExitSegmentOtherSide;

    public bool InjuredWaypoint;

    public bool CanFrighten;

    public bool IsLocked;

    public void Init(AnimationManager animMan)
    {
        AnimType = EWaypointAnimType.BasicAnim;
#if UNITY_EDITOR
        AnimName = AnimationManager.Walk;
#endif
        AnimID = animMan.AnimData.WalkID;
        MinRepeat = 3;
        MaxRepeat = 5;
        IsLocked = false;
        PossibleTasks = EWaypointTaskType.All;
    }
}

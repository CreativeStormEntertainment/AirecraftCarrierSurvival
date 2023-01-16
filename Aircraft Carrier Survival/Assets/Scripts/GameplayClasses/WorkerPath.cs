using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class WorkerPath : MonoBehaviour
{
#if UNITY_EDITOR
    private static readonly Dictionary<ENeighbourDirection, ENeighbourDirection> DirectionHelper = new Dictionary<ENeighbourDirection, ENeighbourDirection>()
    {
        {ENeighbourDirection.Up, ENeighbourDirection.Down },
        {ENeighbourDirection.Down, ENeighbourDirection.Up },
        {ENeighbourDirection.Left, ENeighbourDirection.Right },
        {ENeighbourDirection.Right, ENeighbourDirection.Left },
        {ENeighbourDirection.FireOnly, ENeighbourDirection.FireOnly },
        {ENeighbourDirection.WaterOnly, ENeighbourDirection.WaterOnly },
        {ENeighbourDirection.FireAndWater, ENeighbourDirection.FireAndWater }
    };
#endif
    public List<Waypoint> Waypoints;
    [HideInInspector]
    public GameObject LOC;
    public List<Waypoint> Exits;
    public List<Waypoint> AnimWaypoints;
    [NonSerialized]
    public Dictionary<EWaypointTaskType, List<Waypoint>> CategorisedAnimWaypoints;
    [NonSerialized]
    public Dictionary<EWaypointTaskType, List<Waypoint>> CategorisedExits;
    [NonSerialized]
    public Dictionary<EWaypointTaskType, List<HashSet<Waypoint>>> FreeAnimWaypoints;
    [NonSerialized]
    public Dictionary<SectionSegment, Dictionary<EWaypointTaskType, List<HashSet<Waypoint>>>> FreeAnimWaypointsBySegments;
    [NonSerialized]
    public Dictionary<SectionSegment, HashSet<Waypoint>> DCIdle;
    [NonSerialized]
    public Dictionary<SectionSegment, Dictionary<SectionSegment, Waypoint>> DCSegmentTransition;
    [NonSerialized]
    public Dictionary<SectionSegment, List<Waypoint>> ExitsBySegment;
    [NonSerialized]
    public Dictionary<SectionSegment, Dictionary<EWaypointTaskType, HashSet<Waypoint>>> ActionsBySegments;

    private List<EWaypointTaskType> flagList;

    private void Awake()
    {
        Assert.IsTrue(Waypoints.Count > 0);
        Assert.IsTrue(Exits.Count > 0);
        flagList = Utils.ListWaypointsFlags();

        CategorisedAnimWaypoints = new Dictionary<EWaypointTaskType, List<Waypoint>>();
        CategorisedExits = new Dictionary<EWaypointTaskType, List<Waypoint>>();
        FreeAnimWaypoints = new Dictionary<EWaypointTaskType, List<HashSet<Waypoint>>>();
        foreach (var taskType in flagList)
        {
            CategorisedAnimWaypoints[taskType] = new List<Waypoint>();
            CategorisedExits[taskType] = new List<Waypoint>();
            FreeAnimWaypoints[taskType] = new List<HashSet<Waypoint>>() { new HashSet<Waypoint>(), new HashSet<Waypoint>() };
        }

        foreach (var waypoint in Exits)
        {
            foreach (var taskType in flagList)
            {
                if ((waypoint.Data.PossibleTasks & taskType) == taskType)
                {
                    CategorisedExits[taskType].Add(waypoint);
                }
            }
        }

        var subsection = GetComponent<SubSectionRoom>();
        string subsectionString = "";
        if (subsection != null)
        {
            subsectionString = subsection.ParentSection.name + ", subsection: " + (subsection.ParentSection.SubsectionRooms.IndexOf(subsection) + 1);
        }
        FreeAnimWaypointsBySegments = new Dictionary<SectionSegment, Dictionary<EWaypointTaskType, List<HashSet<Waypoint>>>>();
        foreach (var waypoint in AnimWaypoints)
        {
            foreach (var taskType in flagList)
            {
                if ((waypoint.Data.PossibleTasks & taskType) == taskType)
                {
                    CategorisedAnimWaypoints[taskType].Add(waypoint);
                    FreeAnimWaypoints[taskType][0].Add(waypoint);
                    FreeAnimWaypoints[taskType][1].Add(waypoint);
                }
            }
            if (!flagList.Contains(waypoint.Data.PossibleTasks))
            {
                Debug.LogError(subsectionString + ", waypoint doesn't have proper task type", waypoint);
            }
            else if (waypoint.Data.PossibleTasks != EWaypointTaskType.Normal)
            {
                if (waypoint.Data.Segment == null)
                {
                    Debug.LogError(subsectionString + ", waypoint " + Enum.GetName(typeof(EWaypointAnimType), waypoint.Data.PossibleTasks) + ", doesn't have parent segment", waypoint);
                }
                else
                {
                    if (!FreeAnimWaypointsBySegments.TryGetValue(waypoint.Data.Segment, out var animDict))
                    {
                        animDict = new Dictionary<EWaypointTaskType, List<HashSet<Waypoint>>>();
                        FreeAnimWaypointsBySegments[waypoint.Data.Segment] = animDict;
                    }
                    if (!animDict.TryGetValue(waypoint.Data.PossibleTasks, out var waypoints))
                    {
                        waypoints = new List<HashSet<Waypoint>>() { new HashSet<Waypoint>(), new HashSet<Waypoint>() };
                        animDict[waypoint.Data.PossibleTasks] = waypoints;
                    }
                    waypoints[0].Add(waypoint);
                    waypoints[1].Add(waypoint);
                }
            }
        }

        ExitsBySegment = new Dictionary<SectionSegment, List<Waypoint>>();

        var text = subsection == null ? "DECK" : $"{subsection.ParentSection.name}/{subsection.name}";
        if (Waypoints == null && Waypoints.Count == 0)
        {
            Debug.LogError("Waypoints null or empty, " + text, this);
        }
        foreach (Transform wayp in Waypoints[0].transform.parent)
        {
            if (!wayp.TryGetComponent(out Waypoint waypoint))
            {
                Debug.LogError("Non-Waypoint object in Waypoints, " + text, wayp);
                continue;
            }
            int waypointIndex = Waypoints.IndexOf(waypoint);
            var indexText = waypointIndex == -1 ? "" : $"in list: {waypointIndex + 1}, ";
            var newText = $"{waypoint.name}, {indexText}in hierarchy: {wayp.GetSiblingIndex() + 1}, {text}";
            if (waypointIndex == -1)
            {
                Debug.LogError("Waypoint not in waypoints list " + newText, waypoint);
            }
            if (waypoint.Data.AnimType == EWaypointAnimType.ActionAnim && !AnimWaypoints.Contains(waypoint))
            {
                Debug.LogError("Action waypoint not in action waypoints list " + newText, waypoint);
            }
            if (waypoint.Data.AnimType == EWaypointAnimType.Exit && !Exits.Contains(waypoint))
            {
                Debug.LogError("Exit waypoint not in exit waypoints list " + newText, waypoint);
            }
        }

        SetupWaypoints(null);
    }

#if UNITY_EDITOR
    private void Start()
    {
        if (transform.parent.name != "Deck")
        {
            var subsection = GetComponent<SubSectionRoom>();
            string sectionText = subsection.ParentSection.name + " subsection " + (subsection.ParentSection.SubsectionRooms.IndexOf(subsection) + 1);
            CheckExits(sectionText);

            for (int i = 0; i < Waypoints.Count; i++)
            {
                if (Waypoints[i].Data.AnimType > EWaypointAnimType.DCIdle)
                {
                    if (Waypoints[i].Data.ExitSegmentOtherSide == null)
                    {
                        Debug.LogWarning(sectionText + " waypoint " + (i + 1) + " has no reference to segment" + (Waypoints[i].Data.AnimType == EWaypointAnimType.DCSegmentTransition ? "(in prefab)" : "") + " to transition to", Waypoints[i]);
                    }
                    else if (Waypoints[i].Data.Segment == Waypoints[i].Data.ExitSegmentOtherSide)
                    {
                        Debug.LogWarning(sectionText + " waypoint " + (i + 1) + " exits to the same segment it is", Waypoints[i]);
                    }
                }
            }

            bool wreck = subsection.ParentSection.name.StartsWith("WreckSection");
            foreach (var segment in subsection.Segments)
            {
                var text = $"{segment.name} not in {subsection.ParentSection.name}/{subsection.name}";
                if (!DCIdle.TryGetValue(segment, out var set) || set.Count == 0)
                {
                    Debug.LogError("DCIdle - " + text);
                    continue;
                }
                if (!DCSegmentTransition.TryGetValue(segment, out var dict) || dict.Count == 0)
                {
                    bool found = false;
                    foreach (var dir in segment.NeighboursDirectionDictionary.Values)
                    {
                        if (dir == ENeighbourDirection.Left || dir == ENeighbourDirection.Right)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        Debug.LogError("DCSegmentTransition - " + text);
                        continue;
                    }
                }
                bool exit = false;
                foreach (var pair in segment.NeighboursDirectionDictionary)
                {
                    if (!pair.Key.NeighboursDirectionDictionary.TryGetValue(segment, out var dir) || !DirectionHelper.TryGetValue(dir, out var dir2) || pair.Value != dir2)
                    {
                        text = $"{segment.name} in {subsection.ParentSection.name}/{subsection.name}";
                        Debug.LogError($"Neighbours dictionary mismatch with ({segment.name} in {subsection.ParentSection.name}/{subsection.name}) AND ({pair.Key.name} in {pair.Key.Parent.ParentSection.name}/{pair.Key.Parent.name})");
                        exit = true;
                        break;
                    }
                    if ((dir == ENeighbourDirection.Left || dir == ENeighbourDirection.Right) && (!DCSegmentTransition.TryGetValue(segment, out var dictor) || !dictor.ContainsKey(pair.Key)))
                    {
                        text = $"{segment.name} in {subsection.ParentSection.name}/{subsection.name}";
                        Debug.LogError($"{segment.name} in {subsection.ParentSection.name}/{subsection.name}) has transition to ({pair.Key.name} in {pair.Key.Parent.ParentSection.name}/{pair.Key.Parent.name}");
                        exit = true;
                        break;
                    }
                }
                if (exit)
                {
                    continue;
                }
                if (!ExitsBySegment.TryGetValue(segment, out var list) || list.Count == 0)
                {
                    Debug.LogError("ExitsBySegment - " + text);
                    continue;
                }
                if (wreck)
                {
                    continue;
                }

                int count = 0;
                foreach (var waypoint in AnimWaypoints)
                {
                    if (waypoint.Data.PossibleTasks == EWaypointTaskType.Normal)
                    {
                        count++;
                    }
                }
                if (count < 4)
                {
                    Debug.LogError($"not enough action waypoints({count}) Normal - " + text);
                }
                if (ActionsBySegments.TryGetValue(segment, out var waypoints))
                {
                    if (!CheckWaypoints(waypoints, EWaypointTaskType.Firefighting, text))
                    {
                        continue;
                    }
                    if (!CheckWaypoints(waypoints, EWaypointTaskType.Repair, text))
                    {
                        continue;
                    }
                    if (!CheckWaypoints(waypoints, EWaypointTaskType.Rescue, text))
                    {
                        continue;
                    }
                    if (!CheckWaypoints(waypoints, EWaypointTaskType.Waterpump, text))
                    {
                        continue;
                    }
                    //if (!CheckWaypoints(waypoints, EWaypointTaskType.Rescue2, text))
                    //{
                    //    continue;
                    //}
                    //if (!CheckWaypoints(waypoints, EWaypointTaskType.Rescue3, text))
                    //{
                    //    continue;
                    //}
                }
                else
                {
                    Debug.LogError("ActionsBySegments - " + text);
                    continue;
                }
            }

            //foreach (var segment in GetComponent<SubSectionRoom>().Segments)
            //{
            //    if (!ExitsBySegment.TryGetValue(segment, out List<Waypoint> waypoints2) || waypoints2.Count == 0)
            //    {
            //        Debug.LogWarning(segment.name + " has no exits");
            //    }
            //}
        }

        string log = ValidateWaypoints();
        if (log.Length > 0)
        {
            Debug.LogWarning(log, this);
        }
    }

    public string ValidateWaypoints()
    {
        var builder = new StringBuilder();
        var childrenWaypoints = GetComponentsInChildren<Waypoint>();
        if (childrenWaypoints.Length != Waypoints.Count)
        {
            builder.AppendLine("Waypoints data in path - waypoints children in path mismatch");
        }

        foreach (var waypoint in childrenWaypoints)
        {
            if (!Waypoints.Contains(waypoint))
            {
                builder.Append("Waypoint not in data: ");
                builder.AppendLine(waypoint.transform.position.ToString("F3"));
            }
        }

        var dict = new Dictionary<EWaypointTaskType, List<Waypoint>>();
        foreach (var waypoint in Waypoints)
        {
            if (waypoint.Data.AnimType == EWaypointAnimType.ActionAnim)
            {
                if (!flagList.Contains(waypoint.Data.PossibleTasks))
                {
                    builder.Append("Waypoint task not specified: ");
                    builder.AppendLine(waypoint.transform.position.ToString("F3"));
                }
                else
                {
                    if (!dict.TryGetValue(waypoint.Data.PossibleTasks, out List<Waypoint> waypoints))
                    {
                        waypoints = new List<Waypoint>();
                        dict[waypoint.Data.PossibleTasks] = waypoints;
                    }
                    waypoints.Add(waypoint);
                }
            }
            else //if (waypoint.Data.AnimType < EWaypointAnimType.DCIdle)
            {
                if (waypoint.Data.PossibleTasks != EWaypointTaskType.All)
                {
                    builder.Append("Waypoint not-action task not available to all: ");
                    if (transform.parent.name == "Deck")
                    {
                        builder.AppendLine(name);
                    }
                    else
                    {
                        var subsection = GetComponent<SubSectionRoom>();
                        builder.AppendLine(subsection.ParentSection.name + ", subsection: " + (subsection.ParentSection.SubsectionRooms.IndexOf(subsection) + 1));
                    }
                    builder.AppendLine(waypoint.transform.position.ToString("F3"));
                    builder.AppendLine(waypoint.GetInstanceID().ToString());
                    builder.AppendLine(waypoint.gameObject.GetInstanceID().ToString());
                }

            }
        }
#if UNITY_EDITOR
        foreach (var exit in Exits)
        {
            foreach (var exit2 in Exits)
            {
                if (exit != exit2)
                {
                    try
                    {
                        InstanceData.ShortestPath(exit, exit2);
                    }
                    catch (Exception _)
                    {

                    }
                }
            }
            foreach (var anim in AnimWaypoints)
            {
                try
                {
                    InstanceData.ShortestPath(exit, anim);
                }
                catch (Exception _)
                {

                }
            }
        }
#endif

        //foreach (var type in typeList)
        //{
        //    if (type == EWaypointTaskType.Normal || dcCount > 0)
        //    {
        //        if (dict.TryGetValue(type, out List<Waypoint> waypoints))
        //        {
        //            int needed = type == EWaypointTaskType.Normal ? (sailorCount + officerCount) : dcCount;
        //            if (type != EWaypointTaskType.Rescue)
        //            {
        //                needed++;
        //            }
        //            if (waypoints.Count < needed)
        //            {
        //                builder.Append("Not enough anim waypoints for ");
        //                builder.Append(type.ToString());
        //                builder.Append(" - needs at least ");
        //                builder.AppendLine(needed.ToString());
        //            }
        //        }
        //        else if(type != EWaypointTaskType.Waterpump && type != EWaypointTaskType.RepairDoor)
        //        {
        //            builder.Append("No anim waypoints for ");
        //            builder.AppendLine(type.ToString());
        //        }
        //    }
        //    else
        //    {
        //        if (dict.ContainsKey(type))
        //        {
        //            builder.Append("Deck should not contain waypoints for task ");
        //            builder.AppendLine(type.ToString());
        //        }
        //    }
        //}
        return builder.ToString().Trim();
    }
#else
    private void Start()
    {
        CheckExits("");
    }
#endif

    public void SetupWaypoints(StringBuilder text)
    {
        DCIdle = new Dictionary<SectionSegment, HashSet<Waypoint>>();
        DCSegmentTransition = new Dictionary<SectionSegment, Dictionary<SectionSegment, Waypoint>>();

        var set = new HashSet<Waypoint>();
        foreach (var waypoint in Waypoints)
        {
            if (text == null)
            {
                if (!set.Add(waypoint))
                {
                    Debug.LogError("Duplicate waypoint in all waypoints", waypoint);
                }
                if (waypoint.transform.parent != Waypoints[0].transform.parent)
                {
                    Debug.LogError("Waypoint outside hierarchy", waypoint);
                }
            }
            if (waypoint.Data.AnimType == EWaypointAnimType.DCIdle)
            {
                if (waypoint.Data.Segment == null)
                {
                    continue;
                }
                if (!DCIdle.TryGetValue(waypoint.Data.Segment, out HashSet<Waypoint> set2))
                {
                    set2 = new HashSet<Waypoint>();
                    DCIdle[waypoint.Data.Segment] = set2;
                }
                set2.Add(waypoint);
            }
            else if (waypoint.Data.AnimType == EWaypointAnimType.DCSegmentTransition || waypoint.Data.AnimType == EWaypointAnimType.DCSectionTransition)
            {
                if (waypoint.Data.ExitSegmentOtherSide == null || waypoint.Data.ExitSegmentOtherSide == waypoint.Data.Segment)
                {
                    if (text != null && (waypoint.Data.ExitSegmentOtherSide != null || waypoint.Data.AnimType != EWaypointAnimType.DCSectionTransition))
                    {
                        text.AppendLine("waypoint " + Waypoints.IndexOf(waypoint) + "in segment " + (waypoint.Data.Segment == null ? "none" : waypoint.Data.Segment.name) + " " +
                            (waypoint.Data.ExitSegmentOtherSide == null ? "doesn't specified exit" : "exit to the same segment it is"));
                    }
                }
                else
                {
                    if (waypoint.Data.Segment == null)
                    {
                        var error = "Null segment for waypoint";
                        if (text == null)
                        {
                            Debug.LogWarning(error, waypoint);
                        }
                        else
                        {
                            text.AppendLine(error);
                        }
                        continue;
                    }
                    if (!DCSegmentTransition.TryGetValue(waypoint.Data.Segment, out var segmentDict))
                    {
                        segmentDict = new Dictionary<SectionSegment, Waypoint>();
                        DCSegmentTransition[waypoint.Data.Segment] = segmentDict;
                    }
                    var segment = waypoint.Data.ExitSegmentOtherSide;
                    if (segmentDict.ContainsKey(segment))
                    {
                        var error = waypoint.Data.Segment.name + " has more than one door to " + segment.name + " " + Waypoints.IndexOf(segmentDict[segment]) + " " + Waypoints.IndexOf(waypoint);
                        if (text == null)
                        {
                            Debug.LogWarning(error, waypoint.Data.Segment);
                        }
                        else
                        {
                            text.AppendLine(error);
                        }
                    }
                    segmentDict[segment] = waypoint;
                }
            }
        }

        ActionsBySegments = new Dictionary<SectionSegment, Dictionary<EWaypointTaskType, HashSet<Waypoint>>>();
        set.Clear();
        foreach (var waypoint in AnimWaypoints)
        {
            if (text == null)
            {
                if (!Waypoints.Contains(waypoint))
                {
                    Debug.LogError("Anim waypoint not in Waypoints list", waypoint);
                }
                if (!set.Add(waypoint))
                {
                    Debug.LogError("Anim waypoint duplicated", waypoint);
                }
                if (waypoint.Data.AnimType != EWaypointAnimType.ActionAnim)
                {
                    Debug.LogError("Non anim waypoint in AnimWaypoints list", waypoint);
                }
            }

            var task = waypoint.Data.PossibleTasks;
            if (task == EWaypointTaskType.Normal)
            {
                continue;
            }
            if (waypoint.Data.Segment == null)
            {
                var error = "Anim waypoint (" + waypoint.transform.position.ToString("F3") + "), nr " + Waypoints.IndexOf(waypoint) + " has no segment";
                if (text == null)
                {
                    Debug.LogError(error, waypoint);
                }
                else
                {
                    text.AppendLine(error);
                }
                continue;
            }
            if (task != EWaypointTaskType.Firefighting && task != EWaypointTaskType.Repair && task != EWaypointTaskType.RepairDoor && ((task & EWaypointTaskType.Rescues) == 0) && task != EWaypointTaskType.Waterpump)
            {
                var error = "Anim waypoint (" + waypoint.transform.position.ToString("F3") + "), nr " + Waypoints.IndexOf(waypoint) + " has bad task";
                if (text == null)
                {
                    Debug.LogError(error, waypoint);
                }
                else
                {
                    text.AppendLine(error);
                }
                continue;
            }
            if (!ActionsBySegments.TryGetValue(waypoint.Data.Segment, out var segmentTasks))
            {
                segmentTasks = new Dictionary<EWaypointTaskType, HashSet<Waypoint>>();
                ActionsBySegments[waypoint.Data.Segment] = segmentTasks;
            }
            if (!segmentTasks.TryGetValue(waypoint.Data.PossibleTasks, out var taskWaypoints))
            {
                taskWaypoints = new HashSet<Waypoint>();
                segmentTasks[waypoint.Data.PossibleTasks] = taskWaypoints;
            }
            taskWaypoints.Add(waypoint);
        }
    }

    private bool CheckWaypoints(Dictionary<EWaypointTaskType, HashSet<Waypoint>> waypoints, EWaypointTaskType type, string text, int count = 3)
    {
        if (waypoints.TryGetValue(type, out var set))
        {
            if (set.Count < count)
            {
                Debug.LogError($"not enough action waypoints({set.Count}) {type} - " + text);
                return false;
            }
        }
        else
        {
            Debug.LogError($"no action waypoints for {type} - " + text);
            return false;
        }
        return true;
    }

    private void CheckExits(string sectionText)
    {
        foreach (var exit in Exits)
        {
            if (exit.Data.Segment == null)
            {
                Debug.LogWarning(sectionText + " exit " + Waypoints.IndexOf(exit) + " (" + exit.Trans.position.ToString("F3") + ") has no segment assigned");
                continue;
            }
            if (!ExitsBySegment.TryGetValue(exit.Data.Segment, out List<Waypoint> exitList))
            {
                exitList = new List<Waypoint>();
                ExitsBySegment[exit.Data.Segment] = exitList;
            }
            exitList.Add(exit);
        }

        var subsection = GetComponent<SubSectionRoom>();
        if (subsection != null)
        {
            foreach (var segment in subsection.Segments)
            {
                if (!ExitsBySegment.ContainsKey(segment))
                {
                    Debug.Log(sectionText + " has no exit from " + segment.name);
                    continue;
                }
                if (!segment.TryGetComponent(out Neighbours _))
                {
                    Debug.Log(sectionText + ", segment " + segment.name + " doesn't have Neighbours script");
                    continue;
                }
                if (segment.HorizontalNeighbours.Contains(segment) && !DCSegmentTransition.ContainsKey(segment))
                {
                    Debug.Log(sectionText + " has no segment transition from " + segment.name);
                    continue;
                }
            }
        }
    }
}
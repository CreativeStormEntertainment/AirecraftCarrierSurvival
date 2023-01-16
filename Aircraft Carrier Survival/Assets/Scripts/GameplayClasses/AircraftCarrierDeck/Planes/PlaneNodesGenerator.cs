using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlaneNodesGenerator
{
    private const string Line = "Line_";
    private const string State = "State_";
    private const string Way = "Way_";
    private const string Elevator = "Elevator";

    private static readonly int ElevatorLength = Elevator.Length;

    public Dictionary<EPlaneNodeGroup, PlaneNodeGroup> GroupNodes;
    public Dictionary<EPlaneNodeGroup, Dictionary<EPlaneNodeGroup, PlaneNodeGroup>> PathNodes;
    public List<List<Quaternion>> DeckLaunchingRotations;
    public List<List<Quaternion>> DeckRecoveringRotations;
    public List<PlaneNode> AllNodes;

    public PlaneNodesGenerator(Transform root)
    {
        GroupNodes = new Dictionary<EPlaneNodeGroup, PlaneNodeGroup>();
        PathNodes = new Dictionary<EPlaneNodeGroup, Dictionary<EPlaneNodeGroup, PlaneNodeGroup>>();
        DeckLaunchingRotations = new List<List<Quaternion>>();
        DeckRecoveringRotations = new List<List<Quaternion>>();

        foreach (Transform child in root)
        {
            if (child.name.StartsWith(State))
            {
                var groupType = (EPlaneNodeGroup)Enum.Parse(typeof(EPlaneNodeGroup), child.name.Substring(State.Length));
                List<List<Quaternion>> list = null;
                if (groupType == EPlaneNodeGroup.DeckLaunching)
                {
                    list = DeckLaunchingRotations;
                }
                else if (groupType == EPlaneNodeGroup.DeckRecovering)
                {
                    list = DeckRecoveringRotations;
                }
                GroupNodes[groupType] = GetGroup(child, list);
            }
            else
            {
                if (!child.name.StartsWith(Way))
                {
                    Debug.LogError("Unknown go in root plane waypoints, " + child.name, child);
                    continue;
                }
                int index = child.name.IndexOf("_", State.Length);
                if (index == -1 || index <= State.Length)
                {
                    Debug.LogError("Way go " + child.name + " has invalid name protocol: \"Way_FROMGROUP_TOGROUP\"", child);
                    continue;
                }
                var fromString = child.name.Substring(Way.Length, index - Way.Length);
                var fromGroup = (EPlaneNodeGroup)Enum.Parse(typeof(EPlaneNodeGroup), fromString);
                var toString = child.name.Substring(index + 1);
                var toGroup = (EPlaneNodeGroup)Enum.Parse(typeof(EPlaneNodeGroup), toString);
                if (!PathNodes.TryGetValue(fromGroup, out var toDict))
                {
                    toDict = new Dictionary<EPlaneNodeGroup, PlaneNodeGroup>();
                    PathNodes[fromGroup] = toDict;
                }
                if (toDict.ContainsKey(toGroup))
                {
                    Debug.LogError("Way from " + fromString + " to " + toString + " already defined", child);
                    continue;
                }

                toDict[toGroup] = GetGroup(child, null);
            }
        }

        for (int i = 0; i < (int)EPlaneNodeGroup.Count; i++)
        {
            if (!GroupNodes.TryGetValue((EPlaneNodeGroup)i, out var group))
            {
                Debug.LogError("There is no " + Enum.GetName(typeof(EPlaneNodeGroup), (EPlaneNodeGroup)i) + " state");
            }
            else if ((group.LineNodes.Count == 1) == 
                (i == (int)EPlaneNodeGroup.DeckLaunching || i == (int)EPlaneNodeGroup.DeckRecovering))
            {
                Debug.LogError("Line count mismatch in " + Enum.GetName(typeof(EPlaneNodeGroup), (EPlaneNodeGroup)i) + " state, there should be " + (group.LineNodes.Count == 1 ? "more than" : "just") + " one line");
            }
        }

        CheckPath(EPlaneNodeGroup.DeckLaunching, EPlaneNodeGroup.DeckRecovering);
        CheckPath(EPlaneNodeGroup.DeckLaunching, EPlaneNodeGroup.LiftLaunchingToHangar);
        CheckPath(EPlaneNodeGroup.DeckLaunching, EPlaneNodeGroup.Starting);
        if (CheckPath(EPlaneNodeGroup.DeckLaunching, EPlaneNodeGroup.WaitLaunching, out var to) && to.Count != 5)
        {
            Debug.LogError("There is too many ways from DeckLaunching!");
        }

        CheckPath(EPlaneNodeGroup.DeckRecovering, EPlaneNodeGroup.LiftRecoveringToHangar);
        if (CheckPath(EPlaneNodeGroup.DeckRecovering, EPlaneNodeGroup.WaitRecovering, out to) && to.Count != 3)
        {
            Debug.LogError("There is too many ways from DeckRecovering!");
        }

        CheckPath(EPlaneNodeGroup.Hangar, EPlaneNodeGroup.LiftLaunchingToDeck);
        if (CheckPath(EPlaneNodeGroup.Hangar, EPlaneNodeGroup.LiftRecoveringToDeck, out to) && to.Count != 2)
        {
            Debug.LogError("There is too many ways from Hangar!");
        }

        CheckPath(EPlaneNodeGroup.Landing, EPlaneNodeGroup.DeckRecovering);
        if (CheckPath(EPlaneNodeGroup.Landing, EPlaneNodeGroup.Hangar, out to) && to.Count != 2)
        {
            Debug.LogError("There is too many ways from Landing!");
        }

        if (CheckPath(EPlaneNodeGroup.Starting, EPlaneNodeGroup.AirLaunching, out to) && to.Count != 1)
        {
            Debug.LogError("There is too many ways from Starting!");
        }

        if (CheckPath(EPlaneNodeGroup.LiftLaunchingToDeck, EPlaneNodeGroup.DeckLaunching, out to) && to.Count != 1)
        {
            Debug.LogError("There is too many ways from LiftLaunchingToDeck!");
        }
        if (CheckPath(EPlaneNodeGroup.LiftRecoveringToDeck, EPlaneNodeGroup.DeckRecovering, out to) && to.Count != 1)
        {
            Debug.LogError("There is too many ways from LiftRecoveringToDeck!");
        }
        if (CheckPath(EPlaneNodeGroup.LiftLaunchingToHangar, EPlaneNodeGroup.Hangar, out to) && to.Count != 1)
        {
            Debug.LogError("There is too many ways from LiftLaunchingToHangar!");
        }
        if (CheckPath(EPlaneNodeGroup.LiftRecoveringToHangar, EPlaneNodeGroup.Hangar, out to) && to.Count != 1)
        {
            Debug.LogError("There is too many ways from LiftRecoveringToHangar!");
        }

        if (CheckPath(EPlaneNodeGroup.WaitLaunching, EPlaneNodeGroup.SwapLaunching, out to) && to.Count != 1)
        {
            Debug.LogError("There is too many ways from WaitLaunching!");
        }
        if (CheckPath(EPlaneNodeGroup.WaitRecovering, EPlaneNodeGroup.SwapRecovering, out to) && to.Count != 1)
        {
            Debug.LogError("There is too many ways from WaitRecovering!");
        }
        CheckPath(EPlaneNodeGroup.SwapLaunching);
        CheckPath(EPlaneNodeGroup.SwapRecovering);

        var set = new HashSet<PlaneNode>();
        AllNodes = new List<PlaneNode>();
        foreach (var node in GetAllNodesInGroup(GroupNodes))
        {
            if (set.Add(node))
            {
                AllNodes.Add(node);
            }
        }
        for (int i = 0; i < (int)EPlaneNodeGroup.Count; i++)
        {
            if (PathNodes.TryGetValue((EPlaneNodeGroup)i, out var dict))
            {
                foreach (var node in GetAllNodesInGroup(dict))
                {
                    if (set.Add(node))
                    {
                        AllNodes.Add(node);
                    }
                }
            }
        }

        var fromSwapLaunching = new Dictionary<EPlaneNodeGroup, PlaneNodeGroup>();
        fromSwapLaunching.Add(EPlaneNodeGroup.DeckLaunching, PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.SwapLaunching].Reverse());
        fromSwapLaunching.Add(EPlaneNodeGroup.WaitLaunching, PathNodes[EPlaneNodeGroup.WaitLaunching][EPlaneNodeGroup.SwapLaunching].Reverse());
        PathNodes[EPlaneNodeGroup.SwapLaunching] = fromSwapLaunching;

        var fromSwapRecovering = new Dictionary<EPlaneNodeGroup, PlaneNodeGroup>();
        fromSwapRecovering.Add(EPlaneNodeGroup.DeckRecovering, PathNodes[EPlaneNodeGroup.DeckRecovering][EPlaneNodeGroup.SwapRecovering].Reverse());
        fromSwapRecovering.Add(EPlaneNodeGroup.WaitRecovering, PathNodes[EPlaneNodeGroup.WaitRecovering][EPlaneNodeGroup.SwapRecovering].Reverse());
        PathNodes[EPlaneNodeGroup.SwapRecovering] = fromSwapRecovering;

        PathNodes[EPlaneNodeGroup.DeckLaunching].Add(EPlaneNodeGroup.LiftLaunchingToDeck, PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.LiftLaunchingToHangar].Reverse());
        PathNodes[EPlaneNodeGroup.DeckRecovering].Add(EPlaneNodeGroup.LiftRecoveringToDeck, PathNodes[EPlaneNodeGroup.DeckRecovering][EPlaneNodeGroup.LiftRecoveringToHangar].Reverse());

        PathNodes[EPlaneNodeGroup.DeckRecovering][EPlaneNodeGroup.DeckLaunching] = PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.DeckRecovering].Reverse();
        PathNodes[EPlaneNodeGroup.Starting][EPlaneNodeGroup.DeckLaunching] = PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.Starting].Reverse();

        PathNodes[EPlaneNodeGroup.WaitLaunching][EPlaneNodeGroup.DeckLaunching] = PathNodes[EPlaneNodeGroup.DeckLaunching][EPlaneNodeGroup.WaitLaunching].Reverse();
        PathNodes[EPlaneNodeGroup.WaitRecovering][EPlaneNodeGroup.DeckRecovering] = PathNodes[EPlaneNodeGroup.DeckRecovering][EPlaneNodeGroup.WaitRecovering].Reverse();
    }

    public PlaneNode GetNode(int index)
    {
        return AllNodes[index];
    }

    public int IndexOf(PlaneNode node)
    {
        return AllNodes.IndexOf(node);
    }

    private IEnumerable<PlaneNode> GetAllNodesInGroup(Dictionary<EPlaneNodeGroup, PlaneNodeGroup> dict)
    {
        for (int i = 0; i < (int)EPlaneNodeGroup.Count; i++)
        {
            if (dict.TryGetValue((EPlaneNodeGroup)i, out var group))
            {
                foreach (var line in group.LineNodes)
                {
                    foreach (var node in line)
                    {
                        yield return node;
                    }
                }
            }
        }
    }

    private PlaneNodeGroup GetGroup(Transform groupRoot, List<List<Quaternion>> rotations)
    {
        var result = new PlaneNodeGroup();
        var set = new HashSet<int>();
        for (int i = 0; i < groupRoot.childCount; i++)
        {
            result.LineNodes.Add(new List<PlaneNode>());
            if (rotations != null)
            {
                rotations.Add(new List<Quaternion>());
            }
        }
        foreach (Transform child in groupRoot)
        {
            int index;
            if (!child.name.StartsWith(Line) || !int.TryParse(child.name.Substring(Line.Length), out index) || (groupRoot.childCount < index || index < -1))
            {
                Debug.LogError("Gameobject " + child.name + " doesn't follow name protocol: \"Line_NUMBEROFLINE\" in " + groupRoot.parent.name, child);
                continue;
            }
            if (!set.Add(index))
            {
                Debug.LogError("Gameobject " + child.name + " has duplicated line " + index.ToString() + ", in " + groupRoot.parent.name, child);
                continue;
            }
            foreach (Transform node in child)
            {
                if (node.childCount > 0)
                {
                    Debug.LogWarning("Node in line " + index + ", in " + groupRoot.parent.name + " has children", node);
                }
                var planeNode = new PlaneNode(node.position);
                result.LineNodes[index - 1].Add(planeNode);
                if (rotations != null)
                {
                    rotations[index - 1].Add(node.rotation);
                }
                if (node.name.StartsWith(Elevator))
                {
                    if (node.TryGetComponent(out LiftNodeData data))
                    {
                        planeNode.ElevatorUp = data.Up;
                        planeNode.ElevatorDown = !data.Up;
                        planeNode.Lift = data.Lift;
                        if (data.WaitForNodeData != null)
                        {
                            if (data.WaitForNode == null)
                            {
                                Assert.IsNull(data.WaitForNodeData.WaitForNode, "Internal error ;(");
                                data.WaitForNodeData.WaitForNode = planeNode;
                            }
                            else
                            {
                                data.WaitForNode.WaitForNode = planeNode;
                                planeNode.WaitForNode = data.WaitForNode;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Node " + node.name + " needs to have script PlaneNodeData in " + child.name + "/" + groupRoot.parent.name, node);
                    }
                }
            }
            if (result.LineNodes[index - 1].Count == 0)
            {
                Debug.LogWarning("Line " + index + ", in " + groupRoot.parent.name + " is empty", child);
            }
        }
        return result;
    }

    private bool CheckPath(EPlaneNodeGroup from, EPlaneNodeGroup to, out Dictionary<EPlaneNodeGroup, PlaneNodeGroup> toDict)
    {
        if (!PathNodes.TryGetValue(from, out toDict) || !toDict.ContainsKey(to))
        {
            Debug.LogError("There is no way from " + Enum.GetName(typeof(EPlaneNodeGroup), from) + " to " + Enum.GetName(typeof(EPlaneNodeGroup), to));
            return false;
        }
        return true;
    }

    private void CheckPath(EPlaneNodeGroup from, EPlaneNodeGroup to)
    {
        if (!PathNodes.TryGetValue(from, out var toDict) || !toDict.ContainsKey(to))
        {
            Debug.LogError("There is no way from " + Enum.GetName(typeof(EPlaneNodeGroup), from) + " to " + Enum.GetName(typeof(EPlaneNodeGroup), to));
        }
    }

    private void CheckPath(EPlaneNodeGroup from)
    {
        if (PathNodes.ContainsKey(from))
        {
            Debug.LogError("There should be no way from " + Enum.GetName(typeof(EPlaneNodeGroup), from));
        }
    }
}

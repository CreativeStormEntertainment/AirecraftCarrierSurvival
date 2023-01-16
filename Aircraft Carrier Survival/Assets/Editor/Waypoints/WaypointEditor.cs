using GambitUtils;
using GPUInstancer.CrowdAnimations;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(Waypoint))]
public class WaypointEditor : Editor
{
    private static readonly string[] SectionOptions = { "1.1_meteo", "1.2_piloci", "1.3_pompy", "1.4_namierzanie", "1.5_paliwo", "1.6_komunikacja",
        "2.1_hangar", "2.2_szpital", "2.4_zaloga", "2.5_generatory",
        "3.2_bomby", "3.3_DC", "3.4_warsztat", "3.6_generatory", "3.7_generatory",
        "4.1_szatnie", "4.2_amunicja", "4.3_zaloga", "4.4_warsztaty", "4.5_generatory", "4.6_sterownia" };

    private AnimationManager animMan;
    private Waypoint waypoint;
    private string[] animNamesBasic;
    private int[] animIDsBasic;
    private Vector3 pos;
    private WorkerPath path;
    private string error;
    private List<int> maxIndexActions;

    private int rescueAnim;
    private bool rescuee;

    private int currentAnimTest;

    private void OnEnable()
    {
        error = "";
        waypoint = (Waypoint)target;
        if (SceneManager.GetActiveScene().name == WaypointWindow.SceneName)
        {
            path = waypoint.GetComponentInParent<WorkerPath>();
            pos = waypoint.transform.position;

            animMan = FindObjectOfType<AnimationManager>();
            Assert.IsNotNull(animMan, "Please provide animation manager to scene");

            animNamesBasic = new string[animMan.AnimData.AnimStrings.Count - animMan.AnimData.AnimGroups.Count];
            animIDsBasic = new int[animNamesBasic.Length];
            int i = 0;
            maxIndexActions = new List<int>();
            for (int j = 0; j < AnimUtils.Prefixes.Count; j++)
            {
                maxIndexActions.Add(0);
            }
            foreach (var pair in animMan.AnimData.AnimStrings)
            {
                if (animMan.AnimData.AnimGroups.ContainsKey(pair.Value))
                {
                    for (int j = 0; j < AnimUtils.Prefixes.Count; j++)
                    {
                        if (pair.Key.StartsWith(AnimUtils.Prefixes[j]))
                        {
                            int index = AnimUtils.GetActionIndex(pair.Key);
                            if (index > maxIndexActions[j])
                            {
                                maxIndexActions[j] = index;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    animNamesBasic[i] = pair.Key;
                    animIDsBasic[i] = pair.Value;
                    i++;
                }
            }
            for (int j = 0; j < maxIndexActions.Count; j++)
            {
                Assert.IsFalse(maxIndexActions[j] == 0, j.ToString());
            }
        }
    }

    private void OnDisable()
    {
        error = "";
    }

    private void OnSceneGUI()
    {
        if (SceneManager.GetActiveScene().name == WaypointWindow.SceneName)
        {
            if (waypoint != null)
            {
                if (waypoint.Data.IsLocked)
                {
                    waypoint.transform.position = pos;
                }
                else if (pos != waypoint.transform.position)
                {
                    pos = waypoint.transform.position;
                    waypoint.PositionChanged();
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        if (SceneManager.GetActiveScene().name == WaypointWindow.SceneName)
        {
            EditorGUI.BeginChangeCheck();
            var data = waypoint.Data;
            bool enabled = GUI.enabled;
            GUI.enabled = !data.IsLocked;

            var animValue = (EWaypointAnimType)EditorGUILayout.EnumPopup("Animation type", data.AnimType);
            int prefix = (int)AnimUtils.GetPrefix(data.PossibleTasks);
            if (data.AnimType != animValue)
            {
                data.AnimType = animValue;
                path.Exits.Remove(waypoint);
                path.AnimWaypoints.Remove(waypoint);
                data.PossibleTasks = EWaypointTaskType.All;
                switch (data.AnimType)
                {
                    case EWaypointAnimType.Exit:
                        path.Exits.Add(waypoint);
                        data.AnimName = "";
                        data.AnimID = -1;
                        break;
                    case EWaypointAnimType.BasicAnim:
                        data.AnimName = AnimationManager.Walk;
                        data.AnimID = animMan.AnimData.WalkID;
                        break;
                    case EWaypointAnimType.ActionAnim:
                        path.AnimWaypoints.Add(waypoint);

                        SetActionName(ref data, AnimUtils.Prefixes[prefix], 0, maxIndexActions[prefix]);
                        data.PossibleTasks = EWaypointTaskType.Normal;
                        break;
                    case EWaypointAnimType.DCIdle:
                        data.PossibleTasks = EWaypointTaskType.Normal;
                        break;
                    case EWaypointAnimType.DCSegmentTransition:
                    case EWaypointAnimType.DCSectionTransition:
                        data.PossibleTasks = EWaypointTaskType.Normal;
                        break;
                }
            }

            switch (data.AnimType)
            {
                case EWaypointAnimType.Exit:
                    //data.PossibleTasks = (EWaypointTaskType)EditorGUILayout.EnumPopup("AvailableTasks", data.PossibleTasks);
                    break;
                case EWaypointAnimType.BasicAnim:
                    //data.AnimID = EditorGUILayout.IntPopup("Animation", data.AnimID, animNamesBasic, animIDsBasic);
                    //data.AnimName = animNamesBasic[IndexOf(animIDsBasic, data.AnimID)];
                    data.CanFrighten = EditorGUILayout.Toggle("Can frighten", data.CanFrighten);
                    break;
                case EWaypointAnimType.ActionAnim:
                    bool injureOptions = data.PossibleTasks == EWaypointTaskType.Rescue || data.PossibleTasks == EWaypointTaskType.Rescue2 || data.PossibleTasks == EWaypointTaskType.Rescue3;
                    if (!injureOptions)
                    {
                        int index = EditorGUILayout.IntSlider(AnimUtils.GetActionIndex(data.AnimName), 1, maxIndexActions[prefix]);
                        SetActionName(ref data, AnimUtils.Prefixes[prefix], index, maxIndexActions[prefix]);

                        data.MinRepeat = EditorGUILayout.IntField("MinRepeat", data.MinRepeat);
                        data.MaxRepeat = EditorGUILayout.IntField("MaxRepeat", data.MaxRepeat);
                    }

                    var prevTask = data.PossibleTasks;
                    data.PossibleTasks = (EWaypointTaskType)EditorGUILayout.EnumPopup("AvailableTasks", data.PossibleTasks);
                    switch (data.PossibleTasks)
                    {
                        case EWaypointTaskType.Normal:
                        case EWaypointTaskType.Firefighting:
                        case EWaypointTaskType.Rescue:
                        case EWaypointTaskType.Repair:
                        case EWaypointTaskType.RepairDoor:
                        case EWaypointTaskType.Waterpump:
                        case EWaypointTaskType.Rescue2:
                        case EWaypointTaskType.Rescue3:
                            break;
                        default:
                            data.PossibleTasks = prevTask;
                            break;
                    }
                    if (injureOptions)
                    {
                        data.InjuredWaypoint = EditorGUILayout.Toggle("Waypoint for injured", data.InjuredWaypoint);

                        EditorGUILayout.LabelField(" ");
                        rescueAnim = EditorGUILayout.IntSlider("Rescue anim to test:", rescueAnim, 0, 3);
                        rescuee = EditorGUILayout.Toggle("Test rescuee anim", rescuee);
                    }

                    if (data.PossibleTasks == EWaypointTaskType.Normal)
                    {
                        data.CanFrighten = EditorGUILayout.Toggle("Can frighten", data.CanFrighten);
                    }
                    break;
                case EWaypointAnimType.DCIdle:
                    break;
                case EWaypointAnimType.DCSegmentTransition:
                case EWaypointAnimType.DCSectionTransition:
                    data.ExitSegmentOtherSide = EditorGUILayout.ObjectField("Transition to: ", data.ExitSegmentOtherSide, typeof(SectionSegment), true) as SectionSegment;
                    break;
            }

            GUI.enabled = enabled;
            data.IsLocked = EditorGUILayout.Toggle("Is Locked", data.IsLocked);
            if (data.AnimType == EWaypointAnimType.ActionAnim)
            {
                EditorGUILayout.LabelField(" ");
                var animTests = SceneUtils.FindObjectsOfType<AnimTest>();
                var animTest = animTests[currentAnimTest];
                if (GUILayout.Button("Test anim"))
                {
                    //var animTest = FindObjectOfType<AnimTest>();
                    //animTest.transform.position = waypoint.transform.position;
                    //Vector3 dir = (waypoint.transform.position - waypoint.Branches[0].transform.position).normalized;
                    //animTest.transform.rotation = waypoint.Branches.Count > 0 ?
                    //    Quaternion.Euler(0f, Vector3.Angle(dir, Vector3.back), 0f)  : Quaternion.identity;
                    //animTest.Active = false;
                    //var group = animMan.AnimData.AnimGroups[data.AnimID];
                    //animTest.Clips[0] = group.inID;
                    //animTest.Clips[1] = group.outID;
                    //animTest.Current = 0;
                    //animTest.PrevTime = EditorApplication.timeSinceStartup;
                    //animTest.FrameTime = 0d;
                    //animTest.Active = true;

                    List<Vector3> path = null;
                    if (animTest.AnimType == AnimTest.TestType.GoToInOut || animTest.AnimType == AnimTest.TestType.GoToAllOnce)
                    {
                        path = new List<Vector3>();

                        var trans = waypoint.transform;

                        while (true)
                        {
                            if (trans.TryGetComponent(out WorkerPath workerPath))
                            {
                                int count = 0;
                                var dict = new Dictionary<Waypoint, List<Waypoint>>();
                                var queueList = new List<System.Tuple<Waypoint, float>>();
                                var set = new HashSet<Waypoint>();
                                dict[workerPath.Exits[0]] = new List<Waypoint>() { workerPath.Exits[0] };
                                queueList.Add(new System.Tuple<Waypoint, float>(workerPath.Exits[0], 0f));
                                bool found = false;
                                while (queueList.Count > 0)
                                {
                                    Assert.IsFalse(++count > 1_000_000);
                                    var item = queueList[0];
                                    if (set.Add(item.Item1))
                                    {
                                        if (item.Item1 == waypoint)
                                        {
                                            foreach (var point in dict[item.Item1])
                                            {
                                                path.Add(point.transform.position);
                                            }
                                            found = true;
                                            break;
                                        }
                                        var partialPath = dict[item.Item1];
                                        foreach (var branch in item.Item1.Branches)
                                        {
                                            if (!set.Contains(branch) && !dict.ContainsKey(branch))
                                            {
                                                var branchPath = new List<Waypoint>(partialPath);
                                                branchPath.Add(branch);
                                                dict.Add(branch, branchPath);
                                                queueList.Add(new System.Tuple<Waypoint, float>(branch, item.Item2 + Vector3.SqrMagnitude(branch.transform.position - item.Item1.transform.position)));
                                            }
                                        }
                                    }
                                    dict.Remove(item.Item1);
                                    queueList.RemoveAt(0);
                                    queueList.Sort((x, y) => Comparer<float>.Default.Compare(x.Item2, y.Item2));
                                }
                                Assert.IsTrue(found);
                                break;
                            }
                            else
                            {
                                trans = trans.parent;
                            }
                        }
                    }

                    Vector3 dir = (waypoint.transform.position - waypoint.Branches[0].transform.position);
                    var list = (animMan.GPUICrowdManager.prototypeList[0] as GPUICrowdPrototype).animationData.clipDataList;
                    animTest.Setup(waypoint.transform, dir, animMan, list, data, rescuee, rescueAnim, path);
                }
                currentAnimTest = EditorGUILayout.IntSlider("Which anim tester:", currentAnimTest, 0, animTests.Count - 1);
                animTest.AnimType = (AnimTest.TestType)EditorGUILayout.EnumPopup("AnimType", animTest.AnimType);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (data.IsLocked != waypoint.Data.IsLocked)
                {
                    pos = waypoint.transform.position;
                }
                //if (data.PossibleTasks != EWaypointTaskType.All && waypoint.Branches.Count > 1)
                //{
                //    data.PossibleTasks = EWaypointTaskType.All;
                //    error = "Cannot change possible tasks, because this waypoint is not an alone node.\nOnly for waypoints with 1 connection";
                //}

                if (data.PossibleTasks != waypoint.Data.PossibleTasks)
                {
                    prefix = (int)AnimUtils.GetPrefix(data.PossibleTasks);
                    SetActionName(ref data, AnimUtils.Prefixes[prefix], 0, maxIndexActions[prefix]);
                }

                Undo.RecordObject(waypoint, "Changed waypoint");
                waypoint.Data = data;
            }

            if (error.Length > 0)
            {
                var content = new GUIContent(error);
                EditorGUILayout.LabelField(content, GUILayout.Height(GUI.skin.label.CalcSize(content).y));
            }

            //enabled = GUI.enabled;
            //GUI.enabled = !data.IsLocked;
            //if (GUILayout.Button("Delete"))
            //{
            //    DestroyImmediate(waypoint.gameObject);
            //    waypoint = null;
            //}
            //GUI.enabled = enabled;
        }
        else
        {
            var style = GUI.skin.label;
            Label(style, "PossibleTasks", waypoint.Data.PossibleTasks.ToString());
            Label(style, "AnimType", waypoint.Data.AnimType.ToString());
            Label(style, "AnimName", waypoint.Data.AnimName);
            Label(style, "AnimID", waypoint.Data.AnimID.ToString());
            Label(style, "MinRepeat", waypoint.Data.MinRepeat.ToString());
            Label(style, "MaxRepeat", waypoint.Data.MaxRepeat.ToString());
            Label(style, "IsLocked", waypoint.Data.IsLocked.ToString());
            if (waypoint.Data.AnimType == EWaypointAnimType.DCSectionTransition)
            {
                waypoint.Data.ExitSegmentOtherSide = EditorGUILayout.ObjectField("Transition to: ", waypoint.Data.ExitSegmentOtherSide, typeof(SectionSegment), true) as SectionSegment;
            }
            else
            {
                Label(style, "Exit segment other side", waypoint.Data.ExitSegmentOtherSide == null ? "none" : waypoint.Data.ExitSegmentOtherSide.ToString());
            }
        }
    }

    private void Label(GUIStyle style, string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        var content = new GUIContent(label);
        EditorGUILayout.LabelField(content, GUILayout.Width(style.CalcSize(content).x));
        content = new GUIContent(value);
        EditorGUILayout.LabelField(content, GUILayout.Width(style.CalcSize(content).x));
        EditorGUILayout.EndHorizontal();
    }

    private int IndexOf(int[] array, int value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == value)
            {
                return i;
            }
        }
        Assert.IsTrue(false);
        return -1;
    }

    private void SetActionName(ref WaypointData data, string prefix, int index, int maxIndex)
    {
        int tries = 0;
        do
        {
            data.AnimName = AnimUtils.GetName(prefix, index);
            index = (index % maxIndex) + 1;
            Assert.IsFalse(tries++ > maxIndex);
        }
        while (!animMan.AnimData.AnimStrings.TryGetValue(data.AnimName, out data.AnimID));
    }

    private void SegmentGUI(ref SectionSegmentTransitionData data)
    {
        SegmentPopup(ref data, "Section of segment");
        data.Subsection = EditorGUILayout.Toggle("First subsection", data.Subsection == 0) ? 0 : 1;
        data.Segment = EditorGUILayout.IntSlider("Segment id", data.Segment + 1, 1, 10) - 1;
    }

    private void SegmentPopup(ref SectionSegmentTransitionData data, string label)
    {
        data.SectionID = EditorGUILayout.Popup(label, data.SectionID, SectionOptions);
        data.Section = SectionOptions[data.SectionID];
    }
}

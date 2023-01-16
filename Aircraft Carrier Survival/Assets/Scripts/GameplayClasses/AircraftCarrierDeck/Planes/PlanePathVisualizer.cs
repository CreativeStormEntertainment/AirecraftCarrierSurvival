using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlanePathVisualizer : MonoBehaviour
{
    [SerializeField]
    private bool refresh = false;
    [SerializeField]
    private bool draw = true;
    [SerializeField]
    private bool alternativeRecovery = false;

    [SerializeField]
    private EPlaneNodeGroup from = default;
    [SerializeField]
    private EPlaneNodeGroup to = default;
    [SerializeField]
    private int line = 0;
    [SerializeField]
    private bool found = false;
    [SerializeField]
    private bool show = false;

    private Dictionary<GameObject, Vector3> positions;
    private Dictionary<EPlaneNodeGroup, Dictionary<EPlaneNodeGroup, List<List<GameObject>>>> paths;
    private Dictionary<EPlaneNodeGroup, List<GameObject>> states;

#if UNITY_EDITOR
    private void Update()
    {
        if (refresh || paths == null)
        {
            refresh = false;
            positions = new Dictionary<GameObject, Vector3>();
            paths = new Dictionary<EPlaneNodeGroup, Dictionary<EPlaneNodeGroup, List<List<GameObject>>>>();
            states = new Dictionary<EPlaneNodeGroup, List<GameObject>>();

            foreach (Transform t1 in transform)
            {
                if (!t1.name.StartsWith("State_") || !t1.gameObject.activeSelf)
                {
                    continue;
                }
                var list = new List<GameObject>();
                foreach (Transform t2 in t1)
                {
                    foreach (Transform t3 in t2)
                    {
                        positions.Add(t3.gameObject, t3.position);
                        list.Add(t3.gameObject);
                    }
                }

                states.Add((EPlaneNodeGroup)Enum.Parse(typeof(EPlaneNodeGroup), t1.name.Substring(6, t1.name.Length - 6)), list);
            }
            foreach (Transform t1 in transform)
            {
                if (!t1.name.StartsWith("Way_") || !t1.gameObject.activeSelf)
                {
                    continue;
                }
                int index = t1.name.LastIndexOf('_');
                var node1 = (EPlaneNodeGroup)Enum.Parse(typeof(EPlaneNodeGroup), t1.name.Substring(4, index - 4));
                index++;
                var node2 = (EPlaneNodeGroup)Enum.Parse(typeof(EPlaneNodeGroup), t1.name.Substring(index, t1.name.Length - index));

                index = 0;

                bool alternative = alternativeRecovery && node1 == EPlaneNodeGroup.Hangar && (node2 == EPlaneNodeGroup.LiftRecoveringToDeck || node2 == EPlaneNodeGroup.LiftRecoveringToHangar);
                bool alternative2 = alternativeRecovery && node2 == EPlaneNodeGroup.Hangar && (node1 == EPlaneNodeGroup.LiftRecoveringToDeck || node1 == EPlaneNodeGroup.LiftRecoveringToHangar);
                bool swap = node2 == EPlaneNodeGroup.SwapLaunching || node2 == EPlaneNodeGroup.SwapRecovering;

                if (!paths.TryGetValue(node1, out var dict))
                {
                    dict = new Dictionary<EPlaneNodeGroup, List<List<GameObject>>>();
                    paths.Add(node1, dict);
                }
                if (!dict.TryGetValue(node2, out var lists))
                {
                    lists = new List<List<GameObject>>();
                    dict.Add(node2, lists);
                }

                foreach (Transform t2 in t1)
                {
                    var list = new List<GameObject>();

                    int newIndex = (alternative && index == 1) ? 3 : index;
                    var state = states[node1];

                    bool alternate = false;

                    if ((node1 == EPlaneNodeGroup.DeckLaunching && (node2 == EPlaneNodeGroup.LiftLaunchingToHangar || node2 == EPlaneNodeGroup.AirLaunching || node2 == EPlaneNodeGroup.WaitLaunching)) ||
                        (node2 == EPlaneNodeGroup.DeckRecovering && (node2 == EPlaneNodeGroup.LiftRecoveringToHangar || node2 == EPlaneNodeGroup.WaitRecovering)))
                    {
                        alternate = true;
                    }

                    foreach (Transform t3 in t2)
                    {
                        positions.Add(t3.gameObject, t2.position);
                    }
                    if (alternate && (node2 == EPlaneNodeGroup.WaitLaunching || node2 == EPlaneNodeGroup.WaitRecovering))
                    {
                        for (int i = 0; i < (state.Count / 3 - 1); i++)
                        {
                            list = new List<GameObject>();
                            for (int j = i; j < (state.Count / 3); j++)
                            {
                                list.Add(state[j * 3]);
                            }

                            AddToLists(list, node2, t2, swap, i, alternative2);
                            lists.Add(list);
                        }
                    }
                    else
                    {
                        int state2Index = index;
                        if (alternate)
                        {
                            state2Index = 0;
                            if (newIndex > 0)
                            {
                                for (int i = 0; i < (state.Count / 3); i++)
                                {
                                    list.Add(state[i * 3]);
                                }
                                AddToLists(list, node2, t2, swap, newIndex, alternative2);
                                lists.Add(list);
                                list = new List<GameObject>();
                            }
                            for (int i = 0; i < (state.Count / 3); i++)
                            {
                                list.Add(state[i * 3 + newIndex]);
                            }
                        }
                        else
                        {
                            list.Add(state[newIndex]);
                        }
                        AddToLists(list, node2, t2, swap, index, alternative2);
                        lists.Add(list);
                    }

                    index++;
                }
            }
        }

        var retardedCsh = new List<GameObject>();
        foreach (var pair in positions)
        {
            if (pair.Key.transform.position != pair.Value)
            {
                retardedCsh.Add(pair.Key);
            }
        }
        foreach (var key in retardedCsh)
        {
            positions[key] = key.transform.position;
        }
    }

    private void OnDrawGizmos()
    {
        if (!draw || paths == null)
        {
            return;
        }

        if ((!paths.TryGetValue(from, out var dict) || !dict.TryGetValue(to, out var lists) || lists.Count <= line) &&
            (!paths.TryGetValue(to, out dict) || !dict.TryGetValue(from, out lists) || lists.Count <= line))
        {
            found = false;
            return;
        }

        found = true;

        if (show)
        {
            Debug.Log("--------------------");
        }

        var path = lists[line];
        bool backward = false;
        Gizmos.DrawSphere(positions[path[0]], .5f);
        for (int i = 1; i < path.Count; i++)
        {
            var pos1 = positions[path[i - 1]];
            var pos2 = positions[path[i]];
            Gizmos.DrawSphere(pos2, .5f);

            var dir = Vector3.Normalize(pos2 - pos1);
            if (dir.y != 0f)
            {
                if (dir.x != 0f || dir.z != 0f)
                {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawLine(pos1, pos2);
                Gizmos.color = Color.white;
                continue;
            }
            Gizmos.DrawLine(pos1, pos2);

            float angle = Quaternion.LookRotation(dir).eulerAngles.y;
            float angle2 = Mathf.Abs(angle - 180f);
            bool newBackward = angle2 > 90f;
            if (newBackward != backward && Mathf.Abs(angle2 - 90f) < 5f)
            {
                backward = newBackward;
            }
            if (!backward)
            {
                angle += 180f;
                if (angle > 360f)
                {
                    angle -= 360f;
                }
            }

            var pos3 = (pos2 + pos1) / 2f;
            if (show)
            {
                Debug.Log(angle);
            }
            pos1 = pos3 + Quaternion.Euler(0f, angle - 10f, 0f) * (Vector3.forward * 5f);
            pos2 = pos3 + Quaternion.Euler(0f, angle + 10f, 0f) * (Vector3.forward * 5f);

            Gizmos.DrawLine(pos1, pos3);
            Gizmos.DrawLine(pos2, pos3);
            pos2 = pos3 + Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * 5f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos2, pos3);
            Gizmos.color = Color.white;
        }
        show = false;
    }

    private void AddToLists(List<GameObject> list, EPlaneNodeGroup node2, Transform t2, bool swap, int index, bool alternative2)
    {
        foreach (Transform t3 in t2)
        {
            list.Add(t3.gameObject);
        }
        int newIndex;
        if (swap)
        {
            newIndex = 0;
        }
        else if (alternative2 && index == 1)
        {
            newIndex = 3;
        }
        else
        {
            newIndex = index;
        }
        if (states[node2].Count <= newIndex)
        {
            newIndex = (alternative2 && index == 1) ? 3 : index;
            Debug.LogError("wtf, " + node2 + ";" + newIndex);
        }
        list.Add(states[node2][newIndex]);
    }
#endif
}

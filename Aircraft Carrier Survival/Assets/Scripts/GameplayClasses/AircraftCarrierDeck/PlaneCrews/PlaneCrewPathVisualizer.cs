using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class PlaneCrewPathVisualizer : MonoBehaviour
{
    private const string CrewWaypointName = "CrewWaypoint";
    private static readonly int CrewCharIndex = CrewWaypointName.Length;

    private enum WaypointType
    {
        Waypoint,
        Here,
        Before,
        After
    }

    private struct PlaneCrewData
    {
        public Vector3 Position;
        public WaypointType Type;
    }

    [SerializeField]
    private bool refresh = false;
    [SerializeField]
    private bool draw = true;

    [SerializeField]
    private bool recovering = false;
    [SerializeField]
    private bool front = false;
    [SerializeField]
    private int line = 0;
    [SerializeField]
    private int plane = 0;
    [SerializeField]
    private bool rightCrew = false;

    private List<List<List<List<List<List<PlaneCrewData>>>>>> data;

#if UNITY_EDITOR
    private void Update()
    {
        if (refresh || data == null)
        {
            refresh = false;
            data = new List<List<List<List<List<List<PlaneCrewData>>>>>>() { new List<List<List<List<List<PlaneCrewData>>>>>(), new List<List<List<List<List<PlaneCrewData>>>>>() };

            foreach (Transform t1 in transform)
            {
                bool launching = t1.name == "Launching";
                if (!launching && t1.name != "Recovering")
                {
                    continue;
                }
                var group = data[launching ? 0 : 1];
                group.Add(new List<List<List<List<PlaneCrewData>>>>());
                group.Add(new List<List<List<List<PlaneCrewData>>>>());
                Assert.IsTrue(t1.childCount % 2 == 0, t1.name);
                int count = t1.childCount / 2;
                int value1 = 0;
                int value2 = 0;
                foreach (Transform t2 in t1)
                {
                    var innerGroup = group[0];
                    if (t2.name.StartsWith("Alt"))
                    {
                        innerGroup = group[1];
                        Assert.IsTrue(t2.name.Length == 5, t2.name);
                        Assert.IsTrue(((int)(t2.name[4] - '0')) == value2++, t2.name);
                    }
                    else
                    {
                        Assert.IsTrue(t2.name.Length == 1, t2.name);
                        Assert.IsTrue(((int)(t2.name[0] - '0')) == value1++, t2.name);
                    }
                    var line = new List<List<List<PlaneCrewData>>>();

                    Assert.IsTrue(t2.childCount == 3, t2.name);
                    for (int i = 0; i < 3; i++)
                    {
                        var plane = new List<List<PlaneCrewData>>() { new List<PlaneCrewData>(), new List<PlaneCrewData>() };

                        bool both = false;
                        foreach (Transform t3 in t2.GetChild(i))
                        {
                            Assert.IsTrue(t3.name.StartsWith(CrewWaypointName), $"{t1.name}; {t2.name}; {t3.name}");
                            char ch = t3.name[CrewCharIndex];
                            var data = new PlaneCrewData() { Position = t3.position };
                            if (t3.name.Length > (CrewCharIndex + 1))
                            {
                                char ch2 = t3.name[CrewCharIndex + 1];
                                if (ch2 == '*')
                                {
                                    if (t3.name.Length > (CrewCharIndex + 2))
                                    {
                                        char ch3 = t3.name[CrewCharIndex + 2];
                                        if (ch3 == '+')
                                        {
                                            data.Type = WaypointType.After;
                                        }
                                        else if (ch3 == '-')
                                        {
                                            data.Type = WaypointType.Before;
                                        }
                                        else
                                        {
                                            Assert.IsTrue(ch3 == ' ', $"{t1.name}; {t2.name}; {t3.name}; {ch3}");
                                            data.Type = WaypointType.Here;
                                        }
                                    }
                                    else
                                    {
                                        data.Type = WaypointType.Here;
                                    }
                                }
                                else
                                {
                                    Assert.IsTrue(ch2 == ' ', $"{t1.name}; {t2.name}; {t3.name}; {ch2}");
                                }
                            }
                            if (ch == 'L')
                            {
                                Assert.IsFalse(both, $"{t1.name}; {t2.name}; {t3.name}; {ch}");
                                plane[0].Add(data);
                            }
                            else if (ch == 'R')
                            {
                                Assert.IsFalse(both, $"{t1.name}; {t2.name}; {t3.name}; {ch}");
                                plane[1].Add(data);
                            }
                            else if (ch == 'B')
                            {
                                both = true;
                                plane[0].Add(data);
                                plane[1].Add(data);
                            }
                            else
                            {
                                Assert.IsTrue(false, $"{t1.name}; {t2.name}; {t3.name}; , {ch}");
                            }
                        }

                        line.Add(plane);
                    }

                    innerGroup.Add(line);
                }
                Assert.IsTrue(group[0].Count == count, $"{t1.name}, {count} - {group.Count}");
                Assert.IsTrue(group[1].Count == count, $"{t1.name}, {count} - {group.Count}");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!draw || data == null)
        {
            return;
        }

        try
        {
            var path = data[recovering ? 1 : 0][front ? 1 : 0][line][plane][rightCrew ? 1 : 0];

            SetColor(path[0].Type);
            Gizmos.DrawSphere(path[0].Position, .5f);
            for (int i = 1; i < path.Count; i++)
            {
                SetColor(path[i].Type);
                Gizmos.DrawSphere(path[i].Position, .5f);

                Gizmos.color = Color.white;
                Gizmos.DrawLine(path[i - 1].Position, path[i].Position);
            }

            foreach (var waypoint in path)
            {
                switch (waypoint.Type)
                {
                    case WaypointType.Waypoint:
                        Gizmos.color = Color.white;
                        break;
                    case WaypointType.Here:
                        Gizmos.color = Color.green;
                        break;
                    case WaypointType.Before:
                        Gizmos.color = Color.blue;
                        break;
                    case WaypointType.After:
                        Gizmos.color = Color.black;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }

            }
        }
        catch (Exception ex)
        {
            draw = false;
            Debug.LogError(ex.StackTrace);
            throw ex;
        }
    }

    private void SetColor(WaypointType type)
    {
        switch (type)
        {
            case WaypointType.Waypoint:
                Gizmos.color = Color.white;
                break;
            case WaypointType.Here:
                Gizmos.color = Color.green;
                break;
            case WaypointType.Before:
                Gizmos.color = Color.blue;
                break;
            case WaypointType.After:
                Gizmos.color = Color.black;
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
    }
#endif
}

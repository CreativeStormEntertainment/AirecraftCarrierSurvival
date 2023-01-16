using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Waypoint : MonoBehaviour
{
#if UNITY_EDITOR
    public event Action OnPositionChanged = delegate { };
#endif

    public List<Waypoint> Branches = new List<Waypoint>();
    public Dictionary<Waypoint, Dictionary<Waypoint, List<Vector3>>> PathsDict;
    public WaypointData Data;

    const float StartRoundDist = .3f;
    const float RoundAngleAccuracy = 1350f;

    [NonSerialized]
    public Transform Trans;

    private void Awake()
    {
        Trans = transform;

        PathsDict = new Dictionary<Waypoint, Dictionary<Waypoint, List<Vector3>>>();

        foreach (var branch01 in Branches)
        {
            Dictionary<Waypoint, List<Vector3>> dict;
            if (!PathsDict.TryGetValue(branch01, out dict))
            {
                dict = new Dictionary<Waypoint, List<Vector3>>();
                PathsDict[branch01] = dict;
            }

            foreach (var branch02 in Branches)
            {
                if (branch01 != branch02)
                {
                    var point1 = branch01.transform.position;
                    var point2 = Trans.position;
                    var point3 = branch02.transform.position;
                    var dir12 = point2 - point1;
                    float mag12 = dir12.magnitude;

                    var dir23 = point3 - point2;
                    float mag23 = dir23.magnitude;
                    if (mag12 < .1f || mag23 < .1f)
                    {
                        dict[branch02] = new List<Vector3>() { point1, point3 };
                        continue;
                    }

                    dir12 /= mag12;
                    dir23 /= mag23;

                    float startRoundDist = Mathf.Min(StartRoundDist, mag12 / 2f, mag23 / 2f);
                    point1 = point2 - dir12 * startRoundDist;
                    point3 = point2 + dir23 * startRoundDist;

                    var bezier = new PathCreation.BezierPath(Vector3.zero);
                    bezier.SetPoint(0, point1, true);
                    //bezier.SetPoint(1, point2, true);
                    //bezier.SetPoint(2, point2, true);
                    bezier.SetPoint(1, (point1 + 3f * point2) / 4f, true);
                    bezier.SetPoint(2, (3f * point2 + point3) / 4f, true);
                    bezier.SetPoint(3, point3);

                    //var arr = bezier.GetPointsInSegment(0);
                    //float value = PathCreation.Utility.CubicBezierUtility.EstimateCurveLength(arr[0], arr[1], arr[2], arr[3]);
                    if (Vector3.Angle(dir12, -dir23) < 1f)
                    {
                        dict[branch02] = new List<Vector3>() { point1, point3 };
                    }
                    else
                    {
                        dict[branch02] = PathCreation.Utility.VertexPathUtility.SplitBezierPathByAngleError(bezier, .3f, .01f, (RoundAngleAccuracy / Vector3.Angle(dir12, -dir23))).vertices;
                    }
                    //for (int i = 1; i < list.Count; i++)
                    //{
                    //    if(Mathf.Approximately((list[i] - list[i-1]).magnitude, 0f))
                    //    {
                    //    }
                    //}
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (SceneManager.GetActiveScene().name == "Waypoints")
        {
            if (UnityEditor.Selection.activeObject == gameObject)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                switch (Data.AnimType)
                {
                    case EWaypointAnimType.Exit:
                        Gizmos.color = Color.red;
                        break;
                    case EWaypointAnimType.ActionAnim:
                        switch (Data.PossibleTasks)
                        {
                            case EWaypointTaskType.Firefighting:
                                Gizmos.color = Color.yellow;
                                break;
                            case EWaypointTaskType.Repair:
                                Gizmos.color = Color.blue;
                                break;
                            case EWaypointTaskType.Rescue:
                                Gizmos.color = new Color(1f, 0f, 1f);
                                break;
                            case EWaypointTaskType.Rescue2:
                                Gizmos.color = new Color(.9f, 0f, .9f);
                                break;
                            case EWaypointTaskType.Rescue3:
                                Gizmos.color = new Color(.8f, 0f, .8f);
                                break;
                            case EWaypointTaskType.Normal:
                                Gizmos.color = Color.cyan;
                                break;
                            case EWaypointTaskType.Waterpump:
                                Gizmos.color = new Color(0f, .5f, 0f);
                                break;
                            default:
                                Gizmos.color = new Color(.125f, .125f, .125f);
                                break;
                        }
                        break;
                    case EWaypointAnimType.DCIdle:
                        Gizmos.color = new Color(.5f, 0f, 1f);
                        break;
                    case EWaypointAnimType.DCSegmentTransition:
                        Gizmos.color = new Color(1f, 1f, .25f);
                        break;
                    case EWaypointAnimType.DCSectionTransition:
                        Gizmos.color = new Color(1f, .5f, .25f);
                        break;
                    default:
                        Gizmos.color = Color.white;
                        break;

                }
            }

            Gizmos.DrawSphere(transform.position, .1f);

            Gizmos.color = Color.red;
            foreach (var waypoint in Branches)
            {
                if (waypoint == null)
                {
                    Debug.LogError("empty object in branches...", this);
                    continue;
                }
                Gizmos.DrawLine(waypoint.transform.position, transform.position);
            }
        }
    }

    public void Init(int waypointLayerID)
    {
        gameObject.layer = waypointLayerID;
        Branches = new List<Waypoint>();
        Data = new WaypointData();
        Data.Init(FindObjectOfType<AnimationManager>());
        GetComponent<BoxCollider>().size = new Vector3(.15f, .15f, .15f);
    }

    public void Add(Waypoint waypoint)
    {
        AddInner(waypoint);
        waypoint.AddInner(this);
    }

    public void Remove(Waypoint waypoint)
    {
        RemoveInner(waypoint);
        waypoint.RemoveInner(this);
    }

    public void GetConnectedPoints(HashSet<Waypoint> set)
    {
        set.Add(this);
        foreach (var waypoint in Branches)
        {
            if (set.Add(waypoint))
            {
                waypoint.GetConnectedPoints(set);
            }
        }
    }

    public void PositionChanged()
    {
        OnPositionChanged();
    }

    private void AddInner(Waypoint waypoint)
    {
        if (!Branches.Contains(waypoint))
        {
            Branches.Add(waypoint);
        }
    }

    private void RemoveInner(Waypoint waypoint)
    {
        Branches.Remove(waypoint);
    }
#endif
}

using System.Linq;
using UnityEngine;

public class InjuredButton : GameButton
{
    public SectionSegment Segment;
    public Vector3 Offset;

    private void Start()
    {
        Waypoint rescueWaypoint = null;
        var rescueWaypoints = Segment.Parent.Path.ActionsBySegments[Segment][EWaypointTaskType.Rescue];

        foreach (Waypoint waypoint in rescueWaypoints)
        {
            if (waypoint.Data.InjuredWaypoint)
            {
                rescueWaypoint = waypoint;
                break;
            }
        }
        if (rescueWaypoint == null)
        {
            rescueWaypoint = rescueWaypoints.First();
        }
        var rescueWaypointPos = rescueWaypoint.Trans.position;
        ButtonTrans.position = new Vector3(Segment.Center.x, rescueWaypointPos.y + Offset.y, rescueWaypointPos.z);
    }

    public override void OnClickEnd(bool success)
    {
        //base.OnClickEnd(success);
        //if (success)
        //{
        //    var group = DamageControlManager.Instance.SelectedGroup;

        //    var secRoomManager = SectionRoomManager.Instance;
        //    if (group == null)
        //    {
        //        secRoomManager.PlayEvent(ESectionUIState.NoDCClick);
        //    }
        //    else if (group.SetPath(Segment, EWaypointTaskType.Rescue))
        //    {
        //        secRoomManager.PlayEvent(ESectionUIState.DCRescue);
        //    }
        //}
    }


}

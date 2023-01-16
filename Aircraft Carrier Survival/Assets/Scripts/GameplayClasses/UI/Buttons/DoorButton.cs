using System;

public class DoorButton : GameButton
{
    //[NonSerialized]
    //public Door Door;

    //public override void OnClickEnd(bool success)
    //{
    //    base.OnClickEnd(success);
    //    if (success)
    //    {
    //        var group = DamageControlManager.Instance.SelectedGroup;

    //        Door.Check(Door.Parent1);
    //        Door.Check(Door.Parent2);

    //        if (group != null)
    //        {
    //            var segment = Door.Parent1;
    //            if ((Door.Parent1.IsFlooding() && !Door.Parent2.IsFlooding()) || Door.Parent1.IsFlooded())
    //            {
    //                segment = Door.Parent2;
    //            }

    //            group.SetPath(segment, EWaypointTaskType.RepairDoor, Door);
    //        }
    //    }
    //}
}

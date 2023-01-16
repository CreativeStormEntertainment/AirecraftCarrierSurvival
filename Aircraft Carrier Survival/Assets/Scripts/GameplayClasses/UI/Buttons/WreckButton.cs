using UnityEngine;

public class WreckButton : GameButton
{
    [SerializeField]
    private EWreckType type = EWreckType.Wreck;

    public override void OnClickEnd(bool success)
    {
        base.OnClickEnd(success);
        var dcMan = DamageControlManager.Instance;
        var group = dcMan.SelectedGroup;
        var sectionRoomMan = SectionRoomManager.Instance;
        if (group == null || dcMan.AutoDC)
        {
            sectionRoomMan.PlayEvent(ESectionUIState.NoDCClick);
        }
        else
        {
            var voiceSoundsMan = VoiceSoundsManager.Instance;
            var segment = dcMan.WreckSection.SubsectionRooms[(int)type].Segments[0];
            foreach (var subsection in dcMan.WreckSection.SubsectionRooms)
            {
                if (subsection.Segments.Contains(group.CurrentSegment))
                {
                    segment = group.CurrentSegment;
                }
            }
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                group.QueuePath(segment, ESectionUIState.DCWreck);
            }
            else if (group.SetPath(segment, EWaypointTaskType.Repair, true, true))
            {
                sectionRoomMan.PlayEvent(ESectionUIState.DCWreck);
                voiceSoundsMan.PlayPositive(EVoiceType.DC);
            }
            else
            {
                sectionRoomMan.PlayEvent(ESectionUIState.DCNegative);
                voiceSoundsMan.PlayNegative(EVoiceType.DC);
            }
        }
    }
}

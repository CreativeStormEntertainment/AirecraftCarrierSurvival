using UnityEngine;

public class CrewInventoryItem : PeopleInventoryItem<CrewIntermissionData, CrewInventoryItem>
{
    [SerializeField]
    private IntermissionCrewPortrait portrait = null;
    [SerializeField]
    private CrewInspector inspector = null;

    public override void Setup(CrewIntermissionData data)
    {
        base.Setup(data);
        portrait.Setup(data.Data);
    }

    protected override void SetShowHighlight(bool show)
    {
        base.SetShowHighlight(show);
        portrait.Highlight.SetActive(show);
    }

    protected override void SetShowDragHighlight(bool show)
    {
        base.SetShowDragHighlight(show);
        portrait.DragHighlight.SetActive(show);
    }

    protected override void SetShowTooltip(bool show)
    {
        base.SetShowTooltip(show);
        if (show)
        {
            inspector.Setup(Data.Data);
        }
        else
        {
            inspector.Hide();
        }
    }
}

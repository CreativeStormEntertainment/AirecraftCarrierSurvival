using UnityEngine;

public class OfficerInventoryItem : PeopleInventoryItem<OfficerIntermissionData, OfficerInventoryItem>
{
    [SerializeField]
    private IntermissionOfficerPortrait intermissionOfficerPortrait = null;
    [SerializeField]
    private CrewInspector inspector = null;

    public override void Setup(OfficerIntermissionData data)
    {
        base.Setup(data);
        intermissionOfficerPortrait.SetupOfficer(data.Data, data.Index);
    }

    protected override void SetShowHighlight(bool show)
    {
        base.SetShowHighlight(show);
        intermissionOfficerPortrait.Highlight.SetActive(show);
    }

    protected override void SetShowDragHighlight(bool show)
    {
        base.SetShowDragHighlight(show);
        intermissionOfficerPortrait.DragHighlight.SetActive(show);
    }

    protected override void SetShowTooltip(bool show)
    {
        base.SetShowTooltip(show);
        if (show)
        {
            inspector.Setup(Data.Data, Data.Index);
        }
        else
        {
            inspector.Hide();
        }
    }
}

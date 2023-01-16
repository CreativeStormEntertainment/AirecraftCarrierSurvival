using System;
using UnityEngine;
using UnityEngine.UI;

public class OfficerShopButton : PeopleShopButton<OfficerIntermissionData>
{
    [SerializeField]
    private IntermissionOfficerPortrait intermissionOfficerPortrait = null;
    [SerializeField]
    private ManeuversList maneuversScriptable = null;
    [SerializeField]
    private OfficerList officerList = null;
    [SerializeField]
    protected Text desc = null;
    [SerializeField]
    private string signatureManeuverId = "SignatureManeuver";
    [SerializeField]
    private CrewInspector inspector = null;

    public override void Setup(OfficerIntermissionData data, int index, Action click)
    {
        signatureManeuverId = LocalizationManager.Instance.GetText(signatureManeuverId);
        base.Setup(data, index, click);
    }

    public override void Refresh(OfficerIntermissionData data)
    {
        base.Refresh(data);
        if (data != null)
        {
            intermissionOfficerPortrait.SetupOfficer(data.Data, data.Index);
            var listData = officerList.Officers[data.Index];
            var locMan = LocalizationManager.Instance;
            desc.text = locMan.GetText(listData.Name) + "\n" + signatureManeuverId + "\n" + locMan.GetText(maneuversScriptable.Maneuvers[listData.ManeuverIndex].Name);
            Unlocked = data.Unlocked;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    protected override void SetShowHighlight(bool show)
    {
        base.SetShowHighlight(show);
        intermissionOfficerPortrait.Highlight.SetActive(show);

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

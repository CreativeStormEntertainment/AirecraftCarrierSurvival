using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrewShopButton : PeopleShopButton<CrewIntermissionData>
{
    [SerializeField]
    private IntermissionCrewPortrait portrait = null;
    [SerializeField]
    protected Text desc = null;
    [SerializeField]
    private string noSpecDescId = "NoviceUnit";
    [SerializeField]
    private string skillsId = "Skills";
    [SerializeField]
    private CrewInspector inspector = null;

    public override void Setup(CrewIntermissionData data, int index, Action click)
    {
        var locMan = LocalizationManager.Instance;
        noSpecDescId = locMan.GetText(noSpecDescId);
        skillsId = locMan.GetText(skillsId);
        base.Setup(data, index, click);
    }

    public override void Refresh(CrewIntermissionData data)
    {
        base.Refresh(data);
        portrait.Setup(data.Data);
        var locMan = LocalizationManager.Instance;
        var savedSpecialities = new List<ECrewmanSpecialty>(data.Data.GetSpecialties());
        string a = "";
        string b = "";
        string c = "";

        if (savedSpecialities.Count == 0)
        {
            a = noSpecDescId;
        }
        else
        {
            a = skillsId;
            if (savedSpecialities.Count > 0)
            {
                b = "\n" + locMan.GetText(savedSpecialities[0].ToString() + "Upgrade");
            }
            if (savedSpecialities.Count > 1)
            {
                c = "\n" + locMan.GetText(savedSpecialities[1].ToString() + "Upgrade");
            }
        }
        desc.text = $"{a}{b}{c}";
        Unlocked = data.Unlocked;
    }

    protected override void SetShowHighlight(bool show)
    {
        base.SetShowHighlight(show);
        portrait.Highlight.SetActive(show);

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

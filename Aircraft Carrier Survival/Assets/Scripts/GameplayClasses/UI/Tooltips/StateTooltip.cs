using System;
using UnityEngine;

public class StateTooltip : TooltipCaller
{
    private static readonly string[] emptyParams = new string[0];

    [SerializeField]
    private int ignoreStateDecription = -1;

    private int state = 1;

    [NonSerialized]
    private string[] titleParams = emptyParams;
    [NonSerialized]
    private string[] descParams = emptyParams;

    public void ChangeState(Enum state)
    {
        ChangeState(state, emptyParams, emptyParams);
    }

    public void ChangeStateTitleParams(Enum state, params string[] titleParams)
    {
        ChangeState(state, titleParams, emptyParams);
    }

    public void ChangeStateDescriptionParams(Enum state, params string[] descParams)
    {
        ChangeState(state, emptyParams, descParams);
    }

    public void ChangeState(Enum state, string[] titleParams, string[] descParams)
    {
        this.state = Convert.ToInt32(state);
        this.titleParams = titleParams;
        this.descParams = descParams;
        FireParamsChanged();
    }

    protected override void UpdateText()
    {
        var locMan = LocalizationManager.Instance;
        if (!string.IsNullOrWhiteSpace(TitleID))
        {
            string localizationID = $"{TitleID}.{state}";
            title = locMan.GetText(localizationID, titleParams);
            if (state == ignoreStateDecription)
            {
                description = "";
            }
            else
            {
                description = locMan.GetText($"{localizationID}Desc", false, descParams);
            }
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentOrderTooltip : TooltipCaller
{
    private void Update()
    {
        if (isShowing)
        {
            UpdateText();
        }
    }

    protected override void UpdateText()
    {
        locMan = LocalizationManager.Instance;
        if (!string.IsNullOrWhiteSpace(TitleID))
        {
            if (IslandsAndOfficersManager.Instance.CurrentBuff != null)
            {
                var name = IslandsAndOfficersManager.Instance.CurrentBuff.IslandBuffType.ToString();
                name = name.Replace(" ", "");
                var orderName = locMan.GetText(name + "Title");
                title = locMan.GetText(TitleID, orderName);
            }
            else
            {
                title = locMan.GetText(TitleID, "-");
            }
        }
        if (!string.IsNullOrWhiteSpace(DescriptionID))
        {

            description = locMan.GetText(DescriptionID);
        }
        Tooltip.Instance.UpdateText(title, description);
    }
}

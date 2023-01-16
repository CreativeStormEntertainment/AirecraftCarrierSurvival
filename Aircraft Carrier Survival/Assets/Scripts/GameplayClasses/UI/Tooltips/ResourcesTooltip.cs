using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesTooltip : TooltipCaller
{
    private ResourceManager resourceManager = null;

    private void Start()
    {
        resourceManager = ResourceManager.Instance;
    }

    private void Update()
    {
        if (isShowing)
        {
            UpdateText();
        }
    }

    protected override void UpdateText()
    {
        if (!string.IsNullOrWhiteSpace(TitleID))
        {
            title = LocalizationManager.Instance.GetText(TitleID);
        }
        if (!string.IsNullOrWhiteSpace(DescriptionID))
        {
            description = LocalizationManager.Instance.GetText(DescriptionID, ((int)ResourceManager.Instance.Supplies).ToString(), ((int)ResourceManager.Instance.SuppliesCapacity).ToString());
        }
        Tooltip.Instance.UpdateText(title, description);
    }
}

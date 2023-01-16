using UnityEngine;
using UnityEngine.UI;

public class OneTextTooltip : TooltipCaller
{
    [SerializeField]
    private Text text = null;

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
            title = locMan.GetText(TitleID);
        }
        if (!string.IsNullOrWhiteSpace(DescriptionID))
        {
            description = locMan.GetText(DescriptionID, text.text);
        }
    }
}


public class ParamedTooltip : TooltipCaller
{
    private string[] parametersArray = new string[0];

    public void SetParameter(params string[] param)
    {
        parametersArray = param;
        FireParamsChanged();
    }

    protected override void UpdateText()
    {
        var locMan = LocalizationManager.Instance;
        if (!string.IsNullOrWhiteSpace(TitleID))
        {
            title = locMan.GetText(TitleID);
        }
        if (!string.IsNullOrWhiteSpace(DescriptionID))
        {
            description = locMan.GetText(DescriptionID, parametersArray);
        }
    }
    private void OnDestroy()
    {
        if (isShowing)
        {
            Tooltip.Instance.Hide();
        }
    }
}

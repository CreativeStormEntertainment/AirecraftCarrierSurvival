
public class IntTooltip : TooltipCaller
{
    private int param;
    public void SetInt(int count)
    {
        param = count;
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
            description = locMan.GetText(DescriptionID, param.ToString());
        }
    }
}

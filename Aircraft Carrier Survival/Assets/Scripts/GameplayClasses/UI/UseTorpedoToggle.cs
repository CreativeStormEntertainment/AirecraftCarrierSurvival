using UnityEngine;

public class UseTorpedoToggle : CustomToggle
{
    [SerializeField]
    private TooltipCaller caller = null;
    [SerializeField]
    private string bomberTooltipTitle = null;
    [SerializeField]
    private string bomberTooltipDesc = null;
    [SerializeField]
    private string torpedoTooltipTitle = null;
    [SerializeField]
    private string torpedoTooltipDesc = null;

    public override bool State
    {
        get => base.State;
        set
        {
            base.State = value;
            if (value)
            {
                caller.SetTitles(torpedoTooltipTitle, torpedoTooltipDesc);
            }
            else
            {
                caller.SetTitles(bomberTooltipTitle, bomberTooltipDesc);
            }
            caller.RefreshIfVisible();
        }
    }

    public void Setup(bool active)
    {
        Setup(false, active);
    }
}

using UnityEngine;

public class LaunchButtonIntermissionPanel : ButtonIntermissionPanel
{
    [SerializeField]
    private string[] tooltipIds = new string[2];

    [SerializeField]
    private TooltipCaller tooltipCaller = null;
    protected override void Awake()
    {
        isLaunchButton = true;
        base.Awake();
    }

    public void SetTooltip(bool tutorial)
    {
        tooltipCaller.SetTitles(tooltipIds[tutorial ? 1 : 0], "");
    }

    public override void OnClickSFX()
    {
        BackgroundAudio.Instance.PlayEvent(EIntermissionClick.LaunchClick);
    }
}

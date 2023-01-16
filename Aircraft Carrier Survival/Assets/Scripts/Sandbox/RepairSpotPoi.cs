
public class RepairSpotPoi : SandboxPoi
{
    public override void Setup(SandboxPoiData data)
    {
        base.Setup(data);
    }

    public override void OnClick()
    {
        base.OnClick();
        if (InRange)
        {
            SandboxManager.Instance.ShowRepairSpotPopup(this);
        }
    }
}

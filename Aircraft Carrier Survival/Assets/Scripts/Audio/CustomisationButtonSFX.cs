
public class CustomisationButtonSFX : ButtonSFX
{
    public override void OnClickSFX()
    {
        BackgroundAudio.Instance.PlayEvent(EIntermissionClick.PlaneCustomisationClick);
    }
}

using UnityEngine;

public class MainSceneButtonSFX : ButtonSFX
{
    [SerializeField]
    private EMainSceneUI click = EMainSceneUI.PauseTime;

    public override void OnClickSFX()
    {
        BackgroundAudio.Instance.PlayEvent(click);
    }
}

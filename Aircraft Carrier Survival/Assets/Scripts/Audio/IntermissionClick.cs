using UnityEngine;

public class IntermissionClick : ButtonSFX
{
    [SerializeField]
    private EIntermissionClick click = EIntermissionClick.UpgradeClick;

    public override void OnClickSFX()
    {
        BackgroundAudio.Instance.PlayEvent(click);
    }
}

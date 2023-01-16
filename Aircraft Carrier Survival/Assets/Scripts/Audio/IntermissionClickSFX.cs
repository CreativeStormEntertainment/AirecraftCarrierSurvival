using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntermissionClickSFX : ButtonSFX
{
    [SerializeField]
    private EIntermissionClick click = EIntermissionClick.PlaneCustomisationClick;
    public override void OnClickSFX()
    {
        BackgroundAudio.Instance.PlayEvent(click);
    }
}

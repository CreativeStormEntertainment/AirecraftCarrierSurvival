using FMODUnity;
using UnityEngine;

public class CustomEventClickSFX : ButtonSFX
{
    [SerializeField]
    private StudioEventEmitter emitter = null;

    public override void OnClickSFX()
    {
        emitter.Play();
    }
}

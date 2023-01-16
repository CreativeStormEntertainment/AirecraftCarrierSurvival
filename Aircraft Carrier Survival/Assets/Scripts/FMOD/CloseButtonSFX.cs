using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

public class CloseButtonSFX : ButtonSFX
{
    [SerializeField]
    private StudioEventEmitter emitter = null;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (button.enabled && button.interactable)
        {
            emitter.Play();
        }
    }
}

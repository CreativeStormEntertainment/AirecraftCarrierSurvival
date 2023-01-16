using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

public class DCButtonsSfx : ButtonSFX
{
    [SerializeField]
    private StudioEventEmitter emitter = null;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.enabled && button.interactable)
        {
            emitter.Play();
        }
    }
}

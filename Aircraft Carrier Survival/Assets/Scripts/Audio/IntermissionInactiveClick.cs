using UnityEngine;
using UnityEngine.EventSystems;

public class IntermissionInactiveClick : ButtonSFX, IPointerClickHandler
{
    [SerializeField]
    private bool muteHoverSound = false;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!muteHoverSound)
        {
            base.OnPointerEnter(eventData);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.enabled || !button.interactable)
        {
            BackgroundAudio.Instance.PlayEvent(EIntermissionClick.InactiveClick);
        }
    }
}

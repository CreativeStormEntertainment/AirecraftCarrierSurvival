using UnityEngine.EventSystems;

public class MenuButtonSFX : ButtonSFX
{
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.enabled && button.interactable)
        {
            BackgroundAudio.Instance.SpecialHover();
        }
    }
}
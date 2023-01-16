using UnityEngine.EventSystems;

public class IntermissionInactiveButtonSFX : IntermissionInactiveClick
{
    public override void OnPointerEnter(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class StrikeGroupHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private StrikeGroupButton strikeGroupButton = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    public void Setup(StrikeGroupButton button)
    {
        strikeGroupButton = button;
    }


}

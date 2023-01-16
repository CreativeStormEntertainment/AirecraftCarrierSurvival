using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDownUpSFX : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
    }
}

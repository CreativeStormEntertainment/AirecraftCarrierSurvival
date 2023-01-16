using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOnlySFX : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
    }
}

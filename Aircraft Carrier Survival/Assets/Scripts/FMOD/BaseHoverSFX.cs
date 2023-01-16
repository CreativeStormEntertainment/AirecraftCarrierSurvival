using UnityEngine;
using UnityEngine.EventSystems;

public class BaseHoverSFX : MonoBehaviour, IPointerEnterHandler
{
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
    }
}

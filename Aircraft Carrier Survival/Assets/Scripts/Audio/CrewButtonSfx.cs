using UnityEngine;
using UnityEngine.EventSystems;

public class CrewButtonSfx : MonoBehaviour, IPointerEnterHandler
{
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        CrewManager.Instance.PlayEvent(ECrewUIState.Hover);
    }
}

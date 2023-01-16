using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectableHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private Selectable selectable = null;
    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    private GameObject pressed = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable.enabled && selectable.interactable)
        {
            highlight.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable.enabled && selectable.interactable)
        {
            highlight.SetActive(false);
            pressed.SetActive(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed.SetActive(false);
    }
}

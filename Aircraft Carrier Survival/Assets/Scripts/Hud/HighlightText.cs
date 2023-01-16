using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HighlightText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Text text = null;
    [SerializeField]
    private Color color = new Color(1f, 0.9529412f, 0.6156863f);
    [SerializeField]
    private Color normalColor = new Color(0.654902f, 0.654902f, 0.654902f);

    private Selectable selectable;

    private void Start()
    {
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable == null || (selectable != null && selectable.interactable && selectable.enabled))
        {
            text.color = color;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
    }
}

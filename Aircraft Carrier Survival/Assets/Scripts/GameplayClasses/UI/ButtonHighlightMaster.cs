using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHighlightMaster : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private GameObject highlight = null;

    [SerializeField]
    private GameObject markHighlight = null;

    [SerializeField]
    private Image img = null;

    private Color color;

    private void Awake()
    {
        color = img.color;
        ResetHighlight();
    }

    public void OnPointerEnter(PointerEventData data)
    {
        highlight.SetActive(true);
        markHighlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData data)
    {
        ResetHighlight();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        color.a = .5f;
        img.color = color;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        color.a = 1f;
        img.color = color;
    }

    public void ResetHighlight()
    {
        highlight.SetActive(false);
        markHighlight.SetActive(false);

        color.a = 1f;
        img.color = color;
    }
}

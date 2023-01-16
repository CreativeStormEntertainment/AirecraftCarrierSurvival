using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TopRightButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IEnableable
{
    public Button Button => button;
    public ToggleObject ToggleObject => toggleObject;

    [SerializeField]
    private GameObject highlightedImage = null;
    [SerializeField]
    private GameObject pressedImage = null;
    [SerializeField]
    private ToggleObject toggleObject = null;
    [SerializeField]
    private Button button = null;

    private bool highlight;

    protected virtual void Start()
    {
        if (button != null && toggleObject != null)
        {
            button.onClick.AddListener(() => toggleObject.Toggle());
        }
    }

    public virtual void SetEnable(bool enable)
    {
        if (enable)
        {
            highlightedImage.SetActive(highlight);
        }
        else
        {
            pressedImage.SetActive(false);
            highlightedImage.SetActive(false);
        }
        enabled = enable;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (enabled)
        {
            pressedImage.SetActive(true);
            highlightedImage.SetActive(false);
            highlight = false;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (enabled && pressedImage.activeSelf)
        {
            pressedImage.SetActive(false);
            highlightedImage.SetActive(false);
            highlight = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (enabled)
        {
            highlightedImage.SetActive(true);
        }
        highlight = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightedImage.SetActive(false);
        pressedImage.SetActive(false);
        highlight = false;
    }
}

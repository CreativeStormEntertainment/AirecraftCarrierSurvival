using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class TacticalMapTooltip : MonoBehaviour
{
    public static TacticalMapTooltip Instance;

    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private Text info = null;
    [SerializeField]
    private Vector3 tooltipOffset = new Vector3(-150f, 0f, 0f);
    [SerializeField]
    private StudioEventEmitter showSound = null;

    private RectTransform parent;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Setup(string content, RectTransform parent)
    {
        gameObject.SetActive(true);
        this.parent = parent;
        info.text = content;
        transform.position = parent.position;
        rectTransform.anchoredPosition += (Vector2)tooltipOffset;
        if (transform.position.x - rectTransform.sizeDelta.x / 2f < 0)
        {
            rectTransform.anchoredPosition -= (Vector2)tooltipOffset * 2f;
        }
        showSound.Play();
        //transform.position = newPos + (newPos.x - rectTransform.sizeDelta.x / 2f < 0f ? -2f * tooltipOffset : Vector3.zero);
    }

    public void UpdateText(string text, RectTransform parent)
    {
        if (parent == this.parent)
        {
            info.text = text;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void HideOnDisable(RectTransform parent)
    {
        if (parent == this.parent)
        {
            Hide();
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChooseMainMenuButtons : MenuButtonSFX, IPointerExitHandler, IPointerClickHandler
{
    public RectTransform RectTransform;
    public Sprite RibbonButton;
    public Compass Compass;

    public float Offset;
    public bool IgnoreParent;
    public float WholeOffset => Offset + (IgnoreParent ? parent.anchoredPosition.y : 0f);

    protected Text text;
    protected Image arrow;
    protected Image chooseButton;

    [SerializeField]
    protected bool isLabel = false;
    [SerializeField]
    protected bool isEnabled = true;
    [SerializeField]
    protected Color32 selectedText = new Color32(182, 200, 192, 255);
    [SerializeField]
    protected Color disableText = new Color(0.55f, 0.55f, 0.55f);
    [SerializeField]
    protected Color32 normalText = new Color32(90, 109, 98, 255);
    [SerializeField]
    protected Color32 arrowColor = new Color32(197, 183, 127, 255);
    [SerializeField]
    protected Color32 showImage = new Color32(255, 255, 255, 255);
    [SerializeField]
    protected Color32 hideImage = new Color32(255, 255, 255, 0);
    [SerializeField]
    protected Color32 hideArrow = new Color32(211, 138, 49, 0);

    private RectTransform parent;

    protected override void Awake()
    {
        base.Awake();
        text = gameObject.GetComponentInChildren<Text>();
        var arrowObj = gameObject.transform.Find("Arrow");
        if (arrowObj != null)
        {
            arrow = arrowObj.GetComponent<Image>();
        }
        chooseButton = gameObject.GetComponent<Image>();
        if (button != null && !isEnabled)
        {
            text.color = disableText;
        }

        if (IgnoreParent)
        {
            parent = transform.parent as RectTransform;
        }
        if (button == null)
        {
            text.color = normalText;
        }
    }

    private void OnDisable()
    {
        if (button == null || (button.enabled && button.interactable && isEnabled))
        {
            chooseButton.color = hideImage;
            chooseButton.sprite = null;
            text.color = normalText;
            if (arrow != null)
            {
                arrow.color = hideArrow;
            }
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (button == null || (button.enabled && button.interactable && isEnabled) || isLabel)
        {
            chooseButton.color = showImage;
            chooseButton.sprite = RibbonButton;
            text.color = selectedText;
            if (Compass != null)
            {
                Compass.CurrentTarget = this;
            }
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (button == null || (button.enabled && button.interactable && isEnabled) || isLabel)
        {
            chooseButton.color = hideImage;
            chooseButton.sprite = null;
            text.color = normalText;
            if (arrow != null)
            {
                arrow.color = hideArrow;
            }
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (button == null || !isLabel && eventData.button == PointerEventData.InputButton.Left && button.enabled && button.interactable && isEnabled)
        {
            chooseButton.color = hideImage;
            chooseButton.sprite = null;
            text.color = normalText;
            if (arrow != null)
            {
                arrow.color = hideArrow;
            }
        }
    }

    //public void OnSelect(BaseEventData eventData)
    //{
    //    arrow.color = arrowColor;
    //    Invoke("UnselectPreviousChoice", 0.5f);
    //}

    //void UnselectPreviousChoice()
    //{
    //    chooseButton.color = hideImage;
    //    chooseButton.sprite = null;
    //    text.color = normalText;
    //    arrow.color = hideArrow;
    //}

}




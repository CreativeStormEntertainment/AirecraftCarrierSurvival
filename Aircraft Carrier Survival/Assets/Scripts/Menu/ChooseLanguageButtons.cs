using UnityEngine;
using UnityEngine.EventSystems;

public class ChooseLanguageButtons : ChooseMainMenuButtons, IPointerClickHandler
{
    public GameObject Image => image;

    [SerializeField]
    private SetLanguage setter = null;

    [SerializeField]
    private GameObject image = null;

    [SerializeField]
    private string lang = "en";

    protected override void Awake()
    {
        base.Awake();
        if (SaveManager.Instance.PersistentData.Lang == lang)
        {
            image.SetActive(true);
            button.interactable = false;
        }
        text.color = button.interactable ? normalText : selectedText;
        if (setter.LanguageButtons.Contains(this))
        {
            isEnabled = true;
        }
        else
        {
            isEnabled = false;
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (isEnabled)
        {
            foreach (ChooseLanguageButtons b in setter.LanguageButtons)
            {
                b.button.interactable = true;
                b.Image.SetActive(false);
                b.chooseButton.color = hideImage;
                b.chooseButton.sprite = null;
                b.text.color = normalText;
            }
            image.SetActive(true);
            button.interactable = false;
            chooseButton.color = showImage;
            chooseButton.sprite = RibbonButton;
            text.color = selectedText;
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
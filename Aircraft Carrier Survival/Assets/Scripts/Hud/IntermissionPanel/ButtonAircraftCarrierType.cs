using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAircraftCarrierType : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField]
    private ECarrierType carrierType = default;
    [SerializeField]
    private GameObject popup = null;
    [SerializeField]
    private GameObject highlightImage = null;
    [SerializeField]
    private GameObject pressedImage = null;

    [SerializeField]
    private Image shipIconToSet = null;
    [SerializeField]
    private Text typeTextToSet = null;

    [SerializeField]
    private Text typeToSetOnButton = null;

    [SerializeField]
    private bool shouldLocked = false; // for DEMO
    [SerializeField]
    private Image shipIcon = null;
    [SerializeField]
    private ShowTooltip tooltipToShow = null;
    [SerializeField]
    private Button button = null;

    private void Start()
    {
        button.onClick.AddListener(ButtonClicked);

        //shipIconToSet.sprite = IntermissionManager.Instance.GetAircraftCarrierTypeList()[index].GetImageIcon();
        //typeTextToSet.text = IntermissionManager.Instance.GetAircraftCarrierTypeList()[index].GetTypeText();

        if (shouldLocked)
        {
            button.interactable = false;
            shipIcon.color = Color.gray;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        if (!shouldLocked)
        {
            highlightImage.SetActive(true);
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!shouldLocked)
        {
            highlightImage.SetActive(false);
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!shouldLocked)
        {
            highlightImage.SetActive(false);
            pressedImage.SetActive(true);
            if (tooltipToShow)
            {
                tooltipToShow.HideTooltip();
            }
        }
    }
    public void ButtonClicked()
    {
        pressedImage.SetActive(false);
        popup.SetActive(false);

        SetNameAndType();
    }

    public void SetNameAndType()
    {
        //typeToSetOnButton.text = IntermissionManager.Instance.GetAircraftCarrierTypeList()[index].GetTypeText();
    }

}

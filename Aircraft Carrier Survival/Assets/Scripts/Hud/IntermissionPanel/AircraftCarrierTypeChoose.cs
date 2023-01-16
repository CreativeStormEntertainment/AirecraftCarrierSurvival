using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AircraftCarrierTypeChoose : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject selectedImage = null;
    [SerializeField]
    private GameObject PopupToOpen = null;
    [SerializeField]
    private ButtonAircraftCarrierType firstTypeChosenOnStart;
    [SerializeField]
    private ShowTooltip tooltipToShow = null;

    private bool isShown = false;

    public GameObject SelectedImage
    {
        get
        {
            return selectedImage;
        }
    }

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(() => SetImageAndPopUp(!isShown));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipToShow.FillTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipToShow.HideTooltip();
    }

    public void SetImageAndPopUp(bool active)
    {
        isShown = active;
        selectedImage.SetActive(isShown);
        PopupToOpen.SetActive(isShown);
    }
}

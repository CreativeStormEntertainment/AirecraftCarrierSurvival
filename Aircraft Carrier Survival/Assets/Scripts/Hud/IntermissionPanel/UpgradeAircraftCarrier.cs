using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeAircraftCarrier : UpgradeShipButton
{
    [SerializeField]
    private List<Text> textsToChangeColour = null;
    private List<Image> iconToShow = new List<Image>();

    [SerializeField]
    private GameObject currentUpgrade = null;

    [SerializeField]
    private GameObject hideOnHover = null;

    [SerializeField]
    private HideObjectOnHover hideObjectOnHover = null;

    [SerializeField]
    private GameObject crewToHide = null;

    private Color32 activeImageColor = new Color32(255, 255, 255, 255);

    public override void Setup(List<Sprite> sprites)
    {
        base.Setup(sprites);
        foreach (GameObject image in imagesToShow)
        {
            iconToShow.Add(image.GetComponent<Image>());
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (!ShouldLockedOnStart && button.interactable)
        {
            if (hideOnHover != null)
            {
                hideOnHover.SetActive(false);
            }
            if (crewToHide != null)
            {
                crewToHide.SetActive(false);
            }
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (!ShouldLockedOnStart && button.interactable && !isConfirmWindowOpen)
        {
            if (hideOnHover != null)
            {
                hideOnHover.SetActive(true);
            }
            if (crewToHide != null)
            {
                crewToHide.SetActive(true);
            }
        }
    }

    protected override void ShowUnlockedElements()
    {
        if (indexIcon < stepsNumber)
        {
            iconToShow[indexIcon].color = activeImageColor;
            textsToChangeColour[indexIcon].color = activeTextColor;
        }
    }

    public override void ConfirmUpgrade()
    {
        currentUpgrade.SetActive(false);
        currentUpgrade = assetsToShow[index];
        if (hideOnHover != null)
        {
            hideOnHover.SetActive(true);
        }
        SetAssetsHighlight(false);
        if (hideObjectOnHover != null)
        {
            hideObjectOnHover.IsLocked = false;
        }
        base.ConfirmUpgrade();
        OnPointerExit(null);
    }

    public override void StopUpgrade()
    {
        if (hideOnHover != null)
        {
            hideOnHover.SetActive(true);
        }
        if (hideObjectOnHover != null)
        {
            hideObjectOnHover.IsLocked = false;
        }
        base.StopUpgrade();
        OnPointerExit(null);
    }

    protected override void OpenConfirmWindow()
    {
        if (hideObjectOnHover != null)
        {
            hideObjectOnHover.IsLocked = true;
        }
        base.OpenConfirmWindow();
    }
}

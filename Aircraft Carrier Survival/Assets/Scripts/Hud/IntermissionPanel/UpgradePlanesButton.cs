using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradePlanesButton : IntermissionInactiveClick, IPointerExitHandler
{
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private Text planeName = null;
    [SerializeField]
    private Text offencePercent = null;
    private int offencePoints = 0;
    [SerializeField]
    private Text defencePercent = null;
    private int defencePoints = 0;
    [SerializeField]
    private Image buttonBackground = null;
    [SerializeField]
    private Sprite buttonBackgroundDisabled = null;
    [SerializeField]
    private List<Sprite> planesImagesToSwap = null;
    [SerializeField]
    private Image planeImage = null;

    [SerializeField]
    public List<GameObject> PlanesTypeA = null;
    [SerializeField]
    public List<GameObject> PlanesTypeB = null;
    [SerializeField]
    public List<GameObject> PlanesTypeC = null;

    [SerializeField]
    List<GameObject> highlightPlanesTypeA = null;
    [SerializeField]
    List<GameObject> highlightPlanesTypeB = null;

    [SerializeField]
    private ShowTooltip tooltipToShow = null;

    [SerializeField]
    private string textConfirmWindow = null;
    [SerializeField]
    private ConfirmWindow confirmWindow = null;

    public EPlaneType PlaneType;

    private int index = 0;
    private int stepsNumber = 2;
    private int upgradeNumber = 0;

    private List<PlaneUpgradeData> planeUpgrade = new List<PlaneUpgradeData>();
    private List<Sprite> iconSprites = null;

    public void Setup(List<Sprite> sprites)
    {
        iconSprites = sprites;
        button = GetComponent<Button>();
        planeImage.sprite = planesImagesToSwap[index];

        //foreach (PlaneUpgradeData planeupgrade in IntermissionManager.Instance.GetPlaneList())
        //{
        //    if (planeupgrade.GetPlaneType() == PlaneType)
        //    {
        //        planeUpgrade.Add(planeupgrade);
        //    }
        //}

        SetPlaneNameAndParams(upgradeNumber);

        button.onClick.AddListener(OpenConfirmWindow);

        var planeLevel = SaveManager.Instance.Data.GetPlaneLv(PlaneType);
        for (int i = 0; i < planeLevel; i++)
        {
            ConfirmUpgrade();
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (tooltipToShow)
        {
            tooltipToShow.FillTooltip();
        }
        if (button.interactable)
        {
            ShowHideHighlight(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipToShow.HideTooltip();
        if (button.interactable)
        {
            ShowHideHighlight(false);
        }
    }

    private void OpenConfirmWindow()
    {
        confirmWindow.Setup(EDeckUpgradeType.Aircraft, textConfirmWindow, icon.sprite);
        IntermissionPanel.Instance.ActivateClickBlocker(true);
    }

    public void StopUpgrade()
    {
        ShowHideHighlight(false);
    }

    public void ConfirmUpgrade()
    {
        ShowHideHighlight(false);

        planeImage.sprite = planesImagesToSwap[index + 1];

        SetPlaneNameAndParams(upgradeNumber);

        if (index < stepsNumber)
        {
            if (index == 0)
            {
                foreach (GameObject plane in PlanesTypeA)
                {
                    plane.SetActive(false);
                }
                foreach (GameObject plane in PlanesTypeB)
                {
                    plane.SetActive(true);
                }
            }
            else
            {
                foreach (GameObject plane in PlanesTypeB)
                {
                    plane.SetActive(false);
                }
                foreach (GameObject plane in PlanesTypeC)
                {
                    plane.SetActive(true);
                }
            }
            index++;
        }
        if (index < iconSprites.Count)
        {
            icon.sprite = iconSprites[index];
        }
        if (index == stepsNumber)
        {
            gameObject.SetActive(false);
        }
        SaveManager.Instance.Data.SetPlaneLv(PlaneType, index);
    }

    public void Disable()
    {
        if (button.interactable)
        {
            buttonBackground.sprite = buttonBackgroundDisabled;
            button.interactable = false;
            tooltipToShow.IndexTooltip++;
        }
    }

    private void SetPlaneNameAndParams(int upgrade)
    {
        var currentPlane = planeUpgrade[upgrade];
        planeName.text = currentPlane.GetPlaneName();
        offencePoints += currentPlane.GetSurvivabilityPercent();
        offencePercent.text = "+" + offencePoints + "%";
        //defencePoints += currentPlane.GetDefencePercent();
        //defencePercent.text = "+" + defencePoints + "%";
        upgradeNumber++;
    }

    private void ShowHideHighlight(bool isShow)
    {
        if (index == 0)
        {
            foreach (GameObject highlight in highlightPlanesTypeA)
            {
                highlight.SetActive(isShow);
            }
        }
        else
        {
            foreach (GameObject highlight in highlightPlanesTypeB)
            {
                highlight.SetActive(isShow);
            }
        }

    }
}

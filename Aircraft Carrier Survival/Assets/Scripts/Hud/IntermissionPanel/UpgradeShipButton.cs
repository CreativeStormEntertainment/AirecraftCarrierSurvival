using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeShipButton : IntermissionInactiveClick, IPointerExitHandler
{
    public int UpgradeTier => upgradeNumber + 1;

    [SerializeField]
    private Image icon = null;
    private List<Sprite> iconSprites = null;
    [SerializeField]
    protected List<GameObject> imagesToShow = null;
    [SerializeField]
    private List<Text> textsToShow = null;
    protected Color32 activeTextColor = new Color32(255, 248, 170, 255);
    [SerializeField]
    private Image buttonBackground = null;
    [SerializeField]
    private Sprite buttonBackgroundDisabled = null;
    [SerializeField]
    protected List<GameObject> assetsToShow = null;
    [SerializeField]
    private List<GameObject> planes1upgrade = null;
    [SerializeField]
    private List<GameObject> planes2upgrade = null;
    private int upgradeNumber = 0;
    [SerializeField]
    private ShowTooltip tooltipToShow = null;

    [SerializeField]
    private string textConfirmWindow = null;

    protected int index = 0;
    protected int stepsNumber = 2;
    protected int indexIcon = 0;

    [SerializeField]
    protected bool ShouldLockedOnStart = false; // just for DEMO

    protected IntermissionPanel intermissionPanel;

    protected bool isConfirmWindowOpen = false;

    public virtual void Setup(List<Sprite> sprites)
    {
        iconSprites = sprites;
        intermissionPanel = IntermissionPanel.Instance;
        button = GetComponent<Button>();
        if (ShouldLockedOnStart)
        {
            button.interactable = false;
        }
        button.onClick.AddListener(OpenConfirmWindow);
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
            SetAssetsHighlight(true);
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipToShow)
        {
            tooltipToShow.HideTooltip();
            if (button.interactable && !isConfirmWindowOpen)
            {
                SetAssetsHighlight(false);
            }
        }
    }

    protected virtual void OpenConfirmWindow()
    {
        isConfirmWindowOpen = true;
        intermissionPanel.ConfirmWindow.Setup(EDeckUpgradeType.Carrier, textConfirmWindow, icon.sprite);
        intermissionPanel.ActivateClickBlocker(true);
    }

    public virtual void StopUpgrade()
    {
        isConfirmWindowOpen = false;
        SetAssetsHighlight(false);
    }

    public virtual void ConfirmUpgrade()
    {
        isConfirmWindowOpen = false;
        upgradeNumber++;
        if (index < stepsNumber)
        {
            if (assetsToShow.Count - index > 0)
            {
                assetsToShow[index].SetActive(true);
            }
            index++;
        }

        if (index == stepsNumber)
        {
            gameObject.SetActive(false);
        }

        if (indexIcon < iconSprites.Count)
        {
            icon.sprite = iconSprites[indexIcon];
        }
        ShowUnlockedElements();

        indexIcon++;
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

    protected virtual void ShowUnlockedElements()
    {
        if (indexIcon < stepsNumber)
        {
            imagesToShow[indexIcon].SetActive(true);
            textsToShow[indexIcon].color = activeTextColor;
        }
    }

    protected void SetAssetsHighlight(bool isShow)
    {
        if (upgradeNumber == 0)
        {
            foreach (GameObject asset in planes1upgrade)
            {
                asset.SetActive(isShow);
            }
        }
        else
        {
            foreach (GameObject asset in planes2upgrade)
            {
                asset.SetActive(isShow);
            }
        }
    }
}



using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotShipButton : ButtonSFX, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private RectTransform shipsPanel = null;
    public int ShipIndex = 0;
    public bool IsUnlock;
    public bool Last;

    public int Nr { get; private set; }
    [SerializeField]
    private Image shipIcon = null;
    [SerializeField]
    private RectTransform slotsPanelRect = null;
    [SerializeField]
    private Text shipName = null;
    [SerializeField]
    private ShowTooltip tooltipToShow = null;

    [SerializeField]
    private GameObject lockedIcon = null;
    [SerializeField]
    private GameObject highlightTexture = null;
    [SerializeField]
    private GameObject pressedTexture = null;
    [SerializeField]
    private GameObject pressedTexture2 = null;
    [SerializeField]
    private GameObject selectedTexture = null;


    private RectTransform rect = null;

    private static SlotShipButton LastSelected;

    public int LastChosenIndex { get; set; } = -1;

    private ShowTooltip unlockedButtonTooltip = null;
    private ShowTooltip lockedButtonTooltip = null;
    //private EscortSubpanel escortPanel;

    [SerializeField]
    private bool shouldLocked = false; // just for now to lock one slot

    public Text ShipName
    {
        get
        {
            return shipName;
        }
    }

    public Image ShipIcon
    {
        get
        {
            return shipIcon;
        }
    }

    public Button Button
    {
        get
        {
            return button ?? (button = GetComponent<Button>());
        }
    }

    public void Setup(int n)
    {
        Nr = n;
        ShowTooltip[] tooltips = tooltipToShow.GetComponents<ShowTooltip>();
        if (tooltips.Length > 0)
        {
            unlockedButtonTooltip = tooltips[0];
            if (tooltips.Length > 1)
            {
                lockedButtonTooltip = tooltips[1];
            }
        }
        rect = GetComponent<RectTransform>();

        if (!IsUnlock)
        {
            Button.interactable = false;
        }

        Button.onClick.AddListener(ShowShipPanel);

        if (shouldLocked)
        {
            Button.interactable = false;
        }
        //escortPanel = IntermissionPanel.Instance.Escort;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        ShowTooltip tooltipToShow = IsUnlock ? unlockedButtonTooltip : lockedButtonTooltip;
        if (tooltipToShow)
        {
            if (IsUnlock)
            {
                if (shouldLocked)
                {
                    tooltipToShow.FillTooltip();
                }
                else
                {
                    tooltipToShow.IndexTooltip += 1;
                    tooltipToShow.FillTooltip();
                    tooltipToShow.IndexTooltip -= 1;
                    tooltipToShow.HeaderTooltip.text = shipName.text;
                }
            }
            else
            {
                tooltipToShow.FillTooltip();
            }
        }

        if (IsUnlock)
        {
            highlightTexture.SetActive(true);

        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipToShow)
        {
            tooltipToShow.HideTooltip();
        }


        if (IsUnlock)
        {
            highlightTexture.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!shouldLocked && IsUnlock)
        {
            if (LastSelected != null)
            {
                LastSelected.Deselect();
            }
            highlightTexture.SetActive(false);
            pressedTexture.SetActive(true);
            pressedTexture2.SetActive(true);

            LastSelected = this;
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!shouldLocked && IsUnlock)
        {
            pressedTexture2.SetActive(false);
            selectedTexture.SetActive(true);
        }
    }

    public void Deselect()
    {
        pressedTexture.SetActive(false);
        selectedTexture.SetActive(false);
    }

    private void ShowShipPanel()
    {
        if (shouldLocked)
        {
            return;
        }
        var shipsPanelPosition = shipsPanel.anchoredPosition;

        var posY = rect.anchoredPosition.y;
        var slotsPanelPosY = slotsPanelRect.anchoredPosition.y;
        shipsPanel.anchoredPosition = new Vector3(shipsPanelPosition.x, posY + slotsPanelPosY + 150);

        if (!shipsPanel.gameObject.activeInHierarchy || ShipButton.CurrentChosenSlot != this)
        {
            shipsPanel.gameObject.SetActive(true);
            //escortPanel.SetTooltipsPosition(Last);

            ShipButton.CurrentChosenSlot = this;
        }
        else
        {
            shipsPanel.gameObject.SetActive(false);
            ShipButton.CurrentChosenSlot = null;
        }
    }

    public void ShowShipAndIcon()
    {
        shipName.gameObject.SetActive(true);
        //escortPanel.shipModels[Nr][ShipIndex].SetActive(true);
        //shipIcon.sprite = escortPanel.Data.Data[ShipIndex].Icon;
    }

    public void ShowHideHighlightSelections(int shipIndex, bool isShow)
    {
        //escortPanel.shipModels[Nr][shipIndex].SetActive(isShow);
    }

    public void ShowHideHighlightSelection(bool isShow)
    {
        if (LastChosenIndex != -1)
        {
            //escortPanel.shipModels[Nr][LastChosenIndex].SetActive(isShow);
        }
    }

    public void ActivateSlot()
    {
        lockedIcon.SetActive(false);
        shipName.gameObject.SetActive(true);
        shipIcon.gameObject.SetActive(true);
        //shipName.text = LocalizationManager.Instance.GetText(escortPanel.Data.Data[ShipIndex].NameID);
    }
}

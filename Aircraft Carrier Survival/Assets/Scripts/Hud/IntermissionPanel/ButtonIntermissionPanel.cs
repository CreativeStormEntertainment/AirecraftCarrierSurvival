using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonIntermissionPanel : ButtonSFX, IPointerExitHandler
{
    public static ButtonIntermissionPanel LastSelected = null;

    public event Action OnShowPanel = delegate { };

    public int CameraIndex => cameraIndex;

    protected bool isLaunchButton = false;

    [SerializeField]
    private bool isSelected = false;
    [SerializeField]
    private IntermissionSubpanel panelToShow = null;
    [SerializeField]
    private int cameraIndex = 0;

    [SerializeField]
    private ShowTooltip tooltipToShow = null;

    [SerializeField]
    private bool shouldStrikePopupOpen = false;
    [SerializeField]
    private GameObject strikeGroupPopup = null;

    [SerializeField]
    private TutorialIntermission tutorialIntermission = null;

    [SerializeField]
    private GameObject popupToClose = null;

    private Sprite sprite;

    private bool isTooltipChanged;
    private int tooltip2Index;

    private void Start()
    {
        if (tooltipToShow)
        {
            tooltip2Index = tooltipToShow.IndexTooltip += 1;
        }
        sprite = button.image.sprite;
        if (isSelected)
        {
            button.interactable = false;
            ShowPanel();
            LastSelected = this;
        }

        button?.onClick.AddListener(ShowPanel);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (LastSelected != this)
        {
            base.OnPointerEnter(eventData);
        }
        if (tooltipToShow)
        {
            if (tutorialIntermission.Clicked.Count >= 4 && !isTooltipChanged)
            {
                tooltipToShow.IndexTooltip = tooltip2Index;
                isTooltipChanged = true;
            }
            tooltipToShow.FillTooltip();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipToShow != null)
        {
            tooltipToShow.HideTooltip();
        }
    }

    public void ShowPanel()
    {
        var saveMan = SaveManager.Instance;
        if (saveMan.Data.GameMode != EGameMode.Fabular && isLaunchButton)
        {
            saveMan.Data.GameMode = EGameMode.Fabular;
            saveMan.TransientData.HighlightHelp = true;
        }

        popupToClose.SetActive(false);
        strikeGroupPopup.SetActive(shouldStrikePopupOpen);

        if (LastSelected != null)
        {
            LastSelected.Deselect();
        }

        button.interactable = false;

        panelToShow?.Show();


        LastSelected = this;

        OnShowPanel();
        SetCamera();
    }

    public virtual void Deselect()
    {
        panelToShow?.Hide();
        button.interactable = true;
    }

    public virtual void SetCamera()
    {
        if (!isLaunchButton)
        {
            CamerasIntermission.Instance.SetIntermissionCameras(cameraIndex);
        }
    }
}

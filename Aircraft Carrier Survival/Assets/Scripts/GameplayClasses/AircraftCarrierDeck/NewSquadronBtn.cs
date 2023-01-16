using UnityEngine;
using UnityEngine.EventSystems;

public class NewSquadronBtn : MonoBehaviour, IEnableable
{
    [SerializeField]
    private Camera mainCamera = null;
    [SerializeField]
    private SpriteRenderer perimeterSprite = null;
    [SerializeField]
    private GameObject highlightSprite = null;
    [SerializeField]
    private GameObject pressedSprite = null;
    [SerializeField]
    private GameObject inactiveSprite = null;
    [SerializeField]
    private GameObject plusIcon = null;
    [SerializeField]
    private GameObject inactiveIcon = null;
    [SerializeField]
    private Vector3 offset = default;

    private bool clicked;
    private bool hovered;
    private bool disabled;
    private bool inactive;
    private bool tooltip;

    private DragPlanesManager dpManager;

    private void Start()
    {
        dpManager = DragPlanesManager.Instance;
    }

    private void OnMouseEnter()
    {
        var hudMan = HudManager.Instance;
        if (!disabled && !hudMan.IsSettingsOpened && hudMan.AcceptInput && !EventSystem.current.IsPointerOverGameObject())
        {
            if (inactive)
            {
                ShowTooltip();
            }
            else
            {
                highlightSprite.SetActive(true);
                BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
            }
        }
        hovered = true;
    }

    private void OnMouseExit()
    {
        if (tooltip)
        {
            Tooltip.Instance.Hide();
            tooltip = false;
        }

        highlightSprite.SetActive(false);

        perimeterSprite.enabled = true;
        pressedSprite.SetActive(false);
        clicked = false;
        hovered = false;
    }

    private void OnMouseDown()
    {
        var hudMan = HudManager.Instance;
        if (!disabled && !dpManager.LiftPanelIsOpen && !hudMan.IsSettingsOpened && hudMan.AcceptInput && !inactive)
        {
            highlightSprite.SetActive(false);
            perimeterSprite.enabled = false;
            pressedSprite.SetActive(true);
            clicked = true;
        }
    }

    private void OnMouseUp()
    {
        var hudMan = HudManager.Instance;
        if (!disabled && clicked && !dpManager.LiftPanelIsOpen && !hudMan.IsSettingsOpened && hudMan.AcceptInput && !inactive)
        {
            highlightSprite.SetActive(true);
            dpManager.ShowLiftPanel(true);
            BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
        }
        perimeterSprite.enabled = true;
        pressedSprite.SetActive(false);
        clicked = false;
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
        if (!enable)
        {
            perimeterSprite.enabled = true;
            pressedSprite.SetActive(false);
            highlightSprite.SetActive(false);
        }
    }

    public void SetActive(bool active)
    {
        if (tooltip && active)
        {
            Tooltip.Instance.Hide();
            tooltip = false;
        }
        inactive = !active;
        plusIcon.SetActive(active);
        inactiveIcon.SetActive(inactive);
        inactiveSprite.SetActive(inactive);
        var hudMan = HudManager.Instance;
        if (inactive && !tooltip && hovered && !disabled && !hudMan.IsSettingsOpened && hudMan.AcceptInput && !EventSystem.current.IsPointerOverGameObject())
        {
            ShowTooltip();
        }
    }

    private void ShowTooltip()
    {
        Tooltip.Instance.SetupSquadronTooltip(dpManager.InactiveSlotTooltipTitle, dpManager.InactiveSlotTooltipDesc, mainCamera.WorldToScreenPoint(transform.position) + offset);
        tooltip = true;
    }
}

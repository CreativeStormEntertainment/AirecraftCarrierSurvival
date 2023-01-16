using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeEscortSlot : IntermissionInactiveClick, IPointerExitHandler
{
    [SerializeField]
    private Image buttonBackground = null;
    [SerializeField]
    private Sprite buttonBackgroundDisabled = null;

    [SerializeField]
    private ShowTooltip tooltipToShow = null;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        tooltipToShow.FillTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipToShow.HideTooltip();
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
}

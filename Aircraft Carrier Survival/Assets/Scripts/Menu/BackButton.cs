using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackButton : MenuButtonSFX, IPointerExitHandler
{
    public Image Arrow;

    private Color32 brighteningArrow = new Color32(255, 255, 255, 255);
    private Color32 darkeningArrow = new Color32(160, 174, 180, 255);

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        Arrow.color = brighteningArrow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Arrow.color = darkeningArrow;
    }
}

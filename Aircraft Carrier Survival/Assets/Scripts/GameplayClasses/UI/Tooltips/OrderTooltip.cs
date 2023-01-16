using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class OrderTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    string Title = "";

    private Image orderTooltipImage = null;
    private string description = "";
    private string effect = "";
    private string duration;
    private int costNavy = 0;
    private int costAir = 0;

    private LocalizationManager locMan = null;

    public void Setup(string name, int air, int navy, string time, EIslandBuffEffectParam param)
    {
        name = name.Replace(" ", "");
        locMan = LocalizationManager.Instance;
        Title = locMan.GetText(name + "Title");
        effect = locMan.GetText(name + "Effect");
        duration = locMan.GetText(name + "Duration");

        costNavy = navy;
        costAir = air;
        description = locMan.GetText(name + "Desc");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OrderDetailsTooltipCall.Instance.ShowOrderDetailsTooltip();
        orderTooltipImage = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        HudManager.Instance.ShowBuffTooltip(Title, description, duration, effect, costAir, costNavy, orderTooltipImage);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OrderDetailsTooltipCall.Instance.HideOrderDetailsTooltip();
    }
}

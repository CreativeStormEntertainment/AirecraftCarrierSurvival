using System;
using UnityEngine.EventSystems;

public class UpgradeButton : BuyButton, IPointerEnterHandler, IPointerExitHandler
{
    private bool highlight;
    private Action<bool> highlightAction;

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight = true;
        highlightAction?.Invoke(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight = false;
        highlightAction?.Invoke(false);
    }

    public void Setup(bool commandPoints, int maxUpgrades, Action upgrade, Action<bool> highlightAction)
    {
        Setup(commandPoints, maxUpgrades, upgrade);
        this.highlightAction = highlightAction;
    }

    public override void SetUpgrade(int upgrade, int cost, bool isUnlock)
    {
        base.SetUpgrade(upgrade, cost, isUnlock);
        if (highlight && highlightAction != null)
        {
            highlightAction(true);
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class CardPlace : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CardHandler CurrentCard => currentCard;
    public GameObject Blocker => blocker;
    public GameObject DropHere => dropHere;

    [SerializeField]
    private GameObject blocker = null;

    [SerializeField]
    private GameObject dropHere = null;

    [SerializeField]
    private TooltipCaller tooltipCaller = null;

    private StrategySelectionPanel strategyPanel = null;
    private CardHandler currentCard = null;

    private int blockerIndex;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (blocker.activeSelf)
        {
            strategyPanel.SetShowHighlight(true, blockerIndex);
        }
        if (CurrentCard != null)
        {
            foreach (int index in CurrentCard.DebuffIndices)
            {
                strategyPanel.SetShowHighlight(true, index);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        strategyPanel.SetShowHighlight(false, blockerIndex);
        if (CurrentCard != null)
        {
            foreach (int index in CurrentCard.DebuffIndices)
            {
                strategyPanel.SetShowHighlight(false, index);
            }
        }
    }

    public void Setup(StrategySelectionPanel panel)
    {
        dropHere.SetActive(false);
        strategyPanel = panel;
        tooltipCaller.enabled = true;
    }

    public void CardPlaced(CardHandler card)
    {
        if (currentCard != null)
        {
            currentCard.DebuffIndices.Clear();
            if (currentCard.SlotModifierBlocked)
            {
                currentCard.SlotModifierBlocked = false;
                currentCard.ModifierBlocker.SetActive(false);
                currentCard.BlockerIndex = -1;
            }
            currentCard.ResetPlacement();
        }

        currentCard = card;
        strategyPanel.RecalculateValues();
        tooltipCaller.enabled = currentCard == null;
        if (currentCard == null)
        {
            tooltipCaller.HideTooltip();
        }
    }

    public void SetBlocker(bool block, int index = -1)
    {
        blockerIndex = index;
        blocker.SetActive(block);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyPanel : MonoBehaviour
{
    [SerializeField]
    private Text torpedoText = null;
    [SerializeField]
    private Text bombsText = null;
    [SerializeField]
    private Text ammoText = null;

    [SerializeField]
    private List<Strategy> strategies = new List<Strategy>();
    [SerializeField]
    private List<StrategyDragDrop> strategiesDragDrop = new List<StrategyDragDrop>();

    [SerializeField]
    private Transform strategiesSlotsParent = null;
    [SerializeField]
    private List<StrategyItemSlot> strategySlots = new List<StrategyItemSlot>();

    private void Awake()
    {
        if (strategySlots.Count == 0)
        {
            foreach (Transform t in strategiesSlotsParent)
            {
                strategySlots.Add(t.GetComponent<StrategyItemSlot>());
            }
        }
    }

    private void OnEnable()
    {
        if (strategies.Count > 0)
        {
            foreach (StrategyItemSlot slot in strategySlots)
            {
                if (!slot.Busy)
                {
                    strategiesDragDrop[0].AssignToSlot(slot);
                    UnpackStrategy(strategiesDragDrop[0]);
                }
            }
        }
    }

    public void AssignStrategy(StrategyDragDrop strategy)
    {
        TacticManager.Instance.SetStrategy(strategy.StrategyItem.strategy);
        if (strategies.Count < 3)
        {
            strategies.Add(strategy.StrategyItem.strategy);
            strategiesDragDrop.Add(strategy);
        }
        else
        {
            strategiesDragDrop[0] = strategy;
            strategies[0] = strategy.StrategyItem.strategy;
        }

        RecalculateSlots();
    }

    public void UnpackStrategy(StrategyDragDrop strategy)
    {
        if (strategies.Count > 0)
        {
            strategies.Remove(strategy.StrategyItem.strategy);
            strategiesDragDrop.Remove(strategy);
        }

        if (strategy.StrategyItem != null)
        {
            TacticManager.Instance.UnsetStrategy(strategy.StrategyItem.strategy);
        }

        RecalculateSlots();
    }

    public void LockStrategiesForTutorial(List<StrategyItem> unlocked)
    {
        if (strategySlots.Count == 0)
        {
            foreach (Transform t in strategiesSlotsParent)
            {
                strategySlots.Add(t.GetComponent<StrategyItemSlot>());
            }
        }
        foreach (StrategyItemSlot slot in strategySlots)
        {
            if (slot.Busy && slot.CurrentStrategy != null && !unlocked.Contains(slot.CurrentStrategy.StrategyItem))
            {
                slot.CurrentStrategy.SetLock(true);
            }
        }
    }
    public void UnlockAllStrategiesAfterTutorial()
    {
        foreach (StrategyItemSlot slot in strategySlots)
        {
            if (slot.Busy && slot.CurrentStrategy != null && slot.CurrentStrategy.LockImage != null)
            {
                slot.CurrentStrategy.SetLock(false);
            }
        }
    }

    private void RecalculateSlots()
    {
        TacticManager.Instance.GetPlanesFromStrategies(strategies, out int bombers, out int fighters, out int torpedoes);

        torpedoText.text = torpedoes.ToString();
        bombsText.text = bombers.ToString();
        ammoText.text = fighters.ToString();
    }
}
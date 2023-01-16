/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StrategyItemSlot : MonoBehaviour, IDropHandler
{
    public bool noAction = false;
    public StrategyDragDrop CurrentStrategy
    {
        get
        {
            if (currentStrategy == null)
            {
                currentStrategy = GetComponentInChildren<StrategyDragDrop>();
                if (currentStrategy != null)
                {
                    currentStrategy.StrategyItem = currentStrategy.GetComponent<StrategyItem>();
                }
            }
            return currentStrategy;
        }
    }
    private StrategyDragDrop currentStrategy = null;
    public bool Busy = false;

    public void UnpackStrategy()
    {
        if (currentStrategy == null)
            return;
        currentStrategy = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        }
    }

    public void AssignStrategy(StrategyDragDrop drop)
    {
        currentStrategy = drop;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LaunchButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public List<GameObject> Warnings => warnings;

    [SerializeField]
    private GameObject tooltip = null;
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private List<GameObject> warnings = null;

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable)
        {
            tooltip.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bool can = false;
        foreach (var war in warnings)
        {
            if (war.activeSelf)
            {
                can = true;
                break;
            }
        }
        if (!button.interactable && can)
        {
            tooltip.SetActive(true);
        }
    }
}

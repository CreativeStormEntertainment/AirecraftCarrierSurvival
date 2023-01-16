using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverHighlightButton : Button
{
    public List<GameObject> ListToHighlight;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        foreach (var obj in ListToHighlight)
        {
            obj.SetActive(true);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        foreach (var obj in ListToHighlight)
        {
            obj.SetActive(false);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var obj in ListToHighlight)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DifficultyButton : MenuButtonSFX, IPointerExitHandler
{
    [SerializeField] private Image frameMissionButton = null;
    [SerializeField] private Text text = null;
    public bool IsSelected = false;

    private Color32 selectedFrameColor = new Color32(197, 183, 127, 255);
    private Color32 frameColor = new Color32(182, 200, 192, 255);

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (!IsSelected)
        {
            frameMissionButton.color = selectedFrameColor;
            text.color = selectedFrameColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsSelected)
        {
            Deselect();
        }
    }

    public void Deselect()
    {
        frameMissionButton.color = frameColor;
        text.color = frameColor;
    }

}

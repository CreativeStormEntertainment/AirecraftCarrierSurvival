using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChooseMissionButton : ButtonSFX, IPointerExitHandler
{

    public Image FrameMissionButton;
    public Image ImageMissionButton;
    public GameObject GradientDescriptionButton;
    public bool IsSelected = false;
    private Image gradientDescription;

    private Color32 orangeColor = new Color32(211, 138, 49, 255);
    private Color32 whiteColor = new Color32(255, 255, 255, 255);
    private Color32 brightening = new Color32(255, 255, 255, 255);
    private Color32 darkening = new Color32(236, 236, 236, 255);
    private Color32 hideGradient = new Color32(236, 236, 236, 0);
    private Color32 showGradient = new Color32(236, 236, 236, 255);

    private void Start()
    {
        gradientDescription = GradientDescriptionButton.GetComponent<Image>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (!IsSelected)
        {
            FrameMissionButton.color = orangeColor;
            ImageMissionButton.color = brightening;
            gradientDescription.color = hideGradient;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsSelected)
        {
            FrameMissionButton.color = whiteColor;
            ImageMissionButton.color = darkening;
        }
    }
}

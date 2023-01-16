using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChooseDifficultyButtons : ButtonSFX, IPointerEnterHandler, IPointerExitHandler
{
    private Color32 brightening = new Color32(255, 255, 255, 255);
    private Color32 darkening = new Color32(200, 200, 200, 255);
    private Vector3 smallerButton;
    private Image imageDifficultyButton;
    private Vector3 buttonPosition;
    private float edgeOffset;

    private void Start()
    {
        imageDifficultyButton = GetComponent<Image>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        imageDifficultyButton.color = brightening;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        imageDifficultyButton.color = darkening;
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    buttonPosition = transform.localPosition;
    //    transform.localScale = smallerButton;
    //    imageDifficultyButton.color = darkening;
    //    if(name == "NormalLvl")
    //    {
    //        transform.localPosition = new Vector3(buttonPosition.x + edgeOffset, buttonPosition.y, buttonPosition.z); //middle edge stays in the same x position
    //    }
    //    else
    //    {
    //        transform.localPosition = new Vector3(buttonPosition.x - edgeOffset, buttonPosition.y, buttonPosition.z);
    //    }
    //}

}

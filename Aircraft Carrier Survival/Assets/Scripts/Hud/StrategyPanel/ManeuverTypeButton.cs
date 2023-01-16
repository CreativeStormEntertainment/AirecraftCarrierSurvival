using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FMODUnity;

public class ManeuverTypeButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField]
    private StudioEventEmitter hover = null;
    [SerializeField]
    private StudioEventEmitter click = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        click.Play();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hover.Play();
    }
}

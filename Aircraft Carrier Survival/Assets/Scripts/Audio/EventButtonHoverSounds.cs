using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FMODUnity;

public class EventButtonHoverSounds : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private StudioEventEmitter emitter = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        emitter.Play();
    }
}

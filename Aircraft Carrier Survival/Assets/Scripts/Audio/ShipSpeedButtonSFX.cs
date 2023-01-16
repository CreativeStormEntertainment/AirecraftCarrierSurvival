using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMODUnity;

public class ShipSpeedButtonSFX : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private StudioEventEmitter disabledSound = null;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.enabled)
        {
            disabledSound.Play();
        }
    }
}

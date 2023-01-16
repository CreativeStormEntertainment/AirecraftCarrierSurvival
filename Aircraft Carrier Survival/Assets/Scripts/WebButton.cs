using System.Diagnostics;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WebButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private string website = null;
    [SerializeField]
    private StudioEventEmitter emitter = null;
    
    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Process.Start(website);
        BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        emitter.Play();
    }
}

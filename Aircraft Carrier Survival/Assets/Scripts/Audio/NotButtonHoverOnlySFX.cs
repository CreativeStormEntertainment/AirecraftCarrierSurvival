using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class NotButtonHoverOnlySFX : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private StudioEventEmitter emitter = null;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        emitter.Play();
    }
}

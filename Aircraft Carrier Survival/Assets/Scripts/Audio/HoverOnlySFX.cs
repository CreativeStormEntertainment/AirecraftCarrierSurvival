using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class HoverOnlySFX : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private StudioEventEmitter emitter = null;
    private Button button;

    protected virtual void Awake()
    {
        button = GetComponent<Button>();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (button.enabled && button.interactable)
        {
            emitter.Play();
        }
    }
}

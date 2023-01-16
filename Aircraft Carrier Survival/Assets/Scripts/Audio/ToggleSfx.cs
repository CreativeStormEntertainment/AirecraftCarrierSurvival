using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleSfx : MonoBehaviour, IPointerEnterHandler
{
    [HideInInspector]
    public Toggle toggle;

    [SerializeField]
    protected EButtonState clickEvent = EButtonState.Click;

    protected void Awake()
    {
        toggle = GetComponent<Toggle>();

        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnClickSFX);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (toggle != null && toggle.enabled && toggle.interactable)
        {
            BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
        }
    }

    public void OnClickSFX(bool value)
    {
        BackgroundAudio.Instance.PlayEvent(clickEvent);
    }
}

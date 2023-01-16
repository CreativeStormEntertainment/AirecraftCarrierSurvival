using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSFX : MonoBehaviour, IPointerEnterHandler
{
    [HideInInspector]
    public Button button;

    [SerializeField]
    protected EButtonState clickEvent = EButtonState.Click;

    protected virtual void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnClickSFX);
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.enabled && button.interactable)
        {
            BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
        }
    }

    public virtual void OnClickSFX()
    {
        BackgroundAudio.Instance.PlayEvent(clickEvent);
    }
}

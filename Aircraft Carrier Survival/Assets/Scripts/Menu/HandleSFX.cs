using GambitUtils;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandleSFX : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler
{
    private bool isDown = false;
    private bool setup = false;

    public void OnPointerUp(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
        isDown = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
    }

    public void OnValueChanged()
    {
        if (setup)
        {
            this.StartCoroutineActionAfterFrames(() =>
            {
                if (!isDown)
                {
                    BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
                }
            }, 1);
        }
        setup = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDown)
        {
            BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
        }
    }
}

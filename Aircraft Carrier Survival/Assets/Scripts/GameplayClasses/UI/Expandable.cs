using UnityEngine;
using UnityEngine.EventSystems;

public class Expandable : MonoBehaviour, IPointerClickHandler, IEnableable
{
    private static readonly int Show = Animator.StringToHash("Show");

    [SerializeField]
    protected Animator animator = null;

    [SerializeField]
    protected EMainSceneUI soundParam = EMainSceneUI.ObjectivesShow;

    protected bool panelShowed;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (enabled)
        {
            OnClick(false);
        }
    }

    public virtual void SetEnable(bool enable)
    {
        enabled = enable;
        if (!enable)
        {
            Hide();
        }
    }

    public void Display(bool silent)
    {
        if (!panelShowed)
        {
            OnClick(silent);
        }
    }

    public void Hide()
    {
        if (panelShowed)
        {
            OnClick(false);
        }
    }

    private void OnClick(bool silent)
    {
        EMainSceneUI soundParam = this.soundParam;
        if (panelShowed)
        {
            soundParam++;
        }
        panelShowed = !panelShowed;
        animator.SetBool(Show, panelShowed);
        if (!silent)
        {
            BackgroundAudio.Instance.PlayEvent(soundParam);
        }
    }
}

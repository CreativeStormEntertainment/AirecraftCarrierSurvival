using UnityEngine;

public class GameButton : MonoBehaviour, IInteractive, IEnableable
{
    public Transform ButtonTrans;
    public GameObject Highlight;
    public Vector3 ClickScale = new Vector3(.75f, .75f, .75f);

    protected bool disabled;

    private Vector3 baseScale;

    protected virtual void Awake()
    {
        baseScale = ButtonTrans.localScale;
        Highlight.SetActive(false);
    }

    public void OnHoverEnter()
    {
        if (!disabled)
        {
            SectionRoomManager.Instance.PlayEvent(ESectionUIState.HoverButton);
            Highlight.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        Highlight.SetActive(false);
    }

    public void OnHoverStay()
    {

    }

    public float GetHoverStayTime()
    {
        return 1e9f;
    }

    public virtual void OnClickStart()
    {
        if (!disabled)
        {
            ButtonTrans.localScale = ClickScale;
        }
    }

    public void OnClickHold()
    {

    }

    public virtual void OnClickEnd(bool success)
    {
        ButtonTrans.localScale = baseScale;
    }

    public float GetClickHoldTime()
    {
        return 1e9f;
    }

    public virtual void SetEnable(bool enable)
    {
        disabled = !enable;
        if (!enable)
        {
            OnHoverExit();
            ButtonTrans.localScale = baseScale;
        }
    }
}

using UnityEngine;

public class TogglePopupObject : ToggleObject, IPopupPanel, IEnableable
{
    public EWindowType Type => type;

    [SerializeField]
    private EWindowType type = EWindowType.Other;

    public override void SetShow(bool show)
    {
        base.SetShow(show);
        if (show)
        {
            HudManager.Instance.PopupShown(this);
        }
        else
        {
            HudManager.Instance.PopupHidden(this);
        }
    }

    public void Hide()
    {
        SetShow(false);
    }

    public void SetEnable(bool enable)
    {
        enabled = enable;
        if (!enable)
        {
            Hide();
        }
    }
}

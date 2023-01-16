using System;
using UnityEngine;
using UnityEngine.UI;

public class EventPopup : MonoBehaviour
{
    public Text Title;
    public Text Description;
    public GameObject Accept;
    public GameObject OKForAccept;
    public GameObject OK;
    //public GameObject StartFight;

    public Action ActionOnAccept;

    public void Show()
    {
        gameObject.SetActive(true);
        HudManager.Instance.OnPausePressed();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        HudManager.Instance.PlayLastSpeed();
    }

    public void AcceptAction()
    {
        var action = ActionOnAccept;
        ActionOnAccept = null;
        action?.Invoke();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntermissionSubpanel : MonoBehaviour
{
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    public virtual void RefreshUpgradeButton()
    {

    }
    public virtual void ConfirmUpgrade()
    {

    }

    public virtual void StopUpgrade()
    {

    }
}

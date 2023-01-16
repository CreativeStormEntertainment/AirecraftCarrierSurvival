using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResupplyShipLinesController : MonoBehaviour
{
    public void FireStartAnimationEnd()
    {
        ResourceManager.Instance.SwitchObjectsState(false);
        ResourceManager.Instance.PlaySupplyAnim();
    }

    public void FireSupplyAnimationEnd()
    {

    }

    public void FireEndAnimationEnd()
    {
        ResourceManager.Instance.PlayEndAnim();
    }
}

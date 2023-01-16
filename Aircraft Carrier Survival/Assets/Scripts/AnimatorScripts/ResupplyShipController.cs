using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResupplyShipController : MonoBehaviour
{
    public void FireStartAnimationEnd()
    {
        ResourceManager.Instance.PlaySupplyStartAnim();
    }

    public void FireEndAnimationEnd()
    {
        //ResourceManager.Instance.ResupplyCargo();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapViewsFromDock : MonoBehaviour
{
    [SerializeField]
    private GameObject IconHover = null;
    public bool HoverActive;

    public void IconHoverActive()
    {
        IconHover.SetActive(true);
        HoverActive = true;
    }
    public void IconHoverHide()
    {
        IconHover.SetActive(false);
        HoverActive = false;
    }
}


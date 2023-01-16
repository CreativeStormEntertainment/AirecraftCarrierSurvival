using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandUIHoverKeeper : MonoBehaviour
{
    private IslandUI islandUI = null;

    private void OnMouseEnter()
    {
        islandUI.IsUIHovered(true);
    }
    private void OnMouseExit()
    {
        islandUI.IsUIHovered(false);
    }
    public void Setup(IslandUI uI)
    {
        islandUI = uI;
    }
}

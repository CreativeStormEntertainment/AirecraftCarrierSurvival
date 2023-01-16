using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotCanvasTooltipCaller : TooltipCallerBase, IInteractive
{
    protected IEnumerator ShowTooltip(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        UpdateText();
        isShowing = true;
        Tooltip.Instance.ShowNotCanvasTooltip(title, description);
        showTooltipCoroutine = null;
    }

    public void OnHoverEnter()
    {
        if (((HudManager.Instance.AcceptInput && HUDMode) || !HUDMode) && showTooltipCoroutine == null)
        {
            showTooltipCoroutine = StartCoroutine(ShowTooltip(tooltipShowDelay));
        }
    }

    public void OnHoverExit()
    {
        OnExit();
    }

    public void OnHoverStay()
    {

    }

    public float GetHoverStayTime()
    {
        return 0f;
    }

    public void OnClickStart()
    {

    }

    public void OnClickHold()
    {

    }

    public void OnClickEnd(bool success)
    {

    }

    public float GetClickHoldTime()
    {
        return 0f;
    }
}

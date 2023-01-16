using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipCaller : TooltipCallerBase, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<bool> TooltipStateChanged = delegate { };

    [Range(0f, 1f)]
    public float ArrowOffsetX;
    [Range(0f, 1f)]
    public float ArrowOffsetY;
    public RectTransform Trans;
    public Vector2 OffsetPosition;
    public float Width;
    public ENeighbourDirection ArrowDirection;

    [SerializeField]
    protected bool isIntermission = false;

    public void RefreshIfVisible()
    {
        if (isShowing)
        {
            UpdateText();
            Tooltip.Instance.Show(title, description, ArrowOffsetX, ArrowOffsetY, Trans, this, OffsetPosition, isAnimated, Width, ArrowDirection);
        }
    }

    public virtual void SetTitles(string title, string description)
    {
        TitleID = title;
        DescriptionID = description;
        FireParamsChanged();
    }

    public virtual void SetTitles(string description)
    {
        DescriptionID = description;
        FireParamsChanged();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if ((isIntermission || !HUDMode || HudManager.Instance.BlockTooltips) && showTooltipCoroutine == null)
        {
            ShowTooltip();
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    public void ShowTooltip()
    {
        showTooltipCoroutine = StartCoroutine(ShowTooltip(tooltipShowDelay));
    }

    public void HideTooltip()
    {
        OnExit();
        TooltipStateChanged(false);
    }

    protected IEnumerator ShowTooltip(float timeDelay)
    {
        yield return new WaitForSecondsRealtime(timeDelay);
        UpdateText();
        isShowing = true;
        Tooltip.Instance.Show(title, description, ArrowOffsetX, ArrowOffsetY, Trans, this, OffsetPosition, isAnimated, Width, ArrowDirection);
        showTooltipCoroutine = null;
        TooltipStateChanged(true);
    }
}

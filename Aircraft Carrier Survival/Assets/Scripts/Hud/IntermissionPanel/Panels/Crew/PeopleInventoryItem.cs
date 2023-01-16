using UnityEngine;
using UnityEngine.EventSystems;

public class PeopleInventoryItem<T1, T2> : InventoryDrag<T1, T2>, IPointerEnterHandler, IPointerExitHandler
    where T1 : PeopleIntermissionData
    where T2 : PeopleInventoryItem<T1, T2>
{
    private bool showHighlight;
    private bool showDragHighlight;
    private bool showTooltip;

    public override void OnEnable()
    {
        base.OnEnable();
        SetShowHighlight(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (showDragHighlight)
        {
            showHighlight = true;
        }
        else
        {
            SetShowHighlight(true);
        }
        SetShowTooltip(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetShowHighlight(false);
        if (!showDragHighlight)
        {
            SetShowTooltip(false);
        }
    }

    protected override void OnLeftDragged(Vector2 delta, PointerEventData data)
    {
        base.OnLeftDragged(delta, data);
        SetShowHighlight(false);
        showHighlight = true;
        SetShowDragHighlight(true);
    }

    protected override void OnLeftDragFinished()
    {
        base.OnLeftDragFinished();
        if (showHighlight)
        {
            SetShowHighlight(true);
        }
        else if (showTooltip)
        {
            SetShowTooltip(false);
        }
        SetShowDragHighlight(false);
    }

    protected override void OnRightClicked()
    {
        base.OnRightClicked();
        SetShowHighlight(false);
        SetShowDragHighlight(false);
        if (showTooltip)
        {
            SetShowTooltip(false);
        }
    }

    protected virtual void SetShowHighlight(bool show)
    {
        showHighlight = show;
    }

    protected virtual void SetShowDragHighlight(bool show)
    {
        showDragHighlight = show;
    }

    protected virtual void SetShowTooltip(bool show)
    {
        showTooltip = show;
    }
}

using System;
using UnityEngine.EventSystems;

public class PeopleShopButton<T> : ShopButton<T>, IPointerEnterHandler, IPointerExitHandler where T : PeopleIntermissionData
{
    private bool show;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetShowHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetShowHighlight(false);
    }

    public override void Setup(T data, int index, Action click)
    {
        Clicked -= OnClicked;
        Clicked += OnClicked;

        base.Setup(data, index, click);
        Refresh(data);
    }

    public virtual void Refresh(T data)
    {
        Data = data;
    }

    protected virtual void SetShowHighlight(bool show)
    {
        this.show = show;
    }

    private void OnClicked()
    {
        if (show)
        {
            SetShowHighlight(false);
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class WorldMapWaypoint : MapWaypoint, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool Hovered
    {
        get;
        set;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        WorldMap.Instance.OnPointerClick(eventData);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hovered = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }
}
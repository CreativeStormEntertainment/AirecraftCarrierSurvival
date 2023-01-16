using UnityEngine;
using UnityEngine.EventSystems;

public class ReportButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private GameObject selected = null;

    public void OnPointerDown(PointerEventData eventData)
    {
        selected.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        selected.SetActive(false);
    }
}

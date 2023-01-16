using UnityEngine;
using UnityEngine.EventSystems;

public class ShowOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject objToShow = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        objToShow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        objToShow.SetActive(false);
    }
}

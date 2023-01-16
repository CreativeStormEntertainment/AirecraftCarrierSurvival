using UnityEngine;
using UnityEngine.EventSystems;

public class OrderIcon : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private int orderNumber = 0;

    public void OnPointerClick(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Right)
        {
            AircraftCarrierDeckManager.Instance.CancelOrder(orderNumber);
        }
    }
}

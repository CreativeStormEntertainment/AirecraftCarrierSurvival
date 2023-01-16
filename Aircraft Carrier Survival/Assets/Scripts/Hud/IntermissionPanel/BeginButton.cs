using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BeginButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private ShowTooltip tooltipToShow = null;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipToShow.Show(button.interactable ? EIntermissionTooltip.BeginMission : EIntermissionTooltip.MissionUnavailable);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipToShow.gameObject.SetActive(false);
    }
}

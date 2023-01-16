using UnityEngine;
using UnityEngine.EventSystems;

public class TacticalMapTooltipCaller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool Localized
    {
        get;
        set;
    }

    [SerializeField]
    private RectTransform rect = null;
    [SerializeField]
    private string tooltipText = default;

    private void Awake()
    {
        if (!Localized && !string.IsNullOrEmpty(tooltipText))
        {
            tooltipText = LocalizationManager.Instance.GetText(tooltipText);
            Localized = true;
        }
    }

    private void OnDisable()
    {
        TacticalMapTooltip.Instance.HideOnDisable(rect);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TacticalMapTooltip.Instance.Setup(tooltipText, rect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TacticalMapTooltip.Instance.Hide();
    }

    public void SetText(string text)
    {
        tooltipText = text;
        Localized = true;
        TacticalMapTooltip.Instance.UpdateText(tooltipText, rect);
    }
}

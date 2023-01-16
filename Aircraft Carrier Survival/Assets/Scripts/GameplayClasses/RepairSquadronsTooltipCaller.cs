using UnityEngine;
using UnityEngine.EventSystems;

public class RepairSquadronsTooltipCaller : TooltipCaller
{
    [SerializeField]
    private SetResourceText manager = null;
    [SerializeField]
    private EPlaneType planeType = EPlaneType.Bomber;

    protected override void Awake()
    {
        base.Awake();
        if (manager != null)
        {
            manager.AnimChanged += OnAnimChanged;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (showTooltipCoroutine != null)
        {
            var deckMan = AircraftCarrierDeckManager.Instance;
            deckMan.RepairPlaneChanged -= OnRepairPlaneChanged;
            deckMan.RepairPlaneChanged += OnRepairPlaneChanged;
            OnRepairPlaneChanged(true, planeType);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        AircraftCarrierDeckManager.Instance.RepairPlaneChanged -= OnRepairPlaneChanged;
        base.OnPointerExit(eventData);
    }

    protected override void UpdateText()
    {
        var locMan = LocalizationManager.Instance;

        var deckMan = AircraftCarrierDeckManager.Instance;
        float timer = deckMan.RepairTimer / (float)TimeManager.Instance.TicksForHour;
        int hours = (int)timer;
        title = locMan.GetText(TitleID, "-" + hours.ToString("00") + "h" + ((int)((timer - hours) * 60f)).ToString("00"));

        description = locMan.GetText(DescriptionID, deckMan.GetBrokenSquadronCount(planeType).ToString());
        Tooltip.Instance.UpdateText(title, description);
    }

    private void OnAnimChanged()
    {
        AircraftCarrierDeckManager.Instance.RepairPlaneChanged -= OnRepairPlaneChanged;
    }

    private void OnRepairPlaneChanged(bool _, EPlaneType __)
    {
        UpdateText();
    }
}

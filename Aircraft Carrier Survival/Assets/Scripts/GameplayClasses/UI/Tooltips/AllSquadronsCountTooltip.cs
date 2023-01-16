using UnityEngine;

public class AllSquadronsCountTooltip : TooltipCaller
{
    [SerializeField]
    private EPlaneType planeType = EPlaneType.Fighter;

    private AircraftCarrierDeckManager aircraftCarrierDeckManager = null;

    private void Start()
    {
        aircraftCarrierDeckManager = AircraftCarrierDeckManager.Instance;
        aircraftCarrierDeckManager.PlaneCountChanged += OnPlanesCountChanged;
        aircraftCarrierDeckManager.DeckSquadronsCountChanged += OnPlanesCountChanged;
    }

    private void OnPlanesCountChanged()
    {
        if (isShowing)
        {
            UpdateText();
        }
    }

    protected override void UpdateText()
    {
        if (!string.IsNullOrWhiteSpace(TitleID))
        {
            title = LocalizationManager.Instance.GetText(TitleID);
        }
        if (!string.IsNullOrWhiteSpace(DescriptionID))
        {
            description = LocalizationManager.Instance.GetText(DescriptionID, aircraftCarrierDeckManager.GetAvailableSquadronCount(planeType).ToString());
        }
        Tooltip.Instance.UpdateText(title, description);
    }
}

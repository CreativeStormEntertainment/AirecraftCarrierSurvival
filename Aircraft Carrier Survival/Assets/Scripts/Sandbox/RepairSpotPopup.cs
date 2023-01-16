using UnityEngine;
using UnityEngine.UI;

public class RepairSpotPopup : SandboxPopup, IPopupPanel
{
    public EWindowType Type => EWindowType.SandboxPopup;

    [SerializeField]
    private Button buttonC = null;
    [SerializeField]
    private DamagedEscortPanel damagedEscorts = null;
    [SerializeField]
    private PlanesPanel planesPanel = null;

    protected override void Start()
    {
        base.Start();
        buttonC.onClick.AddListener(OnClickC);
    }

    public override void Show(SandboxPoi poi)
    {
        base.Show(poi);
        bool inRange = CheckInRange();

        buttonB.interactable = inRange && StrikeGroupManager.Instance.AliveMembers.Count > 0;
        var deckMan = AircraftCarrierDeckManager.Instance;
        int squadronsCount = deckMan.GetAllSquadronsCount(EPlaneType.Bomber) + deckMan.GetAllSquadronsCount(EPlaneType.Fighter) + deckMan.GetAllSquadronsCount(EPlaneType.TorpedoBomber);
        buttonC.interactable = inRange && squadronsCount < deckMan.MaxAllSquadronsCount;
    }

    protected override void OnClickA()
    {
        base.OnClickA();
    }

    protected override void OnClickB()
    {
        if (damagedEscorts.gameObject.activeSelf)
        {
            damagedEscorts.Hide();
        }
        else
        {
            damagedEscorts.ShowDamagedShips(OnRepairSpotUsed);
            planesPanel.gameObject.SetActive(false);
        }
    }

    private void OnClickC()
    {
        if (planesPanel.gameObject.activeSelf)
        {
            damagedEscorts.Hide();
        }
        else
        {
            planesPanel.Show(OnRepairSpotUsed, poi);
            damagedEscorts.gameObject.SetActive(false);
        }
    }

    private void OnRepairSpotUsed()
    {
        Hide();
        SandboxManager.Instance.PoiManager.RemovePoi(poi);
    }
}

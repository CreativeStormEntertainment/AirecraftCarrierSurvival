using UnityEngine;
using UnityEngine.UI;

public class PlaneResourceText : MonoBehaviour
{
    [SerializeField]
    private EPlaneType type = default;
    [SerializeField]
    private Text available = null;
    [SerializeField]
    private Text deckText = null;
    [SerializeField]
    private Text hangarText = null;
    [SerializeField]
    private Text missionText = null;
    [SerializeField]
    private Text repairText = null;
    [SerializeField]
    private Text totalText = null;

    public void Refresh()
    {
        var deck = AircraftCarrierDeckManager.Instance;

        int deckCount = deck.GetDeckSquadronCount(type);
        deckText.text = deckCount.ToString();

        var data = deck.GetPlaneData(type);
        hangarText.text = data.Free.ToString();
        repairText.text = data.Damaged.ToString();

        int missionCount = deck.GetMissionSquadronCount(type);
        missionText.text = missionCount.ToString();

        int totalCount = deckCount + data.Free;
        available.text = totalCount.ToString();

        totalCount += data.Damaged + missionCount;
        totalText.text = totalCount.ToString();
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TacticalFightPlayerPilotAbillityButton : MonoBehaviour
{
    TacticalFightPilot pilotOnPanel;
    int playerPilotAbillityIndex;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnTacticalFightPlayerPlanebillityButtonClick);
    }

    private void Start()
    {

    }

    private void OnTacticalFightPlayerPlanebillityButtonClick()
    {

    }

    public void SetPlayerPilotAbilltyButton(TacticalFightPilot pilot, int abillityIndex)
    {
        pilotOnPanel = pilot;
        playerPilotAbillityIndex = abillityIndex;
        GetComponentInChildren<Text>().text = pilotOnPanel.GetPilotAbillities()[abillityIndex].EffectType.ToString() + " " + pilotOnPanel.GetPilotAbillities()[abillityIndex].AbilityType.ToString();
    }
}

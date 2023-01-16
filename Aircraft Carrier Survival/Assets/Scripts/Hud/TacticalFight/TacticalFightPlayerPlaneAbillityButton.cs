using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TacticalFightPlayerPlaneAbillityButton : MonoBehaviour
{
    TacticalFightPlayerUnit playerUnit;
    int playerAbillityIndex;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnTacticalFightPlayerPlanebillityButtonClick);
    }

    private void Start()
    {

    }

    private void OnTacticalFightPlayerPlanebillityButtonClick()
    {
        TacticalFightManager.Instance.SelectUnit(playerUnit, GetComponentInParent<TacticalFightPlayerPlaneButton>().GetPlayerUnitIndexInPollForButton());
        GetComponentInParent<TacticalFightPlayerPlaneButton>().DeactivateAbillityButtons();
    }

    public void SetPlayerPlaneAbilltyButton(TacticalFightPlayerUnit unit, int abillityIndex)
    {
        playerUnit = unit;
        playerAbillityIndex = abillityIndex;
        GetComponentInChildren<Text>().text = playerUnit.GetPlayerAbillities()[abillityIndex].EffectType.ToString() + " " + playerUnit.GetPlayerAbillities()[abillityIndex].AbilityType.ToString();
    }
}

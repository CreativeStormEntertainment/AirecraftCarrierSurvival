using System;
using System.Collections.Generic;
using UnityEngine;

public class DamagedEscortPanel : MonoBehaviour
{
    [SerializeField]
    private List<EscortRepairButton> strikeGroupButtons = null;
    [SerializeField]
    private GameObject noShipsText = null;

    private Action onOptionChoose;

    public void ShowDamagedShips(Action onOptionChoose)
    {
        this.onOptionChoose = onOptionChoose;
        var members = StrikeGroupManager.Instance.AliveMembers;
        int index = 0;
        foreach (var button in strikeGroupButtons)
        {
            button.gameObject.SetActive(false);
        }
        foreach (var member in members)
        {
            strikeGroupButtons[index].Setup(member);
            strikeGroupButtons[index].Repaired += OnRepair;
            index++;
        }
        noShipsText.SetActive(index == 0);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnRepair(EscortRepairButton escort)
    {
        Hide();
        onOptionChoose();
    }
}

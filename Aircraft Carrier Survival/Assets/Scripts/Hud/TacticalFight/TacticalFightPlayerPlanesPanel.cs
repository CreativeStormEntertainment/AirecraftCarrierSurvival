using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TacticalFightPlayerPlanesPanel : MonoBehaviour
{
    List<TacticalFightPlayerPlaneButton> playerPlaneButtons = new List<TacticalFightPlayerPlaneButton>();
    int playerMaxAvailableUnits = 5;

    void Awake()
    {
        playerPlaneButtons = GetComponentsInChildren<TacticalFightPlayerPlaneButton>(true).ToList();
    }

    public void InitializePlayerPlanesPanel(TacticalFightPilot chosenPilot)
    {
        playerPlaneButtons.ForEach(x => x.gameObject.SetActive(false));

        int currentIndex = 0;
        foreach (TacticalFightPlayerUnit playerUnit in chosenPilot.GetPlayerUnits())
        {
            playerPlaneButtons[currentIndex].gameObject.SetActive(true);
            playerPlaneButtons[currentIndex].SetPlayerPlaneButton(currentIndex, playerUnit);

            if (currentIndex < playerMaxAvailableUnits - 1)
                currentIndex++;
        }
    }

    public TacticalFightPlayerPlaneButton GetPlayerPlaneButton(int indexOfPlane)
    {
        return playerPlaneButtons[indexOfPlane];
    }

    public void DeselectAllButtons()
    {
        playerPlaneButtons.ForEach(x => x.UnSelect());
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.EventSystems;

public class TacticalFightPilotPanel : MonoBehaviour
{
    public SpriteAtlas portraitsAtlas;
    TacticalFightPilot pilotOnPanel;
    List<TacticalFightPlayerPilotAbillityButton> playerPlaneAbillityButtons;
    //int pilotMaxAvailableAbillities = 3;
    public Image pilotImage;
    public Text healthText;

    void Awake()
    {
        playerPlaneAbillityButtons = GetComponentsInChildren<TacticalFightPlayerPilotAbillityButton>(true).ToList();
        //pilotImage = GetComponentInChildren<Image>(true);
        //healthText = GetComponentInChildren<Text>(true);
    }

    private void OnEnable()
    {
        TacticalFightPilot.OnHealthChanged += SetHealthText;
    }

    private void OnDisable()
    {
        TacticalFightPilot.OnHealthChanged -= SetHealthText;
    }

    public void InitializePanel(TacticalFightPilot chosenPilot)
    {
        pilotOnPanel = chosenPilot;
        pilotImage.sprite = portraitsAtlas.GetSprite(chosenPilot.GetPictureName());

        //int currentIndex = 0;
        //foreach (TacticalFightAbility pilotAbillity in pilotOnPanel.GetPilotAbillities())
        //{
        //    playerPlaneAbillityButtons[currentIndex].gameObject.SetActive(true);
        //    playerPlaneAbillityButtons[currentIndex].SetPlayerPilotAbilltyButton(pilotOnPanel, currentIndex);

        //    if (currentIndex < pilotMaxAvailableAbillities - 1)
        //        currentIndex++;
        //}

        SetHealthText(pilotOnPanel.GetCurrentMoralePoints());
    }

    public void SetHealthText(int healthAmount)
    {
        healthText.text = healthAmount.ToString();
    }
}

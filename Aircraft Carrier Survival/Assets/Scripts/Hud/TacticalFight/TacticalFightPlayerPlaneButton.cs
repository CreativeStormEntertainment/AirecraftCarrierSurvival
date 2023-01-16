using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class TacticalFightPlayerPlaneButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    TacticalFightPlayerPlanesPanel playerPlanesPanel;
    TacticalFightPlayerUnit playerUnit;
    [SerializeField]
    int playerUnitIndexInPool;
    int maxAbillitiesCountToDisplay = 4;
    [SerializeField]
    List<TacticalFightPlayerPlaneAbillityButton> playerAbillityButtons;
    Sprite absentModel;
    Sprite withPropellerModel;
    Sprite noPropellerModel;
    Sprite outLineGray;
    Sprite outLineOrange;
    Sprite shadowPlane;
    [SerializeField]
    Image propellerImage = null;
    [SerializeField]
    Image shadowImage = null;
    [SerializeField]
    Image modelImage = null;
    [SerializeField]
    Image outlineImage = null;
    [SerializeField]
    Image roundCounterImage = null;
    [SerializeField]
    bool isSelected;
    bool isBlocked;

    private void Awake()
    {
        playerAbillityButtons = GetComponentsInChildren<TacticalFightPlayerPlaneAbillityButton>(true).ToList();
        playerPlanesPanel = GetComponentInParent<TacticalFightPlayerPlanesPanel>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            SetIsButtonHighlighted(true);
            modelImage.color = Color.white;
        }
        if (isBlocked)
        {
            modelImage.color = Color.white;
        }

        TacticalFightHudManager.Instance.ShowPlayerInfoPanel(playerUnit);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
        {
            SetIsButtonHighlighted(false);
            modelImage.color = new Color(0.8f, 0.8f, 0.8f);
        }
        if (isBlocked)
        {
            modelImage.color = new Color(0.8f, 0.8f, 0.8f);
        }

        TacticalFightHudManager.Instance.HideUnitInfoPanel();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        TacticalFightHudManager.Instance.PlayOnButtonPlaneClickClip();

        if (isSelected == false)
        {
            playerPlanesPanel.DeselectAllButtons();
            if (TacticalFightManager.Instance.GetChosenPilot().CheckIsPlayerIndexBlocked(playerUnitIndexInPool) == false)
            {
                TacticalFightManager.Instance.SelectUnit(playerUnit, GetComponentInParent<TacticalFightPlayerPlaneButton>().GetPlayerUnitIndexInPollForButton());
                SetIsButtonSelected();
                isSelected = true;

                if (TacticalFightCameraSwitcher.Instance.IsAfterPlayerPlaneButtonClickCameraChanged)
                    TacticalFightManager.Instance.OnPlayerChosenChange(playerUnit);
            }
            else
                TacticalFightHudManager.Instance.SetInfoTextMessage("Player unit currently is blocked");
        }
        else
        {
            playerPlanesPanel.DeselectAllButtons();
            UnSelect();
        }
    }

    public void UnSelect()
    {
        TacticalFightManager.Instance.UnSelectUnit();
        DeactivateAbillityButtons();
        if (!isBlocked)
        {
            SetIsButtonBlocked(false);
        }
    }

    public void SetRoundCounter(int amount)
    {
        roundCounterImage.GetComponentInChildren<Text>().text = amount.ToString();
    }

    public void SetIsButtonBlocked(bool isButtonBlocked)
    {
        propellerImage.gameObject.SetActive(false);
        modelImage.color = new Color(0.8f,0.8f,0.8f);
        //modelImage.color = Color.gray;
        if (isButtonBlocked)
        {
            roundCounterImage.gameObject.SetActive(true);
            modelImage.sprite = absentModel;
            isBlocked = true;
            SetIsButtonHighlighted(true);
        }
        else
        {
            roundCounterImage.gameObject.SetActive(false);
            modelImage.sprite = withPropellerModel;
            isBlocked = false;
            isSelected = false;
            SetIsButtonHighlighted(false);
        }
    }

    public void SetIsButtonSelected()
    {
        outlineImage.color = Color.white;
        modelImage.color = Color.white;
        modelImage.sprite = noPropellerModel;
        propellerImage.gameObject.SetActive(true);
        outlineImage.sprite = outLineOrange;
    }

    public void SetIsButtonHighlighted(bool isHiglighted)
    {
        if (isHiglighted)
        {
            outlineImage.color = Color.white;
            outlineImage.sprite = outLineGray;
        }
        else
        {
            outlineImage.color = Color.clear;
        }
    }

    public void ActivateAbillityButtons()
    {
        int currentIndex = 0;
        foreach(TacticalFightAbility ability in playerUnit.GetPlayerAbillities())
        {
            if (ability.AbilityType != ETacticalFightAbilityType.PassiveAbility)
                playerAbillityButtons[currentIndex].gameObject.SetActive(true);

            if(currentIndex < maxAbillitiesCountToDisplay-1)
                currentIndex++;
        }
    }

    public void DeactivateAbillityButtons()
    {
        foreach (TacticalFightPlayerPlaneAbillityButton abilityButton in playerAbillityButtons)
        {
            abilityButton.gameObject.SetActive(false);
        }
    }

    public void SetPlayerPlaneButton(int playerUnitIndex,TacticalFightPlayerUnit unit)
    {
        playerUnitIndexInPool = playerUnitIndex;
        playerUnit = unit;

        int currentIndex = 0;
        foreach (TacticalFightAbility ability in playerUnit.GetPlayerAbillities())
        {
            if (ability.AbilityType != ETacticalFightAbilityType.PassiveAbility)
                playerAbillityButtons[currentIndex].SetPlayerPlaneAbilltyButton(playerUnit, currentIndex);

            if (currentIndex < maxAbillitiesCountToDisplay)
                currentIndex++;
        }

        switch (unit.GetPlaneType())
        {
            case (ETacticalFightPlayerPlaneType.Torpedo):
                absentModel = TacticalFightVisualizationManager.Instance.DevastatorButtonSpriteVisualization[0];
                withPropellerModel = TacticalFightVisualizationManager.Instance.DevastatorButtonSpriteVisualization[1];
                noPropellerModel = TacticalFightVisualizationManager.Instance.DevastatorButtonSpriteVisualization[2];
                outLineGray = TacticalFightVisualizationManager.Instance.DevastatorButtonSpriteVisualization[3];
                outLineOrange = TacticalFightVisualizationManager.Instance.DevastatorButtonSpriteVisualization[4];
                shadowPlane = TacticalFightVisualizationManager.Instance.DevastatorButtonSpriteVisualization[5];
                break;

            case (ETacticalFightPlayerPlaneType.Bomber):
                absentModel = TacticalFightVisualizationManager.Instance.HelldiverButtonSpriteVisualization[0];
                withPropellerModel = TacticalFightVisualizationManager.Instance.HelldiverButtonSpriteVisualization[1];
                noPropellerModel = TacticalFightVisualizationManager.Instance.HelldiverButtonSpriteVisualization[2];
                outLineGray = TacticalFightVisualizationManager.Instance.HelldiverButtonSpriteVisualization[3];
                outLineOrange = TacticalFightVisualizationManager.Instance.HelldiverButtonSpriteVisualization[4];
                shadowPlane = TacticalFightVisualizationManager.Instance.HelldiverButtonSpriteVisualization[5];
                break;

            case (ETacticalFightPlayerPlaneType.Fighter):
                absentModel = TacticalFightVisualizationManager.Instance.WildcatButtonSpriteVisualization[0];
                withPropellerModel = TacticalFightVisualizationManager.Instance.WildcatButtonSpriteVisualization[1];
                noPropellerModel = TacticalFightVisualizationManager.Instance.WildcatButtonSpriteVisualization[2];
                outLineGray = TacticalFightVisualizationManager.Instance.WildcatButtonSpriteVisualization[3];
                outLineOrange = TacticalFightVisualizationManager.Instance.WildcatButtonSpriteVisualization[4];
                shadowPlane = TacticalFightVisualizationManager.Instance.WildcatButtonSpriteVisualization[5];
                break;
        }


        shadowImage.sprite = shadowPlane;
        SetIsButtonBlocked(false);
    }

    public int GetPlayerUnitIndexInPollForButton()
    {
        return playerUnitIndexInPool;
    }
}

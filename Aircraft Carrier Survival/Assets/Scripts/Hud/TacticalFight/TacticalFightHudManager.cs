using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TacticalFightHudManager : MonoBehaviour
{
    public static TacticalFightHudManager Instance;

    public AudioClip OnButtonPlaneClickClip;
    public AudioClip OnButtonClickClip;
    public AudioClip OnMapAbillityHoverClip;

    bool isShowingAttackFieldsAll = true;
    bool isShowingLandFields = false;
    Text infoText;
    Button playerRetreatButton;
    Button changeAttackFieldsVisualisationButton;
    TacticalFightPilotPanel pilotPanel;
    TacticalFightPlayerPlanesPanel playerPlanesPanel;
    TacticalFightUnitInfoPanel unitInfoPanel;
    TacticalFightEndGamePanel endGamePanel;
    TacticalFightBeginGamePanel beginGamePanel;

    private void Awake()
    {
        Instance = this;

        playerRetreatButton = GetComponentsInChildren<Button>(true)[0];
        changeAttackFieldsVisualisationButton = GetComponentsInChildren<Button>(true)[1];
        playerRetreatButton.onClick.AddListener(OnRetreatPlayerButtonClickAction);
        changeAttackFieldsVisualisationButton.onClick.AddListener(ChangeIsShowingAttackFieldsAllState);
        pilotPanel = GetComponentInChildren<TacticalFightPilotPanel>();
        playerPlanesPanel = GetComponentInChildren<TacticalFightPlayerPlanesPanel>();
        infoText = GetComponentInChildren<Text>(true);
        unitInfoPanel = GetComponentInChildren<TacticalFightUnitInfoPanel>(true);
        endGamePanel = GetComponentInChildren<TacticalFightEndGamePanel>(true);
        beginGamePanel = GetComponentInChildren<TacticalFightBeginGamePanel>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ChangeShowingDebugLand();
        }
    }

    public void InitializeHud(TacticalFightPilot chosenPilot, List<TacticalFightEnemyUnit> enemyUnitsOnMap)
    {
        beginGamePanel.gameObject.SetActive(true);
        isShowingAttackFieldsAll = true;
        isShowingLandFields = false;
        pilotPanel.InitializePanel(chosenPilot);
        playerPlanesPanel.InitializePlayerPlanesPanel(chosenPilot);
        HideUnitInfoPanel();
    }

    public void PlayOnButtonPlaneClickClip()
    {
        //BackgroundAudio.Instance.SFXSource.PlayOneShot(OnButtonPlaneClickClip);
    }

    public void PlayOnButtonClickClip()
    {
        //BackgroundAudio.Instance.SFXSource.PlayOneShot(OnButtonClickClip);
    }

    public void PlayOnMapAbillityHoverClip()
    {
        //BackgroundAudio.Instance.SFXSource.PlayOneShot(OnMapAbillityHoverClip);
    }

    public bool GetIsShowingAttackFieldsAll()
    {
        return isShowingAttackFieldsAll;
    }

    public void SetInfoTextMessage(string textToSet)
    {
        infoText.text = textToSet;
    }

    public void ResetInfoText()
    {
        infoText.text = "";
    }

    private void OnRetreatPlayerButtonClickAction()
    {
        PlayOnButtonClickClip();
        TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.PlayerRetreat);
    }

    public void SetBlockPlayerPlaneButton(int indexOfPlaneToBlock, bool isBlocked)
    {
        playerPlanesPanel.GetPlayerPlaneButton(indexOfPlaneToBlock).SetIsButtonBlocked(isBlocked);
    }

    public void SetRoundCounterForPlaneButton(int indexOfPlaneToBlock, int countOfRounds)
    {
        playerPlanesPanel.GetPlayerPlaneButton(indexOfPlaneToBlock).SetRoundCounter(countOfRounds);
    }

    public void ShowEnemyInfoPanel(TacticalFightEnemyUnit enemyToShow)
    {
        unitInfoPanel.SetEnemyInfoPanel(enemyToShow);
    }

    public void ShowPlayerInfoPanel(TacticalFightPlayerUnit playerToShow)
    {
        unitInfoPanel.SetPlayerInfoPanel(playerToShow);
    }

    public void HideUnitInfoPanel()
    {
        unitInfoPanel.SetEmptyUnitInfoPanel();
    }

    public void DeselectAllPlaneButtons()
    {
        playerPlanesPanel.DeselectAllButtons();
    }

    public void ShowEndGamePanel(bool isGameWon, string message)
    {
        endGamePanel.InitializeEndGamePanel(isGameWon,message);
    }

    public void ChangeIsShowingAttackFieldsAllState()
    {
        PlayOnButtonClickClip();

        foreach (TacticalFightEnemyUnit enemyUnit in TacticalFightManager.Instance.GetEnemyListOnMap())
        {
            enemyUnit.UnSetVisualizationForAttack();
        }

        isShowingAttackFieldsAll = !isShowingAttackFieldsAll;

        foreach(TacticalFightEnemyUnit enemyUnit in TacticalFightManager.Instance.GetEnemyListOnMap())
        {
            enemyUnit.SetVisualizationForAttack();
        }
    }

    public void ChangeShowingDebugLand()
    {
        isShowingLandFields = !isShowingLandFields;

        foreach(TacticalFightMapField mapField in FindObjectsOfType<TacticalFightMapField>())
        {
            mapField.SetFieldVisualization();
        }

    }

    public bool GetIsShowingLand()
    {
        return isShowingLandFields;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardTab : MonoBehaviour
{
    public bool Used => button.gameObject.activeSelf;

    [SerializeField]
    private MissionSuccessLeftPanel leftPanel = null;
    [SerializeField]
    private TabButton button = null;
    [SerializeField]
    private BuffCard buffCard = null;
    [SerializeField]
    private Text upgradePoints = null;
    [SerializeField]
    private List<ManeuversCard> maneuversCards = null;
    [SerializeField]
    private GameObject arrow = null;
    [SerializeField]
    private GameObject airPoints = null;
    [SerializeField]
    private GameObject navyPoints = null;

    public void Init()
    {
        button.Button.onClick.AddListener(OnClick);
    }

    public void SetupManeuversCard(PlayerManeuverData maneuverData, int level)
    {
        HideAll();
        if (level == 2)
        {
            maneuversCards[0].Setup(maneuverData, 1);
            maneuversCards[1].Setup(maneuverData.Level2, 2);
            maneuversCards[1].HighlightUpgrades(maneuverData, maneuverData.Level2);
        }
        else if (level == 3)
        {
            maneuversCards[0].Setup(maneuverData.Level2, 2);
            maneuversCards[1].Setup(maneuverData.Level3, 3);
            maneuversCards[1].HighlightUpgrades(maneuverData.Level2, maneuverData.Level3);
        }
        else
        {
            Debug.LogError("Can't upgrade maneuver of level " + maneuverData.Level.ToString());
        }
        button.gameObject.SetActive(true);
        arrow.SetActive(true);
    }

    public void SetupBuffCard(IslandBuff buff)
    {
        HideAll();
        buffCard.Setup(buff, false);
        button.gameObject.SetActive(true);
    }

    public void SetupUpgradePoints(int points)
    {
        HideAll();
        SaveManager.Instance.Data.IntermissionData.UpgradePoints += points;
        upgradePoints.gameObject.SetActive(true);
        upgradePoints.text = points.ToString();
        button.gameObject.SetActive(true);
    }

    public void SetupPoints(bool air)
    {
        HideAll();
        if (air)
        {
            airPoints.SetActive(true);
        }
        else
        {
            navyPoints.SetActive(true);
        }
        button.gameObject.SetActive(true);
    }

    public void SelectTab(bool select)
    {
        gameObject.SetActive(select);
        button.SetSelected(select);
        button.SetHighlighted(false);
    }

    public void SetShowButton(bool show)
    {
        button.gameObject.SetActive(show);
    }

    private void OnClick()
    {
        leftPanel.SelectTab(this);
    }

    private void HideAll()
    {
        arrow.SetActive(false);
        buffCard.Hide();
        upgradePoints.gameObject.SetActive(false);
        foreach (var card in maneuversCards)
        {
            card.gameObject.SetActive(false);
        }
        airPoints.SetActive(false);
        navyPoints.SetActive(false);
        button.SetHighlighted(true);
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class TacticalFightUnitInfoPanel : MonoBehaviour
{
    private Text unitNameText;
    private Text hpText;
    private Text unitDescriptionText;
    private Text unitAbillityDescriptionText;
    [SerializeField]
    private Image higherSeparatorImage = null;
    [SerializeField]
    private Image lowerSeparatorImage = null;
    private List<Image> healthCells;

    void Awake()
    {
        unitNameText = GetComponentsInChildren<Text>(true)[0]; 
        unitDescriptionText = GetComponentsInChildren<Text>(true)[1];
        unitAbillityDescriptionText = GetComponentsInChildren<Text>(true)[2];
        hpText = GetComponentsInChildren<Text>(true)[3];
        healthCells = transform.GetChild(6).gameObject.GetComponentsInChildren<Image>(true).ToList();
    }

    public void SetEnemyInfoPanel(TacticalFightEnemyUnit enemyUnitToDisplay)
    {
        unitNameText.gameObject.SetActive(true);
        unitDescriptionText.gameObject.SetActive(true);
        unitAbillityDescriptionText.gameObject.SetActive(true);
        hpText.gameObject.SetActive(true);
        higherSeparatorImage.gameObject.SetActive(true);
        lowerSeparatorImage.gameObject.SetActive(true);

        unitNameText.text = enemyUnitToDisplay.GetUnitName();
        unitDescriptionText.text = enemyUnitToDisplay.GetUnitDescription();
        unitAbillityDescriptionText.text = "";

        foreach (TacticalFightEnemyAbility enemyAbility in enemyUnitToDisplay.GetEnemyAbilities())
        {
            unitAbillityDescriptionText.text += enemyAbility.AbillityName;
            unitAbillityDescriptionText.text += '\n';
            unitAbillityDescriptionText.text += enemyAbility.AbillityDescription;
            unitAbillityDescriptionText.text += '\n';
        }

        SetHealthBarStatus(enemyUnitToDisplay.GetMaxHealth(), enemyUnitToDisplay.GetCurrentHealth());
    }

    public void SetPlayerInfoPanel(TacticalFightPlayerUnit playerUnitToDisplay)
    {
        unitNameText.gameObject.SetActive(true);
        unitDescriptionText.gameObject.SetActive(true);
        unitAbillityDescriptionText.gameObject.SetActive(true);
        hpText.gameObject.SetActive(false);
        higherSeparatorImage.gameObject.SetActive(true);
        lowerSeparatorImage.gameObject.SetActive(true);

        unitNameText.text = playerUnitToDisplay.GetUnitName();
        unitDescriptionText.text = playerUnitToDisplay.GetUnitDescription();
        unitAbillityDescriptionText.text = "";

        foreach (TacticalFightAbility playerAbility in playerUnitToDisplay.GetPlayerAbillities())
        {
            unitAbillityDescriptionText.text += playerAbility.AbillityName;
            unitAbillityDescriptionText.text += '\n';
            unitAbillityDescriptionText.text += playerAbility.AbillityDescription;
            unitAbillityDescriptionText.text += '\n';
        }

        healthCells.ForEach(x => x.gameObject.SetActive(false));
    }

    public void SetEmptyUnitInfoPanel()
    {
        unitNameText.gameObject.SetActive(false);
        unitDescriptionText.gameObject.SetActive(false); 
        unitAbillityDescriptionText.text = "";
        unitAbillityDescriptionText.gameObject.SetActive(false);
        hpText.gameObject.SetActive(false);
        higherSeparatorImage.gameObject.SetActive(false);
        lowerSeparatorImage.gameObject.SetActive(false);

        healthCells.ForEach(x => x.gameObject.SetActive(false));
    }

    private void SetHealthBarStatus(int maxHealth, int currentHealth)
    {
        healthCells.ForEach(x => x.gameObject.SetActive(false));

        for (int i=0;i< maxHealth; i++)
        {
            healthCells[i].gameObject.SetActive(true);

            if (i < currentHealth)
            {
                healthCells[i].sprite = TacticalFightVisualizationManager.Instance.FullHealthCell;
            }
            else
            {
                healthCells[i].sprite = TacticalFightVisualizationManager.Instance.EmptyHealthCell;
            }
        }
    }
}

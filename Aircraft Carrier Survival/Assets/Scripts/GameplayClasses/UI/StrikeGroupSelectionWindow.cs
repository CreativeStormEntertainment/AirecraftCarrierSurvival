using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrikeGroupSelectionWindow : MonoBehaviour
{
    public bool DisableEscortButton
    {
        get;
        set;
    }

    [SerializeField]
    private RectTransform rect = null;
    [SerializeField]
    private List<StrikeGroupSelectionButton> strikeGroupButtons = null;
    [SerializeField]
    private List<EnemySelectionButton> enemySelectionButtons = null;
    [SerializeField]
    private CarrierEscortButton carrierButton = null;
    [SerializeField]
    private CarrierEscortButton escortButton = null;

    [SerializeField]
    private Text title = null;
    [SerializeField]
    private string defenseID = null;
    [SerializeField]
    private string baseID = null;
    [SerializeField]
    private string cargoID = null;
    [SerializeField]
    private string damagedEscortID = null;
    [SerializeField]
    private string cooldownEscortID = null;

    public void ShowCarrierAndEscort()
    {
        HideButtons();
        carrierButton.gameObject.SetActive(true);
        escortButton.gameObject.SetActive(true);
        if (DisableEscortButton)
        {
            escortButton.DisableButton();
        }
        title.text = LocalizationManager.Instance.GetText(defenseID);
        gameObject.SetActive(true);
    }

    public void ShowEnemyBases()
    {
        var enemies = TacticManager.Instance.GetAllShips();
        int index = 0;
        title.text = LocalizationManager.Instance.GetText(baseID);
        HideButtons();
        foreach (var enemy in enemies)
        {
            if (enemy.Visible && enemy.Side == ETacticalObjectSide.Enemy && enemy.Type == ETacticalObjectType.Outpost && !enemy.Dead && enemy.BonusManeuversDefence == 0)
            {
                enemySelectionButtons[index].SetupEnemy(enemy, EStrikeGroupActiveSkill.BonusManeuversDefence);
                index++;
            }
        }
        gameObject.SetActive(true);
    }

    public void ShowEnemyCargoShips()
    {
        var enemies = TacticManager.Instance.GetAllShips();
        int index = 0;
        title.text = LocalizationManager.Instance.GetText(cargoID);
        HideButtons();
        foreach (var enemy in enemies)
        {
            if (enemy.Visible && !enemy.IsDisabled && !enemy.Dead && enemy.Side == ETacticalObjectSide.Enemy)
            {
                bool show = false;
                foreach (var block in enemy.Blocks)
                {
                    if (block.Visible && !block.Dead && block.Data.ShipType == EEnemyShipType.Cargo)
                    {
                        show = true;
                    }
                }
                if (show)
                {
                    enemySelectionButtons[index].SetupEnemy(enemy, EStrikeGroupActiveSkill.SinkCargoShip);
                    index++;
                }
            }
        }
        gameObject.SetActive(true);
    }

    public void ShowDamagedShips()
    {
        var members = StrikeGroupManager.Instance.AliveMembers;
        int index = 0;
        title.text = LocalizationManager.Instance.GetText(damagedEscortID);
        HideButtons();
        foreach (var member in members)
        {
            if (member.CurrentDurability < member.Data.Durability)
            {
                strikeGroupButtons[index].SetupDurability(member);
                index++;
            }
        }
        gameObject.SetActive(true);
    }

    public void ShowShipsOnCooldown()
    {
        var members = StrikeGroupManager.Instance.AliveMembers;
        int index = 0;
        title.text = LocalizationManager.Instance.GetText(cooldownEscortID);
        HideButtons();
        foreach (var member in members)
        {
            if (member.Cooldown > 0f)
            {
                strikeGroupButtons[index].SetupCooldown(member);
                index++;
            }
        }
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetWindowPosition(StrikeGroupMember member)
    {
        rect.SetParent(member.Button.WindowContainer);
        rect.anchoredPosition = Vector2.zero;
    }

    private void HideButtons()
    {
        foreach (var button in strikeGroupButtons)
        {
            button.Hide();
        }
        foreach (var button in enemySelectionButtons)
        {
            button.Hide();
        }
        carrierButton.gameObject.SetActive(false);
        escortButton.gameObject.SetActive(false);
    }
}

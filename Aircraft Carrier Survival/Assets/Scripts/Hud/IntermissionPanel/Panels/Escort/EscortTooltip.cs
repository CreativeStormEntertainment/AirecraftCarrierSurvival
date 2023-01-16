using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscortTooltip : MonoBehaviour
{
    [SerializeField]
    private List<StrikeGroupShipStats> shipStats = null;

    [SerializeField]
    private Sprite maxSuppliesSprite = null;
    [SerializeField]
    private Sprite escortSprite = null;
    [SerializeField]
    private Sprite defenceSprite = null;
    [SerializeField]
    private Sprite maxSquadronsSprite = null;
    [SerializeField]
    private Sprite dcReparirSpeedSprite = null;
    [SerializeField]
    private Sprite spottingBonusSprite = null;
    [SerializeField]
    private Sprite maneuversDefSprite = null;
    [SerializeField]
    private Sprite fasterResupplySprite = null;
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private Text nameText = null;
    [SerializeField]
    private Text duration = null;
    [SerializeField]
    private Text cooldown = null;
    [SerializeField]
    private Text ability = null;
    [SerializeField]
    private Text description = null;
    [SerializeField]
    private List<GameObject> fillDurabilityIcons = null;
    [SerializeField]
    private GameObject activeInfo = null;


    public void Show(StrikeGroupMemberData data)
    {
        gameObject.SetActive(true);
        Setup(data);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Setup(StrikeGroupMemberData data)
    {
        var locMan = LocalizationManager.Instance;
        int i = 0;
        for (; i < data.PassiveSkills.Count; i++)
        {
            shipStats[i].gameObject.SetActive(true);
            switch (data.PassiveSkills[i].Skill)
            {
                case EStrikeGroupPassiveSkill.Defense:
                    shipStats[i].Image.sprite = defenceSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString();
                    break;
                case EStrikeGroupPassiveSkill.Escort:
                    shipStats[i].Image.sprite = escortSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString();
                    break;
                case EStrikeGroupPassiveSkill.MaxSquadrons:
                    shipStats[i].Image.sprite = maxSquadronsSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString();
                    break;
                case EStrikeGroupPassiveSkill.MaxSupplies:
                    shipStats[i].Image.sprite = maxSuppliesSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString() + "%";
                    break;
                case EStrikeGroupPassiveSkill.DcRepairSpeed:
                    shipStats[i].Image.sprite = dcReparirSpeedSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString() + "%";
                    break;
                case EStrikeGroupPassiveSkill.RadarRange:
                    shipStats[i].Image.sprite = spottingBonusSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString();
                    break;
                case EStrikeGroupPassiveSkill.BonusManeuversDefense:
                    shipStats[i].Image.sprite = maneuversDefSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString();
                    break;
                case EStrikeGroupPassiveSkill.FasterResupply:
                    shipStats[i].Image.sprite = fasterResupplySprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString() + "%";
                    break;
                default:
                    break;
            }
        }
        for (; i < shipStats.Count; i++)
        {
            shipStats[i].gameObject.SetActive(false);
        }
        nameText.text = locMan.GetText(data.NameID);
        icon.sprite = data.Icon;
        description.text = "";
        if (data.ActiveSkill != EStrikeGroupActiveSkill.None)
        {
            ability.text = LocalizationManager.Instance.GetText(data.ActiveTitle);
            string param = "";
            switch (data.ActiveSkill)
            {
                case EStrikeGroupActiveSkill.ReplenishSquadrons:
                    param = data.Uses.ToString();
                    break;
            }
            if (string.IsNullOrEmpty(param))
            {
                description.text = LocalizationManager.Instance.GetText(data.ActiveDescription);
            }
            else
            {
                description.text = LocalizationManager.Instance.GetText(data.ActiveDescription, param);
            }
        }
        duration.text = (data.DurationHours + data.PrepareHours).ToString() + "h";
        cooldown.text = data.CooldownHours.ToString() + "h";

        for (int j = 0; j < fillDurabilityIcons.Count; j++)
        {
            fillDurabilityIcons[j].SetActive(j < data.Durability);
        }

        activeInfo.SetActive(data.ActiveSkill != EStrikeGroupActiveSkill.None);
    }
}

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EscortShopButton : ShopButton<EscortShopButtonData>, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<StrikeGroupMemberData, bool> SetShowTooltip = delegate { };

    [SerializeField]
    private Image image = null;
    [SerializeField]
    private Text shipName = null;
    [SerializeField]
    private Text description = null;
    [SerializeField]
    private Color commandPointsColor = Color.green;
    [SerializeField]
    private Color upgradePointsColor = Color.yellow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetShowTooltip(Data.Member, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetShowTooltip(Data.Member, false);
    }

    public override void Setup(EscortShopButtonData data, int index, Action click)
    {
        base.Setup(data, index, click);
        image.sprite = data.Member.Icon;
        var locMan = LocalizationManager.Instance;
        shipName.text = LocalizationManager.Instance.GetText(data.Member.NameID);
        description.text = locMan.GetText(data.Member.StrikeGroupType.ToString() + "Intermission");
        if (data.Member.PassiveSkills.Count == 0)
        {
            description.text += "\n" + locMan.GetText(GetSkillNameID(EStrikeGroupPassiveSkill.Count));
        }
        foreach (var skill in data.Member.PassiveSkills)
        {
            description.text += "\n" + locMan.GetText(GetSkillNameID(skill.Skill)) + " +" + skill.Param + GetSkillParamSymbol(skill);
        }
        gameObject.SetActive(true);
    }

    public override void SetUpgrade(int upgrade, int cost, bool isUnlock)
    {
        base.SetUpgrade(upgrade, cost, isUnlock);
        icon.sprite = icons[upgrade];
        costText.color = Unlocked ? commandPointsColor : upgradePointsColor;
    }

    private string GetSkillNameID(EStrikeGroupPassiveSkill skill)
    {
        string id = "";
        switch (skill)
        {
            case EStrikeGroupPassiveSkill.BonusManeuversDefense:
                id = "ManeuversDefense";
                break;
            case EStrikeGroupPassiveSkill.DcRepairSpeed:
                id = "DcRepairSpeed";
                break;
            case EStrikeGroupPassiveSkill.Defense:
                id = "IntermissionDefenseBonus";
                break;
            case EStrikeGroupPassiveSkill.Escort:
                id = "EscortDefense";
                break;
            case EStrikeGroupPassiveSkill.FasterResupply:
                id = "FasterResupply";
                break;
            case EStrikeGroupPassiveSkill.MaxSupplies:
                id = "MaxSupply";
                break;
            case EStrikeGroupPassiveSkill.RadarRange:
                id = "RadarRange";
                break;
            case EStrikeGroupPassiveSkill.Count:
                id = "NoPassiveBonus";
                break;
        }
        return id;
    }

    private string GetSkillParamSymbol(PassiveSkillData data)
    {
        switch (data.Skill)
        {
            case EStrikeGroupPassiveSkill.MaxSupplies:
            case EStrikeGroupPassiveSkill.DcRepairSpeed:
            case EStrikeGroupPassiveSkill.FasterResupply:
                return "%";
            default:
                break;
        }
        return "";
    }
}

using System;
using UnityEngine;

[Serializable]
public class OfficerSkill
{
    public EOfficerSkills SkillEnum
    {
        get => skillEnum;
        private set => skillEnum = value;
    }

    public byte BaseLevel
    {
        get => currentLevel;
    }

    public byte CurrentLevel
    {
        get => (byte)(currentLevel + GetUpgradeValue());
    }

    [SerializeField]
    private EOfficerSkills skillEnum = EOfficerSkills.CommandingAirForce;

    [SerializeField]
    private byte currentLevel = 0b0;

    [SerializeField]
    private uint currentExperience = 0;

    private int upgradeIndex = 0;

    public OfficerSkill(EOfficerSkills skillType, int index)
    {
        SkillEnum = skillType;
        upgradeIndex = index;
    }

    private OfficerSkill()
    {

    }

    public OfficerSkill Duplicate()
    {
        var result = new OfficerSkill();

        result.skillEnum = skillEnum;
        result.currentLevel = currentLevel;
        result.currentExperience = currentExperience;

        return result;

    }

    public void SetUpgradeIndex(int index)
    {
        upgradeIndex = index;
    }

    public byte GetUpgradeValue()
    {
        int bonusPoints;
        var data = SaveManager.Instance.Data;
        if (upgradeIndex != -1)
        {
            var upgrade = data.MissionRewards.OfficersUpgrades[upgradeIndex];
            if (SkillEnum == EOfficerSkills.CommandingAirForce)
            {
                bonusPoints = upgrade.UpgradedAirPoints;
            }
            else
            {
                bonusPoints = upgrade.UpgradedNavyPoints;
            }
        }
        else
        {
            if (SkillEnum == EOfficerSkills.CommandingAirForce)
            {
                bonusPoints = data.AdmiralAirLevel;
            }
            else
            {
                bonusPoints = data.AdmiralNavyLevel;
            }
        }
        return (byte)bonusPoints;
    }
}

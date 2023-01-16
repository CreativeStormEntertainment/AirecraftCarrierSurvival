using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StrikeGroupMemberData
{
    public string Name;
    public Sprite Icon;
    public GameObject MainPrefab;
    public GameObject IntermissionPrefab;
    public int Durability;
    public int Uses = -1;
    public int SandboxUses = -1;
    public List<PassiveSkillData> PassiveSkills;
    public EStrikeGroupActiveSkill ActiveSkill;
    public int ActiveParam;
    public float DurationHours;
    public float PrepareHours;
    public float CooldownHours;
    public EStrikeGroupType StrikeGroupType;
    public string NameID;
    public string ActiveTitle;
    public string ActiveDescription;

    public int Tier;
    public int UnlockCost;
    public int BuyCost;
    public string UnlockText;
    public string BuyText;
}

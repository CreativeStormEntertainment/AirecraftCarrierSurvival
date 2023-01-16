using System;

[Serializable]
public class BonusData
{
    public EBonusType ModifierType;
    public BonusExtendData Data;
    public bool ThisSlot;
    public EPlaneType SquadronType;

    public EBonusRequirement Requirement;
    public BonusExtendData RequirementData;

    public EBonusBenefitor Benefitor;

    public EBonusValue Effect;
    public BonusValueData EffectData;

    public bool IsPlayer;
}

using UnityEngine;
using System.Collections;

[System.Serializable]
public class DescriptiveUpgradeData : UpgradeData
{
    [SerializeField] private int DefenceBonus = 0;
    [SerializeField] private int SpottingBonus = 0;
    [SerializeField] private int ResupplySpeed = 0;
    [SerializeField] private int ShieldingUses = 0;
    [SerializeField] private int AntiSubmarinesBonus = 0;

    public int GetDefenceBonus()
    {
        return DefenceBonus;
    }

    public int GetSpottingBonus()
    {
        return SpottingBonus;
    }

    public int GetResupplySpeed()
    {
        return ResupplySpeed;
    }

    public int GetShieldingUses()
    {
        return ShieldingUses;
    }

    public int GetAntiSubmarinesBonus()
    {
        return AntiSubmarinesBonus;
    }


}

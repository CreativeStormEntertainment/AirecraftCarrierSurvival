using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

[Serializable]
public struct IntermissionData
{
    public const int MaxUpgradeCount = (1 << (MaxUpgradeBit)) - 1;
    public const int MaxCountCount = (1 << (MaxCountBit)) - 1;
    private const int MaxSkinCount = (1 << (MaxSkinBit)) - 1;
    private const int MaxUpgradeBit = 3;
    private const int MaxCountBit = 10;
    private const int MaxSkinBit = 4;
    private const int SkinShift = MaxUpgradeBit * 3;

    public int CommandPoints;
    public int UpgradePoints;

    public OfficerPeopleSaveData OfficerData;
    public CrewPeopleSaveData CrewData;

    public int HangarUpgrades;
    public int UpgradesAircraft;
    public int CurrentAircraft;

    public int AvailableCarriers;
    public int CarriersUpgrades;

    public int UnlockedEscorts;
    public List<int> OwnedEscorts;
    public List<int> SelectedEscort;
    public List<int> Broken;

    public bool SkipPopup;

    public List<int> PlannedOperationsBasket;

    public IntermissionData Duplicate()
    {
        var result = this;

        result.OfficerData = OfficerData.Duplicate();
        result.CrewData = CrewData.Duplicate();
        result.OwnedEscorts = new List<int>(OwnedEscorts);
        result.SelectedEscort = new List<int>(SelectedEscort);
        result.Broken = new List<int>(Broken);
        result.PlannedOperationsBasket = new List<int>(PlannedOperationsBasket);

        return result;
    }

    public int GetUpgrade(EPlaneType type)
    {
        return GetAircraft(UpgradesAircraft, MaxUpgradeBit, type);
    }

    public int GetCurrent(EPlaneType type)
    {
        return GetAircraft(CurrentAircraft, MaxCountBit, type);
    }

    public int GetSkin(EPlaneType type)
    {
        return GetAircraft(UpgradesAircraft >> SkinShift, MaxSkinBit, type);
    }

    public void SetCurrent(int bombers, int fighters, int torpedoes)
    {
        SetAircrafts(ref CurrentAircraft, MaxCountBit, MaxCountCount, bombers, fighters, torpedoes);
    }

    public void SetSkinAndUpgrades(int bomberUpgrade, int fighterUpgrade, int torpedoUpgrade, int bomberSkin, int fighterSkin, int torpedoSkin)
    {
        int value = 0;
        SetAircrafts(ref value, MaxSkinBit, MaxSkinCount, bomberSkin, fighterSkin, torpedoSkin);
        
        SetAircrafts(ref UpgradesAircraft, MaxUpgradeBit, MaxUpgradeCount, bomberUpgrade, fighterUpgrade, torpedoUpgrade);
        UpgradesAircraft += (value << SkinShift);
    }

    private int GetAircraft(int value, int maxBit, EPlaneType type)
    {
        int count = (int)type;
        return BinUtils.GetBits(value, (count + 1) * maxBit - 1, count * maxBit);
    }

    private void SetAircrafts(ref int value, int maxBit, int maxCount, int bombers, int fighters, int torpedoes)
    {
        Assert.IsFalse(bombers > maxCount);
        Assert.IsFalse(fighters > maxCount);
        Assert.IsFalse(torpedoes > maxCount);

        value = torpedoes;
        value <<= maxBit;
        value += fighters;
        value <<= maxBit;
        value += bombers;
    }
}

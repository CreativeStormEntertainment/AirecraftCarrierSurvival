using System;
using UnityEngine.Assertions;

[Serializable]
public class DifficultyData
{
#if UNITY_EDITOR
    static class DataOrder
    {
        public const int ID = 0;
        public const int StorageCapacity = 1;
        public const int Supplies = 2;
        public const int Ammo = 3;
        public const int Tools = 4;
        public const int LocalResProductionTime = 5;
        public const int CommandProductionTime = 6;
        public const int CrewEffectiveness = 7;
        public const int AA = 8;
        public const int Dodge = 9;
        public const int ViewRangeModifier = 10;
        public const int Count = 11;
    }

    public DifficultyData(string[] data)
    {
        Assert.IsTrue(data.Length == DataOrder.Count);
        StartStorageCapacity = int.Parse(data[DataOrder.StorageCapacity]);
        StartSupplies = int.Parse(data[DataOrder.Supplies]);
        StartAmmo = int.Parse(data[DataOrder.Ammo]);
        StartTools = int.Parse(data[DataOrder.Tools]);
        LocalResProductionTime = int.Parse(data[DataOrder.LocalResProductionTime]);
        CommandProductionTime = int.Parse(data[DataOrder.CommandProductionTime]);
        CrewEffectiveness = float.Parse(data[DataOrder.CrewEffectiveness]);
        AAPower = float.Parse(data[DataOrder.AA]);
        DodgeChance = float.Parse(data[DataOrder.Dodge]);
        ViewRangeModifier = float.Parse(data[DataOrder.ViewRangeModifier]);
    }
#endif

    private DifficultyData() { }

    public int StartStorageCapacity;
    public int StartSupplies;
    public int StartAmmo;
    public int StartTools;
    public int LocalResProductionTime;
    public int CommandProductionTime;
    public float CrewEffectiveness;
    public float AAPower;
    public float DodgeChance;
    public float ViewRangeModifier;
}

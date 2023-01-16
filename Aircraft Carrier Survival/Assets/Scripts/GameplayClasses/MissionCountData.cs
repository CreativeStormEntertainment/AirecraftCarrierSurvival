using System.Collections.Generic;

public class MissionCountData
{
    public int Count
    {
        get;
        private set;
    }

    public int BaseCount;
    public Dictionary<IslandRoom, int> Bonuses;

    public MissionCountData(int baseCount)
    {
        BaseCount = baseCount;
        Bonuses = new Dictionary<IslandRoom, int>();
        Count = baseCount;
    }

    public void SetBonus(IslandRoom room, int bonus)
    {
        Bonuses[room] = bonus;
        Count = BaseCount;
        foreach (var key in Bonuses)
        {
            Count += key.Value;
        }
    }
}

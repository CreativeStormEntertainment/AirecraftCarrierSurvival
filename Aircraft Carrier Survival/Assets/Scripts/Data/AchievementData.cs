using System;

[Serializable]
public class AchievementData
{
    public EAchievementType Type;
    public string Id;
    public int Data;
    public string StatID;
    public string Stat2ID;
    [NonSerialized]
    public int StoredData;
}

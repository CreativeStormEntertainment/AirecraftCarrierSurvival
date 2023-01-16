using UnityEngine;

public class OfficerIntermissionData : PeopleIntermissionData
{
    public OfficerUpgrades Data;

    public int Index
    {
        get;
        set;
    }

    public OfficerIntermissionData(Canvas canvas, RectTransform root, string text) : base(canvas, root, 0, text)
    {
    }
}

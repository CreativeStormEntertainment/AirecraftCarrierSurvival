using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewIntermissionData : PeopleIntermissionData
{
    public CrewUpgradeSaveData Data;

    public CrewIntermissionData(Canvas canvas, RectTransform root, int cost, string text) : base(canvas, root, cost, text)
    {

    }
}

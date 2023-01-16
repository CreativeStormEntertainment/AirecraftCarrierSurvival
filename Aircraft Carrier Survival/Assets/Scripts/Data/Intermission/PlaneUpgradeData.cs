using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlaneUpgradeData : UpgradeData
{
    [SerializeField] private EPlaneType planeType = 0;
    [SerializeField] private string planeName = "";
    [SerializeField] private int survivabilityPercent = 0;

    public EPlaneType GetPlaneType()
    {
        return planeType;
    }

    public string GetPlaneName()
    {
        return planeName;
    }

    public int GetSurvivabilityPercent()
    {
        return survivabilityPercent;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AircraftCarrierTypeData : UpgradeData
{
    [SerializeField]
    private string type = null;

    public string GetTypeText()
    {
        return type;
    }

}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

public class DeckVisualsManager : MonoBehaviour
{
    public static DeckVisualsManager Instance = null;

    [SerializeField]
    private Transform aaStationsObjectsTransform = null;
    [SerializeField]
    private Transform radarsObjectsTransform = null;

    private int prevAAAsigned = 0;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    public void Setup()
    {
        var upgrades = SaveManager.Instance.Data.CarrierUpgrades;
        for (int i = 0; i < 3; i++)
        {
            radarsObjectsTransform.GetChild(i).gameObject.SetActive((i + 1) == upgrades[(int)EShipUpgrades.RadarRange]);
        }

        foreach (Transform t in aaStationsObjectsTransform)
        {
            for (int i = 0; i < 3; i++)
            {
                t.GetChild(i).gameObject.SetActive(i < upgrades[(int)EShipUpgrades.AAStations]);
            }
        }
    }

    public void OnAADepartNumberChanged(int currentAssigned)
    {
        if (currentAssigned == prevAAAsigned)
        {
            return;
        }

        prevAAAsigned = currentAssigned;
    }
}

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VisualizeContainer
{
    public List<GameObject> Objects;
}

public class VisualizeUpgradeShip : MonoBehaviour
{
    [SerializeField]
    private int dataIndex = 0; //0 - radar, 1 - aa
    [SerializeField]
    private List<VisualizeContainer> tiers = new List<VisualizeContainer>();

    private void Start()
    {
        var data = SaveManager.Instance.Data;
        int level = BinUtils.ExtractData(data.IntermissionData.CarriersUpgrades, 3, (int)data.SelectedAircraftCarrier + ((int)ECarrierType.Count * dataIndex));

        for (int i = 0; i < tiers.Count; ++i)
        {
            for (int j = 0; j < tiers[i].Objects.Count; ++j)
            {
                bool show = dataIndex == 0 ? i == level : i <= level;
                tiers[i].Objects[j].SetActive(show);
            }
        }
    }
}

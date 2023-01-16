using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionSummaryType", menuName = "Ui/MissionSummatyType", order = 1)]
public class SummaryTypeScriptable : ScriptableObject
{
    public List<SummaryPopupData> SummaryTypeData => summaryTypeData;

    [SerializeField]
    private List<SummaryPopupData> summaryTypeData = new List<SummaryPopupData>();
}

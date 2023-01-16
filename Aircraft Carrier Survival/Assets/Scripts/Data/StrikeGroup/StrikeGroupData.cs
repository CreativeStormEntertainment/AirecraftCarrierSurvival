using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StrikeGroupData", menuName = "Datas/Strike Group", order = 1)]
public class StrikeGroupData : ScriptableObject
{
    public List<StrikeGroupMemberData> Data = new List<StrikeGroupMemberData>();
}

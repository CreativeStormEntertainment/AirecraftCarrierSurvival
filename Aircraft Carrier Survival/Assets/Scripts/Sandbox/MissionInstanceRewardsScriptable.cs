using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionInstanceRewardsScriptable", menuName = "Sandbox/MissionInstanceRewardsScriptable")]
public class MissionInstanceRewardsScriptable : ScriptableObject
{
    public List<MissionInstanceRewardsData> RewardsList;
}

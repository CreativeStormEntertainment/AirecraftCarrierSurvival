using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TacticMapCategory", menuName = "Sandbox/TacticMapCategory")]
public class TacticMapCategory : ScriptableObject
{
    public ESandboxObjectiveType ObjectiveType;
    public List<SOTacticMap> TacticMaps;
}

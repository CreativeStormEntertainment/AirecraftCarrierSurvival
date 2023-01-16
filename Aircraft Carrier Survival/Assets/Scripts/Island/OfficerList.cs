using System.Collections.Generic;
using UnityEngine;

public class OfficerList : ScriptableObject
{
    public List<OfficerSetup> Officers;
    public List<DcImages> Portraits;
    public List<int> OfficerLevelThreshold;
}

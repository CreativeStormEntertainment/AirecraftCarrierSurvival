using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectiveTypesList
{
    public string DescriptionId;
    public List<ESandboxObjectiveType> SandboxObjectiveTypes; //Size of this list must be the same as largest count of maps of any of planned operations 
}

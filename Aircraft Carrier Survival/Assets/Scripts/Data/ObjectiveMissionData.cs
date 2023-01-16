using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ObjectiveMissionData
{
    public string Title = "";
    public List<ObjectiveStepData> Steps = null;
    public bool Hidden = false;
}

[Serializable]
public class ObjectiveStepData
{
    public string Name = "";
    public GameObject ObjectiveSprite = null;
    public bool Hidden = true;
}

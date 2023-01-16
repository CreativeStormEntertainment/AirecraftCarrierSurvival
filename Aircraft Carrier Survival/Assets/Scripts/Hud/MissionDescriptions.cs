using System;
using UnityEngine.Serialization;

[Serializable]
public class MissionDescriptions
{
    [FormerlySerializedAs("stage")]
    public EMissionStage stage;
    public string desc;
}

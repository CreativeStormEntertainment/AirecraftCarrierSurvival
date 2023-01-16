using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class MissionName
{
    [FormerlySerializedAs("type")]
    public EMissionOrderType Type;
    public string Title;
    public List<MissionDescriptions> Descriptions;
    public Sprite Icon;
}

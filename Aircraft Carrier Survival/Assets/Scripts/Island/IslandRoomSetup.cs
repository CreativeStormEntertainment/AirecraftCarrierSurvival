using System;
using UnityEngine;

[Serializable]
public struct IslandRoomSetup
{
    public EIslandRoomType roomType;
    public string name;
    public int maxOfficersAsigned;
    [Range(1, 3)]
    public int switchCount;
    [Range(1, 3)]
    public int defaultSwitch;
    public Sprite roomIcon;
}

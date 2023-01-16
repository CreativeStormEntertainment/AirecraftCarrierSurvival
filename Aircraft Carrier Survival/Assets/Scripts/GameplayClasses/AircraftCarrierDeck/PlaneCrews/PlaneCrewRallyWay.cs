using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlaneCrewRallyWay
{
    public PlaneCrewNode JumpDown;
    public PlaneCrewNode JumpUp;
    public List<PlaneCrewNode> Additional = new List<PlaneCrewNode>();
    public PlaneCrewRallyPoint Rally;
}

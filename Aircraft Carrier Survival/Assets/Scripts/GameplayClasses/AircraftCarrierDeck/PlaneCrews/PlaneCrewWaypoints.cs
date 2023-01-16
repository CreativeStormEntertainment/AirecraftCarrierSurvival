using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlaneCrewWaypoints
{
    public List<List<PlaneCrewNode>> Left = new List<List<PlaneCrewNode>>();
    public List<List<PlaneCrewNode>> Right = new List<List<PlaneCrewNode>>();
}

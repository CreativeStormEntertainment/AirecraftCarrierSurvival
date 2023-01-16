using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlanesTiers", menuName = "Ui/PlanesTiers")]
public class PlanesTiersData : ScriptableObject
{
    public List<PlaneTiersData> Data;
}

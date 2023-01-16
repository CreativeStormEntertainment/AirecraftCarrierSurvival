using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneCrewRallyPoint : MonoBehaviour
{
    public Transform Transf
    {
        get;
        private set;
    }
    public int Nr
    {
        get;
        private set;
    }
    public bool IsFree = true;

    public PlaneCrewRallyWay Way = new PlaneCrewRallyWay();

    public void Init(PlaneCrewRallyWay Way)
    {
        Transf = transform;
        this.Way = Way;
    }
}

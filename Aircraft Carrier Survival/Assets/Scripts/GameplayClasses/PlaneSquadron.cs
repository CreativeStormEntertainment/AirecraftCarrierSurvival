using System.Collections.Generic;
using UnityEngine;

public class PlaneSquadron
{
    public EPlaneType PlaneType
    {
        get;
        set;
    }
    public bool IsDamaged
    {
        get;
        set;
    }

    public bool FromHangar
    {
        get;
        set;
    }

    public List<PlaneMovement> Planes;

    public List<Transform> Hollows;

    public bool AnimationPlay;

    public PlaneSquadron(EPlaneType planeType)
    {
        PlaneType = planeType;
        Planes = new List<PlaneMovement>();
        Hollows = new List<Transform>();
        AnimationPlay = true;
    }
}

using System;
using UnityEngine;

[Serializable]
public class PlaneNode
{
    public Vector3 Position;
    public PlaneSquadron OccupiedBy;
    public bool ElevatorDown;
    public bool ElevatorUp;
    public int Lift;
    public bool Wait;
    public bool Wait2;
    public PlaneNode WaitForNode;

    public PlaneNode(Vector3 position)
    {
        Position = position;
    }
}

using System;
using UnityEngine;

[Serializable]
public class PlaneCrewNode
{
    [SerializeField]
    public Vector3 Position;
    public Quaternion Rotation;
    public int IsJump = -1;
    public bool ThisLineCrouch = false;
    public bool NextLineCrouch = false;
    public bool PrevLineCrouch = false;
    public PlaneCrewNode(Vector3 position)
    {
        Position = position;
    }
}

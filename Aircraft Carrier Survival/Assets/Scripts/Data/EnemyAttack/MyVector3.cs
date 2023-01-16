using System;
using UnityEngine;

[Serializable]
public struct MyVector3
{
    public float X
    {
        get => vec.X;
        set => vec.X = value;
    }
    public float Y
    {
        get => vec.Y;
        set => vec.Y = value;
    }
    public float Z;

    [SerializeField]
    private MyVector2 vec;

    public static implicit operator Vector3(MyVector3 data)
    {
        return new Vector3(data.X, data.Y, data.Z);
    }

    public static implicit operator MyVector3(Vector3 data)
    {
        return new MyVector3(data);
    }

    public MyVector3(float x, float y, float z)
    {
        vec = new MyVector2(x, y);
        Z = z;
    }

    public MyVector3(Vector3 vec) : this(vec.x, vec.y, vec.z)
    {

    }
}

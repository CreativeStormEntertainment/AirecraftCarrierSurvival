using System;
using UnityEngine;

[Serializable]
public struct MyVector4
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
    public float Z
    {
        get => vec.Z;
        set => vec.Z = value;
    }
    public float W;

    [SerializeField]
    private MyVector3 vec;

    public static implicit operator Vector4(MyVector4 data)
    {
        return new Vector4(data.X, data.Y, data.Z, data.W);
    }

    public static implicit operator Quaternion(MyVector4 data)
    {
        return new Quaternion(data.X, data.Y, data.Z, data.W);
    }

    public static implicit operator MyVector4(Vector4 data)
    {
        return new MyVector4(data);
    }

    public static implicit operator MyVector4(Quaternion data)
    {
        return new MyVector4(data);
    }

    public MyVector4(float x, float y, float z, float w)
    {
        vec = new MyVector3(x, y, z);
        W = w;
    }

    public MyVector4(Vector4 vec) : this(vec.x, vec.y, vec.z, vec.w)
    {

    }

    public MyVector4(Quaternion quat) : this(quat.x, quat.y, quat.z, quat.w)
    {

    }
}

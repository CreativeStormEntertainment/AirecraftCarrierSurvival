using System;
using UnityEngine;

[Serializable]
public struct MyVector2
{
    public float X;
    public float Y;

    public static implicit operator Vector2(MyVector2 data)
    {
        return new Vector2(data.X, data.Y);
    }

    public static implicit operator MyVector2(Vector2 data)
    {
        return new MyVector2(data);
    }

    public MyVector2(Vector2 vec) : this(vec.x, vec.y)
    {

    }

    public MyVector2(float x, float y)
    {
        X = x;
        Y = y;
    }
}

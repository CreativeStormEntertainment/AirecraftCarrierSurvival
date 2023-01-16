using System;
using UnityEngine;

[Serializable]
public class MapCornerData
{
    public EOrientation CornerOrientation;
    public Vector2 CornerPositional;
    [NonSerialized]
    public Action Callback;
}

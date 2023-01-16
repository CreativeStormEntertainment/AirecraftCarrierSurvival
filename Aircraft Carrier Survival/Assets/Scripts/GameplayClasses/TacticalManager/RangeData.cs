using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeData
{
    public Vector2 Position;
    public float RangeSqr;
    public int Timer;
    public int MaxTime;

    public RangeData(Vector2 pos, float rangeSqr, int maxTime)
    {
        Position = pos;
        RangeSqr = rangeSqr;
        MaxTime = maxTime;
    }
}

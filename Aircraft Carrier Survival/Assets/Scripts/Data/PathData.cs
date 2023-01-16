using UnityEngine;

public class PathData
{
    public Vector3 Pos;
    public Vector3? HelperPos;
    public float Percent;

    public PathData(Vector3 pos)
    {
        Pos = pos;
    }
}

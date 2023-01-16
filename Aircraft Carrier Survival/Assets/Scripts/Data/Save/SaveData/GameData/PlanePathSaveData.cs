using System;
using System.Collections.Generic;

[Serializable]
public struct PlanePathSaveData
{
    public int Squadron;
    public int Index;
    public EPlaneType Type;
    public int PlaneCount;

    public MyVector3 Position;
    public float RotationY;
    public float Delay;

    public int NodeIndex;

    public List<int> Path;
    public bool Anim;
    public int PathNode;
    public float NodeLengthDone;

    [NonSerialized]
    public PlaneNode CurrentNode;
    [NonSerialized]
    public List<PlaneNode> CurrentPath;

    public PlanePathSaveData Duplicate()
    {
        var result = this;

        if (Path != null)
        {
            result.Path = new List<int>(Path);
        }

        return result;
    }
}

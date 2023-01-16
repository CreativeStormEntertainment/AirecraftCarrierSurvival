using UnityEngine;

public struct SecondaryCameraParamsData
{
    public Transform Transform;
    public bool Set;

    public SecondaryCameraParamsData(Transform transform, bool set)
    {
        Transform = transform;
        Set = set;
    }
}

using UnityEngine;

public struct MeshData
{
    public int[] Triangles;
    public Vector3[] Vertices;
    public Transform Transform;

    public MeshData(MeshFilter filter)
    {
        Triangles = filter.sharedMesh.triangles;
        Vertices = filter.sharedMesh.vertices;
        Transform = filter.transform;
    }
}
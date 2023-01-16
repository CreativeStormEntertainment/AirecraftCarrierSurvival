using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleTest : MonoBehaviour
{
    [SerializeField]
    private Vector3 a = Vector3.zero;
    [SerializeField]
    private Vector3 b = Vector3.zero;
    [SerializeField]
    private Vector3 c = Vector3.zero;
    [SerializeField]
    private Vector3 interiorPoint = Vector3.zero;
    [SerializeField]
    private Vector3 secondPoint = Vector3.zero;
    [SerializeField]
    private float epsilon = 0.0001f;

    public void Test()
    {
        var t = new Triangle(a, b, c, epsilon);
        var point = t.GetTrianglePoint(interiorPoint, secondPoint);
        Debug.Log(point.ToString());
    }
}

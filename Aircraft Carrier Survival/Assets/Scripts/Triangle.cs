using UnityEngine;

public class Triangle
{
    private readonly Vector2 triangleA;
    private readonly Vector2 triangleB;
    private readonly Vector2 triangleC;

    private readonly float aSegmentAB;
    private readonly float bSegmentAB;
    private readonly float cSegmentAB;
    private readonly float aSegmentBC;
    private readonly float bSegmentBC;
    private readonly float cSegmentBC;
    private readonly float aSegmentAC;
    private readonly float bSegmentAC;
    private readonly float cSegmentAC;

    private readonly float epsilon;

    private readonly float area;

    public Triangle(Vector3 a, Vector3 b, Vector3 c, float epsilon)
    {
        triangleA = new Vector2(a.x, a.z);
        triangleB = new Vector2(b.x, b.z);
        triangleC = new Vector2(c.x, c.z);

        this.epsilon = epsilon;

        area = Area(triangleA, triangleB, triangleC);

        aSegmentAB = triangleA.y - triangleB.y;
        bSegmentAB = triangleB.x - triangleA.x;
        cSegmentAB = -(aSegmentAB * triangleA.x) - (bSegmentAB * triangleA.y);
        aSegmentBC = triangleB.y - triangleC.y;
        bSegmentBC = triangleC.x - triangleB.x;
        cSegmentBC = -(aSegmentBC * triangleB.x) - (bSegmentBC * triangleB.y);
        aSegmentAC = triangleA.y - triangleC.y;
        bSegmentAC = triangleC.x - triangleA.x;
        cSegmentAC = -(aSegmentAC) * triangleC.x - (bSegmentAC * triangleC.y);
    }

    public Vector3 GetTrianglePoint(Vector3 fromV3, Vector3 toV3)
    {
        var from = new Vector2(fromV3.x, fromV3.z);
        var to = new Vector2(toV3.x, toV3.z);
        //Assert.IsTrue(PointInTriangle(from));
        var trianglePoint = to;
        if (!PointInTriangle(trianglePoint))
        {
            float lineA = from.y - to.y;
            float lineB = to.x - from.x;
            float lineC = (to.y - from.y) * from.x - ((to.x - from.x) * from.y);

            var possiblePoint1 = GetPossiblePoint(to, lineA, lineB, lineC, aSegmentAB, bSegmentAB, cSegmentAB, triangleA, triangleB);
            var possiblePoint2 = GetPossiblePoint(to, lineA, lineB, lineC, aSegmentBC, bSegmentBC, cSegmentBC, triangleB, triangleA);
            var possiblePoint3 = GetPossiblePoint(to, lineA, lineB, lineC, aSegmentAC, bSegmentAC, cSegmentAC, triangleA, triangleC);

            float sqr1 = Vector2.SqrMagnitude(possiblePoint1 - to);
            float sqr2 = Vector2.SqrMagnitude(possiblePoint2 - to);
            float sqr3 = Vector2.SqrMagnitude(possiblePoint3 - to);

            trianglePoint = possiblePoint1;
            if (sqr1 > sqr2)
            {
                sqr1 = sqr2;
                trianglePoint = possiblePoint2;
            }
            if (sqr1 > sqr3)
            {
                trianglePoint = possiblePoint3;
            }
        }
        return new Vector3(trianglePoint.x, toV3.y, trianglePoint.y);
    }

    private Vector2 GetPossiblePoint(Vector2 linePointB, float lineA, float lineB, float lineC, float segmentA, float segmentB, float segmentC, Vector2 segmentPointA, Vector2 segmentPointB)
    {
        var result = GetPointOnLineSegment(linePointB, lineA, lineB, lineC, segmentA, segmentB, segmentC, segmentPointA, segmentPointB);
        result = PointInTriangle(result) ? result : Vector2.positiveInfinity;

        return result;
    }

    private float Area(Vector2 a, Vector2 b, Vector2 c)
    {
        return Mathf.Abs((a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) / 2f);
    }

    private bool PointInTriangle(Vector2 pt)
    {
        float a1 = Area(pt, triangleB, triangleC);
        float a2 = Area(triangleA, pt, triangleC);
        float a3 = Area(triangleA, triangleB, pt);

        return Mathf.Abs(area - (a1 + a2 + a3)) < epsilon;
    }

    private Vector2 GetPointOnLineSegment(Vector2 linePointB, float lineA, float lineB, float lineC, float segmentA, float segmentB, float segmentC, Vector2 segmentPointA, Vector2 segmentPointB)
    {
        float determinant = segmentA * lineB - lineA * segmentB;

        Vector2 result;
        if (Mathf.Abs(determinant) > epsilon)
        {
            float determinantX = lineC * segmentB - segmentC * lineB;
            float determinantY = segmentC * lineA - lineC * segmentA;
            result.x = determinantX / determinant;
            result.y = determinantY / determinant;
        }
        else
        {
            float sqr1 = Vector2.SqrMagnitude(linePointB - segmentPointA);
            float sqr2 = Vector2.SqrMagnitude(linePointB - segmentPointB);
            result = sqr1 > sqr2 ? segmentPointB : segmentPointA;
        }

        return result;
    }
}

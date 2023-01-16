using UnityEngine;
using UnityEngine.Assertions;

public class Quadrilateral
{
    private Vector3 topLeft;
    private Vector3 bottomRight;

    private Vector3 quadTopLeft;
    private Vector3 quadTopRight;
    private Vector3 quadBottomLeft;
    private Vector3 quadBottomRight;

    public Quadrilateral(Vector3 quadTopLeft, Vector3 quadTopRight, Vector3 quadBottomLeft, Vector3 quadBottomRight)
    {
        Assert.IsTrue(quadTopRight.x >= quadTopLeft.x);
        Assert.IsTrue(quadBottomRight.x >= quadBottomLeft.x);
        Assert.IsTrue(quadTopLeft.z >= quadBottomLeft.z);
        Assert.IsTrue(quadTopRight.z >= quadBottomRight.z);

        this.quadTopLeft = quadTopLeft;
        this.quadTopRight = quadTopRight;
        this.quadBottomLeft = quadBottomLeft;
        this.quadBottomRight = quadBottomRight;

        SetupMargins();

        Assert.IsTrue(bottomRight.x >= topLeft.x);
        Assert.IsTrue(topLeft.z >= bottomRight.z);
    }

    public Vector3 SnapToQuadrilateral(Vector3 pos, Vector3 fallback)
    {
#if UNITY_EDITOR
        SetupMargins();
#endif

        float axisPercentage = Mathf.InverseLerp(topLeft.x, bottomRight.x, pos.x);
        pos.z = Mathf.Clamp(pos.z, Mathf.Lerp(quadBottomLeft.z, quadBottomRight.z, axisPercentage), Mathf.Lerp(quadTopLeft.z, quadTopRight.z, axisPercentage));
        axisPercentage = Mathf.InverseLerp(bottomRight.z, topLeft.z, pos.z);
        pos.x = Mathf.Clamp(pos.x, Mathf.Lerp(quadBottomLeft.x, quadTopLeft.x, axisPercentage), Mathf.Lerp(quadBottomRight.x, quadTopRight.x, axisPercentage));
        if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z))
        {
            Debug.LogError("NaN");
            return fallback;
        }
        return pos;
    }

    private void SetupMargins()
    {
        topLeft.x = Mathf.Min(quadTopLeft.x, quadBottomLeft.x);
        topLeft.z = Mathf.Max(quadTopLeft.z, quadTopRight.z);
        bottomRight.x = Mathf.Max(quadTopRight.x, quadBottomRight.x);
        bottomRight.z = Mathf.Min(quadBottomLeft.z, quadBottomRight.z);
    }
}

using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class FreeViewData
{
    public IList<CinemachineFreeLook.Orbit> StartOrbits => startOrbits;

    [Header("This ref don't touch")]
    public CinemachineFreeLook FreeCamera;
    [Tooltip("Max zoom in")]
    public float MinRangeDeviation = 300f;
    [Header("Don't touch")]
    public float MaxRangeDeviation = 100f;
    [Tooltip("Camera horizontal rotation speed(omega)")]
    public float RotateStep = 80f;
    [Tooltip("Scroll speed")]
    public float ScrollStep = 32f;
    [Tooltip("Camera vertical rotation speed(omega)")]
    public float Speed = 2f;
    [Tooltip("Start horizontal angle")]
    public float StartAngle;

    public bool AllowTargetMovement;
    [Tooltip("Specify XY movement ranges of the target")]
    public Transform BottomLeft;
    [Tooltip("Specify XY movement ranges of the target")]
    public Transform TopRight;
    public Transform Target;

    [Header("Don't touch")]
    public bool DisableVerticalMovement;
    public bool ScrollOut;
    [Tooltip("Start zoom in")]
    public float StartScroll = 0f;

    private float baseMinX;
    private float baseMaxX;
    private float baseMinY;
    private float baseMaxY;

    private MyVector2 bottomLeft;
    private MyVector2 topRight;

    private float minScrollPerc;

    private List<CinemachineFreeLook.Orbit> startOrbits;

    public void Setup()
    {
        baseMinX = FreeCamera.m_XAxis.m_MinValue;
        baseMaxX = FreeCamera.m_XAxis.m_MaxValue;
        baseMinY = FreeCamera.m_YAxis.m_MinValue;
        baseMaxY = FreeCamera.m_YAxis.m_MaxValue;

        FreeCamera.m_XAxis.Value = StartAngle;

        startOrbits = new List<CinemachineFreeLook.Orbit>(FreeCamera.m_Orbits);

        minScrollPerc = MinRangeDeviation / FreeCamera.m_Orbits[2].m_Radius;

        Assert.IsFalse(AllowTargetMovement == (BottomLeft == null || TopRight == null));
        if (AllowTargetMovement)
        {
            bottomLeft = new MyVector2(BottomLeft.position.z, BottomLeft.position.y);
            topRight = new MyVector2(TopRight.position.z, TopRight.position.y);
            Assert.IsTrue(topRight.X > bottomLeft.X);
            Assert.IsTrue(topRight.Y > bottomLeft.Y);
        }

        if (StartScroll > 0f)
        {
            Scroll(StartScroll);
        }
    }

    public void LoadData(ref CameraSaveData data)
    {
        FreeCamera.gameObject.SetActive(false);

        float y0 = data.Scroll / startOrbits[2].m_Radius;
        for (int i = 0; i < FreeCamera.m_Orbits.Length; i++)
        {
            float height = startOrbits[i].m_Height - startOrbits[2].m_Height;
            FreeCamera.m_Orbits[i].m_Height = startOrbits[2].m_Height + height * y0;
            FreeCamera.m_Orbits[i].m_Radius = startOrbits[i].m_Radius * y0;
        }

        FreeCamera.m_XAxis.Value = data.Axes.X;
        FreeCamera.m_YAxis.Value = data.Axes.Y;

        FreeCamera.gameObject.SetActive(true);

        FreeCamera.transform.position = data.Position;
        FreeCamera.transform.rotation = data.Rotation;

        if (AllowTargetMovement && data.TargetPosition.Y > 1f)
        {
            Target.position = data.TargetPosition;
        }
    }

    public void SaveData(ref CameraSaveData data)
    {
        data.Scroll = FreeCamera.m_Orbits[2].m_Radius;
        data.Axes = new MyVector2(FreeCamera.m_XAxis.Value, FreeCamera.m_YAxis.Value);
        data.Position = FreeCamera.transform.position;
        data.Rotation = FreeCamera.transform.rotation;

        if (AllowTargetMovement)
        {
            data.TargetPosition = Target.position;
        }
        else
        {
            data.TargetPosition.Y = 0f;
        }
    }

    public void Move(float horizontal, float vertical)
    {
        if (float.IsNaN(horizontal) ||float.IsNaN(RotateStep))
        {
            Debug.LogError("NaN");
            return;
        }

        if (AllowTargetMovement)
        {
            var pos = Target.position;
            pos.z = Mathf.Clamp(pos.z + horizontal * Speed, bottomLeft.X, topRight.X);
            pos.y = Mathf.Clamp(pos.y + vertical * Speed, bottomLeft.Y, topRight.Y);

            Target.position = pos;
        }
        else
        {
            FreeCamera.m_XAxis.Value -= RotateStep * horizontal;
            if (!DisableVerticalMovement)
            {
                if (float.IsNaN(vertical) || float.IsNaN(Speed))
                {
                    Debug.LogError("NaN");
                    return;
                }
                FreeCamera.m_YAxis.Value += vertical * Speed;
            }
        }
    }

    public void Rotate(float angle)
    {
        if (!AllowTargetMovement)
        {
            return;
        }

        if (float.IsNaN(angle) || float.IsNaN(RotateStep))
        {
            Debug.LogError("NaN");
            return;
        }

        FreeCamera.m_XAxis.Value -= RotateStep * angle;
    }

    public bool Scroll(float sign)
    {
        float y0 = FreeCamera.m_Orbits[2].m_Radius / startOrbits[2].m_Radius;
        float x0 = Mathf.Sqrt(y0);
        float x1 = x0 - sign * ScrollStep;
        float y1 = Mathf.Clamp(x1 * x1, minScrollPerc, 1f);
        for (int i = 0; i < FreeCamera.m_Orbits.Length; i++)
        {
            float height = startOrbits[i].m_Height - startOrbits[2].m_Height;
            FreeCamera.m_Orbits[i].m_Height = startOrbits[2].m_Height + height * y1;
            FreeCamera.m_Orbits[i].m_Radius = startOrbits[i].m_Radius * y1;
        }
        return Mathf.Approximately(y1, (ScrollOut ? 1f : minScrollPerc));
    }

    public float GetScroll()
    {
        return FreeCamera.m_Orbits[2].m_Radius / startOrbits[2].m_Radius;
    }

    public void SetLockRanges(bool lockRange)
    {
        SetLockRanges(lockRange, FreeCamera);
    }

    public void SetLockRanges(bool lockRange, CinemachineFreeLook look)
    {
        if (lockRange)
        {
            look.m_XAxis.Value = StartAngle;
            look.m_XAxis.m_MinValue = StartAngle - 1f;
            look.m_XAxis.m_MaxValue = StartAngle + 1f;
            look.m_YAxis.m_MinValue = FreeCamera.m_YAxis.Value;
            look.m_YAxis.m_MaxValue = FreeCamera.m_YAxis.Value;
        }
        else
        {
            look.m_XAxis.m_MinValue = baseMinX;
            look.m_XAxis.m_MaxValue = baseMaxX;
            look.m_YAxis.m_MinValue = baseMinY;
            look.m_YAxis.m_MaxValue = baseMaxY;
            Scroll(10000f);
        }
    }
}

using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class ViewData
{
    public Action CameraMoved = delegate { };
    [Header("This ref don't touch")]
    public CinemachineVirtualCameraBase Camera;
    [Header("This ref don't touch")]
    [Tooltip("Starting position of camera")]
    public Transform Target;
    public Transform TopLeft;
    public Transform TopRight;
    public Transform BottomLeft;
    public Transform BottomRight;
    public Transform MaxZoom;
    [Header("Don't touch")]
    public Transform HorizontalDir = null;
    [Tooltip("Scroll speed")]
    public float ScrollStep;
    [Tooltip("Camera speed")]
    public float Speed;

    private Vector3 horizontalMovement = Vector3.zero;

    private Triangle triangle;

    public void Setup()
    {
        horizontalMovement = Camera.transform.right;
        horizontalMovement.y = 0f;
        horizontalMovement.Normalize();

        triangle = new Triangle(BottomLeft.position, TopLeft.position, MaxZoom.position, .01f);

        Assert.IsTrue(MaxZoom.position.y <= TopLeft.position.y);
    }

    public void LoadData(ref CameraSaveData data)
    {
        Camera.gameObject.SetActive(false);
        Camera.transform.position = Target.position = data.CurrentCameraPos;
        Camera.gameObject.SetActive(true);
    }

    public void SaveData(ref CameraSaveData data)
    {
        data.CurrentCameraPos = Target.position;
        data.Rotation = Camera.transform.rotation;
    }

    public void Move(float horizontal)
    {
        MoveAbsolute(Target.position + Speed * horizontal * horizontalMovement);
    }

    public void SetPos(Vector3 pos)
    {
        var result = triangle.GetTrianglePoint(MaxZoom.position, pos);
        Target.position = result;
        CameraMoved();
    }

    public void MoveAbsolute(Vector3 pos)
    {
        //Target.position = triangle.GetTrianglePoint(Target.position, pos);
        var result = triangle.GetTrianglePoint(Target.position, pos);
        if (result.x > MaxZoom.position.x)
        {
            result = MaxZoom.position;
        }
        Target.position = result;
        CameraMoved();
    }

    public bool Scroll(Vector3 dir, float sign)
    {
        dir.y = 0f;
        var oldPos = Target.position;
        MoveAbsolute(Target.position + dir * ScrollStep);
        return (Target.position - oldPos).sqrMagnitude < 0.05f && sign < 0f;
    }
}

using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class DeckViewData
{
    [Header("This ref don't touch")]
    public CinemachineVirtualCameraBase Camera;
    [Header("This ref don't touch")]
    [Tooltip("Starting position of camera")]
    public Transform CameraGhost;

    [Tooltip("Camera speed")]
    public float CameraSpeed = 5000f;
    [Tooltip("Scroll speed")]
    public float ZoomSpeed = 1000f;
    [Tooltip("Camera speed change on max zoom in")]
    public float MaxZoomFactor = 5f;

    [Tooltip("Max movement on right on max zoom in")]
    public Transform BottomRight;
    [Tooltip("Max movement on right on max zoom out")]
    public Transform TopRight;
    [Tooltip("Max movement on left(relative to BottomRight) on max zoom in")]
    public float MaxDeltaBottom;
    [Tooltip("Max movement on left(relative to TopRight) on max zoom in")]
    public float MaxDeltaTop;

    private float maxLeft;
    private float maxRight;

    private float currentZoomLevel = 1f;

    private Vector3 startPos;

    public void Setup()
    {
        ////CameraGhost.rotation = Camera.transform.rotation;
        //var argument = (BottomLeft.position.x - Top.position.x) / CameraGhost.forward.x;
        //BottomLeft.position = new Vector3(BottomLeft.position.x, Top.position.y + CameraGhost.forward.y * argument, BottomLeft.position.z);
        //Assert.IsFalse(CameraGhost.forward.x == 0, "CameraGhost rotation.x = 0 !!!");
        //SetCameraPos(Top.position + (CameraGhost.forward * argument / 2f));
        startPos = CameraGhost.position;
        SetCameraPos(CameraGhost.position);
    }

    public void LoadData(ref CameraSaveData data)
    {
        Camera.gameObject.SetActive(false);
        Camera.transform.position = CameraGhost.position = data.CurrentCameraPos;
        Camera.gameObject.SetActive(true);
    }

    public void SaveData(ref CameraSaveData data)
    {
        data.CurrentCameraPos = CameraGhost.position;
        data.Rotation = Camera.transform.rotation;
    }

    public void Move(float horizontal)
    {
        float zoomFactor = Mathf.Lerp(MaxZoomFactor, 1f, currentZoomLevel);
        var newPosition = new Vector3(CameraGhost.position.x, CameraGhost.position.y, Mathf.Clamp((CameraGhost.position.z + horizontal * Time.unscaledDeltaTime * CameraSpeed * zoomFactor), maxLeft, maxRight));
        if (float.IsNaN(newPosition.x) || float.IsNaN(newPosition.y) || float.IsNaN(newPosition.z))
        {
            Debug.LogError("NaN");
            return;
        }
        CameraGhost.position = newPosition;
    }

    public bool Scroll(float value)
    {
        var delta = CameraGhost.forward * Time.unscaledDeltaTime * value * ZoomSpeed;
        var newPosition = CameraGhost.position + delta;
        float newY = Mathf.Clamp(newPosition.y, BottomRight.position.y, TopRight.position.y);
        if (newY != newPosition.y)
        {
            newY -= CameraGhost.position.y;
            if (Mathf.Abs(newY) < .01f)
            {
                newY = 0f;
            }
            newPosition = CameraGhost.position + delta * newY / delta.y;
        }
        SetCameraPos(newPosition);
        return Mathf.Abs(CameraGhost.position.x - TopRight.position.x) < .005f;
    }

    public void ResetPos()
    {
        CameraGhost.position = startPos;
    }

    private void ClampGhost()
    {
        CameraGhost.position = new Vector3(Mathf.Clamp(CameraGhost.position.x, BottomRight.position.x, TopRight.position.x),
            Mathf.Clamp(CameraGhost.position.y, BottomRight.position.y, TopRight.position.y), Mathf.Clamp(CameraGhost.position.z, maxLeft, maxRight));
    }

    private void CalculateMaxPos()
    {
        maxLeft = (BottomRight.position.z - MaxDeltaBottom) - ((BottomRight.position.z - MaxDeltaBottom) - (TopRight.position.z - MaxDeltaTop)) * currentZoomLevel;
        maxRight = BottomRight.position.z - (BottomRight.position.z - TopRight.position.z) * currentZoomLevel;
    }

    private void SetCameraPos(Vector3 pos)
    {
        currentZoomLevel = Mathf.InverseLerp(BottomRight.position.y, TopRight.position.y, pos.y);
        CameraGhost.position = pos;
        CalculateMaxPos();
        ClampGhost();
    }
}

using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class ResetPos : MonoBehaviour
{
    private const float MaxDist = 12000f;
    private const float MaxDistSqr = MaxDist * MaxDist;

    [SerializeField]
    private List<Transform> objectsToReset = null;

    [SerializeField]
    private CinemachineBrain brain = null;

    private Transform cameraTrans;

    private void Awake()
    {
        cameraTrans = transform;
    }

    private void Update()
    {
        ResetAll();
    }

    private void LateUpdate()
    {
        ResetAll();
    }

    private void OnPreCull()
    {
        ResetAll();
    }

    private void OnPreRender()
    {
        ResetAll();
    }

    private void ResetAll()
    {
        return;
        foreach (var obj in objectsToReset)
        {
            obj.position = Vector3.zero;
        }

        CorrectCameraPos(cameraTrans);
        CorrectVCameraPos(brain.ActiveVirtualCamera);
        if (brain.ActiveBlend != null)
        {
            CorrectVCameraPos(brain.ActiveBlend.CamA);
            CorrectVCameraPos(brain.ActiveBlend.CamB);
        }
    }

    private void CorrectVCameraPos(ICinemachineCamera cam)
    {
        if (cam != null && cam.VirtualCameraGameObject != null)
        {
            CorrectCameraPos(cam.VirtualCameraGameObject.transform);
        }
    }

    private void CorrectCameraPos(Transform cameraTrans)
    {
        if (cameraTrans.parent.name == "CameraSlot")
        {
            cameraTrans = cameraTrans.parent;
        }
        float dist = cameraTrans.position.sqrMagnitude;
        if (dist > MaxDistSqr)
        {
            var pos = cameraTrans.position;
            pos /= Mathf.Sqrt(dist);
            pos *= MaxDist;
            cameraTrans.position = pos;
        }
    }
}

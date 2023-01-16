using Cinemachine;
using UnityEngine;

public class CinemachineLogger : MonoBehaviour
{
    [SerializeField]
    private CinemachineBrain brain = null;

    private ICinemachineCamera currentCamera;
    private CinemachineBlend currentBlend;
    private ICinemachineCamera camA;
    private ICinemachineCamera camB;

    private void Start()
    {
        currentCamera = brain.ActiveVirtualCamera;
        currentBlend = brain.ActiveBlend;
        camA = currentBlend?.CamA;
        camB = currentBlend?.CamB;
    }

    private void LateUpdate()
    {
        var newCam = brain.ActiveVirtualCamera;
        if (currentCamera != newCam)
        {
            Debug.Log($"Camera changed: {ToString(currentCamera)} -> {ToString(newCam)}");
            currentCamera = newCam;
        }
        bool blendChanged = brain.ActiveBlend != currentBlend;
        if (blendChanged)
        {
            Debug.Log($"Blend changed");
        }
        newCam = brain.ActiveBlend?.CamA;
        if (blendChanged || camA != newCam)
        {
            Debug.Log($"CamA changed: {ToString(camA)} -> {ToString(newCam)}");
            camA = newCam;
        }
        newCam = brain.ActiveBlend?.CamB;
        if (blendChanged || camB != newCam)
        {
            Debug.Log($"CamB changed: {ToString(camB)} -> {ToString(newCam)}");
            camB = newCam;
        }
    }

    private string ToString(ICinemachineCamera camera)
    {
        if (camera == null)
        {
            return "null camera";
        }
        return $"{camera.Name}, {(camera.VirtualCameraGameObject == null ? "none" : ToString(camera.VirtualCameraGameObject.transform))}";
    }

    private string ToString(Transform trans)
    {
        return $"{trans.name}/{(trans.root == null ? string.Empty : trans.root.name)}";
    }
}

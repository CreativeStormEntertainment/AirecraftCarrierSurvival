using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraBrainTest : MonoBehaviour
{
    private void LateUpdate()
    {
        var brain = CameraManager.Instance.Brain;
        if (brain.ActiveBlend != null)
        {
            string text = brain.ActiveVirtualCamera.VirtualCameraGameObject.name;
            text += ";;;;;A:" + (brain.ActiveBlend.CamA.VirtualCameraGameObject == null ? brain.ActiveBlend.CamA.Description : brain.ActiveBlend.CamA.VirtualCameraGameObject.name) + " B:" + (brain.ActiveBlend.CamB == null ? brain.ActiveBlend.CamB.Description : brain.ActiveBlend.CamB.VirtualCameraGameObject.name);
            text += ";;;;;A:" + brain.ActiveBlend.CamA.State.RawPosition + brain.ActiveBlend.CamA.State.RawOrientation.eulerAngles + "B:" + brain.ActiveBlend.CamB.State.RawPosition + brain.ActiveBlend.CamB.State.RawOrientation.eulerAngles;
            Debug.Log(text + ";;;" + brain.transform.position.ToString("F3") + brain.transform.rotation.eulerAngles.ToString());
        }
    }
}

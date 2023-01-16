using Cinemachine;
using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CamerasIntermission : MonoBehaviour
{
    public static CamerasIntermission Instance;

    public event Action BlendFinished = delegate { };

    public List<CinemachineVirtualCamera> IntermissionCameras;

    [SerializeField]
    private CinemachineBrain brain = null;

    private Coroutine coroutine;

    public int CurrentCameraIndex
    {
        get;
        private set;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void SetIntermissionCameras(int cameraIndex)
    {
        IntermissionCameras[CurrentCameraIndex].gameObject.SetActive(false);
        CurrentCameraIndex = cameraIndex;
        IntermissionCameras[CurrentCameraIndex].gameObject.SetActive(true);

        if (coroutine == null)
        {
            coroutine = this.StartCoroutineActionAfterFrames(CheckBlend, 3);
        }
    }

    private void CheckBlend()
    {
        coroutine = this.StartCoroutineActionAfterPredicate(OnBlendFinished, () => brain.IsBlending);
    }

    private void OnBlendFinished()
    {
        coroutine = null;
        BlendFinished();
    }
}

using System;
using Cinemachine;
using UnityEngine;

public class CameraTimeListener : MonoBehaviour
{
    private float _lastTimeScale;

    private void Awake()
    {
        var timeManager = TimeManager.Instance;
        _lastTimeScale = timeManager.TimeSpeed;

        timeManager.TimeScaleChanged += OnTimeScaleChangedHandler;
    }

    private void OnDestroy()
    {
        TimeManager.Instance.TimeScaleChanged -= OnTimeScaleChangedHandler;
    }

    private void OnTimeScaleChangedHandler()
    {
        var currentTimeScale = TimeManager.Instance.TimeSpeed;
        if (currentTimeScale == 0)
        {
            DeactivateCameraNoise();
        }
        else if (Math.Abs(_lastTimeScale - currentTimeScale) > 0.01)
        {
            ActivateCameraNoise();
        }
        
        _lastTimeScale = currentTimeScale;
    }

    private void ActivateCameraNoise()
    {
        const float amplitude = .25f;
        switch (CameraManager.Instance.CurrentCameraView)
        {
            case ECameraView.Deck:
                var virtualCamera = CameraManager.Instance.DeckView.Camera as CinemachineVirtualCamera;
                if (virtualCamera != null)
                {
                    var perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                    perlin.m_AmplitudeGain = amplitude;
                }

                break;
            case ECameraView.Island:
                ChangePerlinForFreeCamera(CameraManager.Instance.IslandView.FreeCamera, amplitude);
                break;
            case ECameraView.Free:
                ChangePerlinForFreeCamera(CameraManager.Instance.FreeView.FreeCamera, amplitude);
                break;
        }
    }

    private void DeactivateCameraNoise()
    {
        const float amplitude = 0f;
        switch (CameraManager.Instance.CurrentCameraView)
        {
            case ECameraView.Deck:
                var virtualCamera = CameraManager.Instance.DeckView.Camera as CinemachineVirtualCamera;
                if (virtualCamera != null)
                {
                    var perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                    perlin.m_AmplitudeGain = amplitude;
                }

                break;
            case ECameraView.Island:
                ChangePerlinForFreeCamera(CameraManager.Instance.IslandView.FreeCamera, amplitude);
                break;
            case ECameraView.Free:
                ChangePerlinForFreeCamera(CameraManager.Instance.FreeView.FreeCamera, amplitude);
                break;
        }
    }

    void ChangePerlinForFreeCamera(CinemachineFreeLook freeLook, float perlinValue)
    {
        freeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = perlinValue;
        freeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = perlinValue;
        freeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = perlinValue;
    }
}
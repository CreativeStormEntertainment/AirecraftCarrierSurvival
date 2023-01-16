using System;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineFreeLook))]
public class SettingsCinemachineFreeLook : MonoBehaviour
{

    [SerializeField]
    private float _amplitudeGain = .25f;
    
    [SerializeField]
    private float _frequencyGain = .25f;
    
    private CinemachineFreeLook _camera;

    private void Awake()
    {
        _camera = GetComponent<CinemachineFreeLook>();
    }

    private void Start()
    {
        var cinemachineBasicMultiChannelPerlin =
            _camera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = _amplitudeGain;
        cinemachineBasicMultiChannelPerlin.m_FrequencyGain = _frequencyGain;
    }
}

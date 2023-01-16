using System.Collections.Generic;
using UnityEngine;

public class SmokeParticles : MonoBehaviour
{
    [SerializeField]
    private List<ParticleSystem> particleSystems = null;
    [SerializeField]
    private List<SmokeParticlesData> datas = null;
    [SerializeField]
    private float timeToChange = 5f;

    private int currentSpeed;
    private float currentValue;
    private float timer;
    private SmokeParticlesData lastData;
    private SmokeParticlesData currentData;

    private void Update()
    {
        timer += Time.deltaTime;
        var value = timer / timeToChange;
        currentValue = Mathf.Lerp(0f, 1f, value);
        foreach (var particleSystem in particleSystems)
        {
            currentData.Lifetime = Mathf.Lerp(lastData.Lifetime, datas[currentSpeed].Lifetime, currentValue);
            currentData.Force = Vector3.Lerp(lastData.Force, datas[currentSpeed].Force, currentValue);
            var main = particleSystem.main;
            main.startLifetime = currentData.Lifetime;
            var force = particleSystem.forceOverLifetime;
            var forceValue = currentData.Force;
            force.x = forceValue.x;
            force.y = forceValue.y;
            force.z = forceValue.z;
        }
    }

    public void SetParticlesValues(int currentSpeed)
    {
        timer = 0f;
        this.currentSpeed = currentSpeed;
        lastData = currentData;
    }
}

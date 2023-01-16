using System.Collections.Generic;
using UnityEngine;

public class SmokeController : MonoBehaviour
{
    [SerializeField]
    private List<SmokeParticles> smokes = null;

    public void Init()
    {
        HudManager.Instance.ShipSpeedChanged += OnShipSpeedChanged;
    }

    public void AddSmokeMember(List<SmokeParticles> effect)
    {
        foreach (var e in effect)
        {
            smokes.Add(e);
        }
    }

    public void RemoveSmokeMember(List<SmokeParticles> effect)
    {
        foreach (var e in effect)
        {
            smokes.Remove(e);
        }
    }

    private void OnShipSpeedChanged(int speed)
    {
        foreach (var smoke in smokes)
        {
            smoke.SetParticlesValues(speed);
        }
    }
}

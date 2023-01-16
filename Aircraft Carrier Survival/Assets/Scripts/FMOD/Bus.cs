using FMODUnity;
using UnityEngine;

public class Bus : MonoBehaviour
{
    [SerializeField]
    private string busPath = "bus:/";

    FMOD.Studio.Bus bus;

    private void Awake()
    {
        bus = RuntimeManager.GetBus(busPath);
    }

    public void SetMute(bool mute)
    {
        bus.setMute(mute);
    }

    public void SetPause(bool pause)
    {
        bus.setPaused(pause);
    }
}

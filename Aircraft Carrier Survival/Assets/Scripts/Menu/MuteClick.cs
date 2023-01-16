using UnityEngine;

public class MuteClick : MonoBehaviour
{
    [SerializeField]
    private AudioSource source = null;
    [SerializeField]
    private FMODStudioController fmod = null;

    private bool silent;

    public void SetEnable(bool enable)
    {
        silent = !enable;
    }

    public void OnClick()
    {
        if (!silent)
        {
            source.volume = fmod.SFXVolume * fmod.MasterVolume;
            source.Play();
        }
    }
}

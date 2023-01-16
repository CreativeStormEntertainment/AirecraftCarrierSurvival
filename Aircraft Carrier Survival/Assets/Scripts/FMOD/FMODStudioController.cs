using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FMODStudioController : MonoBehaviour
{
    public float MasterVolume
    {
        get => masterVolume;
        set => masterVolume = value;
    }
    public float VoiceVolume
    {
        get => voiceVolume;
        set => voiceVolume = value;
    }
    public float SFXVolume
    {
        get => sfxVolume;
        set => sfxVolume = value;
    }
    public float MusicVolume
    {
        get => musicVolume;
        set => musicVolume = value;
    }
    public bool Muted
    {
        get => muted;
        set => muted = value;
    }

    [SerializeField, Range(0f, 1f)]
    private float masterVolume = 0.1f;
    [SerializeField, Range(0f, 1f)]
    private float voiceVolume = 0.1f;
    [SerializeField, Range(0f, 1f)]
    private float musicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)]
    private float sfxVolume = 0.1f;

    [SerializeField]
    private bool muted = false;

    private float masterVolumePrev;
    private float voiceVolumePrev;
    private float musicVolumePrev;
    private float sfxVolumePrev;
    private bool mutedPrev;

    private FMOD.Studio.Bus masterBus;
    private List<FMOD.Studio.Bus> voiceBuses;
    private FMOD.Studio.Bus musicBus;
    private List<FMOD.Studio.Bus> sfxBuses;

    private float volumeToRestore;
    private bool killVoices;

    private void Awake()
    {
        var data = SaveManager.Instance.PersistentData;
        masterVolume = data.MasterVolume;
        voiceVolume = data.VoiceVolume;
        musicVolume = data.MusicVolume;
        sfxVolume = data.SFXVolume;
        muted = data.AudioMuted;
    }

    private void Start()
    {
        masterBus = RuntimeManager.GetBus("bus:/");
        masterBus.setVolume(masterVolume);

        voiceBuses = new List<FMOD.Studio.Bus>();
        voiceBuses.Add(RuntimeManager.GetBus("bus:/VO"));
        SetVoiceVolume();

        musicBus = RuntimeManager.GetBus("bus:/Music");
        musicBus.setVolume(musicVolume);

        sfxBuses = new List<FMOD.Studio.Bus>();
        sfxBuses.Add(RuntimeManager.GetBus("bus:/Ambient"));
        sfxBuses.Add(RuntimeManager.GetBus("bus:/AmbientsIsland"));
        sfxBuses.Add(RuntimeManager.GetBus("bus:/AmbientsSection"));
        sfxBuses.Add(RuntimeManager.GetBus("bus:/AnimationReport"));
        sfxBuses.Add(RuntimeManager.GetBus("bus:/Animations"));
        sfxBuses.Add(RuntimeManager.GetBus("bus:/Briefs"));
        sfxBuses.Add(RuntimeManager.GetBus("bus:/Sounds"));
        sfxBuses.Add(RuntimeManager.GetBus("bus:/UI"));
        SetSfxVolume();

        //FMODStudio.muted = Muted;
    }

    private void Update()
    {
        if (masterVolume != masterVolumePrev)
        {
            masterBus.setVolume(FMODStudio.GetLogNormalized(masterVolume));
            masterVolumePrev = masterVolume;
        }

        if (voiceVolume != voiceVolumePrev)
        {
            SetVoiceVolume();
        }
        if (musicVolume != musicVolumePrev)
        {
            musicBus.setVolume(FMODStudio.GetLogNormalized(musicVolume));
            musicVolumePrev = musicVolume;
        }
        if (sfxVolume != sfxVolumePrev)
        {
            sfxVolumePrev = sfxVolume;
            SetSfxVolume();
        }
        if (muted != mutedPrev)
        {
            masterBus.setMute(muted);
            mutedPrev = muted;
        }
    }

    private void OnDestroy()
    {
        //masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        foreach (var v in voiceBuses)
        {
            v.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        for (int i = 3; i < sfxBuses.Count; i++)
        {
            sfxBuses[i].stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        if (killVoices)
        {
            RestoreVoices();
        }
    }

    public void KillVoices()
    {
        Assert.IsFalse(killVoices);
        killVoices = true;
        volumeToRestore = VoiceVolume;
        VoiceVolume = 0f;
        SetVoiceVolume();
    }

    public void RestoreVoices()
    {
        Assert.IsTrue(killVoices);
        killVoices = false;
        VoiceVolume = volumeToRestore;
        SetVoiceVolume();
    }

    private void SetVoiceVolume()
    {
        float volume = FMODStudio.GetLogNormalized(voiceVolume);
        for (int i = 0; i < voiceBuses.Count; i++)
        {
            voiceBuses[i].setVolume(volume);
        }
        voiceVolumePrev = voiceVolume;
    }

    private void SetSfxVolume()
    {
        float volume = FMODStudio.GetLogNormalized(sfxVolume);
        for (int i = 0; i < sfxBuses.Count; i++)
        {
            sfxBuses[i].setVolume(volume);
        }
    }
}
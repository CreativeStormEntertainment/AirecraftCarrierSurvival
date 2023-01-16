using System;

[Serializable]
public class PersistentData
{
    public string Lang;
    public float MasterVolume;
    public float VoiceVolume;
    public float SFXVolume;
    public int VideoQuality;
    public int VSyncLevel;
    public float MusicVolume;
    public bool AudioMuted;
    public bool InverseX;
    public bool InverseY;

    public PersistentData Duplicate()
    {
        var result = new PersistentData();

        result.Lang = Lang;
        result.MasterVolume = MasterVolume;
        result.VoiceVolume = VoiceVolume;
        result.SFXVolume = SFXVolume;
        result.VideoQuality = VideoQuality;
        result.VSyncLevel = VSyncLevel;
        result.MusicVolume = MusicVolume;
        result.AudioMuted = AudioMuted;
        result.InverseX = InverseX;
        result.InverseY = InverseY;

        return result;
    }
}
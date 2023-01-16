using System;
using UnityEngine;

[Serializable]
public class NewPersistentData : BaseSaveData
{
    private const string PathExt = @"\data.sav";
    private const int Version = 76;

    private static readonly NewSaveDataWrapper<NewPersistentData> StaticWrapper = new NewSaveDataWrapper<NewPersistentData>();
    private static StartPersistentData StartPersistentData;

    public override string LocalPath => PathExt;

    protected override BaseSaveDataWrapper Wrapper => StaticWrapper;

    protected override int MinSaveVersion => LegacySaveVersion;

    protected override int CurrentSaveVersion => saveVersion;

    protected override int MaxSaveVersion => Version;

    public string Lang;

    [Space(10f)]
    [Range(0f,1f)]
    public float MasterVolume;
    [Range(0f, 1f)]
    public float VoiceVolume;
    [Range(0f, 1f)]
    public float SFXVolume;
    [Range(0f, 1f)]
    public float MusicVolume;
    public bool AudioMuted;

    [Space(10f)]
    [Range(0, 5)]
    public int VideoQuality;
    [Range(0, 1)]
    public int VSyncLevel;
    public int SelectedResolutionIndex;
    public FullScreenMode FullScreen;
    public int AntiAliasing;
    public bool AnisotropicFiltering;
    public int ShadowQuality;

    [Space(10f)]
    public bool InverseX;
    public bool InverseY;

    public InputSaveData InputData;

    [HideInInspector]
    [SerializeField]
    private int saveVersion = LegacySaveVersion;

    public static NewPersistentData GetStartData()
    {
        if (StartPersistentData == null)
        {
            StartPersistentData = Resources.Load<StartPersistentData>("Save/StartPersistentData");
        }
        var data = (NewPersistentData)StartPersistentData.Data.Duplicate();
        data.saveVersion = Version;
        return data;
    }

    public override BaseSaveData Duplicate()
    {
        var result = new NewPersistentData();
        result.saveVersion = saveVersion;

        result.Lang = Lang;

        result.MasterVolume = MasterVolume;
        result.VoiceVolume = VoiceVolume;
        result.SFXVolume = SFXVolume;
        result.MusicVolume = MusicVolume;
        result.AudioMuted = AudioMuted;

        result.VideoQuality = VideoQuality;
        result.VSyncLevel = VSyncLevel;
        result.SelectedResolutionIndex = SelectedResolutionIndex;
        result.FullScreen = FullScreen;
        result.AntiAliasing = AntiAliasing;
        result.AnisotropicFiltering = AnisotropicFiltering;
        result.ShadowQuality = ShadowQuality;

        result.InverseX = InverseX;
        result.InverseY = InverseY;

        result.InputData = InputData;

        return result;
    }

    //protected override void FillStartData()
    //{
    //    saveVersion = Version;

    //    Lang = "en";
    //    MasterVolume = .7f;
    //    VoiceVolume = .5f;
    //    SFXVolume = .5f;
    //    VideoQuality = QualitySettings.GetQualityLevel();
    //    AudioMuted = false;
    //    MusicVolume = .5f;
    //    VSyncLevel = 1;
    //    InverseX = false;
    //    InverseY = false;
    //}

    protected override void RefreshVersion()
    {
        saveVersion = Version;
    }

    protected override void UpgradeSaveFrom(BaseSaveData data)
    {
        var persistentData = (NewPersistentData)data;

        if (CurrentSaveVersion < 76)
        {
            InputData = persistentData.InputData;
        }
    }

    protected override void UpgradeSaveFromLegacy(SaveData legacyData)
    {
        saveVersion = LegacySaveVersion;

        Lang = legacyData.PersistentData.Lang;

        MasterVolume = legacyData.PersistentData.MasterVolume;
        VoiceVolume = legacyData.PersistentData.VoiceVolume;
        SFXVolume = legacyData.PersistentData.SFXVolume;
        MusicVolume = legacyData.PersistentData.MusicVolume;
        AudioMuted = legacyData.PersistentData.AudioMuted;

        VideoQuality = legacyData.PersistentData.VideoQuality;
        VSyncLevel = legacyData.PersistentData.VSyncLevel;

        InverseX = legacyData.PersistentData.InverseX;
        InverseY = legacyData.PersistentData.InverseY;
    }
}

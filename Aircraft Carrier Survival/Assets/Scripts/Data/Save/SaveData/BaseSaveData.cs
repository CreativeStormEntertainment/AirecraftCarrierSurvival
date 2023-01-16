using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public abstract class BaseSaveData
{
    protected const int LegacySaveVersion = 73;

    private static string LegacyPath;
    private static string LegacyTempPath;
    private static readonly SaveDataWrapper LegacyWrapper = new SaveDataWrapper();

    public abstract string LocalPath
    {
        get;
    }

    public string Path
    {
        get;
        protected set;
    }
    public string TempPath
    {
        get;
        protected set;
    }

    protected abstract BaseSaveDataWrapper Wrapper
    {
        get;
    }

    protected abstract int MinSaveVersion
    {
        get;
    }

    protected abstract int CurrentSaveVersion
    {
        get;
    }

    protected abstract int MaxSaveVersion
    {
        get;
    }

    public static void Init()
    {
        LegacyPath = Application.persistentDataPath + @"\mission.sav";
        LegacyTempPath = LegacyPath + ".tmp";
    }

    public static T Load<T>(T data, T startData, string path, bool canLoadFromLegacy) where T : BaseSaveData, new()
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            data.InitPath(path, false);
        }
        Load(data, startData, data.TempPath, out T tempData);
        if (!Load(data, startData, data.Path, out T newData) || (tempData != null && tempData.CurrentSaveVersion > newData.CurrentSaveVersion))
        {
            newData = tempData;
        }
        if (newData is NewSaveData && newData.CurrentSaveVersion != newData.MaxSaveVersion)
        {
            newData = null;
        }
        if (newData == null)
        {
            SaveData legacyData = null;
            canLoadFromLegacy = false;
            if (canLoadFromLegacy)
            {
                legacyData = Load(LegacyTempPath);
                if (legacyData == null)
                {
                    legacyData = Load(LegacyPath);
                }
            }

            if (legacyData == null)
            {
                newData = (T)startData.Duplicate();
            }
            else
            {
                newData = new T();
                newData.UpgradeSaveFromLegacy(legacyData);
                newData.UpgradeSaveFrom(startData);

                try
                {
                    Save(newData);
                    newData.EraseLegacyData();
                }
                catch (Exception ex)
                {
                    Debug.LogError("[SAV1]" + ex.ToString());
                }
            }
        }
        newData.RefreshVersion();

        newData.TempPath = data.TempPath;
        newData.Path = data.Path;
        return newData;
    }

    public static void Save<T>(T data) where T : BaseSaveData
    {
        Assert.IsFalse(string.IsNullOrEmpty(data.Path));
        data.Wrapper.Set(data);
        BinUtils.SaveBinary(data.Wrapper, data.TempPath);
        File.Delete(data.Path);
        File.Copy(data.TempPath, data.Path);
        File.Delete(data.TempPath);
    }

    private static bool Load<T>(T data, T startData, string path, out T newData) where T : BaseSaveData
    {
        Assert.IsFalse(string.IsNullOrEmpty(path));
        if (File.Exists(path))
        {
            try
            {
                var newWrapper = BinUtils.LoadBinary<NewSaveDataWrapper<T>>(path);
                var loadedData = newWrapper.Get();
                if (loadedData != null)
                {
                    data.Wrapper.Set(loadedData);
                    if (newWrapper.Checksum == data.Wrapper.Checksum && loadedData.CurrentSaveVersion >= data.MinSaveVersion)
                    {
                        newData = (T)loadedData;
                        if (newData.CurrentSaveVersion != data.MaxSaveVersion)
                        {
                            newData.UpgradeSaveFrom(startData);
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[SAV2]" + ex.ToString());
            }
        }
        newData = null;
        return false;
    }

    private static SaveData Load(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                var wrapper = BinUtils.LoadBinary<SaveDataWrapper>(path);
                if (wrapper.Data != null)
                {
                    LegacyWrapper.SetData(wrapper.Data);
                    if (LegacyWrapper.Checksum == wrapper.Checksum && LegacyWrapper.Data.SaveVersion == LegacySaveVersion)
                    {
                        return wrapper.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[SAV3]" + ex.ToString());
            }
        }
        return null;
    }

    public void InitPath(string startPath, bool force)
    {
        if (force || Path == null || !Path.StartsWith(startPath))
        {
            Path = $"{startPath}{LocalPath}";
            TempPath = $"{Path}.temp";
        }
    }

    public void InitPath(BaseSaveData data)
    {
        Path = data.Path;
        TempPath = data.TempPath;
    }

    public void EraseLegacyData()
    {
        try
        {
            if (File.Exists(LegacyTempPath))
            {
                File.Delete(LegacyTempPath);
            }
            if (File.Exists(LegacyPath))
            {
                File.Delete(LegacyPath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[SAV4]" + ex.ToString());
        }
    }

    public abstract BaseSaveData Duplicate();
    protected abstract void UpgradeSaveFrom(BaseSaveData data);
    protected abstract void RefreshVersion();
    protected abstract void UpgradeSaveFromLegacy(SaveData legacyData);
}

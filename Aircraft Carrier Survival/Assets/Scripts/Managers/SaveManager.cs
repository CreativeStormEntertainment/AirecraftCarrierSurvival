using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class SaveManager
{
    public static SaveManager Instance => _Instance ?? (_Instance = new SaveManager());
    private static SaveManager _Instance;

    private const string CampaignAutosave = "AutosaveCampaign";
    private const string SandboxAutosave = "AutosaveSandbox";
    private const string QuitAutosave = "AutosaveQuit";
    private const string ExtensionSearchPattern = "*.sav";
    private const string ExtensionTempSearchPattern = ExtensionSearchPattern + ".temp";
    private readonly string startPath;

    public NewSaveData Data
    {
        get;
        private set;
    }
    public NewPersistentData PersistentData
    {
        get;
        private set;
    }
    public TransientData TransientData
    {
        get;
        private set;
    }

    private NewSaveData startSaveData;
    private NewPersistentData startPersistentData;
    private HashSet<string> fileHelper;

    private SaveManager()
    {
        BaseSaveData.Init();

        startSaveData = NewSaveData.GetStartData();
        startPersistentData = NewPersistentData.GetStartData();
        PersistentData = BaseSaveData.Load(new NewPersistentData(), startPersistentData, Application.persistentDataPath, true);

        startPath = $@"{Application.persistentDataPath}\Saves\";
        if (!Directory.Exists(startPath))
        {
            Directory.CreateDirectory(startPath);
        }

        try
        {
            var path = $@"{Application.persistentDataPath}\Profile1.sav";
            if (File.Exists(path))
            {
                File.Copy(path, $"{startPath}NewSave.sav");
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[O-SAV]" + ex.ToString());
        }

        fileHelper = new HashSet<string>();

        TransientData = new TransientData();
        UpdateLastSave();

        LoadingManager.Instance.SceneChanged -= OnSceneChanged;
        LoadingManager.Instance.SceneChanged += OnSceneChanged;

#if UNITY_EDITOR
        if (LoadingManager.Instance.CurrentScene != ESceneType.MainMenu)
        {
            //if (string.IsNullOrWhiteSpace(TransientData.LastSave) || !LoadData(TransientData.LastSave))
            {
                NewSave();
            }
        }
#endif
    }

    public void NewSave()
    {
        Data = (NewSaveData)startSaveData.Duplicate();
    }

    public void SavePersistentData()
    {
        try
        {
            BaseSaveData.Save(PersistentData);
        }
        catch (Exception ex)
        {
            Debug.LogError("[P-SAV]" + ex.ToString());
        }
    }

    public void SaveData()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.Tutorial)
        {
            return;
        }
        switch (Data.GameMode)
        {
            case EGameMode.Fabular:
                SaveData(CampaignAutosave);
                break;
            case EGameMode.Sandbox:
                SaveData(SandboxAutosave);
                break;
        }
    }

    public void QuitSaveData()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.Tutorial)
        {
            return;
        }
        SaveData(QuitAutosave);
    }

    public void SaveData(string name)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(name));
        try
        {
            var save = $"{startPath}{name}";
            Data.InitPath(save, true);
            var path = Data.Path;
            var temp = Data.TempPath;
            Data.FreshSave = false;
            BaseSaveData.Save(Data);
            if (File.Exists(path) || File.Exists(temp))
            {
                TransientData.LastSave = name;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[SAV5]" + ex.ToString());
            if (Data != null && !File.Exists(Data.Path) && !File.Exists(Data.TempPath))
            {
                Data.FreshSave = true;
            }
        }
    }

    public bool LoadData(string name)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(name));
        try
        {
            var data = BaseSaveData.Load(new NewSaveData(), startSaveData, $"{startPath}{name}", false);
            if (data.FreshSave)
            {
                return false;
            }
            Data = data;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("[SAV6]" + ex.ToString());
            return false;
        }
    }

    public void DeleteData(string name)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(name));
        try
        {
            name = $"{startPath}{name}";
            var path = $"{name}.sav";
            var tempPath = $"{path}.temp";
            File.Delete(path);
            File.Delete(tempPath);
        }
        catch (Exception ex)
        {
            Debug.LogError("[SAV7]" + ex.ToString());
        }
    }

    public IEnumerable<SaveMenuData> GetSaves()
    {
        fileHelper.Clear();
        foreach (var file in Directory.GetFiles(startPath, ExtensionSearchPattern))
        {
            long ticks = File.GetLastWriteTime(file).Ticks;

            int index = file.LastIndexOf('\\');
            var filename = file.Substring(index + 1, file.Length - index - ExtensionSearchPattern.Length);
            fileHelper.Add(filename);

            yield return new SaveMenuData(filename, ticks);
        }
        foreach (var file in Directory.GetFiles(startPath, ExtensionTempSearchPattern))
        {
            long ticks = File.GetLastWriteTime(file).Ticks;

            int index = file.LastIndexOf('\\');
            var filename = file.Substring(index + 1, file.Length - index - ExtensionTempSearchPattern.Length);
            if (fileHelper.Add(filename))
            {
                yield return new SaveMenuData(Path.GetFileNameWithoutExtension(file), ticks);
            }
        }
    }

    public void UpdateLastSave()
    {
        long ticks = DateTime.MinValue.Ticks;
        foreach (var pair in GetSaves())
        {
            if (ticks < pair.WriteTime)
            {
                ticks = pair.WriteTime;
                TransientData.LastSave = pair.Name;
            }
        }
    }

    public void LoadLastSave()
    {
        SaveMenuData? data = null;
        foreach (var pair in GetSaves())
        {
            if (!data.HasValue || data.Value.WriteTime < pair.WriteTime)
            {
                data = pair;
            }
        }
        if (!data.HasValue)
        {
            return;
        }
        if (LoadData(data.Value.Name))
        {
            LoadingManager.Instance.ChangeScene(Data);
        }
    }

    private void OnSceneChanged()
    {
        if (LoadingManager.Instance.CurrentScene == ESceneType.MainMenu)
        {
            Data = null;
        }
    }
}

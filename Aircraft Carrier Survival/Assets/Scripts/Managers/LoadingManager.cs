using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class LoadingManager
{
    public static LoadingManager Instance => _Instance ?? (_Instance = new LoadingManager());
    private static LoadingManager _Instance;

    public event Action SceneChanged = delegate { };

    public bool IsGameScene => CurrentScene == ESceneType.CV3 || CurrentScene == ESceneType.CV5 || CurrentScene == ESceneType.CV9;

    public ESceneType CurrentScene
    {
        get => currentScene;
        set
        {
            Assert.IsFalse(value == ESceneType.Loading);

            //Debug.Log(GetType() + ".CurrentScene: " + " " + currentScene + " " + value + " " + Time.timeScale);
            if (value == ESceneType.Game)
            {
                switch (SaveManager.Instance.Data.SelectedAircraftCarrier)
                {
                    case ECarrierType.CV3:
                        value = ESceneType.CV3;
                        break;
                    case ECarrierType.CV5:
                        value = ESceneType.CV5;
                        break;
                    case ECarrierType.CV9:
                        value = ESceneType.CV9;
                        break;
                }
            }
            currentScene = value;
            Time.timeScale = 1f;
            SceneManager.LoadScene(GetSceneName(ESceneType.Loading));
        }
    }
    private ESceneType currentScene = ESceneType.MainMenu;

    public static string GetSceneName(ESceneType type)
    {
        switch (type)
        {
            case ESceneType.CreateAdmiral:
                return "IntermissionCaptainScene";
            case ESceneType.Game:
                Debug.LogError("Use ESceneType CV3, CV5 or CV9");
                return "";
            case ESceneType.Intermission:
                return "Intermission";
            case ESceneType.Loading:
                return "LoadingScene";
            case ESceneType.MainMenu:
                return "Menu";
            case ESceneType.CV3:
                return "MainScene";
            case ESceneType.CV5:
                return "CV5_MainScene";
            case ESceneType.CV9:
                return "CV9_MainScene";
            case ESceneType.PlaceholderBriefing:
                return "PlaceholderBriefing";
        }
        return "";
    }

    private LoadingManager()
    {
#if UNITY_EDITOR
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName)
        {
            case "IntermissionCaptainScene":
                currentScene = ESceneType.CreateAdmiral;
                break;
            case "MainScene":
                currentScene = ESceneType.CV3;
                break;
            case "CV5_MainScene":
                currentScene = ESceneType.CV5;
                break;
            case "CV9_MainScene":
                currentScene = ESceneType.CV9;
                break;
            case "Intermission":
                currentScene = ESceneType.Intermission;
                break;
            case "LoadingScene":
                currentScene = ESceneType.Loading;
                break;
            case "Menu":
                currentScene = ESceneType.MainMenu;
                break;
        }
#endif
    }

    public void ForceReloadCurrentScene()
    {
        Time.timeScale = 1f;
        CurrentScene = currentScene;
    }

    public AsyncOperation LoadAsync()
    {
        return SceneManager.LoadSceneAsync(GetSceneName(CurrentScene));
    }

    public void ChangeScene(NewSaveData data)
    {
        if (data.GameMode == EGameMode.Sandbox)
        {
            CurrentScene = data.SavedInIntermission ? ESceneType.Intermission : ESceneType.Game;
        }
        else
        {
            CurrentScene = data.MissionInProgress.InProgress ? ESceneType.Game : ESceneType.Intermission;
        }
    }

    public void Quit()
    {
        var achievements = Achievements.Instance;
        if (achievements != null)
        {
            achievements.ForceSave(FinalQuit);
        }
        else
        {
            FinalQuit();
        }
    }

    private void FinalQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

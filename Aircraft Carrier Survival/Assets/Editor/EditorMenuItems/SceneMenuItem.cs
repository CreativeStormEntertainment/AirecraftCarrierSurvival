using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneMenuItem : Editor
{
    [MenuItem("Scenes/MainSceneCV3")]
    public static void OpenMainScene()
    {
        OpenScene("Assets/GameplayAssets/Scenes/Production/MainScenes/MainScene");
    }

    [MenuItem("Scenes/MainSceneCV5")]
    public static void OpenCV5MainScene()
    {
        OpenScene("Assets/GameplayAssets/Scenes/Production/MainScenes/CV5_MainScene");
    }

    [MenuItem("Scenes/MainSceneCV9")]
    public static void OpenCV9MainScene()
    {
        OpenScene("Assets/GameplayAssets/Scenes/Production/MainScenes/CV9_MainScene");
    }

    [MenuItem("Scenes/PlaceholderBriefing")]
    public static void OpenPlaceholderBriefing()
    {
        OpenScene("Assets/PlaceholderBriefing");
    }

    [MenuItem("Scenes/Intermission")]
    public static void OpenIntermissionScene()
    {
        OpenScene("Assets/GameplayAssets/Scenes/Production/Intermission");
    }

    [MenuItem("Scenes/IntermissionCaptainScene")]
    public static void OpenIntermissionCaptainScene()
    {
        OpenScene("Assets/GameplayAssets/Scenes/Production/IntermissionCaptainScene");
    }

    [MenuItem("Scenes/LoadingScene")]
    public static void OpenLoadingScene()
    {
        OpenScene("Assets/GameplayAssets/Scenes/Production/LoadingScene");
    }

    [MenuItem("Scenes/Menu")]
    public static void OpenMenuScene()
    {
        OpenScene("Assets/GameplayAssets/Scenes/Production/Menu");
    }

    private static void OpenScene(string path)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(path + ".unity");
        }
    }
}
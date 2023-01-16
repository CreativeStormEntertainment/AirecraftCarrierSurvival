using UnityEngine;

public class GameplayMenu : MonoBehaviour
{
    public void MainMenu()
    {
        var hudMan = HudManager.Instance;
        var saveMan = SaveManager.Instance;
        if (hudMan != null)
        {
            hudMan.KillAmbient();
            
            GameSceneManager.Instance.UpdateSave();
        }
        else
        {
            IntermissionManager.Instance.UpdateSave();
        }

        var loadingMan = LoadingManager.Instance;
        if (loadingMan != null)
        {
            loadingMan.CurrentScene = ESceneType.MainMenu;
        }
        saveMan.QuitSaveData();
    }

    public void BackToPearlHarbor()
    {
        GameStateManager.Instance.BackToPearlHarbour();
    }

    public void BackToGame()
    {
        var intermission = IntermissionManager.Instance;
        if (intermission == null)
        {
            var hudMan = HudManager.Instance;
            if (hudMan != null)
            {
                hudMan.OnSetShowSettings(false);
            }
        }
        else
        {
            intermission.SetShowSettings(false);
        }
    }

    public void QuitGame()
    {
        var saveMan = SaveManager.Instance;
        if (HudManager.Instance != null)
        {
            GameSceneManager.Instance.UpdateSave();
        }
        else
        {
            IntermissionManager.Instance.UpdateSave();
        }
        saveMan.QuitSaveData();
        LoadingManager.Instance.Quit();
    }
}
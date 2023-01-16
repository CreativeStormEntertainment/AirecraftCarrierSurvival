using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MissionSummaryPopup : MonoBehaviour
{
    [SerializeField]
    protected List<MissionSummaryObjective> objectives = null;
    [SerializeField]
    protected SummaryTypeScriptable summaryData = null;
    [SerializeField]
    protected Button restartMissionButton = null;

    [SerializeField]
    private Text missionName = null;
    [SerializeField]
    private GameObject confirmRestartWindow = null;
    [SerializeField]
    private Button confirmRestart = null;
    [SerializeField]
    private Button cancelRestart = null;

    //[SerializeField]
    //private Button debugPearlHarborButton = null;

    private void Awake()
    {
        restartMissionButton.onClick.AddListener(() => confirmRestartWindow.SetActive(true));
        confirmRestart.onClick.AddListener(RestartMission);
        cancelRestart.onClick.AddListener(() => confirmRestartWindow.SetActive(false));
        //#if UNITY_EDITOR
        //        var cheatButton = Instantiate(debugPearlHarborButton, debugPearlHarborButton.GetComponent<RectTransform>().parent);
        //        cheatButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(233f, -263f);
        //        cheatButton.onClick.RemoveAllListeners();
        //        cheatButton.onClick.AddListener(() => 
        //            {
        //                Hide();
        //                var gameStateMan = GameStateManager.Instance;
        //                gameStateMan.NoDefeat();
        //                gameStateMan.Clear();
        //                var hudMan = HudManager.Instance;
        //                hudMan.SetBlockSettings(false);
        //                hudMan.PlayLastSpeed();
        //            });
        //        var cheatText = cheatButton.GetComponentInChildren<Text>(true);
        //        Destroy(cheatText.GetComponent<TextSetter>());
        //        Destroy(cheatText.GetComponent<SwitchFont>());
        //        cheatText.text = "Continue";
        //#endif
    }

    public virtual void Setup(EMissionLoseCause cause)
    {
        missionName.text = LocalizationManager.Instance.GetText(TacticManager.Instance.SOTacticMap.MissionTitleID);
        foreach (var objective in objectives)
        {
            objective.Hide();
        }
        Show();
    }

    public void Back()
    {
        var mode = SaveManager.Instance.Data.GameMode;
        if (mode == EGameMode.Fabular)
        {
            BackToPearlHarbor();
        }
        else if (mode == EGameMode.Sandbox)
        {
            BackToWorldMap();
        }
    }

    public void BackToWorldMap()
    {
        Hide();
        var data = SaveManager.Instance.Data;
        data.SandboxData.CurrentMissionSaveData.MapInstanceInProgress = false;
        SandboxManager.Instance.FireMissionInstanceFinished();
        HudManager.Instance.ShowWorldMap();
        GameStateManager.Instance.Clear();
        TacticManager.Instance.SetBonusConsequenceManeuversAttack(0);
    }

    public void BackToPearlHarbor()
    {
        QuitGameplay();
        if (SaveManager.Instance.Data.ShowCongratulations)
        {
            PlayMissionResultVideo();
        }
        else
        {
            LoadingManager.Instance.CurrentScene = ESceneType.Intermission;
        }
    }

    protected void Show()
    {
        gameObject.SetActive(true);
    }

    protected void Hide()
    {
        gameObject.SetActive(false);
    }

    protected void QuitGameplay()
    {
        HudManager.Instance.KillAmbient();
        GameSceneManager.Instance.SaveContemporary();
    }

    private void LoadMainMenuScene()
    {
        Hide();
        QuitGameplay();

        SaveManager.Instance.QuitSaveData();
        LoadingManager.Instance.CurrentScene = ESceneType.MainMenu;
    }

    private void PlayMissionResultVideo()
    {
        MovieManager.Instance.PlayMissionResultVideo();
        MovieManager.Instance.VideoFinished += OnResultVideoFinished;
        Hide();
    }

    private void OnResultVideoFinished()
    {
        MovieManager.Instance.VideoFinished -= OnResultVideoFinished;
        var saveMan = SaveManager.Instance;
        saveMan.TransientData.ShowCredits = true;
        LoadingManager.Instance.CurrentScene = ESceneType.MainMenu;
    }

    private void RestartMission()
    {
        var saveMan = SaveManager.Instance;
        saveMan.Data.MissionInProgress.InProgress = false;
        LoadingManager.Instance.ForceReloadCurrentScene();
    }
}

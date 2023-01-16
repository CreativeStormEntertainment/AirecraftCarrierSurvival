using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject ChooseDifficulty;
    public GameObject ChooseGameMode;
    public GameObject GradientMainMenu;
    public GameObject GradientMissionChoose;
    public GameObject SettingsGradient;
    public GameObject TutorialAskPopup;
    public GameObject SettingsMenu;
    public GameObject ChooseTutorialMap;
    public GameObject Credits;
    public SaveMenu SaveMenu;

    public Slider MasterSlider;
    public Slider VoiceSlider;
    public Slider SFXSlider;
    public Slider MusicSlider;
    public GameObject ContinueButton;
    public GameObject LoadButton;
    public Toggle AudioMuteToggle;
    public MuteClick AudioMuteToggleSound;
    public Toggle InverseXToggle;
    public ButtonSFX InverseXToggleSound;
    public Toggle InverseYToggle;
    public ButtonSFX InverseYToggleSound;
    public List<DifficultyButton> DifficultyButtons = new List<DifficultyButton>();
    public List<DifficultyButton> GameModeButtons = new List<DifficultyButton>();

    [SerializeField]
    private SetLanguage LanguageSetter = null;

    [SerializeField]
    private KeyCode backToMainWindowKey = KeyCode.Escape;

    [SerializeField]
    private GameObject tutorialPopup = null;

    [SerializeField]
    private List<SOTacticMap> tutorialMaps = null;

    [SerializeField]
    private GraphicSettings graphicSettings = null;

    private int chosenTutorialMap = -1;

    private EMenuWindow CurrentMenuWindow = EMenuWindow.Main;

    private EGameMode eGameMode;
    private bool isTutorial;
    private EDifficulty difficulty;

    private FMODStudioController fMODStudio;

    private void Update()
    {
        if (Input.GetKeyDown(backToMainWindowKey))
        {
            if (CurrentMenuWindow != EMenuWindow.Main)
            {
                BackgroundAudio.Instance.PlayEvent(EButtonState.Back);
            }
            OnBackButton();
        }
    }

    private void Start()
    {
        //load scenarios
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        var sceMan = ScenarioManager.Instance;
        CurrentMenuWindow = EMenuWindow.Main;
        fMODStudio = FindObjectOfType<FMODStudioController>();
        graphicSettings.Init();
        var saveMan = SaveManager.Instance;

        ////todo
        //int lastProfile = -1;
        //long timestamp = 0L;
        //for (int i = 0; i < saveMan.TransientData.ActiveProfiles.Count; i++)
        //{
        //    if (saveMan.TransientData.ActiveProfiles[i])
        //    {
        //        var data = saveMan.GetProfile(i);
        //        if (!data.FinishedGame || data.FinishedMissions.Count < 3 && data.Timestamp > timestamp)
        //        {
        //            lastProfile = i;
        //            timestamp = data.Timestamp;
        //        }
        //    }
        //}
        //saveMan.TransientData.LastProfile = lastProfile;

        saveMan.UpdateLastSave();
        if (string.IsNullOrWhiteSpace(saveMan.TransientData.LastSave))
        {
            ContinueButton.SetActive(false);
            LoadButton.SetActive(false);
        }

        fMODStudio.VoiceVolume = saveMan.PersistentData.VoiceVolume;
        fMODStudio.MasterVolume = saveMan.PersistentData.MasterVolume;
        fMODStudio.SFXVolume = saveMan.PersistentData.SFXVolume;
        fMODStudio.Muted = saveMan.PersistentData.AudioMuted;
        LanguageSetter.ChooseLanguage(saveMan.PersistentData.Lang);

        InverseXToggle.isOn = saveMan.PersistentData.InverseX;
        InverseYToggle.isOn = saveMan.PersistentData.InverseY;

        InverseXToggle.onValueChanged.AddListener(InverseXAxis);
        InverseYToggle.onValueChanged.AddListener(InverseYAxis);

        if (saveMan.TransientData.ShowCredits)
        {
            saveMan.TransientData.ShowCredits = false;
            ShowCredits();
        }
        AudioSetup();
        //SaveMenu.SaveDeleted += OnSaveDeleted;
    }

    private void OnDestroy()
    {
        graphicSettings.OnDestroy();
    }

    //public void OnSaveDeleted()
    //{
    //    var saveMan = SaveManager.Instance;
    //    saveMan.UpdateLastSave();
    //    if (string.IsNullOrWhiteSpace(saveMan.TransientData.LastSave))
    //    {
    //        ContinueButton.SetActive(false);
    //        LoadButton.SetActive(false);
    //    }
    //}

    public void GameModSelection()
    {
        CurrentMenuWindow = EMenuWindow.GameMode;
        ShowMenu();
    }

    public void DifficultyLevelSelection()
    {
        CurrentMenuWindow = EMenuWindow.Difficulty;
        ShowMenu();
    }

    public void SaveSelection()
    {
        CurrentMenuWindow = EMenuWindow.Save;
        ShowMenu();
    }

    public void SettingsSelection()
    {
        CurrentMenuWindow = EMenuWindow.Settings;
        ShowMenu();
    }

    public void LanguageSelection()
    {
        CurrentMenuWindow = EMenuWindow.Language;
        ShowMenu();
    }

    public void ControlsSelection()
    {
        CurrentMenuWindow = EMenuWindow.Controls;
        ShowMenu();
    }

    public void AudioSetup()
    {
        if (fMODStudio)
        {
            AudioMuteToggle.onValueChanged.RemoveAllListeners();
            MasterSlider.onValueChanged.RemoveAllListeners();
            VoiceSlider.onValueChanged.RemoveAllListeners();
            SFXSlider.onValueChanged.RemoveAllListeners();
            MusicSlider.onValueChanged.RemoveAllListeners();
            MasterSlider.value = fMODStudio.MasterVolume;
            VoiceSlider.value = fMODStudio.VoiceVolume;
            SFXSlider.value = fMODStudio.SFXVolume;
            MusicSlider.value = fMODStudio.MusicVolume;

            AudioMuteToggleSound.SetEnable(false);
            AudioMuteToggle.isOn = fMODStudio.Muted;
            AudioMuteToggleSound.SetEnable(true);


            MasterSlider.onValueChanged.AddListener((_) => fMODStudio.MasterVolume = MasterSlider.value);
            VoiceSlider.onValueChanged.AddListener((_) => fMODStudio.VoiceVolume = VoiceSlider.value);
            SFXSlider.onValueChanged.AddListener((_) => fMODStudio.SFXVolume = SFXSlider.value);
            MusicSlider.onValueChanged.AddListener((_) => fMODStudio.MusicVolume = MusicSlider.value);
            AudioMuteToggle.onValueChanged.AddListener((_) => fMODStudio.Muted = AudioMuteToggle.isOn);
        }
    }

    public void HideTutorialAskPopup()
    {
        TutorialAskPopup.SetActive(false);
        CurrentMenuWindow = EMenuWindow.Difficulty;
        for (int i = 0; i < DifficultyButtons.Count; ++i)
        {
            DifficultyButtons[i].Deselect();
        }
        ShowMenu();
    }

    public void StartGame()
    {
        var savMan = SaveManager.Instance;
        savMan.NewSave();
        savMan.Data.GameMode = eGameMode;
        savMan.Data.Difficulty = difficulty;
        savMan.Data.IsTutorial = isTutorial;
        var loadMan = LoadingManager.Instance;
        if (isTutorial)
        {
            savMan.TransientData.FabularTacticMap = tutorialMaps[chosenTutorialMap];
            loadMan.CurrentScene = ESceneType.CV3;
        }
        else
        {
            loadMan.CurrentScene = ESceneType.CreateAdmiral;
        }
    }

    public void ResetAchievements()
    {
        Achievements.Instance.ResetAchievements();
    }

    public void Back()
    {
        CurrentMenuWindow = EMenuWindow.Main;
        ShowMenu();

        var saveMan = SaveManager.Instance;
        var data = saveMan.PersistentData;
        data.SFXVolume = fMODStudio.SFXVolume;
        data.MasterVolume = fMODStudio.MasterVolume;
        data.VoiceVolume = fMODStudio.VoiceVolume;
        data.MusicVolume = fMODStudio.MusicVolume;
        data.AudioMuted = fMODStudio.Muted;
        //data.VideoQuality = QualitySettings.GetQualityLevel();
        //data.VSyncLevel = QualitySettings.vSyncCount;
        saveMan.SavePersistentData();
    }

    public void Exit()
    {
        SaveManager.Instance.SavePersistentData();
        LoadingManager.Instance.Quit();
    }

    public void OnDifficultyButtonClick(int difficulty)
    {
        this.difficulty = (EDifficulty)difficulty;
        CurrentMenuWindow = EMenuWindow.TutorialAsk;
        ShowMenu();
    }

    public void OnGameModeClick(int mode)
    {
        eGameMode = (EGameMode)mode;
        if (eGameMode == EGameMode.Tutorial)
        {
            eGameMode = EGameMode.Fabular;
            CurrentMenuWindow = EMenuWindow.ChooseTutorial;
            ShowMenu();
        }
        else if (eGameMode == EGameMode.Sandbox)
        {
            difficulty = EDifficulty.Medium;
            StartSandbox();
        }
        else
        {
            DifficultyLevelSelection();
        }
    }

    public void ShowCredits()
    {
        CurrentMenuWindow = EMenuWindow.Credits;
        ShowMenu();
    }

    public void OnContinueButtonClicked()
    {
        var saveMan = SaveManager.Instance;
        saveMan.UpdateLastSave();
        if (saveMan.LoadData(saveMan.TransientData.LastSave))
        {
            LoadingManager.Instance.ChangeScene(saveMan.Data);
        }
    }

    public void OnBackButton()
    {
        switch (CurrentMenuWindow)
        {
            case EMenuWindow.Main:
                break;

            case EMenuWindow.Settings:
            case EMenuWindow.GameMode:
            case EMenuWindow.Save:
                CurrentMenuWindow = EMenuWindow.Main;
                Back();
                break;

            case EMenuWindow.Audio:
            case EMenuWindow.Controls:
            case EMenuWindow.Graphic:
            case EMenuWindow.Language:
                CurrentMenuWindow = EMenuWindow.Settings;
                ShowMenu();
                break;

            case EMenuWindow.ChooseTutorial:
            case EMenuWindow.Difficulty:
                CurrentMenuWindow = EMenuWindow.GameMode;
                ShowMenu();
                break;
        }
    }

    public void InverseXAxis(bool inverse)
    {
        SaveManager.Instance.PersistentData.InverseX = inverse;
    }

    public void InverseYAxis(bool inverse)
    {
        SaveManager.Instance.PersistentData.InverseY = inverse;
    }

    private void ShowMenu()
    {
        GradientMainMenu.SetActive(true);
        MainMenu.SetActive(false);
        ChooseDifficulty.SetActive(false);
        ChooseGameMode.SetActive(false);
        GradientMissionChoose.SetActive(false);
        SettingsGradient.SetActive(false);
        TutorialAskPopup.SetActive(false);
        SettingsMenu.SetActive(false);
        ChooseTutorialMap.SetActive(false);
        Credits.SetActive(false);
        SaveMenu.SetShow(false);

        switch (CurrentMenuWindow)
        {
            case EMenuWindow.Main:
                MainMenu.SetActive(true);
                break;
            case EMenuWindow.Settings:
                SettingsMenu.SetActive(true);
                SettingsGradient.SetActive(true);
                break;
            case EMenuWindow.GameMode:
                ChooseGameMode.SetActive(true);
                GradientMissionChoose.SetActive(true);
                GradientMainMenu.SetActive(false);
                break;
            case EMenuWindow.Difficulty:
                ChooseDifficulty.SetActive(true);
                GradientMissionChoose.SetActive(true);
                GradientMainMenu.SetActive(false);
                break;
            case EMenuWindow.TutorialAsk:
                tutorialPopup.SetActive(true);
                break;
            case EMenuWindow.ChooseTutorial:
                ChooseTutorialMap.SetActive(true);
                GradientMissionChoose.SetActive(true);
                GradientMainMenu.SetActive(false);
                break;
            case EMenuWindow.Save:
                SaveMenu.SetShow(true, false);
                SettingsGradient.SetActive(true);
                break;
            case EMenuWindow.Credits:
                Credits.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void OnTutorialAskButtonClicked(bool tutorial)
    {
        isTutorial = false;
        StartGame();
        SaveManager.Instance.Data.FinishedTutorial = !tutorial;
    }

    public void ChoseTutorialMap(int index)
    {
        chosenTutorialMap = index;
        isTutorial = true;
        StartGame();
    }

    private void StartSandbox()
    {
        isTutorial = false;
        StartGame();
    }
}
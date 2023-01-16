using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour, IEnableable
{
    public GameplayMenu GameplayMenu => gameplayMenu;
    public bool ClosePopupsDisabled
    {
        get;
        private set;
    }

    [SerializeField]
    private GameObject menu = null;
    [SerializeField]
    private GameObject controlsMenu = null;
    [SerializeField]
    private GameObject audioMenu = null;
    [SerializeField]
    private GameObject graphicsMenu = null;
    [SerializeField]
    private GameObject cameraMenu = null;

    [SerializeField]
    private Button cancelButton = null;
    [SerializeField]
    private Button continueButton = null;
    [SerializeField]
    private Button saveButton = null;
    [SerializeField]
    private Button loadButton = null;
    [SerializeField]
    private Button audioButton = null;
    [SerializeField]
    private Button graphicsButton = null;
    [SerializeField]
    private Button controlsButton = null;
    [SerializeField]
    private Button cameraButton = null;
    [SerializeField]
    private Button phButton = null;
    [SerializeField]
    private Button mainMenuButton = null;
    [SerializeField]
    private Button quitButton = null;
    [SerializeField]
    private Button backButton = null;
    [SerializeField]
    private Button backButton2 = null;
    [SerializeField]
    private Button backButton3 = null;

    [SerializeField]
    private GameplayMenu gameplayMenu = null;
    [SerializeField]
    private SaveMenu saveMenu = null;

    [SerializeField]
    private Toggle inverseXToggle = null;
    [SerializeField]
    private Toggle inverseYToggle = null;

    [SerializeField]
    private FMODStudioController fMODStudio = null;

    [SerializeField]
    private Toggle audioMuteToggle = null;
    [SerializeField]
    private Slider masterSlider = null;
    [SerializeField]
    private Slider voiceSlider = null;
    [SerializeField]
    private Slider sfxSlider = null;
    [SerializeField]
    private Slider musicSlider = null;

    [SerializeField]
    private string settingsTitle = "Settings";
    [SerializeField]
    private string audioTitle = "Audio";
    [SerializeField]
    private string controlsTitle = "CameraControl";

    [SerializeField]
    private MuteClick audioMuteToggleSound = null;

    [SerializeField]
    private ConfirmationWindow confirmationWindow = null;

    private EMenuWindow current;

    private void Awake()
    {
        cancelButton.onClick.AddListener(gameplayMenu.BackToGame);
        continueButton.onClick.AddListener(gameplayMenu.BackToGame);
        saveButton.onClick.AddListener(() =>
            {
                SetShow(EMenuWindow.Save);
                saveMenu.SetShow(true, true);
            });
        loadButton.onClick.AddListener(() =>
            {
                SetShow(EMenuWindow.Save);
                saveMenu.SetShow(true, false);
            });
        audioButton.onClick.AddListener(() => SetShow(EMenuWindow.Audio));
        graphicsButton.onClick.AddListener(() => SetShow(EMenuWindow.Graphic));
        controlsButton.onClick.AddListener(() => SetShow(EMenuWindow.Controls));
        cameraButton.onClick.AddListener(() => SetShow(EMenuWindow.Camera));
        phButton.onClick.AddListener(() => confirmationWindow.Show(() => gameplayMenu.BackToPearlHarbor()));
        mainMenuButton.onClick.AddListener(() => confirmationWindow.Show(() => gameplayMenu.MainMenu()));
        quitButton.onClick.AddListener(() => confirmationWindow.Show(() => gameplayMenu.QuitGame()));

        backButton.onClick.AddListener(() => SetShow(EMenuWindow.Main));
        backButton2.onClick.AddListener(() => SetShow(EMenuWindow.Main));
        backButton3.onClick.AddListener(() => SetShow(EMenuWindow.Main));

        var persistentData = SaveManager.Instance.PersistentData;
        inverseXToggle.isOn = persistentData.InverseX;
        inverseXToggle.onValueChanged.AddListener(InverseXAxis);

        inverseYToggle.isOn = persistentData.InverseY;
        inverseYToggle.onValueChanged.AddListener(InverseYAxis);
        AudioSelection();

        var locMan = LocalizationManager.Instance;
        settingsTitle = locMan.GetText(settingsTitle);
        audioTitle = locMan.GetText(audioTitle);
        controlsTitle = locMan.GetText(controlsTitle);

        saveMenu.WantsToClose += () => SetShow(EMenuWindow.Main);
    }

    public void Setup(EGameMode mode)
    {
        if (mode == EGameMode.Sandbox)
        {
            phButton.gameObject.SetActive(false);
        }
        else if (GameStateManager.Instance.Tutorial)
        {
            phButton.gameObject.SetActive(false);
            saveButton.gameObject.SetActive(false);
            loadButton.gameObject.SetActive(false);
        }
    }

    public void SetEnable(bool enable)
    {
        ClosePopupsDisabled = !enable;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetShow(EMenuWindow.Audio);
    }

    public bool Hide()
    {
        gameObject.SetActive(false);
        SetShow(EMenuWindow.None);

        var saveMan = SaveManager.Instance;
        var data = saveMan.PersistentData;
        data.SFXVolume = fMODStudio.SFXVolume;
        data.MasterVolume = fMODStudio.MasterVolume;
        data.VoiceVolume = fMODStudio.VoiceVolume;
        data.MusicVolume = fMODStudio.MusicVolume;
        data.AudioMuted = fMODStudio.Muted;
        saveMan.SavePersistentData();

        return true;
    }

    private void SetShow(EMenuWindow window)
    {
        current = window;
        //menu.SetActive(window == EMenuWindow.Main);
        controlsMenu.SetActive(window == EMenuWindow.Controls);
        audioMenu.SetActive(window == EMenuWindow.Audio);
        graphicsMenu.SetActive(window == EMenuWindow.Graphic);
        cameraMenu.SetActive(window == EMenuWindow.Camera);
        if (window != EMenuWindow.Save && saveMenu.Show())
        {
            saveMenu.ForceHide();
        }

        //switch (window)
        //{
        //    case EMenuWindow.Main:
        //        titleText.text = settingsTitle;
        //        break;
        //    case EMenuWindow.Audio:
        //        titleText.text = audioTitle;
        //        break;
        //    case EMenuWindow.Controls:
        //        titleText.text = controlsTitle;
        //        break;
        //}
    }

    private void AudioSelection()
    {
        if (fMODStudio)
        {
            audioMuteToggle.onValueChanged.RemoveAllListeners();
            masterSlider.onValueChanged.RemoveAllListeners();
            voiceSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.RemoveAllListeners();
            masterSlider.value = fMODStudio.MasterVolume;
            voiceSlider.value = fMODStudio.VoiceVolume;
            sfxSlider.value = fMODStudio.SFXVolume;
            musicSlider.value = fMODStudio.MusicVolume;

            audioMuteToggleSound.SetEnable(false);
            audioMuteToggle.isOn = fMODStudio.Muted;
            audioMuteToggleSound.SetEnable(true);

            masterSlider.onValueChanged.AddListener((_) => fMODStudio.MasterVolume = masterSlider.value);
            voiceSlider.onValueChanged.AddListener((_) => fMODStudio.VoiceVolume = voiceSlider.value);
            sfxSlider.onValueChanged.AddListener((_) => fMODStudio.SFXVolume = sfxSlider.value);
            musicSlider.onValueChanged.AddListener((_) => fMODStudio.MusicVolume = musicSlider.value);
            audioMuteToggle.onValueChanged.AddListener((_) => fMODStudio.Muted = audioMuteToggle.isOn);

            masterSlider.onValueChanged.AddListener((_) => SaveManager.Instance.PersistentData.MasterVolume = masterSlider.value);
            voiceSlider.onValueChanged.AddListener((_) => SaveManager.Instance.PersistentData.VoiceVolume = voiceSlider.value);
            sfxSlider.onValueChanged.AddListener((_) => SaveManager.Instance.PersistentData.SFXVolume = sfxSlider.value);
            musicSlider.onValueChanged.AddListener((_) => SaveManager.Instance.PersistentData.MusicVolume = musicSlider.value);
            audioMuteToggle.onValueChanged.AddListener((_) => SaveManager.Instance.PersistentData.AudioMuted = audioMuteToggle.isOn);
        }
    }

    private void InverseXAxis(bool inverse)
    {
        SaveManager.Instance.PersistentData.InverseX = inverse;
    }

    private void InverseYAxis(bool inverse)
    {
        SaveManager.Instance.PersistentData.InverseY = inverse;
    }
}

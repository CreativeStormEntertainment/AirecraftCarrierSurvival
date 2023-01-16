using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicSettings : MonoBehaviour
{
    [SerializeField]
    private Toggle qualityVsync = null;
    [SerializeField]
    private Slider qualitySlider = null;
    [SerializeField]
    private Text qualityName = null;

    [SerializeField]
    private Button fullscreenLeft = null;
    [SerializeField]
    private Button fullscreenRight = null;
    [SerializeField]
    private Text fullscreenText = null;

    [SerializeField]
    private Button resolutionLeft = null;
    [SerializeField]
    private Button resolutionRight = null;
    [SerializeField]
    private Text resolutionText = null;

    [SerializeField]
    private Button antiAliasingLeft = null;
    [SerializeField]
    private Button antiAliasingRight = null;
    [SerializeField]
    private Text antiAliasingText = null;

    [SerializeField]
    private Toggle anisotropicFiltering = null;

    [SerializeField]
    private Button shadowLeft = null;
    [SerializeField]
    private Button shadowRight = null;
    [SerializeField]
    private Text shadowText = null;

    [SerializeField]
    private Button apply = null;
    [SerializeField]
    private Button setDefaults = null;

    [SerializeField]
    private StartPersistentData basicPersistentData = null;

    [SerializeField]
    private ResolutionChangeConfirmWindow resolutionWindow = null;

    private List<Resolution> possibleResolutions = new List<Resolution>();
    private List<string> antiAliasingLevels = new List<string>() { "NoAntiAliasing", "FastAntiAliasing", "SubpixelAntiAliasing" };
    private List<string> shadowLevels = new List<string>() { "Low", "Medium", "High", "VeryHigh" };
    private List<string> fullscreenMode = new List<string>() { "GraphicsDisplayModeFullscreen", "GraphicsDisplayModeBorderless", "GraphicsDisplayModeMaximizedWindow", "GraphicsDisplayModeWindowed" };

    private int selectedResolutionIndex;
    private int selectedAntiAliasingIndex;
    private int selectedShadowIndex;
    private int selectedFullscreenIndex;

    private bool inited;

    private void Awake()
    {
        Init();
    }

    public void OnDestroy()
    {
        LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
    }

    public void OnEnable()
    {
        SetData(false, false);
    }

    public void SetData(bool defaultSetting, bool apply)
    {
        var persistentData = defaultSetting ? basicPersistentData.Data : SaveManager.Instance.PersistentData;
        ChangeQualitySettings(persistentData.VideoQuality, true);

        qualityVsync.SetIsOnWithoutNotify(persistentData.VSyncLevel == 1 ? true : false);

        qualitySlider.maxValue = (float)QualitySettings.names.Length - 1;

        selectedFullscreenIndex = (int)persistentData.FullScreen;
        selectedFullscreenIndex = Mathf.Min(selectedFullscreenIndex, 3);
        selectedFullscreenIndex = Mathf.Max(selectedFullscreenIndex, 0);
        fullscreenText.text = LocalizationManager.Instance.GetText(fullscreenMode[selectedFullscreenIndex]);

        selectedResolutionIndex = persistentData.SelectedResolutionIndex;
        if (selectedResolutionIndex == -1 || possibleResolutions.Count <= selectedResolutionIndex)
        {
            selectedResolutionIndex = possibleResolutions.Count - 1;
            //Screen.SetResolution(possibleResolutions[selectedResolutionIndex].width, possibleResolutions[selectedResolutionIndex].height, persistentData.FullScreen);
        }
        else
        {
            //Screen.SetResolution(possibleResolutions[selectedResolutionIndex].width, possibleResolutions[selectedResolutionIndex].height, persistentData.FullScreen);
        }
        var native = selectedResolutionIndex == possibleResolutions.Count - 1 ? " " + LocalizationManager.Instance.GetText("GraphicsResolutionNative") : "";
        resolutionText.text = possibleResolutions[selectedResolutionIndex].width.ToString() + " x " + possibleResolutions[selectedResolutionIndex].height.ToString() + " " + possibleResolutions[selectedResolutionIndex].refreshRate.ToString() + "Hz"
            + native;

        selectedAntiAliasingIndex = persistentData.AntiAliasing;
        antiAliasingText.text = LocalizationManager.Instance.GetText(antiAliasingLevels[selectedAntiAliasingIndex]);

        anisotropicFiltering.SetIsOnWithoutNotify(persistentData.AnisotropicFiltering);

        selectedShadowIndex = persistentData.ShadowQuality;
        shadowText.text = LocalizationManager.Instance.GetText(shadowLevels[selectedShadowIndex]);
        if (apply)
        {
            ApplySettings(true);
        }
    }

    public void Init()
    {
        if (!inited)
        {
            inited = true;
            apply.onClick.AddListener(() => ApplySettings(false));
            setDefaults.onClick.AddListener(() => SetData(true, true));
            foreach (var res in Screen.resolutions)
            {
                if ((float)res.width / res.height > 1.7f && res.refreshRate > 40f)
                {
                    possibleResolutions.Add(res);
                }
            }

            qualityVsync.onValueChanged.AddListener((x) =>
            {
                apply.interactable = true;
            });

            qualitySlider.onValueChanged.AddListener((_) =>
            {
                ChangeQualitySettings(Mathf.FloorToInt(qualitySlider.value), false);
            });

            fullscreenLeft.onClick.AddListener(() => SelectFullscreenMode(true));
            fullscreenRight.onClick.AddListener(() => SelectFullscreenMode(false));

            resolutionLeft.onClick.AddListener(() => SelectResolution(true));
            resolutionRight.onClick.AddListener(() => SelectResolution(false));

            antiAliasingLeft.onClick.AddListener(() => SelectAntiAliasing(true));
            antiAliasingRight.onClick.AddListener(() => SelectAntiAliasing(false));

            anisotropicFiltering.onValueChanged.AddListener((x) =>
            {
                apply.interactable = true;
            });

            shadowLeft.onClick.AddListener(() => SelectShadow(true));
            shadowRight.onClick.AddListener(() => SelectShadow(false));

            SetData(false, true);
            LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
        }
    }

    public void SelectFullscreenMode(bool left)
    {
        if (selectedFullscreenIndex == 1)
        {
            selectedFullscreenIndex = 3;
        }
        else
        {
            selectedFullscreenIndex = 1;
        }
        //if (left)
        //{
        //    if (selectedFullscreenIndex > 0)
        //    {
        //        selectedFullscreenIndex--;
        //    }
        //    else
        //    {
        //        selectedFullscreenIndex = fullscreenMode.Count - 1;
        //    }
        //}
        //else
        //{
        //    if (selectedFullscreenIndex < fullscreenMode.Count - 1)
        //    {
        //        selectedFullscreenIndex++;
        //    }
        //    else
        //    {
        //        selectedFullscreenIndex = 0;
        //    }
        //}
        fullscreenText.text = LocalizationManager.Instance.GetText(fullscreenMode[selectedFullscreenIndex]);
        apply.interactable = true;
    }

    public void SelectResolution(bool left)
    {
        if (left)
        {
            if (selectedResolutionIndex > 0)
            {
                selectedResolutionIndex--;
            }
            else
            {
                selectedResolutionIndex = possibleResolutions.Count - 1;
            }
        }
        else
        {
            if (selectedResolutionIndex < possibleResolutions.Count - 1)
            {
                selectedResolutionIndex++;
            }
            else
            {
                selectedResolutionIndex = 0;
            }
        }
        var native = selectedResolutionIndex == possibleResolutions.Count - 1 ? " " + LocalizationManager.Instance.GetText("GraphicsResolutionNative") : "";
        resolutionText.text = possibleResolutions[selectedResolutionIndex].width.ToString() + " x " + possibleResolutions[selectedResolutionIndex].height.ToString() + " " + possibleResolutions[selectedResolutionIndex].refreshRate.ToString() + "Hz"
            + native;
        apply.interactable = true;

    }

    public void SelectAntiAliasing(bool left)
    {
        if (left)
        {
            if (selectedAntiAliasingIndex > 0)
            {
                selectedAntiAliasingIndex--;
            }
            else
            {
                selectedAntiAliasingIndex = antiAliasingLevels.Count - 1;
            }
        }
        else
        {
            if (selectedAntiAliasingIndex < antiAliasingLevels.Count - 1)
            {
                selectedAntiAliasingIndex++;
            }
            else
            {
                selectedAntiAliasingIndex = 0;
            }
        }
        antiAliasingText.text = LocalizationManager.Instance.GetText(antiAliasingLevels[selectedAntiAliasingIndex]);
        apply.interactable = true;
    }

    public void SelectShadow(bool left)
    {
        if (left)
        {
            if (selectedShadowIndex > 0)
            {
                selectedShadowIndex--;
            }
            else
            {
                selectedShadowIndex = shadowLevels.Count - 1;
            }
        }
        else
        {
            if (selectedShadowIndex < shadowLevels.Count - 1)
            {
                selectedShadowIndex++;
            }
            else
            {
                selectedShadowIndex = 0;
            }
        }
        shadowText.text = LocalizationManager.Instance.GetText(shadowLevels[selectedShadowIndex]);
        apply.interactable = true;
    }

    public void ChangeQualitySettings(int val, bool changeSliderValue)
    {
        if (changeSliderValue)
        {
            qualitySlider.SetValueWithoutNotify(val);
        }
        else
        {
            apply.interactable = true;
        }
        var locMan = LocalizationManager.Instance;
        switch (val)
        {
            case 0:
                qualityName.text = locMan.GetText("FastestQuality");
                break;

            case 1:
                qualityName.text = locMan.GetText("FastQuality");
                break;

            case 2:
                qualityName.text = locMan.GetText("SimpleQuality");
                break;

            case 3:
                qualityName.text = locMan.GetText("GoodQuality");
                break;

            case 4:
                qualityName.text = locMan.GetText("BeautifulQuality");
                break;

            case 5:
                qualityName.text = locMan.GetText("FantasticQuality");
                break;
        }
    }

    public void ApplySettings(bool reset)
    {
        QualitySettings.vSyncCount = qualityVsync.isOn ? 1 : 0;
        QualitySettings.SetQualityLevel(Mathf.FloorToInt(qualitySlider.value));
        Screen.SetResolution(possibleResolutions[selectedResolutionIndex].width, possibleResolutions[selectedResolutionIndex].height, (FullScreenMode)selectedFullscreenIndex, possibleResolutions[selectedResolutionIndex].refreshRate);
        SaveManager.Instance.PersistentData.AntiAliasing = selectedAntiAliasingIndex;
        QualitySettings.anisotropicFiltering = anisotropicFiltering.isOn ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
        QualitySettings.shadowResolution = (ShadowResolution)selectedShadowIndex;
        var persistentData = SaveManager.Instance.PersistentData;
        if (persistentData.SelectedResolutionIndex != selectedResolutionIndex)
        {
            if (!reset && resolutionWindow != null)
            {
                resolutionWindow.Setup(RevertResolutionChange, ConfirmResolutionChange);
            }
            else
            {
                ConfirmResolutionChange();
            }
        }
        Save();
        apply.interactable = false;
    }

    private void Save()
    {
        var persistantData = SaveManager.Instance.PersistentData;
        persistantData.VSyncLevel = QualitySettings.vSyncCount;
        persistantData.VideoQuality = QualitySettings.GetQualityLevel();
        persistantData.AntiAliasing = selectedAntiAliasingIndex;
        persistantData.AnisotropicFiltering = anisotropicFiltering.isOn;
        persistantData.ShadowQuality = selectedShadowIndex;
        persistantData.FullScreen = (FullScreenMode)selectedFullscreenIndex;
    }

    private void RevertResolutionChange()
    {
        SetData(false, true);
    }

    private void ConfirmResolutionChange()
    {
        SaveManager.Instance.PersistentData.SelectedResolutionIndex = selectedResolutionIndex;
    }

    private void OnLanguageChanged()
    {
        ChangeQualitySettings(Mathf.FloorToInt(qualitySlider.value), true);
        var native = selectedResolutionIndex == possibleResolutions.Count - 1 ? " " + LocalizationManager.Instance.GetText("GraphicsResolutionNative") : "";
        resolutionText.text = possibleResolutions[selectedResolutionIndex].width.ToString() + " x " + possibleResolutions[selectedResolutionIndex].height.ToString() + " " + possibleResolutions[selectedResolutionIndex].refreshRate.ToString() + "Hz"
            + native;
        antiAliasingText.text = LocalizationManager.Instance.GetText(antiAliasingLevels[selectedAntiAliasingIndex]);
        shadowText.text = LocalizationManager.Instance.GetText(shadowLevels[selectedShadowIndex]);
    }
}

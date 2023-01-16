using FMODUnity;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IntermissionManager : MonoBehaviour, IEscapeablePopup
{
    public event Action<ECarrierType> CarrierBought = delegate { };
    public event Action<int> UsedUpgradePoints = delegate { };

    public static IntermissionManager Instance;
    public bool IsSettingsOpen => settingsPanel.gameObject.activeSelf;
    public SandboxMainGoalSummary SandboxMainGoalSummary => sandboxMainGoalSummary;

    public EIntermissionCategory CurrentPanel
    {
        get => currentPanel;
        set
        {
            if (currentPanel != value)
            {
                if (panelsDict.TryGetValue(currentPanel, out var panel))
                {
                    panel.SetActive(false);
                }
                if (upgradeData.UpgradeAction != null)
                {
                    popup.Hide(false);
                }
                currentPanel = value;
                if (value == EIntermissionCategory.Map)
                {
                    OnBlendFinished();
                }
                else
                {
                    BackgroundAudio.Instance.PlayEvent(EMainSceneUI.IntermissionCameraBlend);
                    CamerasIntermission.Instance.SetIntermissionCameras((int)value);
                }
                if (currentPanel == EIntermissionCategory.Escort)
                {
                    (panelsDict[currentPanel].Panel as EscortPanel).SetShowWarnings(false);
                }
                launchButton.gameObject.SetActive(currentPanel != EIntermissionCategory.Map);
            }
        }
    }

    public ECarrierType CurrentCarrier
    {
        get => carrier;
        set
        {
            carrier = value;
            Refresh();
        }
    }

    public int CurrentCommandPoints
    {
        get => commandPoints;
        private set
        {
            commandPoints = value;
            Refresh();
        }
    }

    public int CurrentUpgradePoints
    {
        get => upgradePoints;
        private set
        {
            upgradePoints = value;
            Refresh();
        }
    }

    public List<IEscapeablePopup> EscapeablePopups
    {
        get;
        private set;
    } = new List<IEscapeablePopup>();

    [SerializeField]
    private bool forceGameMode = false;

    [SerializeField]
    private EGameMode gameMode = EGameMode.Fabular;

    [Header("Settings")]
    [SerializeField]
    private KeyCode settingsToggle = KeyCode.Escape;

    [SerializeField]
    private Settings settingsPanel = null;

    [SerializeField]
    private Button settingsBtn = null;
    [SerializeField]
    private GameObject blocker = null;

    [SerializeField]
    private List<PanelData> panels = null;

    [SerializeField]
    private UpgradePopup popup = null;

    [SerializeField]
    private CampaignMapPanel mapPanel = null;

    [SerializeField]
    private SandboxLaunchPanel sandboxLaunchPanel = null;

    [SerializeField]
    private Text commandPointsText = null;
    [SerializeField]
    private Text upgradePointsText = null;
    [SerializeField]
    private LaunchButton launchButton = null;

    [SerializeField]
    private SandboxMainGoalSummary sandboxMainGoalSummary = null;

    [SerializeField]
    private GameObject congratulationsWindow = null;
    [SerializeField]
    private Button congratulationsButton = null;

    [SerializeField]
    private IntermissionTutorial tutorial = null;

    [SerializeField]
    private StudioEventEmitter upgradeEmmiter = null;

    private Dictionary<EIntermissionCategory, PanelData> panelsDict;
    private ECarrierType carrier;
    private EIntermissionCategory currentPanel;
    private int commandPoints;
    private int upgradePoints;

    private UpgradePopupData upgradeData;

    private void Awake()
    {
        Instance = this;

#if UNITY_EDITOR
        if (forceGameMode)
        {
            SaveManager.Instance.Data.GameMode = gameMode;
        }
#endif
        settingsBtn.onClick.AddListener(() => SetShowSettings(!IsSettingsOpen));

        panelsDict = new Dictionary<EIntermissionCategory, PanelData>();
        foreach (var data in panels)
        {
            panelsDict.Add(data.Category, data);
            data.PanelButton.onClick.AddListener(() => CurrentPanel = data.Category);
        }
        if (SaveManager.Instance.Data.GameMode == EGameMode.Sandbox)
        {
            panelsDict[EIntermissionCategory.Map].Panel = sandboxLaunchPanel;
        }
        popup.PopupClosed += OnPopupClosed;
        CamerasIntermission.Instance.BlendFinished += OnBlendFinished;
    }

    private void Start()
    {
        var data = SaveManager.Instance.Data;
        UpdatePoints();

        carrier = data.SelectedAircraftCarrier;
        foreach (var panelData in panelsDict.Values)
        {
            panelData.Panel.Setup(data);
        }

        mapPanel.LaunchGame += LaunchGame;
        sandboxLaunchPanel.LaunchGame += LaunchGame;

        currentPanel = EIntermissionCategory.Aircraft;
        CurrentPanel = EIntermissionCategory.Dock;
        OnBlendFinished();

        var achievements = Achievements.Instance;
        if (achievements != null)
        {
            achievements.SetupIntermission();
        }

        var saveData = SaveManager.Instance.Data;
        if (saveData.GameMode == EGameMode.Sandbox && saveData.SandboxData.MainGoal.Type != EMainGoalType.None && !saveData.SavedInIntermission)
        {
            sandboxMainGoalSummary.Setup();
        }

        BasicInput.Instance.Enable();
        var save = BasicInput.Instance.QuickSave;
        save.QuickSave.performed += QuickSaveCallback1;
        save.QuickLoad.performed += QuickSaveCallback2;
        var saveMan = SaveManager.Instance;

        //if (saveData.ShowCongratulations)
        //{
        //    congratulationsWindow.SetActive(true);
        //    congratulationsButton.onClick.AddListener(() =>
        //    {
        //        saveMan.NewSave();
        //        File.Delete(saveMan.TransientData.LastSave);
        //        saveMan.UpdateLastSave();
        //        saveMan.TransientData.ShowCredits = true;

        //        settingsPanel.GameplayMenu.MainMenu();
        //    });
        //    return;
        //}

        saveData.SavedInIntermission = true;
        saveData.MissionInProgress.InProgress = false;
        saveMan.SaveData();
    }

    private void Update()
    {
        if (Input.GetKeyDown(settingsToggle))
        {
            HideEscapeable();
        }
    }

    private void OnDestroy()
    {
        var save = BasicInput.Instance.QuickSave;
        save.QuickSave.performed -= QuickSaveCallback1;
        save.QuickLoad.performed -= QuickSaveCallback2;
    }

    private void HideEscapeable()
    {
        if (EscapeablePopups.Count > 0)
        {
            var popup = EscapeablePopups[EscapeablePopups.Count - 1];
            EscapeablePopups.RemoveAt(EscapeablePopups.Count - 1);
            if (!popup.OnEscape())
            {
                HideEscapeable();
            }
        }
        else
        {
            SetShowSettings(true);
        }
    }

    public void RegisterEscapeable(IEscapeablePopup popup)
    {
        if (!EscapeablePopups.Contains(popup))
        {
            EscapeablePopups.Add(popup);
        }
    }

    public bool OnEscape()
    {
        bool success = settingsPanel.gameObject.activeSelf;
        SetShowSettings(false);
        return success;
    }

    public void SetShowSettings(bool show)
    {
        if (show)
        {
            RegisterEscapeable(this);
            settingsPanel.Show();
        }
        else
        {
            show = !settingsPanel.Hide();
        }
        tutorial.SetPause(show);
        blocker.SetActive(show);
    }

    public void ShowUpgradePopup(bool isUnlock, UpgradePopupData data, bool show)
    {
        upgradeData = data;
        if (SaveManager.Instance.Data.IntermissionData.SkipPopup || !show)
        {
            OnPopupClosed(true);
            if (isUnlock && !upgradeEmmiter.IsPlaying())
            {
                upgradeEmmiter.Play();
            }
        }
        else
        {
            popup.Show(isUnlock, data);
        }
    }

    public void UpdatePoints()
    {
        var data = SaveManager.Instance.Data;
        CurrentCommandPoints = data.IntermissionData.CommandPoints;
        CurrentUpgradePoints = data.IntermissionData.UpgradePoints;
    }

    public void UpdateSave()
    {
        var data = SaveManager.Instance.Data;
        data.IntermissionData.CommandPoints = CurrentCommandPoints;
        data.IntermissionData.UpgradePoints = CurrentUpgradePoints;
        data.SelectedAircraftCarrier = carrier;
        foreach (var panelData in panelsDict.Values)
        {
            panelData.Panel.Save(data);
        }
        data.MissionInProgress.InProgress = false;
        data.SavedInIntermission = true;
    }

    public void SetActiveCrewWarning(bool active)
    {
        launchButton.Warnings[0].SetActive(active);
        UpdateLaunchButton();
    }

    public void SetActiveOfficersWarning(bool active)
    {
        launchButton.Warnings[1].SetActive(active);
        UpdateLaunchButton();
    }

    public void SetActiveFightersWarning(bool active)
    {
        launchButton.Warnings[2].SetActive(active);
        UpdateLaunchButton();
    }

    public void SetActiveBombersWarning(bool active)
    {
        launchButton.Warnings[3].SetActive(active);
        UpdateLaunchButton();
    }

    public void SetActiveTorpedoWarning(bool active)
    {
        launchButton.Warnings[4].SetActive(active);
        UpdateLaunchButton();
    }

    public void UpdateLaunchButton()
    {
        bool interactable = true;
        foreach (var warning in launchButton.Warnings)
        {
            if (warning.activeSelf)
            {
                interactable = false;
            }
        }
        panelsDict[EIntermissionCategory.Map].PanelButton.interactable = interactable;
    }

    public void AddCommandPoints(int points)
    {
        Assert.IsTrue(points > 0);
        CurrentCommandPoints += points;
    }

    public void FireCarrierBought(ECarrierType type)
    {
        CarrierBought(type);
    }

    private void Refresh()
    {
        commandPointsText.text = CurrentCommandPoints.ToString();
        upgradePointsText.text = CurrentUpgradePoints.ToString();
        foreach (var data in panelsDict.Values)
        {
            data.Panel.Refresh();
        }
    }

    private void OnPopupClosed(bool success)
    {
        if (success)
        {
            upgradeData.UpgradeAction();
            if (upgradeData.Command)
            {
                CurrentCommandPoints -= upgradeData.Cost;
            }
            else
            {
                CurrentUpgradePoints -= upgradeData.Cost;
                UsedUpgradePoints(upgradeData.Cost);
            }
        }
        upgradeData.UpgradeAction = null;
    }

    private void OnBlendFinished()
    {
        if (panelsDict.TryGetValue(currentPanel, out var panel))
        {
            panel.SetActive(true);
        }
    }

    private void LaunchGame()
    {
        StartGameSound.Instance.Play();
        var saveMan = SaveManager.Instance;
        var saveData = saveMan.Data;
        if (saveData.GameMode == EGameMode.Fabular)
        {
            var map = mapPanel.SaveTransient();


            var missionData = mapPanel.GetSelectedMissionData();
            saveData.CurrentMission = missionData.MissionID;
            if (saveData.FinishedMissions.Contains(missionData.MissionID))
            {
                saveData.IntermissionMissionData.UpgradePoints = 0;
                saveData.IntermissionMissionData.CommandsPoints = 0;
                saveData.IntermissionMissionData.EscortType = -1;
                saveData.IntermissionMissionData.EnemyBlocksDestroyed = 9999;
                saveData.IntermissionMissionData.SquadronsToLose = -1;
                saveData.IntermissionMissionData.HoursToFinishMission = -1;
                saveData.IntermissionMissionData.Buff = ETemporaryBuff.None;
            }
            else
            {
                saveData.IntermissionMissionData = missionData.MissionData;
            }
            UpdateSave();

            saveMan.TransientData.ForcedDate = missionData.ForcedDate;

            LoadingManager.Instance.CurrentScene = string.IsNullOrWhiteSpace(map.PlaceholderBriefingID) ? ESceneType.Game : ESceneType.PlaceholderBriefing;
        }
        else
        {
            UpdateSave();
            LoadingManager.Instance.CurrentScene = ESceneType.Game;
        }
    }

    private void QuickSaveCallback1(InputAction.CallbackContext _)
    {
        UpdateSave();
        SaveManager.Instance.SaveData("Quicksave");
    }

    private void QuickSaveCallback2(InputAction.CallbackContext _)
    {
        SaveManager.Instance.LoadLastSave();
    }
}
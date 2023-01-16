using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public event Action<int, bool> MissionFinished = delegate { };
    public event Action<int> ChapterFinished = delegate { };
    public event Action<int> TutorialFinished = delegate { };

    public event Action<int> OfficerGainedMedal = delegate { };

    public static GameStateManager Instance;

    public MissionSuccessPopup MissionSuccessPopup => missionCompleteSummaryPopup;

    public bool Tutorial
    {
        get;
        set;
    }

    public bool AlreadyShown
    {
        get;
        private set;
    }

    [SerializeField]
    private MissionSuccessPopup missionCompleteSummaryPopup = null;
    [SerializeField]
    private MissionLostPopup missionFailSummaryPopup = null;
    [SerializeField]
    private TutorialSummary tutorialSummary = null;
    [SerializeField]
    private FMODStudioController fmodController = null;

    private bool success;
    private EMissionLoseCause loseCause;
    private string overrideDescID;
    private bool reportOngoing;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowMissionSummary(bool success, EMissionLoseCause loseCause, string overrideDescID = "")
    {
        if (AlreadyShown)
        {
            return;
        }
        var reportPanel = ReportPanel.Instance;
        if (reportPanel.Container.activeSelf)
        {
            if (!reportOngoing)
            {
                reportOngoing = true;
                this.success = success;
                this.loseCause = loseCause;
                this.overrideDescID = overrideDescID;
                reportPanel.ReportFinished += OnReportFinished;
            }
            return;
        }
        FireSummary(success, loseCause, overrideDescID);
    }

    public void Clear()
    {
        ReportPanel.Instance.ReportFinished -= OnReportFinished;
        reportOngoing = false;
        AlreadyShown = false;
        fmodController.RestoreVoices();
    }

    public void FireOfficerGainedMedal(int medalCount)
    {
        OfficerGainedMedal(medalCount);
    }

    public void BackToPearlHarbour()
    {
        HudManager.Instance.KillAmbient();
        LoadingManager.Instance.CurrentScene = ESceneType.Intermission;
        GameSceneManager.Instance.UpdateSave();
    }

    private void FireSummary(bool success, EMissionLoseCause loseCause, string overrideDescID)
    {
        if (DragDrop.CurrentDrag != null)
        {
            DragDrop.CurrentDrag.ForceEndDrag();
        }
        if (DeckOrder.CurrentDrag != null)
        {
            DeckOrder.CurrentDrag.ForceEndDrag();
        }
        DragPlanesManager.Instance.ForceEndDrag();
        HudManager.Instance.OnPausePressed();

        AlreadyShown = true;
        fmodController.KillVoices();

        var saveMan = SaveManager.Instance;
        var map = TacticManager.Instance.SOTacticMap;
        bool nextMap = map.Overrides.NextMission != null;
        if (map.Overrides.CrewData == null && !nextMap)
        {
            bool completed = false;
            int medals = missionCompleteSummaryPopup.RightPanel.GetMedals(out _, out _, out _);
            if (medals == 4)
            {
                completed = true;
                foreach (var obj in ObjectivesManager.Instance.Objectives)
                {
                    if (obj.Data.ObjectiveCategory == EObjectiveCategory.Optional && (!obj.Finished || obj.Success == obj.Data.InverseFinishStateInSummary))
                    {
                        completed = false;
                        break;
                    }
                }
            }
            MissionSummaryPopup popup = missionFailSummaryPopup;
            if (success)
            {
                popup = missionCompleteSummaryPopup;
                if (saveMan.Data.GameMode == EGameMode.Fabular)
                {
                    MissionFinished(saveMan.Data.FinishedMissions.Count, completed);
                }
                if (saveMan.TransientData.LastMission)
                {
                    ChapterFinished(saveMan.Data.CurrentChapter);
                    if (saveMan.Data.CurrentChapter == 3)
                    {
                        saveMan.Data.ShowCongratulations = true;
                    }
                }
            }
            popup.transform.parent.gameObject.SetActive(true);
            popup.Setup(loseCause);
        }
        else
        {
            if (!nextMap)
            {
                saveMan.NewSave();
            }
            tutorialSummary.Show();
            TutorialFinished(map.TutorialID);
        }
    }

    private void OnReportFinished()
    {
        ReportPanel.Instance.ReportFinished -= OnReportFinished;
        reportOngoing = false;
        ShowMissionSummary(success, loseCause, overrideDescID);
    }
}

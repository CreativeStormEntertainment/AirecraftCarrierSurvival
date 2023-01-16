#if UNITY_EDITOR
//#define DEBUG_TUTORIAL
#endif

using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialIntermission : TutorialPart
{
    [SerializeField]
    private Color disableColor = Color.white;

    [SerializeField]
    private ButtonIntermissionPanel dockButton;

    [SerializeField]
    private ButtonIntermissionPanel crewButton = null;

    [SerializeField]
    private ButtonIntermissionPanel aircraftButton = null;

    [SerializeField]
    private ButtonIntermissionPanel carrierButton = null;

    [SerializeField]
    private ButtonIntermissionPanel escortButton = null;

    [SerializeField]
    private LaunchButtonIntermissionPanel launchButton = null;

    [SerializeField]
    private Button launchButton2 = null;

    //[SerializeField]
    //private Button tutorialMissionButton = null;
    //private Transform tutorialMissionButtonParent = null;

    //private bool tutorialMissionButtonClicked = false;


    public List<ButtonIntermissionPanel> Clicked
    {
        private set;
        get;
    } = new List<ButtonIntermissionPanel>();

    private void Start()
    {

        if (SaveManager.Instance.Data.GameMode != EGameMode.Tutorial)
        {
            gameObject.SetActive(false);
            return;
        }

        launchButton2.interactable = false;
        launchButton2.image.color = disableColor;

        crewButton.OnShowPanel += OnCrew;
        aircraftButton.OnShowPanel += OnAircraft;
        carrierButton.OnShowPanel += OnCarrier;
        escortButton.OnShowPanel += OnEscort;
        launchButton.OnShowPanel += OnLaunch;

#if DEBUG_TUTORIAL
        launchButton2.interactable = true;
        launchButton2.image.color = Color.white;
        SetupStep(20);
#else
        SetupStep(0);
#endif
        launchButton.SetTooltip(true);
        //tutorialMissionButton.onClick.AddListener(OnClickTutorialMission);
    }

    private void Update()
    {
        if (steps[currentStep].IsPressAnyMode && Input.anyKeyDown)
        {
            tutorialPopup.HideTutorialArrow();

            switch (currentStep)
            {
                case 0:
                    SetupStep(1);
                    break;
                case 1:
                    SetupStep(2);
                    break;
                case 2:
                    CheckLaunchTutorial();
                    break;
                case 3:
                    SetupStep(4);
                    break;
                case 4:
                    SetupStep(5);
                    break;
                case 5:
                    CheckLaunchTutorial();
                    break;
                case 6:
                    SetupStep(7);
                    break;
                case 7:
                    SetupStep(8);
                    break;
                case 8:
                    CheckLaunchTutorial();
                    break;
                case 9:
                    SetupStep(10);
                    break;
                case 10:
                    SetupStep(11);
                    break;
                case 11:
                    SetupStep(12);
                    break;
                case 12:
                    CheckLaunchTutorial();
                    break;
                case 13:
                    SetupStep(14);
                    break;
                case 14:
                    SetupStep(15);
                    break;
                case 15:
                    SetupStep(16);
                    break;
                case 16:
                    CheckLaunchTutorial();
                    break;
                case 17:
                    Hide();
                    break;
                case 18:
                    SetupStep(19);
                    break;
                case 19:
                    Hide();
                    break;
            }
        }
    }

    protected override void SetupStep(int newStep)
    {
        var oldStep = steps[currentStep];
        for (int i = 0; i < oldStep.tasks.Count; ++i)
        {
            oldStep.tasks[i].highlight.gameObject.SetActive(false);
        }
        currentStep = newStep;

        var step = steps[currentStep];
        tutorialPopup.Setup(null, LocalizationManager.Instance.GetText(step.TextID), step.VO, false, step.HelperArrow);

        tutorialPopup.HideAllClickBlockerCutouts();
        for (int i = 0; i < steps[currentStep].tasks.Count; ++i)
        {
            steps[currentStep].tasks[i].highlight.Setup(tutorialPopup.HighlightTime, tutorialPopup.HighlightCurve, steps[currentStep].tasks[i].target, steps[currentStep].tasks[i].offset, transform.parent);
            tutorialPopup.SetClickBlockerCutout(i, steps[currentStep].tasks[i].highlight.RectT);
        }
        if (step.arrowPositions != null)
        {
            tutorialPopup.DisplayHelperArrow(step);
        }

        updateClickBlocker = true;
        clickBlockerSiblingIndex = transform.parent.childCount - 1 - steps[currentStep].tasks.Count;
    }

    private void OnCrew()
    {
        if (CheckLaunchButton(crewButton))
        {
            SetupStep(3);
        }
        else
        {
            tutorialPopup.Hide();
        }
    }

    private void OnAircraft()
    {
        if (CheckLaunchButton(aircraftButton))
        {
            SetupStep(6);
        }
        else
        {
            tutorialPopup.Hide();
        }
    }

    private void OnCarrier()
    {
        if (CheckLaunchButton(carrierButton))
        {
            SetupStep(9);
        }
        else
        {
            tutorialPopup.Hide();
        }
    }

    private void OnEscort()
    {
        if (CheckLaunchButton(escortButton))
        {
            SetupStep(13);
        }
        else
        {
            tutorialPopup.Hide();
        }
    }

    private bool CheckLaunchButton(ButtonIntermissionPanel button)
    {
        if (!Clicked.Contains(button))
        {
            Clicked.Add(button);

            return true;
        }
        return false;
    }

    private void CheckLaunchTutorial()
    {
        if (Clicked.Count >= 4)
        {
            launchButton.SetTooltip(false);
            launchButton2.interactable = true;
            launchButton2.image.color = Color.white;
            SetupStep(17);
        }
        else
        {
            Hide();
        }
    }

    private void OnLaunch()
    {
        if (!Clicked.Contains(launchButton))
        {
            Clicked.Add(launchButton);
            Hide();
            return;
            SetupStep(18);

            //tutorialMissionButtonParent = tutorialMissionButton.transform.parent;
            //tutorialMissionButton.transform.SetParent(transform.parent);
            //tutorialMissionButton.transform.SetSiblingIndex(tutorialPopup.ClickBlocker.GetSiblingIndex() + 1);
            transform.SetAsLastSibling();
        }
    }

    private void OnClickTutorialMission()
    {
        //if (!tutorialMissionButtonClicked)
        //{
        //    SetupStep(22);
        //    tutorialMissionButton.transform.SetParent(tutorialMissionButtonParent);
        //    //tutorialMissionButton.GetComponent<MissionDescription>().SelectMission();
        //}
        //tutorialMissionButtonClicked = true;
    }
}

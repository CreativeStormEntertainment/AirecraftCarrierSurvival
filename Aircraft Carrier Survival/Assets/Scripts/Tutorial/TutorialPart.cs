using System.Collections.Generic;
using UnityEngine;

public class TutorialPart : MonoBehaviour
{
    public List<TutorialStep> steps = new List<TutorialStep>();

    [SerializeField]
    protected TutorialPopup tutorialPopup;
    
    protected int currentStep = 0;
    protected bool updateClickBlocker = false;
    protected int clickBlockerSiblingIndex = 0;

    protected void LateUpdate()
    {
        if (updateClickBlocker)
        {
            tutorialPopup.SetPressAny(steps[currentStep].IsPressAnyMode, steps[currentStep].PressAnyTextVisible, clickBlockerSiblingIndex);
            updateClickBlocker = false;
        }
    }

    protected virtual void SetupStep(int newStep)
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

    protected virtual void Hide()
    {
        for (int i = 0; i < steps[currentStep].tasks.Count; ++i)
        {
            steps[currentStep].tasks[i].highlight.gameObject.SetActive(false);
        }
        tutorialPopup.Hide();
    }
}

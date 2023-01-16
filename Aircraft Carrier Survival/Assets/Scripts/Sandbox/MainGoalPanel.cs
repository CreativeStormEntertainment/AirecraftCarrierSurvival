using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGoalPanel : MonoBehaviour
{
    [SerializeField]
    private Toggle toggle = null;
    [SerializeField]
    private SandboxMainGoalDescription panel = null;
    [SerializeField]
    private GameObject selected = null;
    [SerializeField]
    private GameObject notSelected = null;

    private MainGoalData mainGoal;

    private void Awake()
    {
        toggle.onValueChanged.AddListener(TogglePanel);
    }
    public void Setup(MainGoalData goal)
    {
        mainGoal = goal;
        panel.Setup(mainGoal);
        panel.gameObject.SetActive(false);
    }

    private void TogglePanel(bool show)
    {
        panel.gameObject.SetActive(show);
        selected.SetActive(show);
        notSelected.SetActive(!show);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMissionButton : MonoBehaviour
{
    public event Action Clicked = delegate { };

    public MissionButtonData Data => data;
    public DayTime? ForcedDate
    {
        get => data.ForcedDate;
        set => data.ForcedDate = value;
    }

    [SerializeField]
    private MissionButtonData data = null;
    [SerializeField]
    private List<SOTacticMap> tacticMaps = null;

    [SerializeField]
    private Button button = null;
    [SerializeField]
    private GameObject selectIcon = null;
    [SerializeField]
    private GameObject disabledIcon = null;
    [SerializeField]
    private GameObject finishedIcon = null;

    private void Awake()
    {
        button.onClick.AddListener(OnClick);
    }

    public void Setup(ELevelMissionState state)
    {
        data.State = state;

        gameObject.SetActive(true);
        disabledIcon.SetActive(false);
        finishedIcon.SetActive(false);

        //button.interactable = true;

        switch (state)
        {
            case ELevelMissionState.Hidden:
                gameObject.SetActive(false);
                break;
            case ELevelMissionState.NotAvailable:
                disabledIcon.SetActive(true);
                //button.interactable = false;
                break;
            case ELevelMissionState.Available:
                break;
            case ELevelMissionState.Completed:
            case ELevelMissionState.CompletedAvailable:
                finishedIcon.SetActive(true);
                break;
        }
    }

    public void SetSelected(bool select)
    {
        selectIcon.SetActive(select);
        button.interactable = !select;
    }

    public SOTacticMap GetTacticMap()
    {
        return RandomUtils.GetRandom(tacticMaps);
    }

    private void OnClick()
    {
        Clicked();
    }
}

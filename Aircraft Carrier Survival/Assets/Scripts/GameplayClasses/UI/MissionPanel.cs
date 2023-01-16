using GambitUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MissionPanel : MonoBehaviour
{
    public GameObject ReconHelper;
    public GameObject AirStrikeHelper;
    public GameObject IdentifyTargetHelper;
    public GameObject CapHelper;
    public GameObject NighScoutsHelper;

    public MissionDetailsTooltipCall MissionDetails => missionDetails;

    [SerializeField]
    private MissionDetailsTooltipCall missionDetails = null;
    [SerializeField]
    private RectTransform retrievalPanel = null;
    [SerializeField]
    private RectTransform ongoingPanel = null;
    [SerializeField]
    private RectTransform readyPanel = null;
    [SerializeField]
    [FormerlySerializedAs("avaliablePanel")]
    private RectTransform availablePanel = null;

    [SerializeField]
    private GameObject prefab = null;

    private Dictionary<EMissionList, List<ButtonMission>> missionLists;
    private Dictionary<EMissionList, RectTransform> missionParent;

    private List<ButtonMission> retrievalPanelButtons;
    private List<ButtonMission> ongoingPanelButtons;
    private List<ButtonMission> readyPanelButtons;
    private List<ButtonMission> availablePanelButtons;

    private TacticalMission mission;

    private void Awake()
    {
        retrievalPanelButtons = new List<ButtonMission>();
        ongoingPanelButtons = new List<ButtonMission>();
        readyPanelButtons = new List<ButtonMission>();
        availablePanelButtons = new List<ButtonMission>();

        missionLists = new Dictionary<EMissionList, List<ButtonMission>>()
        {
            {EMissionList.Ongoing, ongoingPanelButtons},
            {EMissionList.Retrieval, retrievalPanelButtons},
            {EMissionList.Ready, readyPanelButtons},
            {EMissionList.Available, availablePanelButtons},
        };
        missionParent = new Dictionary<EMissionList, RectTransform>()
        {
            {EMissionList.Ongoing, ongoingPanel},
            {EMissionList.Retrieval, retrievalPanel},
            {EMissionList.Ready, readyPanel},
            {EMissionList.Available, availablePanel},
        };

        HideIfEmpty(EMissionList.Ongoing);
        HideIfEmpty(EMissionList.Ready);
        HideIfEmpty(EMissionList.Retrieval);
        HideIfEmpty(EMissionList.Available);
    }

    public void SpawnButton(TacticalMission m)
    {
        var button = Instantiate(prefab, retrievalPanel).GetComponentInChildren<ButtonMission>();
        button.Setup(m, this);
        m.ButtonMission = button;
        AddMissionButton(m);
    }
    public void AddMissionButton(TacticalMission m)
    {
        if (m.ButtonMission == null)
        {
            return;
        }
        mission = m;
        var button = m.ButtonMission;
        var stage = mission.MissionStage;
        switch (stage)
        {
            case EMissionStage.Available:
                AddToList(EMissionList.Available, button);
                break;
            case EMissionStage.Launching:
                AddToList(EMissionList.Ongoing, button);
                break;
            case EMissionStage.Deployed:
                AddToList(EMissionList.Ongoing, button);
                break;
            case EMissionStage.AwaitingRetrieval:
                AddToList(EMissionList.Retrieval, button);
                break;
            case EMissionStage.ReadyToRetrieve:
                AddToList(EMissionList.Retrieval, button);
                break;
            case EMissionStage.Recovering:
                AddToList(EMissionList.Retrieval, button);
                break;
            default:
                AddToList(EMissionList.Ready, button);
                break;
        }
    }

    private void AddToList(EMissionList eMission, ButtonMission button)
    {
        //button.ResetTime();
        var panel = missionParent[eMission];
        button.Rect.parent.SetParent(panel);
        button.Refresh();
        RemoveMissionButton(mission);
        missionLists[eMission].Add(button);
        mission.MissionList = eMission;
        panel.parent.gameObject.SetActive(true);
        TacticManager.Instance.StartCoroutineActionAfterFrames(() => RebuildPanel(eMission), 1);
    }

    public void RemoveMissionButton(TacticalMission m)
    {
        missionLists[m.MissionList].Remove(m.ButtonMission);
        HideIfEmpty(m.MissionList);
    }

    public void RebuildPanel(EMissionList eMission)
    {
        var panel = missionParent[eMission];
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel.parent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel.parent.parent.GetComponent<RectTransform>());
        SortButtons(missionLists[eMission]);
    }

    public void HighlightButtons(EMissionList listType, bool highlight, EMissionOrderType missionType)
    {
        foreach (var button in missionLists[listType])
        {
            button.StrikeGroupBlink(highlight, missionType);
        }
    }

    private void HideIfEmpty(EMissionList eMission)
    {
        var panel = missionParent[eMission];
        if (missionLists[eMission].Count == 0)
        {
            panel.parent.gameObject.SetActive(false);
        }
    }

    private void SortButtons(List<ButtonMission> buttons)
    {
        for (int i = 0; i < (int)EMissionOrderType.Count; i++)
        {
            for (int j = 0; j < buttons.Count; j++)
            {
                if ((int)buttons[j].Mission.OrderType == i)
                {
                    buttons[j].Rect.parent.SetAsLastSibling();
                }
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntermissionMissionBtn : MonoBehaviour
{
    public int ChapterID => data.ChapterID;

    [SerializeField]
    private List<SOTacticMap> tacticMaps = null;
    [SerializeField]
    private bool unlockedMission = false;

    [SerializeField]
    private GameObject selectIcon = null;
    [SerializeField]
    private GameObject missionDisabledIcon = null;
    [SerializeField]
    private GameObject missionFinishedIcon = null;

    [SerializeField]
    private MissionButtonData data = null;

    private Button button = null;
    private IntermissionMap map;
    private bool disabled;
    private DayTime dayTime;

    private void Awake()
    {
        button = GetComponent<Button>();
        map = GetComponentInParent<IntermissionMap>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnClick);
        var saveData = SaveManager.Instance.Data;
        bool finishedMission = saveData.FinishedMissions.Contains(data.MissionID);
        if (saveData.FinishedMissions.Contains(data.MissionID))
        {
            missionFinishedIcon.SetActive(true);
        }
        else if (data.IsFinalMission && saveData.FinishedMissions.Count < (map.Panel.IntermissionMissionBtns[data.ChapterID].Count - 1))
        //else if (data.IsFinalMission ? (saveData.FinishedMissions.Count < (map.Panel.IntermissionMissionBtns[data.ChapterID].Count - 1)) :
        //    (!unlockedMission && saveData.FinishedMissions.Count == 0))
        {
            missionDisabledIcon.SetActive(true);
            button.interactable = false;
            disabled = true;
        }
    }

    public void Select()
    {
        UnselectAll();
        selectIcon.SetActive(true);
        button.interactable = false;
    }

    public void Unlock()
    {
        missionFinishedIcon.SetActive(false);
        missionDisabledIcon.SetActive(false);
        button.interactable = true;
        disabled = false;
    }

    private void Unselect()
    {
        selectIcon.SetActive(false);
        if (button != null)
        {
            button.interactable = !disabled;
        }
    }

    private void OnClick()
    {
        Select();

        var transData = SaveManager.Instance.TransientData;
        transData.FabularTacticMap = RandomUtils.GetRandom(tacticMaps);
        transData.LastMission = data.IsFinalMission;

        map.ShowGetMissionDescription().SetDescription(data);
    }

    private void UnselectAll()
    {
        foreach (var btnList in map.Panel.IntermissionMissionBtns)
        {
            foreach (var btn in btnList)
            {
                btn.Unselect();
            }
        }
    }
}

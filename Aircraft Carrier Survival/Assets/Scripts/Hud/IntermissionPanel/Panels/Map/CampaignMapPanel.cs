using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignMapPanel : Panel
{
    public event Action LaunchGame = delegate { };

    public Dictionary<ETemporaryBuff, TemporaryBuffIntermissionData> Buffs
    {
        get;
        private set;
    }

    [SerializeField]
    private List<LevelMissionButton> campaignMaps = null;

    [SerializeField]
    private List<LevelMissionButton> tutorials = null;

    [SerializeField]
    private RectTransform mapTransform = null;
    [SerializeField]
    private MissionDescription description = null;
    [SerializeField]
    private Button debugUnlock = null;

    [SerializeField]
    private List<TemporaryBuffIntermissionData> buffList = null;

    [SerializeField]
    private TemporaryBuffObject buffPanel = null;

    [SerializeField]
    private int maxUnlockedChapter = 1;

    [SerializeField]
    private List<DayTime> chapter2Times = null;
    [SerializeField]
    private List<DayTime> chapter3Times = null;
    [SerializeField]
    private List<DayTime> chapter4Times = null;

    private LevelMissionButton selected;
    private Vector2 normalMapPosition;
    private bool once;

    public override void Setup(NewSaveData data)
    {
        base.Setup(data);
        int missions = 0;

        var locMan = LocalizationManager.Instance;
        Buffs = new Dictionary<ETemporaryBuff, TemporaryBuffIntermissionData>();
        foreach (var buff in buffList)
        {
            Buffs[buff.Type] = buff;
            buff.Name = locMan.GetText(buff.Name);
            buff.Description = locMan.GetText(buff.Description);
            buff.ActiveDescription = locMan.GetText(buff.ActiveDescription);
            buff.InactiveDescription = locMan.GetText(buff.InactiveDescription);
        }

        foreach (var button in campaignMaps)
        {
            var state = ELevelMissionState.Available;
            if (button.Data.ChapterID == data.CurrentChapter && data.CurrentChapter <= maxUnlockedChapter && !data.IsTutorial)
            {
                if (data.FinishedMissions.Contains(button.Data.MissionID))
                {
                    state = ELevelMissionState.Completed;
                }
                else if ((data.CurrentChapter == 0 || button.Data.IsFinalMission) && data.FinishedMissions.Count < missions)
                {
                    state = ELevelMissionState.Hidden;
                }
                else if (!button.Data.IsFinalMission && data.CurrentChapter > 0)
                {
                    List<DayTime> list = null;
                    switch (data.CurrentChapter)
                    {
                        case 1:
                            list = chapter2Times;
                            break;
                        case 2:
                            list = chapter3Times;
                            break;
                        case 3:
                            list = chapter4Times;
                            break;
                    }
                    button.ForcedDate = list[Mathf.Min(2, data.FinishedMissions.Count)];

                }
                missions++;
            }
            else if (button.Data.ChapterID < data.CurrentChapter)
            {
                state = ELevelMissionState.Completed;
            }
            else
            {
                state = ELevelMissionState.Hidden;
            }
            button.Clicked += () => OnButtonSelected(button);
            button.Setup(state);
            if (state == ELevelMissionState.Available && !once)
            {
                once = true;
                OnButtonSelected(button);
            }
        }
        foreach (var button in tutorials)
        {
            button.Setup(ELevelMissionState.Hidden);
        }
        description.LaunchGame += FireLaunchGame;

        buffPanel.Setup(data.MissionRewards.ActiveBuff, true);
        buffPanel.Blink = data.MissionRewards.ActiveBuff != ETemporaryBuff.None;

        //mapTransform.anchoredPosition /= resizer.Scale;
#if ALLOW_CHEATS
        debugUnlock.onClick.AddListener(DebugUnlock);
#else
        debugUnlock.gameObject.SetActive(false);
#endif
        normalMapPosition = mapTransform.anchoredPosition;
    }

    public override void Save(NewSaveData data)
    {

    }

    public SOTacticMap SaveTransient()
    {
        var transData = SaveManager.Instance.TransientData;
        transData.FabularTacticMap = selected.GetTacticMap();
        transData.LastMission = selected.Data.IsFinalMission;
        return transData.FabularTacticMap;
    }

    public MissionButtonData GetSelectedMissionData()
    {
        return selected.Data;
    }

    private void OnButtonSelected(LevelMissionButton button)
    {
        if (selected != null)
        {
            selected.SetSelected(false);
        }
        selected = button;
        if (selected == null)
        {
            description.gameObject.SetActive(false);
            //mapTransform.anchoredPosition = normalMapPosition;
        }
        else
        {
            selected.SetSelected(true);

            description.gameObject.SetActive(true);
            description.SetDescription(selected.Data);
            //mapTransform.anchoredPosition = descriptionMapTransform.anchoredPosition;
        }
    }

    private void FireLaunchGame()
    {
        var saveData = SaveManager.Instance.Data;
        LaunchGame();
    }

#if ALLOW_CHEATS
    private void DebugUnlock()
    {
        var saveData = SaveManager.Instance.Data;
        foreach (var button in campaignMaps)
        {
            var state = ELevelMissionState.Available;
            if (button.Data.ChapterID > maxUnlockedChapter)
            {
                state = ELevelMissionState.Hidden;
            }
            else if (button.Data.ChapterID < saveData.CurrentChapter || (button.Data.ChapterID == saveData.CurrentChapter && saveData.FinishedMissions.Contains(button.Data.MissionID)))
            {
                state = ELevelMissionState.CompletedAvailable;
            }
            button.ForcedDate = null;
            button.Setup(state);
        }
        //foreach (var button in tutorials)
        //{
        //    button.Setup(ELevelMissionState.Available);
        //}
    }
#endif
}

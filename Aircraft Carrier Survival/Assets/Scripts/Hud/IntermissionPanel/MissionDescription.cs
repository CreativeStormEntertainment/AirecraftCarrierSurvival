using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionDescription : MonoBehaviour
{
    public event Action LaunchGame = delegate { };

    [SerializeField]
    private Text title = null;

    [SerializeField]
    private TMP_Text description = null;

    [SerializeField]
    private Transform objectiveList = null;

    [SerializeField]
    private DifficultyLvl difficulty = null;

    [SerializeField]
    private ObjectiveDescription objectiveDescriptionPrefab = null;

    [SerializeField]
    private RewardObject commandPoints = null;
    [SerializeField]
    private RewardObject upgradePoints = null;
    [SerializeField]
    private TemporaryBuffObject buff = null;

    [SerializeField]
    private Button beginBtn = null;
    [SerializeField]
    private Button beginBtn2 = null;

    [SerializeField]
    private GameObject missionComplete = null;

    [SerializeField]
    private Text shipsToSink = null;
    [SerializeField]
    private Text squadronsToLose = null;
    [SerializeField]
    private Text timeToFinish = null;

    [SerializeField]
    private string shipsToSinkId = "";
    [SerializeField]
    private string squadronsToLoseId = "";
    [SerializeField]
    private string timeToFinishId = "";

    private void Awake()
    {
        beginBtn.onClick.AddListener(OnMissionClick);
        beginBtn2.onClick.AddListener(OnMissionClick);
    }

    public void SetDescription(MissionButtonData data)
    {
        var locMan = LocalizationManager.Instance;
        title.text = locMan.GetText(data.MissionNameID);
        description.text = locMan.GetText(data.MissionDescriptionID);

        difficulty.SetDifficulty(data.MissionDifficulty);

        //Clear objective container
        foreach (Transform obj in objectiveList)
        {
            Destroy(obj.gameObject);
        }

        foreach (var id in data.MissionObjectiveDescriptionIDs)
        {
            Instantiate(objectiveDescriptionPrefab, objectiveList, false).Setup(locMan.GetText(id));
        }

        bool active = data.State != ELevelMissionState.Completed && data.State != ELevelMissionState.CompletedAvailable;
        int points = data.MissionData.UpgradePoints;
        if (points > 0)
        {
            upgradePoints.Setup($"+{points}", active);
        }
        points = data.MissionData.CommandsPoints;
        if (points > 0)
        {
            commandPoints.Setup($"+{points}", active);
        }
        if (data.MissionData.Buff == ETemporaryBuff.None)
        {
            buff.gameObject.SetActive(false);
        }
        else
        {
            buff.Setup(data.MissionData.Buff, false);
        }
        missionComplete.SetActive(!active);
        beginBtn.interactable = (data.State == ELevelMissionState.Available || data.State == ELevelMissionState.CompletedAvailable);
        beginBtn2.interactable = (data.State == ELevelMissionState.Available || data.State == ELevelMissionState.CompletedAvailable);

        shipsToSink.text = $"{locMan.GetText(shipsToSinkId)} {data.MissionData.EnemyBlocksDestroyed.ToString()}";
        squadronsToLose.text = $"{locMan.GetText(squadronsToLoseId)} {data.MissionData.SquadronsToLose.ToString()}";
        timeToFinish.text = $"{locMan.GetText(timeToFinishId)} {data.MissionData.GetDateToFinish()}";
    }

    private void OnMissionClick()
    {
        LaunchGame();
    }
}

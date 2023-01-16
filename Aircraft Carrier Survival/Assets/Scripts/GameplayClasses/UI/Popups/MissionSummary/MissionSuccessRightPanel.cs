using System;
using UnityEngine;
using UnityEngine.UI;

public class MissionSuccessRightPanel : MonoBehaviour
{
    public event Action CountFinished = delegate { };

    public int MedalsAwarded
    {
        get;
        set;
    } = 1;

    public bool Play
    {
        get;
        set;
    }

    [SerializeField]
    private GameObject sandboxContainer = null;
    [SerializeField]
    private GameObject fabularContainer = null;
    [SerializeField]
    private GameObject skipsSunkMedal = null;
    [SerializeField]
    private Text shipsToSink = null;
    [SerializeField]
    private Text shipsSunk = null;
    [SerializeField]
    private GameObject squadronsLostMedal = null;
    [SerializeField]
    private Text squadronsToLose = null;
    [SerializeField]
    private Text squadronsLost = null;
    [SerializeField]
    private GameObject missionTimeMedal = null;
    [SerializeField]
    private Text timeToFinish = null;
    [SerializeField]
    private Text timePassed = null;
    [SerializeField]
    private float countingTime = 3f;

    [SerializeField]
    private Color completeColor = Color.green;
    [SerializeField]
    private Color failColor = Color.red;

    private int squadronsLostValue;
    private int enemyBlocksDestroyedValue;
    private TimeSpan timeSpan;
    private float time;
    private TimeManager timeMan;
    private TacticManager tacMan;

    private void Update()
    {
        if (Play)
        {
            time += Time.unscaledDeltaTime;
            float val = time / countingTime;
            shipsSunk.text = ((int)Mathf.Lerp(0, enemyBlocksDestroyedValue, val)).ToString();
            squadronsLost.text = ((int)Mathf.Lerp(0, squadronsLostValue, val)).ToString();
            timePassed.text = ((int)Mathf.Lerp(0, timeSpan.Days, val)).ToString() + "d:" + ((int)Mathf.Lerp(0, timeSpan.Hours, val)).ToString("00") + "h:" + ((int)Mathf.Lerp(0, timeSpan.Minutes, val)).ToString("00");
            if (val >= 1)
            {
                Play = false;
                time = 0f;

                MedalsAwarded = GetMedals(out bool enemiesDestroyed, out bool squadronsPreserved, out bool timeCompleted);

                skipsSunkMedal.SetActive(enemiesDestroyed);
                shipsSunk.color = enemiesDestroyed ? completeColor : failColor;

                squadronsLostMedal.SetActive(squadronsPreserved);
                squadronsLost.color = squadronsPreserved ? completeColor : failColor;

                missionTimeMedal.SetActive(timeCompleted);
                timePassed.color = timeCompleted ? completeColor : failColor;
                CountFinished();
            }
        }
    }

    public void Setup()
    {
        tacMan = TacticManager.Instance;
        timeMan = TimeManager.Instance;

        ref var data = ref SaveManager.Instance.Data.IntermissionMissionData;

        shipsToSink.text = data.EnemyBlocksDestroyed.ToString();
        enemyBlocksDestroyedValue = tacMan.EnemyBlocksDestroyed;

        timeSpan = timeMan.GetTimePassed();
        timeToFinish.text = data.GetDateToFinish();

        squadronsToLose.text = data.SquadronsToLose.ToString();
        squadronsLostValue = tacMan.LostSquadrons;

        if (SaveManager.Instance.Data.GameMode == EGameMode.Sandbox)
        {
            MedalsAwarded = 2;
            sandboxContainer.SetActive(true);
            fabularContainer.SetActive(false);
            CountFinished();
            return;
        }
        MedalsAwarded = 1;
        sandboxContainer.SetActive(false);
        fabularContainer.SetActive(true);
        skipsSunkMedal.SetActive(false);
        squadronsLostMedal.SetActive(false);
        missionTimeMedal.SetActive(false);
        shipsSunk.color = Color.white;
        squadronsLost.color = Color.white;
        timePassed.color = Color.white;
    }

    public int GetMedals(out bool enemiesDestroyed, out bool squadronsPreserved, out bool timeCompleted)
    {
        tacMan = TacticManager.Instance;
        ref var data = ref SaveManager.Instance.Data.IntermissionMissionData;
        int result = 1;
        if (tacMan.EnemyBlocksDestroyed < data.EnemyBlocksDestroyed)
        {
            enemiesDestroyed = false;
        }
        else
        {
            enemiesDestroyed = true;
            result++;
        }
        if (data.SquadronsToLose < tacMan.LostSquadrons)
        {
            squadronsPreserved = false;
        }
        else
        {
            squadronsPreserved = true;
            result++;
        }
        int hoursPassed = timeSpan.Days * 24 + timeSpan.Hours;
        if (timeSpan.Minutes > 0)
        {
            hoursPassed++;
        }
        if (hoursPassed > data.HoursToFinishMission)
        {
            timeCompleted = false;
        }
        else
        {
            timeCompleted = true;
            result++;
        }
        return result;
    }
}

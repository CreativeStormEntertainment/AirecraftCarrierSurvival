using System;
using UnityEngine;

public class SandboxGoalsManager : MonoBehaviour
{
    public event Action MainGoalFinished = delegate { };
    public MainGoalData MainGoal => mainGoal;

    private MainGoalData mainGoal;

    private TimeManager timeMan;

    public void Setup()
    {
        mainGoal = SaveManager.Instance.Data.SandboxData.MainGoal;
        UIManager.Instance.MainGoalPanel.Setup(mainGoal);
    }

    public void Save(ref SandboxSaveData data)
    {
        data.MainGoal = mainGoal;
    }

    public void Tick()
    {
        //if (mainGoal.Unlocked && mainGoal.DaysToFinish > 0)
        //{
        //    if ((mainGoal.TicksToUpdateDays += timeMan.WorldMapTickQuotient) >= timeMan.TicksForDay)
        //    {
        //        mainGoal.DaysToFinish--;
        //        mainGoal.TicksToUpdateDays = 0;
        //        UIManager.Instance.MainGoalPanel.UpdateValues();
        //    }
        //}
    }

    public void FinishMainGoal()
    {
        mainGoal.DaysToFinish = 0;
        MainGoalFinished();
    }

    public void ShortenMainGoalTime(int hours)
    {
        //int days = hours / 24;
        //int ticks = hours % 24 * timeMan.TicksForHour;
        //mainGoal.TicksToUpdateDays += ticks;
        //if (mainGoal.TicksToUpdateDays >= timeMan.TicksForDay)
        //{
        //    mainGoal.DaysToFinish--;
        //    mainGoal.TicksToUpdateDays -= timeMan.TicksForDay;
        //}
        //mainGoal.DaysToFinish -= days;
        //mainGoal.DaysToFinish = Mathf.Max(mainGoal.DaysToFinish, 0);
        //UIManager.Instance.MainGoalPanel.UpdateValues();
    }
}

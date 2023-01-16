using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.AzureSky;

public class TimeManager : MonoBehaviour, IEnableable
{
    public static TimeManager Instance;

    public event Action MinutePassed = delegate { };
    public event Action HourPassed = delegate { };
    public event Action DateChanged = delegate { };

    public event Action IsDayChanged = delegate { };

    public event Action PreUpdate = delegate { };
    public event Action TimeScaleChanged = delegate { };

    private const int StartOfDayTime = 4;
    private const int EndOfDayTime = 22;

    public int TicksForHour = 30;
    public int WorldMapTicksForHour = 1;
    //[NonSerialized]
    //public Progresser TimeProgresser;

    public int WorldMapTickQuotient => TicksForHour / WorldMapTicksForHour;

    public int WorldMapTicksForDay
    {
        get;
        private set;
    }

    public int TicksForDay
    {
        get;
        private set;
    }

    public int AdditionalDayTimeHours
    {
        get;
        set;
    }

    public bool IsDay
    {
        get;
        private set;
    }
    public int CurrentYear
    {
        get;
        private set;
    }
    public int CurrentMonth
    {
        get;
        private set;
    }
    public int CurrentDay
    {
        get;
        private set;
    }
    public int CurrentHour
    {
        get;
        private set;
    }
    public int CurrentMinute
    {
        get;
        private set;
    }

    public DayTime GetCurrentTime()
    {
        return new DayTime(CurrentYear, CurrentMonth, CurrentDay, CurrentHour, CurrentMinute);
    }

    public float TimeSpeed
    {
        get => Time.timeScale;
        set
        {
            Time.timeScale = value;
            TimeScaleChanged();
        }
    }

    public float TickTime => tickTime;

    public ESpeed CurrentSpeed
    {
        get
        {
            int time = HudManager.Instance.TimeIndex;
            if (time == 1)
            {
                return ESpeed.Normal;
            }
            else if (time == 2)
            {
                return ESpeed.Fast;
            }
            else
            {
                return ESpeed.Faster;
            }
        }
    }

    [SerializeField]
    private float tickTime = 1f;

    [SerializeField]
    private int ticksForHourWorldMap = 1;

    [SerializeField]
    private AzureTimeController controller = null;

    private List<TickData> queue = new List<TickData>();

    private List<ITickable> tickables;
    private Dictionary<ITickable, bool> toChangeTickables;

    private bool insideTicks;

    private float timer;
    private bool block;

    private int hourTick;
    private int worldDayTick;
    private float worldHours;
    private float minutes;
    private float hourForWorldTick;
    private float minutesForTick;
    private float tickPortionTimer;

    private bool worldMapMode;

    private int savedTimeSpeed;

    private bool disabled;

    private void Awake()
    {
        toChangeTickables = new Dictionary<ITickable, bool>();
        Assert.IsNull(Instance);
        Instance = this;

        minutesForTick = 60f / TicksForHour;
        hourForWorldTick = 1f / ticksForHourWorldMap;
        WorldMapTicksForDay = ticksForHourWorldMap * 24;
        TicksForDay = TicksForHour * 24;

        tickables = new List<ITickable>();
    }

    private void Start()
    {
        WorldMap.Instance.Toggled += OnWorldMapToggled;
    }

    private void Update()
    {
        PreUpdate();
        timer += Time.deltaTime;
        tickPortionTimer += Time.deltaTime;
        while (timer > tickTime)
        {
            timer -= tickTime;
            tickPortionTimer = timer;
            Tick();
        }

        if (HudManager.Instance.HasNo(ETutorialMode.DisableTimeFly))
        {
            if (worldMapMode)
            {
                int hour = (int)(worldHours + tickPortionTimer * hourForWorldTick);
                if (hour > CurrentHour)
                {
                    CurrentHour = hour;
                    controller.SetTimeline(CurrentHour);
                    HourPassed();
                }
            }
            else
            {
                float value = minutes + tickPortionTimer * minutesForTick;
                int minute = (int)value;
                if (minute > CurrentMinute)
                {
                    CurrentMinute = minute;
                    MinutePassed();
                }
                controller.SetTimeline(CurrentHour + value / 60f);
            }
        }
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
    }

    public void LoadData(ref TimeSaveData data)
    {
        SetTime(data.CurrentTime);
        CurrentMinute = data.CurrentTime.Minute;
        hourTick = (int)(data.CurrentTime.Minute * TicksForHour / 60f);
        HudManager.Instance.OnPausePressed();
        savedTimeSpeed = data.TimeSpeed;
        MinutePassed();
    }

    public void LoadTime()
    {
        if (ReportPanel.Instance.Container.activeInHierarchy)
        {
            ReportPanel.Instance.LoadLastSpeed(savedTimeSpeed);
            return;
        }
        HudManager.Instance.LoadTimeSpeed(savedTimeSpeed);
    }

    public void SaveData(ref TimeSaveData data)
    {
        data.CurrentTime.Year = CurrentYear;
        data.CurrentTime.Month = CurrentMonth;
        data.CurrentTime.Day = CurrentDay;
        data.CurrentTime.Hour = CurrentHour;
        data.CurrentTime.Minute = CurrentMinute;

        data.TimeSpeed = HudManager.Instance.SaveTimeSpeed();
    }

    public void Invoke(Action action, int tickCount)
    {
        queue.Add(new TickData() { Action = action, TicksLeft = tickCount });
    }

    public void AddTickable(ITickable tickable)
    {
        if (insideTicks)
        {
            toChangeTickables[tickable] = true;
        }
        else if (!tickables.Contains(tickable))
        {
            tickables.Add(tickable);
        }
    }

    public void RemoveTickable(ITickable tickable)
    {
        if (insideTicks)
        {
            toChangeTickables[tickable] = false;
        }
        else if (tickables.Contains(tickable))
        {
            tickables.Remove(tickable);
        }
    }

    public void BlockTime()
    {
        block = true;
        TimeSpeed = 0f;
    }

    public void UnblockTime()
    {
        block = false;
        TimeSpeed = 1f;
    }

    public void AdvanceTime(DayTime dayTime)
    {
        var currentDate = new DateTime(CurrentYear, CurrentMonth, CurrentDay, CurrentHour, CurrentMinute, 0);
        if (dayTime.Year > 0)
        {
            currentDate.AddYears(dayTime.Year);
        }
        if (dayTime.Month > 0)
        {
            currentDate.AddMonths(dayTime.Month);
        }
        if (dayTime.Day > 0)
        {
            currentDate.AddDays(dayTime.Day);
        }
        if (dayTime.Hour > 0)
        {
            currentDate.AddHours(dayTime.Hour);
        }
        if (dayTime.Minute > 0)
        {
            currentDate.AddMinutes(dayTime.Minute);
        }
        CurrentYear = currentDate.Year;
        CurrentMonth = currentDate.Month;
        CurrentDay = currentDate.Day;
        CurrentHour = currentDate.Hour;
        CurrentMinute = currentDate.Minute;

        hourTick = (int)(CurrentMinute * TicksForHour / 60f);
        minutes = 0f;
        controller.SetTimeline(CurrentHour);
        DateChanged();
        CheckIsDay();
        HourPassed();
    }

    public void SetTime(DayTime dayTime)
    {
        CurrentYear = dayTime.Year;
        CurrentMonth = dayTime.Month;
        CurrentDay = dayTime.Day;
        CurrentHour = dayTime.Hour;
        CurrentMinute = 0;

        hourTick = 0;

        minutes = 0f;
        if (worldMapMode)
        {
            UpdateWorldTick();
        }
        controller.SetTimeline(CurrentHour);
        DateChanged();
        CheckIsDay();
        HourPassed();
    }

    public TimeSpan GetTimePassed()
    {
        var currentDate = new DateTime(CurrentYear, CurrentMonth, CurrentDay, CurrentHour, CurrentMinute, 0);
        var saveData = SaveManager.Instance.Data;

        var dataMission = SaveManager.Instance.Data.MissionInProgress;
        DayTime startDayTime;
        if (saveData.GameMode == EGameMode.Fabular)
        {
            startDayTime = dataMission.HasMissionStartTime ? dataMission.MissionStartTimeB : TacticManager.Instance.SOTacticMap.Date;
        }
        else
        {
            startDayTime = saveData.SandboxData.MissionStartTime.CurrentTime;
        }
        DateTime startData = new DateTime(startDayTime.Year, startDayTime.Month, startDayTime.Day, startDayTime.Hour, startDayTime.Minute, 0);
        TimeSpan diff = currentDate.Subtract(startData);
        return diff;
        //timePassed.Minute++;
        //if (timePassed.Minute == 60)
        //{
        //    timePassed.Minute = 0;
        //    timePassed.Hour++;
        //    if (timePassed.Hour == 24)
        //    {
        //        timePassed.Hour = 0;
        //        timePassed.Day++;
        //    }
        //}
    }

    public DateTime GetDateAfterDays(int days)
    {
        var currentTime = GetCurrentTime();
        DateTime startData = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
        DateTime date = startData.AddDays(days);
        return date;
    }

    public int GetTicksToNightFinish()
    {
        int minutes = StartOfDayTime - CurrentHour;
        if (minutes < 0)
        {
            minutes += 24;
        }
        minutes *= 60;
        minutes -= CurrentMinute;
        int ticks = (int)(minutes * TicksForHour / 60f);
        return ticks;
    }

    private void Tick()
    {
        insideTicks = true;
        UpdateTime();
        foreach (var tickable in tickables)
        {
            tickable.Tick();
        }
        insideTicks = false;
        foreach (var pair in toChangeTickables)
        {
            if (pair.Value)
            {
                tickables.Add(pair.Key);
            }
            else
            {
                tickables.Remove(pair.Key);
            }
        }
        toChangeTickables.Clear();

        var actionList = new List<Action>();
        for (int i = 0; i < queue.Count; i++)
        {
            if (--queue[i].TicksLeft <= 0)
            {
                actionList.Add(queue[i].Action);
                queue.RemoveAt(i--);
            }
        }
        foreach (var action in actionList)
        {
            action();
        }
    }

    private void SandboxTick()
    {

    }

    private void UpdateTime()
    {
        if (HudManager.Instance.HasNo(ETutorialMode.DisableTimeFly))
        {
            if (worldMapMode)
            {
                CurrentMinute = 0;
                if (++worldDayTick >= WorldMapTicksForDay)
                {
                    worldHours = 0f;
                    worldDayTick = 0;
                    NextDay();
                }
                else
                {
                    SetWorldHours();
                    CurrentHour = (int)worldHours;
                }
                CheckIsDay();
                HourPassed();
            }
            else
            {
                if (++hourTick >= TicksForHour)
                {
                    hourTick = 0;
                    minutes = 0f;
                    CurrentMinute = 0;
                    if (!disabled)
                    {
                        if (++CurrentHour >= 24)
                        {
                            NextDay();
                        }
                        CheckIsDay();
                        HourPassed();
                    }
                }
                else
                {
                    minutes = 60f * hourTick / TicksForHour;
                    CurrentMinute = (int)minutes;
                    MinutePassed();
                }
            }
        }
    }

    private void SetWorldHours()
    {
        worldHours = 24f * worldDayTick / WorldMapTicksForDay;
    }

    private void NextDay()
    {
        CurrentHour = 0;
        CurrentDay++;

        int month = CurrentMonth;
        //check max days in months
        if (month > 7)
        {
            month -= 7;
        }

        bool setMonth;
        if (month % 2 == 0)
        {
            if (CurrentMonth == 2)
            {
                int leapYear = CurrentYear % 100 == 0 ? 400 : 4;
                setMonth = CurrentDay > (CurrentYear % leapYear == 0 ? 29 : 28);
            }
            else
            {
                setMonth = CurrentDay > 30;
            }
        }
        else
        {
            setMonth = CurrentDay > 31;
        }

        if (setMonth)
        {
            CurrentDay = 1;
            if (++CurrentMonth > 12)
            {
                CurrentMonth = 1;
                CurrentYear++;
            }
        }
        DateChanged();
    }

    private void CheckIsDay()
    {
        IsDay = StartOfDayTime < CurrentHour && (EndOfDayTime + AdditionalDayTimeHours) > CurrentHour;
        IsDayChanged();
    }

    private void OnWorldMapToggled(bool state)
    {
        worldMapMode = state;
        CurrentMinute = 0;

        hourTick = 0;

        minutes = 0f;

        if (worldMapMode)
        {
            UpdateWorldTick();
        }
        UpdateTicksLength();
        CheckIsDay();
        HudManager.Instance.OnPlayPressed();
    }

    private void UpdateWorldTick()
    {
        worldDayTick = (int)(CurrentHour / 24f * WorldMapTicksForDay);
        SetWorldHours();
    }

    private void UpdateTicksLength()
    {
        WorldMapTicksForHour = worldMapMode ? ticksForHourWorldMap : TicksForHour;
    }
}

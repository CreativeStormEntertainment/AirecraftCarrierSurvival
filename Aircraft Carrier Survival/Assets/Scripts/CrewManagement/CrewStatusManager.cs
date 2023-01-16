using System;
using System.Collections.Generic;
using UnityEngine;

public class CrewStatusManager : MonoBehaviour, ITickable
{
    public event Action CrewStatusRefreshed = delegate { };

    public static CrewStatusManager Instance
    {
        get;
        private set;
    }

    public int AliveCrews => units.Count - dead;

    public int TicksToDeath => ticksToDeath;

    public bool DisableDeath
    {
        get;
        set;
    }

    public bool IsSickBayDamaged
    {
        get;
        private set;
    }

    public string HealthyID = "PP133.1";
    public string NeedHelpID = "";
    public string InjuredID = "PP133.2";
    public string DeadID = "PP133.3";
    public string InjuredDescID = "PP133.2Desc";
    public string NeedHelpDescID = "";

    [SerializeField]
    private int normalTicksToHeal = 3;
    [SerializeField]
    private int worldMapTicksToHeal = 3;
    private List<CrewUnit> units = new List<CrewUnit>();

    private CrewManager crewManager;

    private int ticksToDeath = 20;
    private int ticksToHeal = 0;
    private int baseHealTicks = 0;
    private int bonusHealTicks = 0;

    private int dead = 0;

    private void Awake()
    {
        Instance = this;
        baseHealTicks = normalTicksToHeal;
        ticksToHeal = baseHealTicks;
    }

    private void Start()
    {
        var timeMan = TimeManager.Instance;
        timeMan.AddTickable(this);
        WorldMap.Instance.Toggled += OnWorldMapToggled;
        var locMan = LocalizationManager.Instance;
        HealthyID = locMan.GetText(HealthyID);
        NeedHelpID = locMan.GetText(NeedHelpID);
        InjuredID = locMan.GetText(InjuredID);
        DeadID = locMan.GetText(DeadID);

        ticksToDeath = Mathf.RoundToInt(Parameters.Instance.MaxTimeToDeathHours * timeMan.TicksForHour);
    }

    public void Init(CrewManager crewManager)
    {
        SectionRoomManager.Instance.Sickbay.SectionWorkingChanged += OnSectionActiveChanged;

        this.crewManager = crewManager;
        units = crewManager.CrewUnits;
        for (int i = 0; i < units.Count; ++i)
        {
            units[i].name = "crew" + i;
        }
    }

    public void OnLoad()
    {
        Tick();
        dead = 0;
        foreach (var unit in units)
        {
            if (unit.UnitState == ECrewUnitState.Dead)
            {
                dead++;
            }
        }
    }

    public CrewUnit AddPotentialInjured(SectionSegment segment)
    {
        var healthy = units.FindAll(x => x.UnitState == ECrewUnitState.Healthy);
        if (healthy.Count > 0)
        {
            var unit = RandomUtils.GetRandom(healthy);
            unit.SetState(ECrewUnitState.SoonToInjure);
            unit.Segment = segment;
            return unit;
        }
        return null;
    }

    public void KillCrew(CrewUnit unit)
    {
        unit.SetState(ECrewUnitState.Dead);
        ++dead;
        crewManager.FireCrewDead();
    }

    public void AddInjured(CrewUnit unit)
    {
        unit.SetState(ECrewUnitState.Injured, UnityEngine.Random.Range(0, Mathf.RoundToInt(Parameters.Instance.RangeTimeTimeToDeathHours * TimeManager.Instance.TicksForHour)));
        int minutes = Mathf.RoundToInt(((ticksToDeath - unit.DeathTicks) / (float)TimeManager.Instance.TicksForHour) * 60f);
        unit.SetTime(minutes);
    }

    public void Tick()
    {
        if (dead == units.Count)
        {
            return;
        }
        crewManager.FindNewInjured();
        bool canHeal = !IsSickBayDamaged;
        float ticksForHour = TimeManager.Instance.TicksForHour;
        foreach (var unit in units)
        {
            if (canHeal && unit.CanBeHealed)
            {
                int minutes = Mathf.RoundToInt(((ticksToDeath - unit.DeathTicks) / ticksForHour) * 60f);
                unit.SetTime(minutes);

                unit.SetIsHealing(true);
                unit.UpdateHeal(++unit.HealTicks / (float)ticksToHeal);
                canHeal = false;
                if (unit.HealTicks >= ticksToHeal)
                {
                    unit.SetState(ECrewUnitState.Healthy);
                    unit.SetIsHealing(false);
                    crewManager.FindNewInjured(unit);
                }
            }
            else if (unit.UnitState == ECrewUnitState.Injured)
            {
                unit.SetIsHealing(false);
                ++unit.DeathTicks;
                if (DisableDeath && unit.DeathTicks >= ticksToDeath)
                {
                    unit.DeathTicks = ticksToDeath - 1;
                }

                int minutes = Mathf.RoundToInt(((ticksToDeath - unit.DeathTicks) / ticksForHour) * 60f);
                unit.SetTime(minutes);

                if (unit.DeathTicks >= ticksToDeath)
                {
                    KillCrew(unit);
                }
                if (AliveCrews < 3)
                {
                    GameStateManager.Instance.ShowMissionSummary(false, EMissionLoseCause.CrewMembersDead);
                }
            }
        }
        CrewStatusRefreshed();
    }

    public void SetBonus(float bonus)
    {
        bonusHealTicks = Mathf.RoundToInt(baseHealTicks * bonus);
        ticksToHeal = baseHealTicks + bonusHealTicks;
    }

    private void OnSectionActiveChanged(bool active)
    {
        IsSickBayDamaged = !active;
    }

    private void OnWorldMapToggled(bool state)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].UnitState == ECrewUnitState.Injured)
            {
                units[i].SetTime(10);
                units[i].UpdateHeal(1f);

                units[i].SetState(ECrewUnitState.Healthy);
                units[i].SetIsHealing(false);
            }
        }
    }
}

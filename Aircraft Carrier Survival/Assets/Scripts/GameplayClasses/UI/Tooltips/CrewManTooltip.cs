using UnityEngine;
using UnityEngine.UI;

public class CrewManTooltip : TooltipCaller
{
    public ECrewUnitState State
    {
        get => state;
        set
        {
            state = value;
            if (isShowing)
            {
                UpdateText();
            }
        }
    }

    public CrewUnit Unit
    {
        get;
        set;
    }

    [SerializeField]
    private Text tHours = null;

    [SerializeField]
    private Text tMinutes = null;

    private ECrewUnitState state = ECrewUnitState.Healthy;

    private void Update()
    {
        if (State == ECrewUnitState.Injured && isShowing)
        {
            UpdateText();
        }
    }

    protected override void UpdateText()
    {
        var crewMan = CrewStatusManager.Instance;
        switch (State)
        {
            case ECrewUnitState.Healthy:
                title = crewMan.HealthyID;
                description = "";
                break;
            case ECrewUnitState.SoonToInjure:
                title = crewMan.NeedHelpID;
                description = locMan.GetText(crewMan.NeedHelpDescID, $"{tHours.text}h{tMinutes.text}");
                break;
            case ECrewUnitState.Injured:
                float ticks = crewMan.TicksToDeath - Unit.DeathTicks;
                float hours = ticks / (float)TimeManager.Instance.TicksForHour;
                float minutes = hours * 60f;
                minutes = Mathf.Max(minutes, 0f);
                title = crewMan.InjuredID;
                description = locMan.GetText(crewMan.InjuredDescID, $"{tHours.text}h{tMinutes.text}");
                //description = locMan.GetText(crewMan.InjuredDescID, Mathf.Floor(hours) + "h");
                //description = locMan.GetText(crewMan.InjuredDescID, ((minutes / 60).ToString("00") + ":" + (minutes % 60).ToString("00")));
                break;
            case ECrewUnitState.Dead:
                title = crewMan.DeadID;
                description = "";
                break;
        }
        Tooltip.Instance.UpdateText(title, description);
    }
}

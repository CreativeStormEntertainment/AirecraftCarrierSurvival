using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewStatus
{
    public CrewUnit Unit;
    public int HealDays;
    public int DeathDays;

    public CrewStatus(CrewUnit Unit)
    {
        this.Unit = Unit;
        HealDays = 0;
        DeathDays = 0;
    }
}

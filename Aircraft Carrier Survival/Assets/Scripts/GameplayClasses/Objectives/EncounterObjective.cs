using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterObjective : MapObjective
{
    public EncounterObjective(int intValue, string stringValue)
    {
        this.intValue = intValue;
        this.stringValue = stringValue;
    }

    public override void ActivateObjective()
    {
        //EventManager.Instance.FireTacticalCombatEvent(intValue);
        //Debug.Log(stringValue);
    }
}

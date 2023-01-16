using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class TacticalCombatEventData 
{
    public float TimeToFire;

    [Header("Popup stuff")]
    public bool ShowPopup;
    public string Title;
    [TextArea]
    public string Description;

    public string MapDataFileName;
    public TacticalFightPilot chosenPilot;

}

using UnityEngine;
using System.Collections;

[System.Serializable]
public class TacticalFightAbility
{
    public string AbillityName;
    public string AbillityDescription;
    public ETacticalFightUnitType OnUnitCanStartAbillity;
    public ETacticalFightAbilityType AbilityType;
    public ETacticalFightPlayerAbillityEffectType EffectType;
    public int AmountOfDamage;
    public int AmountOfRoundsPlaneWillStayOnMap;
}

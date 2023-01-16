using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerManeuverData", menuName = "Datas/PlayerManeuvers", order = 1)]
public class PlayerManeuverData : ScriptableObject
{
    public AttackParametersData Values => BaseValues + PlaneBonusValues;

    public AttackParametersData BaseValues;
    public EManeuverType ManeuverType;
    public SquadronData NeededSquadrons;

    public string Name;
    public string Description;

    public Sprite Icon;
    public AnimationClip Clip;
    public ECardAnimation CardSound;

    public List<BonusData> Modifiers;

    public PlayerManeuverData Level2;
    public PlayerManeuverData Level3;

    [NonSerialized]
    public PlayerManeuverData MainLevel;

    [NonSerialized]
    public int Level;
    [NonSerialized]
    public AttackParametersData PlaneBonusValues;
}

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyManeuverData", menuName = "Datas/EnemyManeuvers", order = 1)]
public class EnemyManeuverData : ScriptableObject
{
    public EEnemyShipType ShipType;

    public AttackParametersData MinValues;
    public AttackParametersData MaxValues;

    public int ShipTypeIndex;
    public EMisidentifiedType MisidentifiedType;

    public string Name;
    public string Description;
    [NonSerialized]
    public string LocalizedName;
    [NonSerialized]
    public string LocalizedDescription;

    public int Durability = 1;

    public Sprite Sprite;

    public float ChanceToReveal;

    public List<BonusData> Modifiers;

    public int Tonnage;

    public bool NeedMagicIdentify;

    private void OnValidate()
    {
        ChanceToReveal = Mathf.Clamp(ChanceToReveal, 0f, 1f);
    }
}

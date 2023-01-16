using System;
using System.Collections.Generic;
using UnityEngine;

public class TacticalEnemyMapButton : MonoBehaviour
{
    [SerializeField]
    private List<int> enemyShipsID = new List<int>();

    public MapEnemyShip ShipScript;

    public bool DetectedMast;

    public List<EnemyUnit> FleetUnits
    {
        get;
        private set;
    }

    public void CreateFleet()
    {
        var tacMan = TacticManager.Instance;
        FleetUnits = new List<EnemyUnit>();
        for (int i = 0; i < enemyShipsID.Count; i++)
        {
            if (enemyShipsID[i] == -1)
            {
                continue;
            }
            FleetUnits.Add(tacMan.CreateEnemyShip(enemyShipsID[i], i));
        }
        FleetUnits[FleetUnits.Count - 1].enemyType = EEnemyTypeDemo.Unsure;
    }
}

[Serializable]
public class EnemyUnit
{
    public string UnitName = "Default Name";
    [NonSerialized]
    public int Index;
    public int FunnelCount = 0;
    public int UnitMinLength = 1;
    public int UnitMaxLength = 1;
    public int UnitLength { get; set; }
    public int MastsLength { get; set; }
    public int MastsMinLength = 1;
    public int MastsMaxLength = 1;
    public int ArmamentMinCount = 1;
    public int ArmamentMaxCount = 1;
    public int ArmamentCount { get; set; }
    [NonSerialized]
    public bool isHidden = false;
    public EEnemyTypeDemo enemyType;
    [NonSerialized]
    public EEnemyTypeDemo trueEnemyType;
    [NonSerialized]
    public EEnemyTypeDemo guessedEnemyType;
    public bool IsLight;
    public bool IsHeavy;
    public bool IsCarrier;
    [NonSerialized]
    public bool IsLightGuessed;
    [NonSerialized]
    public bool IsHeavyGuessed;
    [NonSerialized]
    public bool IsCarrierGuessed;
    [NonSerialized]
    public bool IsDead;
    public Sprite Sprite;
    [NonSerialized]
    public int GuessedId;
    [NonSerialized]
    public int ShipId;

    public void Reveal()
    {
        isHidden = false;
        enemyType = guessedEnemyType = trueEnemyType;
        GuessedId = ShipId;
        IsLightGuessed = IsLight;
        IsHeavyGuessed = IsHeavy;
        IsCarrierGuessed = IsCarrier;
    }
}

public enum EEnemyTypeDemo
{
    DontUse,
    OffenceT,
    OffenceB,
    DefenceT,
    DefenceB,
    DefenceHP,
    Fighters,
    None,
    Cargo,
    Unsure
}

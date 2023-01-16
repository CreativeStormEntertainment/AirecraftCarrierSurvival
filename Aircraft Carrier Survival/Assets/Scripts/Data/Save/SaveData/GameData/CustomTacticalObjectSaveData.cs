using System;

[Serializable]
public struct CustomTacticalObjectSaveData
{
    public int EnemyID;
    public int CurrentIndex;
    public EnemyShipData SaveData;

    public CustomTacticalObjectSaveData(int index, TacticalEnemyShip ship)
    {
        EnemyID = ship.Id;
        CurrentIndex = index;
        SaveData = ship.SaveData();
    }
}

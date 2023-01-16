using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TacticalFightMapFieldData
{
    public int XPos;
    public int YPos;
    public bool IsLand;
    public bool IsClouded;

    public List<TacticalFightEnemyData> ListOfEnemiesForField;
    
    public TacticalFightMapFieldData(bool isLand, bool isClouded, List<TacticalFightEnemyData> listOfEnemiesForField, Vector2Int position)
    {
        XPos = position.x;
        YPos = position.y;
        IsLand = isLand;
        IsClouded = isClouded;
        ListOfEnemiesForField = listOfEnemiesForField;
    }
}

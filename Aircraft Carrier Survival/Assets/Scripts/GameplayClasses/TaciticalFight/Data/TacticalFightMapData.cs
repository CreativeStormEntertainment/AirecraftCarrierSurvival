using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TacticalFightMapData
{
    public List<TacticalFightMapFieldData> MapFieldsData;
    public int AmountsOfRoundsToEndFight;
    public ETacticalFightObjectiveType MapObjectiveType;

    public TacticalFightMapData(List<TacticalFightMapFieldData> fieldsDataToSave, int amountsOfRoundsToEndFight, ETacticalFightObjectiveType typeOfObjective)
    {
        MapFieldsData = fieldsDataToSave;
        AmountsOfRoundsToEndFight = amountsOfRoundsToEndFight;
        MapObjectiveType = typeOfObjective;
    }
}

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalFightMap : MonoBehaviour
{
    TacticalFightMapField[,] mapFields;
    TacticalFightMapData mapDataFromFile;

    private void Awake()
    {
        mapFields = new TacticalFightMapField[GetComponent<TacticalFightMapGenerator>().GetArrayWidth(), GetComponent<TacticalFightMapGenerator>().GetArrayHeigth()];

        foreach (TacticalFightMapField field in GetComponentsInChildren<TacticalFightMapField>(true))
        {
            mapFields[field.GetPositionOnMap().x, field.GetPositionOnMap().y] = field;
        }
    }

    public void InitializeMap(string mapDataFileName)
    {

        mapFields = new TacticalFightMapField[GetComponent<TacticalFightMapGenerator>().GetArrayWidth(), GetComponent<TacticalFightMapGenerator>().GetArrayHeigth()];

        foreach (TacticalFightMapField field in GetComponentsInChildren<TacticalFightMapField>(true))
        {
            mapFields[field.GetPositionOnMap().x, field.GetPositionOnMap().y] = field;
        }

        mapDataFromFile = BinUtils.LoadBinaryTextAsset<TacticalFightMapData>(@"TacticalFightMapData\" + mapDataFileName);

        foreach (TacticalFightMapFieldData mapFieldData in mapDataFromFile.MapFieldsData)
        {
            if (IsElementOnPosition(mapFieldData.XPos, mapFieldData.YPos))
            {
                TacticalFightMapField mapFieldForData = mapFields[mapFieldData.XPos, mapFieldData.YPos];
                mapFieldForData.SetIsLand(mapFieldData.IsLand);
                mapFieldForData.SetIsFieldClouded(mapFieldData.IsClouded);
                mapFieldForData.SetEnemyListOnField(mapFieldData.ListOfEnemiesForField);
            }
        }

        switch (mapDataFromFile.MapObjectiveType)
        {
            case (ETacticalFightObjectiveType.DefeatAllEnemiesOnMap):
                TacticalFightEventManager.Instance.AddObjective(new TacticalFightDefeatAllEnemiesObjective());
                break;

            case (ETacticalFightObjectiveType.DefeatYamato):
                TacticalFightEventManager.Instance.AddObjective(new TacticalFightDefeatYamatoObjective());
                break;

            case (ETacticalFightObjectiveType.SaveTanker):
                TacticalFightEventManager.Instance.AddObjective(new TacticalFightSaveTankerObjective());
                break;
        }

        TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightBadWeatherEvent(mapDataFromFile.AmountsOfRoundsToEndFight));
    }

    private void Start()
    {

    }

    public void SetMapFields(TacticalFightMapField[,] fields)
    {
        mapFields = fields;
    }

    public TacticalFightMapField[,] GetMapFields()
    {
        return mapFields;
    }

    public List<TacticalFightMapField> GetPossibleFieldsToMark(TacticalFightMapField from, int distance, ETacticalFightUnitRotationState direction)
    {
        List<TacticalFightMapField> fieldsToMark = new List<TacticalFightMapField>();
        Vector2Int fromPosition = from.GetPositionOnMap();
        int xDirection = 0;
        int yDirection = 0;

        switch (direction)
        {
            case (ETacticalFightUnitRotationState.Up):
                xDirection = 0;
                yDirection = 1;
                break;

            case (ETacticalFightUnitRotationState.RightUp):
                xDirection = 1;
                yDirection = fromPosition.x % 2 != 0 ? 0 : 1;
                break;

            case (ETacticalFightUnitRotationState.RightDown):
                xDirection = 1;
                yDirection = fromPosition.x % 2 != 0 ? -1 : 0;
                break;

            case (ETacticalFightUnitRotationState.Down):
                xDirection = 0;
                yDirection = -1;
                break;

            case (ETacticalFightUnitRotationState.LeftDown):
                xDirection = -1;
                yDirection = fromPosition.x % 2 != 0 ? -1:0;
                break;

            case (ETacticalFightUnitRotationState.LeftUp):
                xDirection = -1;
                yDirection = fromPosition.x % 2 != 0 ? 0 :1;
                break;
        }

        int checkedRowPosition = fromPosition.x;
        int checkedYPosition = fromPosition.y;

        for (int i = 1; i <= distance; i++)
        {
            switch (direction)
            {
                case (ETacticalFightUnitRotationState.Up):
                    xDirection = 0;
                    yDirection = 1;
                    break;

                case (ETacticalFightUnitRotationState.RightUp):
                    xDirection = 1;
                    yDirection = checkedRowPosition % 2 != 0 ? 0 : 1;
                    break;

                case (ETacticalFightUnitRotationState.RightDown):
                    xDirection = 1;
                    yDirection = checkedRowPosition % 2 != 0 ? -1 : 0;
                    break;

                case (ETacticalFightUnitRotationState.Down):
                    xDirection = 0;
                    yDirection = -1;
                    break;

                case (ETacticalFightUnitRotationState.LeftDown):
                    xDirection = -1;
                    yDirection = checkedRowPosition % 2 != 0 ? -1 : 0;
                    break;

                case (ETacticalFightUnitRotationState.LeftUp):
                    xDirection = -1;
                    yDirection = checkedRowPosition % 2 != 0 ? 0 : 1;
                    break;
            }

            if (IsElementOnPosition(fromPosition.x + (i * xDirection), checkedYPosition + yDirection))
                fieldsToMark.Add(mapFields[fromPosition.x + (i * xDirection), checkedYPosition + yDirection]);
            else
                return fieldsToMark;

            checkedRowPosition = fromPosition.x + (i * xDirection);
            checkedYPosition = checkedYPosition + yDirection;
        }
        return fieldsToMark;
    }

    private bool IsElementOnPosition(int x, int y)
    {
        try
        {
            if (mapFields[x, y] != null)
                return true;
            else
                return false;
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }
    }
}

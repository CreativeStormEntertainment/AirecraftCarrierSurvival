using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TacticalFightMapDataGenerator : MonoBehaviour
{
    private string fileName;
    private int amountsOfRoundsToEndFight;
    private ETacticalFightObjectiveType typeOfMapObjective;
    private float radiusOfSearch = 32;

    public TacticalFightMapData GenerateMapInfo()
    {
        List<TacticalFightMapFieldData> mapFieldDatas = new List<TacticalFightMapFieldData>();
        List<TacticalFightSpawner> spawnersOnScene = new List<TacticalFightSpawner>();

        spawnersOnScene = FindObjectsOfType<TacticalFightSpawner>().ToList();

        foreach (TacticalFightSpawner spawner in spawnersOnScene)
        {
            TacticalFightMapField spawnerField = GetNeareastField(spawner);
            if (spawnerField != null)
            {
                mapFieldDatas.Add(new TacticalFightMapFieldData(spawner.IsLand,spawner.IsClouded,spawner.ListOfSpawningEnemies,spawnerField.GetPositionOnMap()));
            }
        }

        return new TacticalFightMapData(mapFieldDatas, amountsOfRoundsToEndFight, typeOfMapObjective);
    }

    private TacticalFightMapField GetNeareastField(TacticalFightSpawner spawner)
    {
        TacticalFightMapField spawnerField = null;
        foreach (var field in FindObjectsOfType<TacticalFightMapField>())
        {
            float newDist = (spawner.GetComponent<RectTransform>().anchoredPosition - field.GetComponent<RectTransform>().anchoredPosition).sqrMagnitude;
            if (newDist < radiusOfSearch)
            {
                spawnerField = field;
            }
        }

        return spawnerField;
    }

    private TacticalFightMapField GetFieldOnWorldPosition(Vector3 position)
    {
        GraphicRaycaster graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = position;
        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        RaycastResult foundedResult = results.FirstOrDefault(x => x.gameObject.layer == 9);

        if (foundedResult.gameObject != null)
        {
            TacticalFightMapField foundedField = foundedResult.gameObject.GetComponent<TacticalFightMapField>();
            return foundedField;
        }
        else
        {
            return null;
        }
    }

    private int GetSpawnersOnFieldAmount(TacticalFightMapField fieldToCheck)
    {
        int amountOfSpawnersOnField = 0;
        foreach (TacticalFightSpawner spawner in FindObjectsOfType<TacticalFightSpawner>())
        {
            TacticalFightMapField spawnerField = GetNeareastField(spawner);
            if (spawnerField == fieldToCheck)
                amountOfSpawnersOnField++;
        }

        return amountOfSpawnersOnField;
    }

    private TacticalFightMapField GetFieldOnHisPosition(Vector2Int searchedPosition)
    {
        List<TacticalFightMapField> mapFieldsOnScene = FindObjectsOfType<TacticalFightMapField>().ToList();

        TacticalFightMapField fieldToSpawnOn = mapFieldsOnScene.FirstOrDefault(x => x.GetPositionOnMap().x == searchedPosition.x && x.GetPositionOnMap().y == searchedPosition.y);

        return fieldToSpawnOn;
    }

#if UNITY_EDITOR
    public void InstiateSpawners(TacticalFightMapData mapData)
    {
        List<TacticalFightMapField> mapFieldsOnScene = FindObjectsOfType<TacticalFightMapField>().ToList();

        TacticalFightSpawner spawnerObject = (TacticalFightSpawner) AssetDatabase.LoadAssetAtPath("Assets/GameplayAssets/Prefabs/TacticalFight/Spawner.prefab", typeof(TacticalFightSpawner));

        foreach (TacticalFightMapFieldData mapFieldData in mapData.MapFieldsData)
        {
            TacticalFightSpawner createdSpawner = Instantiate(spawnerObject, transform.parent);

            TacticalFightMapField fieldToSpawnOn = mapFieldsOnScene.FirstOrDefault(x => x.GetPositionOnMap().x == mapFieldData.XPos && x.GetPositionOnMap().y == mapFieldData.YPos);

            createdSpawner.GetComponent<RectTransform>().anchoredPosition = fieldToSpawnOn.GetComponent<RectTransform>().anchoredPosition;
            createdSpawner.IsLand = mapFieldData.IsLand;
            createdSpawner.IsClouded = mapFieldData.IsClouded;
            createdSpawner.ListOfSpawningEnemies = mapFieldData.ListOfEnemiesForField;
            createdSpawner.UpdateSpawner();
        }
    }

    public bool InstatiateSpawner(Vector2Int positionOfField)
    {
        TacticalFightMapField fieldToSpawnOn = GetFieldOnHisPosition(positionOfField);
        if (fieldToSpawnOn != null)
        {
            if (GetSpawnersOnFieldAmount(fieldToSpawnOn) < 1)
            {
                TacticalFightSpawner spawnerObject = (TacticalFightSpawner)AssetDatabase.LoadAssetAtPath("Assets/GameplayAssets/Prefabs/TacticalFight/Spawner.prefab", typeof(TacticalFightSpawner));
                TacticalFightSpawner createdSpawner = Instantiate(spawnerObject, transform.parent);
                createdSpawner.GetComponent<RectTransform>().anchoredPosition = fieldToSpawnOn.GetComponent<RectTransform>().anchoredPosition;
                return true;
            }
            else
                return false;
        }
        else
        {
            return false;
        }
    }
#endif

    public string GetFileName()
    {
        return fileName;
    }

    public void SetFileName(string newFilename)
    {
        fileName = newFilename;
    }

    public int GetAmountsOfRoundsToEndFight()
    {
        return amountsOfRoundsToEndFight;
    }

    public void SetAmountsOfRoundsToEndFight(int amountOfRounds)
    {
        amountsOfRoundsToEndFight = amountOfRounds;
    }

    public ETacticalFightObjectiveType GetMapObjectiveType()
    {
        return typeOfMapObjective;
    }

    public void SetTypeOfMapObjective(ETacticalFightObjectiveType objectiveType)
    {
        typeOfMapObjective = objectiveType;
    }
}

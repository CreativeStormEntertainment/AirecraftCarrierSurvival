using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(TacticalFightMapDataGenerator))]
public class TacticalFightMapDataGenatorEditor : Editor
{
    TacticalFightMapDataGenerator mapDataGenetor;
    Vector2Int positionToSpawnSpawner;

    private void OnEnable()
    {
        mapDataGenetor = (TacticalFightMapDataGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        mapDataGenetor.SetFileName(EditorGUILayout.TextField("Filename", mapDataGenetor.GetFileName()));
        mapDataGenetor.SetAmountsOfRoundsToEndFight(EditorGUILayout.IntField("Amount Of Rounds To End Fight", mapDataGenetor.GetAmountsOfRoundsToEndFight()));
        mapDataGenetor.SetTypeOfMapObjective((ETacticalFightObjectiveType)EditorGUILayout.EnumPopup("Map Objective", mapDataGenetor.GetMapObjectiveType()));

        if (GUILayout.Button("Save Map Data File") && EditorUtility.DisplayDialog("Are you sure you want save this map ?", "", "Yes", "No"))
        {
            TacticalFightMapData mapData = mapDataGenetor.GenerateMapInfo();
            BinUtils.SaveObjectAsBinaryFile(mapData, @"Assets\Resources\TacticalFightMapData\", mapDataGenetor.GetFileName());
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Load Map Data File") && EditorUtility.DisplayDialog("Are you sure you want load this map data?", "This action will remove current spawners on scene.", "Yes", "No"))
        {
            TacticalFightMapData mapData = BinUtils.LoadObjectFromBinaryFile<TacticalFightMapData>(@"Assets\Resources\TacticalFightMapData\", mapDataGenetor.GetFileName());

            if (mapData != null)
            {
                DestroyAllSpawners();
                mapDataGenetor.InstiateSpawners(mapData);
                mapDataGenetor.SetAmountsOfRoundsToEndFight(mapData.AmountsOfRoundsToEndFight);
                mapDataGenetor.SetTypeOfMapObjective(mapData.MapObjectiveType);
            }
            else
                EditorUtility.DisplayDialog("Warning", "Map data file not found", "Ok");
        }

        if (GUILayout.Button("Destroy All Spawners"))
        {
            DestroyAllSpawners();
        }

        positionToSpawnSpawner = EditorGUILayout.Vector2IntField("Position of field to create spawner", positionToSpawnSpawner);

        if (GUILayout.Button("Create Spawner On Position"))
        {
            bool result = mapDataGenetor.InstatiateSpawner(positionToSpawnSpawner);
            if(result == false)
                EditorUtility.DisplayDialog("Warning", "On selected field is already a spawner.", "Ok");
        }
    }

    private void DestroyAllSpawners()
    {
        foreach(TacticalFightSpawner spawner in FindObjectsOfType<TacticalFightSpawner>())
        {
            DestroyImmediate(spawner.gameObject);
        }
    }
}

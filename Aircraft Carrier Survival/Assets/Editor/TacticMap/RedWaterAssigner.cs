using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
public class RedWaterAssigner
{
    const string redWatersPath = @"Assets/GamePlayAssets/ScriptableData/TacticMaps/Redwaters";
    const string enemiesPath = @"Assets/GamePlayAssets/ScriptableData/EnemyManeuvers";
    [MenuItem("Tools/TacticMaps/Assign RedWater Data in Tactic", false, 202)]
    public static void Assign()
    {
        bool found = false;
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name == "MainScene" && !found)
            {
                found = true;

                var root = s.GetRootGameObjects();
                TacticManager tacticMan = null;
                for (int j = 0; j < root.Length; ++j)
                {
                    tacticMan = root[i].GetComponent<TacticManager>();
                    if (tacticMan != null)
                    {
                        break;
                    }
                }

                if (tacticMan == null)
                {
                    UnityEngine.Debug.LogWarning("TacticManager not found on MainScene!");
                }
                else
                {
                    var files = Directory.GetFiles(redWatersPath, "*.asset");
                    List<SOTacticMap> listMaps = new List<SOTacticMap>();
                    for (int j = 0; j < files.Length; ++j)
                    {
                        listMaps.Add((SOTacticMap)AssetDatabase.LoadAssetAtPath(files[j], typeof(SOTacticMap)));
                    }
                    tacticMan.RedWaterSetup.Maps = listMaps;


                    files = Directory.GetFiles(enemiesPath, "*.asset");
                    List<EnemyManeuverData> listEnemies = new List<EnemyManeuverData>();
                    for (int j = 0; j < files.Length; ++j)
                    {
                        listEnemies.Add((EnemyManeuverData)AssetDatabase.LoadAssetAtPath(files[j], typeof(EnemyManeuverData)));
                    }
                    tacticMan.EnemiesList = listEnemies;

                    EditorUtility.SetDirty(tacticMan);
                }
            }
        }
        if (found)
        {
            UnityEngine.Debug.LogWarning("MainScene not loaded");
        }
    }
}

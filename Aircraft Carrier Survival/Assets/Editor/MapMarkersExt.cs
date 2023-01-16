//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(MapMarkersEditor))]
//public class MapMarkersExt : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        MapMarkersEditor editor = (MapMarkersEditor)target;

//        GUILayout.Space(10);
//        if (GUILayout.Button("Clear Enemy Markers"))
//        {
//            editor.ClearEnemyMarkers();
//        }

//        if (GUILayout.Button("Clear Weather Markers"))
//        {
//            editor.ClearWeatherMarkers();
//        }

//        if (GUILayout.Button("Clear Objective Markers"))
//        {
//            editor.ClearObjectiveMarkers();
//        }
//        GUILayout.Space(10);
//        if (GUILayout.Button("Save"))
//        {
//            editor.SaveMarkersPositioning();
//        }

//        if (GUILayout.Button("Load"))
//        {
//            editor.LoadMarkersPositioning();
//        }
//        GUILayout.Space(10);
//        if (GUILayout.Button("Create Enemy Marker"))
//        {
//            editor.SpawnEnemyMarker();
//        }

//        if (GUILayout.Button("Create Weather Marker"))
//        {
//            editor.SpawnWeatherMarker();
//        }

//        if (GUILayout.Button("Create Objective Marker"))
//        {
//            editor.SpawnObjectiveMarker();
//        }
//        GUILayout.Space(10);
//        if (GUILayout.Button("Create Player Marker"))
//        {
//            editor.SpawnPlayerMarker();
//        }
//        GUILayout.Space(10);
//        if (GUILayout.Button("Refresh List"))
//        {
//            editor.RefreshOrderedList();
//        }
//        base.OnInspectorGUI();
//    }
//}
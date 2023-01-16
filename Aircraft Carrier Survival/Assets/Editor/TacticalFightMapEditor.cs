using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(TacticalFightMapGenerator))]
public class TacticalFightMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TacticalFightMapGenerator map = (TacticalFightMapGenerator)target;

        if (GUILayout.Button("Generate Map Fields"))
        {
            map.GenerateMap();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(map);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

}
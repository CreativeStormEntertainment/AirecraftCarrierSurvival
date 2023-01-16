using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[CustomEditor(typeof(TacticalFightSpawner))]
public class TacticalFightSpawnerEditor : Editor
{
    TacticalFightSpawner spawner;

    private void OnEnable()
    {
        spawner = (TacticalFightSpawner)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (spawner.UpdateSpawner())
        {
            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(spawner.GetComponent<Image>());
            //EditorUtility.SetDirty(spawner.GetComponentInChildren<Text>());
        }
    }

}
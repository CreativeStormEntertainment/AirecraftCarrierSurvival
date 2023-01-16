using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EscortTypeAttribute))]
public class IntermissionMissionDataPropertyDrawer : PropertyDrawer
{
    private GUIContent[] options;
    private int[] values;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (options == null)
        {
            var data = AssetDatabase.LoadAssetAtPath<StrikeGroupData>("Assets/GameplayAssets/ScriptableData/StrikeGroupData.asset");
            options = new GUIContent[data.Data.Count + 1];
            values = new int[data.Data.Count + 1];

            options[0] = new GUIContent("None");
            values[0] = -1;

            for (int i = 0; i < data.Data.Count; i++)
            {
                options[i + 1] = new GUIContent(data.Data[i].Name);
                values[i + 1] = i;
            }
        }
        EditorGUI.IntPopup(position, property, options, values);
    }
}

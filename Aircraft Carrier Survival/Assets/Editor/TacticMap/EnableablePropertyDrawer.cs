using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnableableAttribute))]
public class EnableablePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position = EditorGUI.PrefixLabel(position, label);
        float width = position.width;
        position.x -= EditorGUI.indentLevel * 15f;
        position.width = 58f;
        if (EditorGUI.Toggle(position, property.intValue >= 0))
        {
            position.x += 15f;
            position.width = width - 15f;
            property.intValue = Mathf.Max(0, EditorGUI.IntField(position, property.intValue));
        }
        else
        {
            property.intValue = -1;
        }
    }
}

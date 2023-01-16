using UnityEditor;
using UnityEngine;

namespace GambitUtils.UI
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    class LayerAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
}

using UnityEngine;
using UnityEditor;

namespace GambitUtils.UI
{
    [CustomPropertyDrawer(typeof(ConditionalHideRangeAttribute))]
    public class ConditionalHideRangePropertyDrawer : ConditionalHidePropertyDrawer
    {
        protected override void DrawProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            // First get the attribute since it contains the range for the slider
            ConditionalHideRangeAttribute range = attribute as ConditionalHideRangeAttribute;

            // Draw the property as a Slider or an IntSlider based on whether it's a float or integer.
            if (property.propertyType == SerializedPropertyType.Float)
                EditorGUI.Slider(position, property, range.min, range.max, label);
            else if (property.propertyType == SerializedPropertyType.Integer)
                EditorGUI.IntSlider(position, property, (int)range.min, (int)range.max, label);
            else
                EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
        }
    } 
}
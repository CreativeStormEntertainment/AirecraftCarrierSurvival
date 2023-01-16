using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MissionPrepareData))]
public class MissionPrepareDataPropertyDrawer : PropertyDrawer
{
    private const float ToggleWidth = 30f;
    private static string[] StrikeGroups;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = base.GetPropertyHeight(property, label);
        if (property.isExpanded)
        {
            IterateThroughProperty(property, (prop) =>
                {
                    if (!prop.displayName.Contains("Enable") && !prop.displayName.Contains("Escort"))
                    {
                        height += EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                    }
                });
            if (property.FindPropertyRelative(nameof(MissionPrepareData.OverridenBuffs)).isExpanded)
            {
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            height += 4f * EditorGUIUtility.singleLineHeight;
            height += 5f * EditorGUIUtility.standardVerticalSpacing;
        }
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position.height = base.GetPropertyHeight(property, label);
        var data = (property.serializedObject.targetObject as SOTacticMap).Overrides;

        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float width = position.width;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            position.height = EditorGUIUtility.singleLineHeight;
            position.width = ToggleWidth;
            data.EnableView = EditorGUI.Toggle(position, data.EnableView);
            position.x += ToggleWidth;
            position.width = width - ToggleWidth;
            if (data.EnableView)
            {
                data.OverridenView = (ECameraView)EditorGUI.EnumPopup(position, nameof(MissionPrepareData.OverridenView), data.OverridenView);
            }
            else
            {
                EditorGUI.LabelField(position, nameof(MissionPrepareData.OverridenView));
                data.OverridenView = ECameraView.Deck;
            }
            position.x -= ToggleWidth;
            position.width = width;
            position.y += position.height;
            position.y += EditorGUIUtility.standardVerticalSpacing;

            DrawList(ref position, property.FindPropertyRelative(nameof(MissionPrepareData.OverridenCrewData)), data.OverridenCrewData, ref data.EnableCrewData);
            DrawList(ref position, property.FindPropertyRelative(nameof(MissionPrepareData.OverridenOfficersData)), data.OverridenOfficersData, ref data.EnableOfficersData);
            DrawList(ref position, property.FindPropertyRelative(nameof(MissionPrepareData.OverridenAvailableManeuvers)), data.OverridenAvailableManeuvers, ref data.EnableAvailableManeuvers);
            DrawList(ref position, property.FindPropertyRelative(nameof(MissionPrepareData.OverridenDefaultSwitchesValues)), data.OverridenDefaultSwitchesValues, ref data.EnableDefaultSwitchesValues);
            DrawList(ref position, property.FindPropertyRelative(nameof(MissionPrepareData.OverridenBuffs)), data.OverridenBuffs, ref data.EnableBuffs);

            position.height = EditorGUIUtility.singleLineHeight;
            position.width = ToggleWidth;

            var prop = property.FindPropertyRelative(nameof(data.OverridenEscort));
            data.EnableEscort = EditorGUI.Toggle(position, data.EnableEscort);
            position.x += ToggleWidth;
            position.width = width - ToggleWidth;
            position.height = EditorGUI.GetPropertyHeight(prop);
            bool enabled = GUI.enabled;
            GUI.enabled = data.EnableEscort;
            if (StrikeGroups == null)
            {
                var strikeGroupsData = AssetDatabase.LoadAssetAtPath<StrikeGroupData>("Assets/GameplayAssets/ScriptableData/StrikeGroupData.asset");
                StrikeGroups = new string[strikeGroupsData.Data.Count];
                for (int i = 0; i < strikeGroupsData.Data.Count; i++)
                {
                    StrikeGroups[i] = strikeGroupsData.Data[i].Name;
                }
            }
            if (data.OverridenEscort == null)
            {
                data.OverridenEscort = new List<int>();
            }
            int mask = 0;
            foreach (int value in data.OverridenEscort)
            {
                mask |= 1 << value;
            }
            mask = EditorGUI.MaskField(position, prop.displayName, mask, StrikeGroups);
            GUI.enabled = enabled;
            position.width = width;
            position.y += position.height;
            position.x -= ToggleWidth;

            if (!data.EnableEscort)
            {
                data.OverridenEscort.Clear();
            }
            int index = 0;
            for (int i = 0; i < StrikeGroups.Length; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    if (data.OverridenEscort.Count <= index)
                    {
                        data.OverridenEscort.Add(index);
                    }
                    if (data.OverridenEscort[index] != i)
                    {
                        data.OverridenEscort[index] = i;
                    }
                    index++;
                }
            }
            while (index < data.OverridenEscort.Count)
            {
                data.OverridenEscort.RemoveAt(data.OverridenEscort.Count - 1);
            }

            data.EnableDCNoCategory = EditorGUI.ToggleLeft(position, property.FindPropertyRelative(nameof(data.EnableDCNoCategory)).displayName, data.EnableDCNoCategory);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            data.EnableNoAACasualties = EditorGUI.ToggleLeft(position, property.FindPropertyRelative(nameof(data.EnableNoAACasualties)).displayName, data.EnableNoAACasualties);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            data.EnableNoSegmentDangers = EditorGUI.ToggleLeft(position, property.FindPropertyRelative(nameof(data.EnableNoSegmentDangers)).displayName, data.EnableNoSegmentDangers);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            position.height = EditorGUIUtility.singleLineHeight;
            position.width = ToggleWidth;
            data.EnableNextMission = EditorGUI.ToggleLeft(position, property.FindPropertyRelative(nameof(data.EnableNextMission)).displayName, data.EnableNextMission);

            position.x += ToggleWidth;
            position.width = width - ToggleWidth;

            GUI.enabled = data.EnableNextMission;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(data.NextMission)));
            GUI.enabled = enabled;

            position.x -= ToggleWidth;
            position.width = width;

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void DrawList<T>(ref Rect position, SerializedProperty property, List<T> value, ref bool enable) where T : new()
    {
        if (!DrawToggled(ref position, property, ref enable) && value != null)
        {
            value.Clear();
        }
    }

    private bool DrawToggled(ref Rect position, SerializedProperty property, ref bool enable)
    {
        float width = position.width;
        position.height = EditorGUIUtility.singleLineHeight;
        position.width = ToggleWidth;

        enable = EditorGUI.Toggle(position, enable);
        position.x += ToggleWidth;
        position.width = width - ToggleWidth;
        position.height = EditorGUI.GetPropertyHeight(property);
        EditorGUI.PropertyField(position, property, enable);
        position.x -= ToggleWidth;
        if (!enable)
        {
            property.isExpanded = false;
        }
        position.width = width;
        position.y += position.height;
        return enable;
    }

    private void IterateThroughProperty(SerializedProperty property, Action<SerializedProperty> callback)
    {
        var prop = property.serializedObject.FindProperty(property.propertyPath);
        var propEnd = property.serializedObject.FindProperty(property.propertyPath);
        bool hasNext = propEnd.NextVisible(false);

        if (prop.NextVisible(true) && (!hasNext || prop.propertyPath != propEnd.propertyPath))
        {
            do
            {
                callback(prop);
            }
            while (prop.NextVisible(false) && (!hasNext || prop.propertyPath != propEnd.propertyPath));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(MultiImageButton))]
public class MultiImageButtonEditor : ButtonEditor
{
    SerializedProperty targetImages;
    SerializedProperty targetImagesSprites;

    protected override void OnEnable()
    {
        base.OnEnable();
        targetImages = serializedObject.FindProperty("targetImages");
        targetImagesSprites = serializedObject.FindProperty("targetImagesSprites");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.PropertyField(targetImages);
        EditorGUILayout.PropertyField(targetImagesSprites);
        serializedObject.ApplyModifiedProperties();
    }
 }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CrewStatusManager))]
public class CrewStatusManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //CrewStatusManager script = (CrewStatusManager)target;

        //if (GUILayout.Button("Change Status Injured"))
        //{
        //    script.ChangeInjured(script.DBG_Delta);
        //}

        //if (GUILayout.Button("Change Status Dead"))
        //{
        //    script.ChangeDead(script.DBG_Delta);
        //}
    }
}

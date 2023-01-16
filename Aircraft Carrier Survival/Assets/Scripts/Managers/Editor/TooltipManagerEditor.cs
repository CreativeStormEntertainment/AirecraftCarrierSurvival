using GambitUtils;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TooltipManager))]
public class TooltipManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Restore enums"))
        {
            TooltipManager manager = target as TooltipManager;
            manager.RestoreTooltipEnums();
            EditorUtility.SetDirty(manager);

            //var tooltipsData = manager.GetTooltipsList();
            //var sceneTooltips = SceneUtils.FindObjectsOfType<ShowTooltip>(SceneManager.GetActiveScene());
            //foreach (var tooltipData in tooltipsData)
            //{
            //    tooltipData.CastTooltipIndexToEnum();
            //}
            //foreach (var tooltip in sceneTooltips)
            //{
            //    //Debug.Log(GetType() + ": Rebing tooltips: " + tooltip.name);
            //    tooltip.CastTooltipIndexToEnum();
            //    EditorUtility.SetDirty(tooltip);
            //}

            AssetDatabase.Refresh();
        }
        DrawDefaultInspector();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class VOEditor : Editor
{
    const string VO_Path = @"Assets/Audio/VO";
    const string VO_Data_Path = @"Assets/Scripts/VO/VOData.asset";
    const string VO_Enum_Path = @"Assets/Scripts/VO/EVoiceOver.cs";
    const string enumStructurePart1 = "\npublic enum EVoiceOver\n{\n\tNone,\n";
    const string enumStructurePart2 = "}";

    [MenuItem("Tools/Localization/VO/Highlight VO data", false, 207)]
    static void Higlight()
    {
        Object obj = AssetDatabase.LoadAssetAtPath(VO_Data_Path, typeof(Object));
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
    }

    //[MenuItem("Tools/Localization/VO/Generate VO's Enum", false, 206)]
    //static void GenerateVOEnum()
    //{
    //    var dirs = Directory.GetDirectories(VO_Path);
    //    for (int i = 0; i < dirs.Length; ++i)
    //    {
    //        var files = Directory.GetFiles(dirs[i], "*.wav");
    //        var list = new List<VO>();
    //        string enumString = enumStructurePart1;
    //        List<string> toConvert = new List<string>();
    //        for (int j = 0; j < files.Length; ++j)
    //        {
    //            int index = files[j].LastIndexOf('\\') + 1;
    //            string s = files[j].Substring(index, files[j].Length - index - 4).ToUpper();
    //            enumString += "\t" + s + ",\n";
    //        }
    //        File.WriteAllText(VO_Enum_Path, enumString + enumStructurePart2);
    //        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
    //    }
    //}

    //[MenuItem("Tools/Localization/VO/Setup VO's", false, 206)]
    //static void LoadVOs()
    //{
    //    var data = AssetDatabase.LoadAssetAtPath<VOData>(VO_Data_Path);

    //    data.vosList.Clear();

    //    var dirs = Directory.GetDirectories(VO_Path);
    //    for (int i = 0; i < dirs.Length; ++i)
    //    {
    //        var files = Directory.GetFiles(dirs[i], "*.wav");
    //        var list = new List<VO>();
    //        string enumString = enumStructurePart1;
    //        for (int j = 0; j < files.Length; ++j)
    //        {
    //            int index = files[j].LastIndexOf('\\') + 1;
    //            string s = files[j].Substring(index, files[j].Length - index - 4).ToUpper().Replace("a", "").Replace("b", "");
    //            enumString += "\t"+s+",\n";
    //            list.Add(new VO(s, (EVoiceOver)System.Enum.Parse(typeof(EVoiceOver), s), AssetDatabase.LoadAssetAtPath<AudioClip>(files[j])));
    //        }
    //        if (list.Count > 0)
    //        {
    //            var VOContainer = new VOContainer(dirs[i].Substring(dirs[i].LastIndexOf('\\') + 1), list);
    //            data.vosList.Add(VOContainer);
    //        }
    //    }
    //    EditorUtility.SetDirty(data);
    //}

    //[MenuItem("Tools/Localization/VO/Convert Texts ID to VO in Tutorial Part", false, 206)]
    //static void ConvertTextsToVOs()
    //{
    //    var tutorialParts = FindObjectsOfType(typeof(TutorialPart));
    //    for (int i = 0; i < tutorialParts.Length; ++i)
    //    {
    //        TutorialPart tp = tutorialParts[i] as TutorialPart;
    //        for (int j = 0; j < tp.steps.Count; ++j)
    //        {
    //            string s = tp.steps[j].TextID.Replace('.', '_');
    //            EVoiceOver value = EVoiceOver.None;
    //            if (!string.IsNullOrEmpty(s) && !System.Enum.TryParse(s, true, out value))
    //            {
    //                Debug.LogWarning(tutorialParts[i].name + " step nr [" + j + "] - \"" + tp.steps[j].TextID + "\" parse to enum failed! - set to None");
    //            }
    //            tp.steps[j].VO = value;
    //        }
    //        EditorUtility.SetDirty(tp);
    //    }
    //}
}
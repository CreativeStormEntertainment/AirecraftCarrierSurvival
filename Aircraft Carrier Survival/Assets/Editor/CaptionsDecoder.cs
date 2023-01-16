using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class CaptionsDecoder
{
    private static string TutorialsPath = "Assets/Data/TSV/Subtitles/Tutorials/Tutorial";
    private static string BriefsPath = "Assets/Data/TSV/Subtitles/Briefs/";
    private static string FabularMaps = "Assets/GameplayAssets/ScriptableData/TacticMaps/Fabular/";

    [MenuItem("Tools/Localization/Caption", false, 202)]
    private static void Captions()
    {
        var movieMan = GameObject.FindObjectOfType<MovieManager>();
        Undo.RecordObject(movieMan, "Updated tutorial captions");
        for (int i = 0; i < (int)ETutorialType.Count; i++)
        {
            var values = TSVUtils.LoadData(TutorialsPath + (i + 1) + ".tsv");

            foreach (var data in movieMan.TutorialVideos)
            {
                if (data.Type == (ETutorialType)i)
                {
                    AddCaptions(ref data.Captions, values);
                    break;
                }
            }
        }
    }

    [MenuItem("Tools/Localization/Caption maps", false, 202)]
    private static void CaptionsMaps()
    {
        var files = Directory.GetFiles(FabularMaps, "*.asset");
        foreach (var path in files)
        {
            string briefsFile = null;
            try
            {
                var map = AssetDatabase.LoadAssetAtPath<SOTacticMap>(path);
                briefsFile = map.BriefsFile;
                if (map.BriefsFile.Length == 0 && map.Movies.Count == 0)
                {
                    continue;
                }
                var briefs = Directory.GetFiles(BriefsPath, map.BriefsFile + "*.tsv");
                Assert.IsTrue(briefs.Length == map.Movies.Count, "Miscount brief files count and map. Movies count - " + path + "; " + BriefsPath + map.BriefsFile + "*.tsv");
                foreach (var briefPath in briefs)
                {
                    int value = int.Parse(briefPath.Substring(briefPath.Length - 6, 2)) - 1;

                    AddCaptions(ref map.Movies[value].Captions, TSVUtils.LoadData(briefPath));
                }
                EditorUtility.SetDirty(map);
            }
            catch (Exception ex)
            {
                Debug.LogError(path);
                if (!string.IsNullOrWhiteSpace(briefsFile))
                {
                    Debug.LogError(BriefsPath + briefsFile);
                }
                Debug.LogException(ex);
            }
        }
        AssetDatabase.SaveAssets();
    }

    private static void AddCaptions(ref List<CaptionsData> captions, List<string[]> values)
    {
        captions = new List<CaptionsData>();
        foreach (var pair in values)
        {
            float time = int.Parse(pair[1].Substring(0, 2)) * 60f + int.Parse(pair[1].Substring(3, 2));
            if (pair[1].Length > 6)
            {
                string text = pair[1].Substring(6);
                float parts = int.Parse(text);
                for (int i = 0; i < text.Length; i++)
                {
                    parts /= 10f;
                }
                time += parts;
            }

            captions.Add(new CaptionsData(pair[0], time));
        }
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LocalizationWindow : EditorWindow
{
    private static string[] Languages = new string[]
    {
        "en", "pl", "fr", "pt", "zh"
    };

    private static int[] Values;

    private int selectedLanguage;
    private string text;
    private string id;

    [MenuItem("Tools/Localization/Editor", false, 202)]
    private static void ShowWindow()
    {
        GetWindow<LocalizationWindow>().Show();
    }

    public LocalizationWindow()
    {
        if (Values == null || Values.Length != Languages.Length)
        {
            Values = new int[Languages.Length];
            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = i;
            }
        }
    }

    private void OnGUI()
    {
        selectedLanguage = EditorGUILayout.IntPopup("Language", selectedLanguage, Languages, Values);
        text = EditorGUILayout.TextField("Localized string");
        if (GUILayout.Button("Find ID"))
        {
            var saveData = SaveManager.Instance.PersistentData;
            var locMan = LocalizationManager.Instance;
            if (saveData.Lang == Languages[selectedLanguage])
            {
                id = locMan.GetID(text);
                if (id == null)
                {
                    id = "ID not found";
                }
            }
            else
            {
                var lang = saveData.Lang;
                if (locMan.SetLanguage(Languages[selectedLanguage]))
                {
                    id = locMan.GetID(text);
                    if (id == null)
                    {
                        id = "ID not found";
                    }
                }
                else
                {
                    id = "Language not found - " + Languages[selectedLanguage];
                }
                locMan.SetLanguage(lang);
            }
        }
        if (id != null)
        {
            EditorGUILayout.LabelField(id);
        }
    }
}

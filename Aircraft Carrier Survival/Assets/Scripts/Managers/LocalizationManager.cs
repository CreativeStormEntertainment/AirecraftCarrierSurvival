using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class LocalizationManager
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Localization/Bin localization", false, 201)]
    private static void BinaryLocalization()
    {
        var localFiles = Directory.GetFiles(@"Assets\Data\TSV\Localization\", "*.tsv");
        var data = new LocalizationData();
        foreach (var filePath in localFiles)
        {
            data.Add(TSVUtils.LoadData(filePath, 0), Marker);
        }
        BinUtils.SaveBinary(data, @"Assets\Resources\Localization.bytes");
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log("LOCALIZATION SUCCESS");
    }
#endif

    public event Action LanguageChanged = delegate { };

    public const string FallbackLang = "en";
    private const string Marker = "$";

    private static LocalizationManager instance;
    public static LocalizationManager Instance
    {
        get => instance ?? (instance = new LocalizationManager());
    }

    private Dictionary<string, List<string>> currentLocalDict;
    private readonly LocalizationData data;

    //public void SetCrewVoice(VoiceManager cvm)
    //{
    //    VoiceManager = cvm;
    //    VoiceManager.SetLanguage(SaveManager.Instance.Data.Lang);
    //}

    private LocalizationManager()
    {
        data = BinUtils.LoadBinaryTextAsset<LocalizationData>("Localization");
        data.Init();
        Assert.IsTrue(data.LocalDicts.ContainsKey(FallbackLang));

        SetLanguage(SaveManager.Instance.PersistentData.Lang);
    }

    public bool SetLanguage(string lang)
    {
        var saveMan = SaveManager.Instance;
        saveMan.PersistentData.Lang = lang;

        saveMan.SavePersistentData();

        if (data.LocalDicts.TryGetValue(lang, out currentLocalDict))
        {
            LanguageChanged();
            return true;
        }
        else
        {
            currentLocalDict = data.LocalDicts[FallbackLang];
            LanguageChanged();
            return false;
        }
    }

    public string GetText(string id, params string[] param)
    {
        return GetText(id, true, param);
    }

    public string GetText(string id, bool noEmpties, params string[] param)
    {
        List<string> value;
        if (!currentLocalDict.TryGetValue(id, out value) || (noEmpties && value.Count == 0))
        {
            Debug.LogWarning(id + ((value == null) ? " - not exists" : " - empty"));
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return $"{((value == null) ? "" : "EMPTY_")}BAD_{id}_BAD";
#else
            return id;
#endif
        }
#if UNITY_EDITOR
        var set = new HashSet<int>();
        for (int i = 0; i < param.Length; i++)
        {
            set.Add(i);
        }
        bool extraMarkersNotified = false;
#endif

        var builder = new StringBuilder();
        foreach (var text in value)
        {
            if (text.StartsWith(Marker))
            {
                int index = 0;
                foreach (char c in text)
                {
                    if (char.IsDigit(c))
                    {
                        index *= 10;
                        index += c - '0';
                    }
                }
                index--;
#if UNITY_EDITOR
                set.Remove(index);
#endif
                if (index < param.Length && index >= 0)
                {
                    builder.Append(param[index]);
                }
                else
                {
                    builder.Append(text);
#if UNITY_EDITOR
                    if (!extraMarkersNotified)
                    {
                        Debug.LogWarning(id + " - extra markers");
                        extraMarkersNotified = true;
                    }
#endif
                }
            }
            else
            {
                builder.Append(text);
            }
        }
#if UNITY_EDITOR
        if (set.Count != 0)
        {
            Debug.LogWarning(id + " - extra params");
        }
#endif
        return builder.ToString();
    }

#if UNITY_EDITOR
    public string GetID(string localizedString)
    {
        foreach (var pair in currentLocalDict)
        {
            bool found = pair.Value.Count > 0;
            foreach (var part in pair.Value)
            {
                if (!part.StartsWith(Marker))
                {
                    if (!localizedString.Contains(part))
                    {
                        found = false;
                        break;
                    }
                }
            }
            if (found)
            {
                return pair.Key;
            }
        }
        return null;
    }
#endif
}

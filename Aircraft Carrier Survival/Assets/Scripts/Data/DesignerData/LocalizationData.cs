using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine;

[Serializable]
public class LocalizationData
{
    private List<string> Ignore = new List<string>()
    {
        "KnownEnemyPlayerResultUncertainPartialLosses",
        "KnownEnemyPlayerResultCertainPartialLosses",
        "KnownEnemyPlayerResultUncertainLostOneSquadron",
        "UnknownEnemyPlayerResultCertainTotalLosses",
        "UnknownEnemyPlayerResultUncertainPartialLosses",
        "UnknownEnemyPlayerResultUncertainLostOneSquadron",
        "KnownEnemyEnemyResultUncertainPartialLosses",
        "KnownEnemyEnemyResultCertainPartialLosses",
        "UnknownEnemyEnemyResultUncertainPartialLosses",
        "UnknownEnemyEnemyResultCertainTotalLosses",
        "KnownEnemyPlayerResultTooFewManeuvers",
        "KnownEnemyPlayerResultCertainTotalLosses",
        "KnownEnemyPlayerResultCertainNoLosses",
        "UnknownEnemyPlayerResultTooFewManeuvers",
        "UnknownEnemyPlayerResultCertainNoLosses",
        "KnownEnemyEnemyResultTooFewManeuvers",
        "KnownEnemyEnemyResultCertainNoLosses",
        "KnownEnemyEnemyResultCertainTotalLosses",
        "UnknownEnemyEnemyResultTooFewManeuvers",
        "UnknownEnemyEnemyResultCertainNoLosses",

        "AircraftWorkshopDestroyedDescription",
        "AircraftWorkshopNotDestroyedDescription",
        "MainGeneratorsNotDestroyedDescription",
        "BackupGeneratorsDestroyedDescription",
        "BackupGeneratorsNotDestroyedDescription",
        "ShutdownAircraftWorkshopSectionTitle",
        "ShutdownBackupGeneratorsSectionTitle",
        "DetectionStatusChangedEventUndetected",
    };

    private readonly List<string> keys;
    private readonly List<string> langs;
    private readonly List<List<List<string>>> values;

    public Dictionary<string, Dictionary<string, List<string>>> LocalDicts;

#if UNITY_EDITOR
    public LocalizationData()
    {
        keys = new List<string>();
        langs = new List<string>();
        values = new List<List<List<string>>>();
    }

    public void Add(List<string[]> data, string marker)
    {
        Assert.IsTrue(data[0][0].Length == 0);

        foreach (var item in data)
        {
            for (int i = 0; i < item.Length; i++)
            {
                item[i] = item[i].TrimEnd();
            }
        }
        var langColumnDict = new Dictionary<int, int>();
        if (langs.Count == 0)
        {
            Assert.IsTrue(keys.Count == 0);
            Assert.IsTrue(values.Count == 0);

            for (int i = 1; i < data[0].Length; i++)
            {
                langs.Add(data[0][i]);
                langColumnDict.Add(i, values.Count);
                values.Add(new List<List<string>>());
            }
        }
        else
        {
            Assert.IsTrue(langs.Count == values.Count);
            int i;
            for (i = 1; i < data[0].Length; i++)
            {
                int index = langs.IndexOf(data[0][i]);
                if (index == -1)
                {
                    index = langs.Count;
                    langs.Add(data[0][i]);
                    var list = new List<List<string>>();
                    for (int j = 0; j < keys.Count; j++)
                    {
                        list.Add(new List<string>());
                    }
                    values.Add(list);
                }
                langColumnDict.Add(i, index);
            }
            Assert.IsFalse(langColumnDict.Count > langs.Count);
            if (langColumnDict.Count < langs.Count)
            {
                for (int j = 0; j < langs.Count; j++)
                {
                    if (!langColumnDict.ContainsValue(j))
                    {
                        langColumnDict[i++] = j;
                    }
                }
            }
        }
        Assert.IsTrue(langs.Count == values.Count);
        for (int i = 1; i < data.Count; i++)
        {
            var key = data[i][0].Trim();
            if (key == "")
            {
                Debug.LogError(data[i - 1][0]);
                Assert.IsTrue(false);
            }
            Assert.IsFalse(keys.Contains(key), key + " is duplicated");
            if (key.Length > 35 && !Ignore.Contains(key))
            {
                Debug.Log("Very long key, are you sure it's a key?: " + key);
            }
            if (key.Any(x => char.IsWhiteSpace(x)))
            {
                Debug.Log("Key with whitechars, are you sure it's a key?: " + key);
            }
            keys.Add(key);
            int j = 1;
            for (; j < data[i].Length; j++)
            {
                var list = new List<string>();
                string value = data[i][j].Trim();
                int index = value.IndexOf(marker);
                while (index != -1)
                {
                    int length = 1;
                    while (value.Length > (index + length) && char.IsDigit(value[index + length]))
                    {
                        length++;
                    }
                    if (length == 1)
                    {
                        Debug.Log(key);
                    }
                    Assert.IsFalse(length == 1);
                    list.Add(value.Substring(0, index));
                    list.Add(value.Substring(index, length));
                    value = value.Substring(index + length);
                    index = value.IndexOf(marker);
                }
                if (value.Length > 0)
                {
                    list.Add(value);
                }
                if (!langColumnDict.ContainsKey(j))
                {
                    Debug.LogError("This key has more values than languages: " + key);
                }
                values[langColumnDict[j]].Add(list);
            }
            for (; j <= langs.Count; j++)
            {
                values[langColumnDict[j]].Add(new List<string>());
            }
        }
    }
#else
    private LocalizationData()
    {
        
    }
#endif

    public void Init()
    {
        LocalDicts = new Dictionary<string, Dictionary<string, List<string>>>();

        Assert.IsTrue(langs.Count == values.Count);
        for (int i = 0; i < langs.Count; i++)
        {
            var dict = new Dictionary<string, List<string>>();
            Assert.IsTrue(values[i].Count == keys.Count);
            for (int j = 0; j < keys.Count; j++)
            {
                Assert.IsFalse(dict.ContainsKey(keys[j]));
                dict[keys[j]] = values[i][j];
            }
            Assert.IsFalse(LocalDicts.ContainsKey(langs[i]));
            LocalDicts[langs[i]] = dict;
        }

        keys.Clear();
        langs.Clear();
        values.Clear();
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public static class TSVUtils
{
#if UNITY_EDITOR
    const char CellSeparator = '\t';
    static readonly string[] CellDataSeparator = { ", " };
    public static List<string[]> LoadData(string path, int startIndex = 1)
    {
        var result = new List<string[]>();
        var lines = Regex.Split(File.ReadAllText(path), "\r|\n|\r\n");
        for (int i = startIndex; i < lines.Length; i++)
        {
            if (lines[i].Length > 0)
            {
                //Assert.IsFalse(lines[i].Contains("\""));
                result.Add(lines[i].Split(CellSeparator));
            }
        }
        return result;
    }

    public static string[] SplitCellData(string cellData)
    {
        return cellData.Split(CellDataSeparator, StringSplitOptions.RemoveEmptyEntries);
    }

    public static List<int> GetEffectIndices(EffectManager effMan, string data)
    {
        var result = new List<int>();
        var effectsNames = SplitCellData(data);
        foreach (var name in effectsNames)
        {
            result.Add(effMan.GetEffectIndex(name));
        }
        return result;
    }

    public static T ParseEnum<T>(string data)
    {
        return (T)Enum.Parse(typeof(T), data);
    }
#endif
}

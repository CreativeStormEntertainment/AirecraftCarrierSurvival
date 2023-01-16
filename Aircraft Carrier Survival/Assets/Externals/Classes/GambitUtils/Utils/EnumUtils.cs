using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GambitUtils
{
    public struct EnumUtils
    {
        public static string[] GetNames<T>()
        {
            return Enum.GetNames(typeof(T));
        }

        public static List<string> GetNamesList<T>()
        {
            return Enum.GetNames(typeof(T)).ToList();
        }

        public static T[] GetValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static List<T> GetList<T>()
        {
            return GetValues<T>().ToList();
        }

        public static void AddEnumNamesWithPrefixToFloatDictionary<T>(Dictionary<string, float> dict, string prefix)
        {
            List<string> names = EnumUtils.GetNamesList<T>();
            foreach (var name in names)
            {
                if (name.StartsWith(prefix) && !dict.ContainsKey(name))
                {
                    dict.Add(name, 0);
                }
            }
        }

        public static void AddEnumNamesWithPrefixToList<T>(List<string> list, string prefix)
        {
            List<string> names = EnumUtils.GetNamesList<T>();
            names = names.FindAll(name => name.StartsWith(prefix) && !list.Contains(name));
            list.AddRange(names);
        }

        public static void AddEnumNamesToList<T>(List<string> list)
        {
            List<string> names = EnumUtils.GetNamesList<T>();
            names = names.FindAll(name => !list.Contains(name));
            list.AddRange(names);
        }

        public static void PrintEnumList<T>(Func<string, string> getLine) where T : System.Enum
        {
            var names = EnumUtils.GetNamesList<T>();
            string log = "\n";
            foreach (var name in names)
            {
                log += getLine(name);
            }
            Debug.Log("EnumUtils.PrintEnumList: " + names.Count + log);
        }
    }
}

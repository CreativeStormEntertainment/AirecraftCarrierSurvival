using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GambitUtils
{
    public struct JsonUtils
    {
        public static T LoadFromJson<T>(string filepath)
        {
            if (File.Exists(filepath))
            {
                byte[] data = File.ReadAllBytes(filepath);
                string json = Encoding.ASCII.GetString(data);
                return JsonUtility.FromJson<T>(json);
            }
            return default;
        }

        public static T LoadFromJsonFromResources<T>(string filepath)
        {
            string extension = Path.GetExtension(filepath);
            if (!string.IsNullOrEmpty(extension))
                filepath = filepath.TrimEnd(extension.ToCharArray());
            TextAsset jsonTextFile = Resources.Load<TextAsset>(filepath);
            if (jsonTextFile)
            {
                return JsonUtility.FromJson<T>(jsonTextFile.text);
            }
            else
            {
                Debug.LogError("JsonUtils.LoadFromJsonFromResources: File could not be loaded from " + filepath);
                return default;
            }
        }

        public static void SaveToJson<T>(T serializable, string filepath, Encoding encoding)
        {
            string json = JsonUtility.ToJson(serializable);
            byte[] bytes = encoding.GetBytes(json);
            File.WriteAllBytes(filepath, bytes);
        }

        public static void SaveToJson<T>(T serializable, string filepath)
        {
            SaveToJson(serializable, filepath, Encoding.ASCII);
        }
    }
}

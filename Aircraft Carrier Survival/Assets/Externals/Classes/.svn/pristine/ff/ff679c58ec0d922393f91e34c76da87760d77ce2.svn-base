using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions;

namespace GambitUtils
{
    public static class BinUtils
    {
        public static BinaryFormatter Formatter { get; } = new BinaryFormatter();

        public static string GetHash<T>(T data) where T : class
        {
            Assert.IsNotNull(data);
            using (var stream = new MemoryStream())
            {
                Formatter.Serialize(stream, data);
                using (var md5 = MD5.Create())
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        public static void SaveToBinary<T>(T contents, string folderPath, string filename) where T : class
        {
            SaveToBinary(contents, Path.Combine(folderPath, filename));
        }

        public static void SaveToBinary<T>(T contents, string path) where T : class
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                Formatter.Serialize(stream, contents);
            }
        }

        public static T LoadFromBinary<T>(string folderPath, string filename) where T : class
        {
            return LoadFromBinary<T>(Path.Combine(folderPath, filename));
        }

        public static T LoadFromBinary<T>(string path) where T : class
        {
            if (File.Exists(path))
            {
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    return Formatter.Deserialize(stream) as T;
                }
            }
            else
            {
                return null;
            }
        }

        public static T LoadFromTextAssetFromResources<T>(string pathInResources) where T : class
        {
            var asset = Resources.Load<TextAsset>(pathInResources);
            if (asset != null)
            {
                using (var stream = new MemoryStream(asset.bytes))
                {
                    return Formatter.Deserialize(stream) as T;
                }
            }
            return null;
        }
    }
}

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions;

public static class BinUtils
{
    private static BinaryFormatter formatter = null;
    public static BinaryFormatter Formatter
    {
        get
        {
            return formatter ?? (formatter = new BinaryFormatter());
        }
    }

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

    public static void SaveObjectAsBinaryFile<T>(T contents, string pathToFolder, string filename) where T : class
    {
        SaveBinary(contents, Path.Combine(pathToFolder, filename + ".bytes"));
    }

    public static void SaveBinary<T>(T contents, string path) where T : class
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            Formatter.Serialize(stream, contents);
        }
    }

    public static T LoadObjectFromBinaryFile<T>(string pathToFolder, string filename) where T : class
    {
        string path = Path.Combine(pathToFolder, filename + ".bytes");

        if (File.Exists(path))
        {
            return LoadBinary<T>(path);
        }
        else
        {
            return null;
        }
    }

    public static T LoadBinary<T>(string path) where T : class
    {
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return Formatter.Deserialize(stream) as T;
        }
    }

    public static T LoadBinaryTextAsset<T>(string path) where T : class
    {
        var asset = Resources.Load<TextAsset>(path);
        if (asset != null)
        {
            using (var stream = new MemoryStream(asset.bytes))
            {
                return Formatter.Deserialize(stream) as T;
            }
        }
        return null;
    }

    public static int GetBits(int value, int leftBit, int rightBit)
    {
        int moveLeft = 31 - leftBit;
        value <<= moveLeft;
        value >>= (moveLeft + rightBit);
        return value;
    }

    public static int CountBits(int bits)
    {
        int result;
        for (result = 0; bits != 0; result++)
        {
            bits &= (bits - 1);
        }
        return result;
    }

    public static int ExtractData(int data, int size, int dataIndex)
    {
        int rightBit = dataIndex * size;
        return GetBits(data, rightBit + size - 1, rightBit);
    }

    public static long GetBits(long value, int leftBit, int rightBit)
    {
        int moveLeft = 31 - leftBit;
        value <<= moveLeft;
        value >>= (moveLeft + rightBit);
        return value;
    }

    public static long ExtractData(long data, int size, int dataIndex)
    {
        int rightBit = dataIndex * size;
        return GetBits(data, rightBit + size - 1, rightBit);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class RandomUtils
{
    public static T GetRandom<T>(ICollection<T> collection)
    {
        Assert.IsFalse(collection.Count == 0);
        int index = Random.Range(0, collection.Count);
        using (var enumerator = collection.GetEnumerator())
        {
            for (int i = 0; i <= index; i++)
            {
                bool next = enumerator.MoveNext();
                Assert.IsTrue(next);
            }
            return enumerator.Current;
        }
    }

    public static T GetRandom<T>(List<T> list)
    {
        Assert.IsFalse(list.Count == 0);
        return list[Random.Range(0, list.Count)];
    }

    public static T GetRemoveRandom<T>(List<T> list)
    {
        Assert.IsFalse(list.Count == 0);
        int index = Random.Range(0, list.Count);
        var result = list[index];
        list.RemoveAt(index);
        return result;
    }

    public static int GetAndFillRandomIndex(HashSet<int> set, int count)
    {
        if (set.Count == 0)
        {
            for (int i = 0; i < count; i++)
            {
                set.Add(i);
            }
        }
        Assert.IsFalse(count == 0);
        Assert.IsFalse(set.Count == 0);
        return GetRandom(set);
    }
}

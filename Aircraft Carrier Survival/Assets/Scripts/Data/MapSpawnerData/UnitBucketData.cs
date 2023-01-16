using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class UnitBucketData
{
    [SerializeField]
    private List<CategoryBucketData> levelBucketData = null;

    [SerializeField]
    private List<EnemyUnitData> easyFleets = null;
    [SerializeField]
    private List<EnemyUnitData> mediumFleets = null;
    [SerializeField]
    private List<EnemyUnitData> hardFleets = null;
    [SerializeField]
    private List<EnemyUnitData> veryHardFleets = null;

    private int currentLv;
    private List<EnemyUnitData> bucket;
    private SandboxAdmiralLevels levelsData;

    public void Setup(SandboxAdmiralLevels levelsData)
    {
        this.levelsData = levelsData;
        currentLv = -1;

        for (int i = 1; i < levelBucketData.Count; i++)
        {
            var data1 = levelBucketData[i - 1];
            var data2 = levelBucketData[i];
            Assert.IsTrue(data2.Level > data1.Level);
            Assert.IsTrue((data2.Level - data1.Level) < 10);
            while ((data2.Level - data1.Level) != 1)
            {
                data1 = data1.DuplicateNextLevel();
                levelBucketData.Insert(i++, data1);
            }
        }
        bucket = new List<EnemyUnitData>();

        Setup(easyFleets, 0);
        Setup(mediumFleets, 100);
        Setup(hardFleets, 200);
        Setup(veryHardFleets, 300);
    }

    public EnemyUnitData FromIndex(int index)
    {
        if (index < 100)
        {
            return easyFleets[Mathf.Min(index, easyFleets.Count - 1)];
        }
        else if (index < 200)
        {
            return mediumFleets[Mathf.Min(index - 100, mediumFleets.Count - 1)];
        }
        else if (index < 300)
        {
            return hardFleets[Mathf.Min(index - 200, hardFleets.Count - 1)];
        }
        else
        {
            Assert.IsTrue(index < 400);
            return veryHardFleets[Mathf.Min(index - 300, veryHardFleets.Count - 1)];
        }
    }

    public EnemyUnitData Get()
    {
        int newLv = levelsData.GetAdmiralLevel(SaveManager.Instance.Data.MissionRewards.SandboxAdmiralExp);
        if (currentLv != newLv || bucket.Count == 0)
        {
            currentLv = newLv;
            bucket.Clear();

            var data = levelBucketData[Mathf.Min(currentLv, levelBucketData.Count) - 1];
            FillBucket(data.Easy, easyFleets);
            FillBucket(data.Medium, mediumFleets);
            FillBucket(data.Hard, hardFleets);
            FillBucket(data.VeryHard, veryHardFleets);
        }

        int index = UnityEngine.Random.Range(0, bucket.Count);
        var result = bucket[index];
        bucket.RemoveAt(index);
        return result;
    }

    public void Save(List<ListInt> save)
    {
        save.Clear();
        save.Add(new ListInt());
        save.Add(new ListInt());
        save.Add(new ListInt());
        save.Add(new ListInt ());
        foreach (var unit in bucket)
        {
            int index = easyFleets.IndexOf(unit);
            if (index >= 0)
            {
                save[0].List.Add(index);
                break;
            }
            index = mediumFleets.IndexOf(unit);
            if (index >= 0)
            {
                save[1].List.Add(index);
                break;
            }
            index = hardFleets.IndexOf(unit);
            if (index >= 0)
            {
                save[2].List.Add(index);
                break;
            }
            index = veryHardFleets.IndexOf(unit);
            Assert.IsFalse(index < 0, unit.BuildingBlocks.Count + ";" + ((unit.BuildingBlocks.Count > 0) ? unit.BuildingBlocks[0].name : ""));
            save[3].List.Add(index);
        }
    }

    public void Load(List<ListInt> save)
    {
        bucket.Clear();

        currentLv = levelsData.GetAdmiralLevel(SaveManager.Instance.Data.MissionRewards.SandboxAdmiralExp);

        SetUnits(save, 0, easyFleets);
        SetUnits(save, 1, mediumFleets);
        SetUnits(save, 2, hardFleets);
        SetUnits(save, 3, veryHardFleets);
    }

    public void Check(EnemyUnitData data)
    {
        Assert.IsFalse(easyFleets.Contains(data));
        Assert.IsFalse(mediumFleets.Contains(data));
        Assert.IsFalse(hardFleets.Contains(data));
        Assert.IsFalse(veryHardFleets.Contains(data));
    }

    private void Setup(List<EnemyUnitData> data, int index)
    {
        Assert.IsFalse(data.Count > 90);
        for (int i = 0; i < data.Count; i++)
        {
            data[i].SaveIndex = index + i;
        }
    }

    private void FillBucket(int count, List<EnemyUnitData> list)
    {
        int startIndex = bucket.Count;
        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, list.Count);
            bucket.Add(list[index]);
            list.RemoveAt(index);
        }
        for (int i = startIndex; i < bucket.Count; i++)
        {
            list.Add(bucket[i]);
        }
        list.Sort((x, y) => Comparer<int>.Default.Compare(x.SaveIndex, y.SaveIndex));
    }

    private void SetUnits(List<ListInt> save, int index, List<EnemyUnitData> units)
    {
        foreach (int index2 in save[index].List)
        {
            bucket.Add(units[index2]);
        }
    }
}

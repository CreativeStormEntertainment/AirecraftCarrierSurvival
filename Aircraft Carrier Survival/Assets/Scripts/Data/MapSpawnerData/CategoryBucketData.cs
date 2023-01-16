using System;

[Serializable]
public class CategoryBucketData
{
    public int Level;
    public int Easy;
    public int Medium;
    public int Hard;
    public int VeryHard;

    public CategoryBucketData DuplicateNextLevel()
    {
        var result = new CategoryBucketData();

        result.Level = Level + 1;
        result.Easy = Easy;
        result.Medium = Medium;
        result.Hard = Hard;
        result.VeryHard = VeryHard;

        return result;
    }
}

using System;

[Serializable]
public class SaveDataWrapper
{
    public SaveData Data;
    public string Checksum;

    public void SetData(SaveData data)
    {
        Data = data;
        Checksum = BinUtils.GetHash(data);
    }
}

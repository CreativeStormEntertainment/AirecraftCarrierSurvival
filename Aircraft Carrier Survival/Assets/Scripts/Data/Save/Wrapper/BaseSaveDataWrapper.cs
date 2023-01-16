using System;
using UnityEngine;

[Serializable]
public abstract class BaseSaveDataWrapper
{
    public string Checksum => checksum;

    [SerializeField]
    private string checksum = default;

    public abstract BaseSaveData Get();
    public virtual void Set(BaseSaveData data)
    {
        checksum = BinUtils.GetHash(data);
    }

}

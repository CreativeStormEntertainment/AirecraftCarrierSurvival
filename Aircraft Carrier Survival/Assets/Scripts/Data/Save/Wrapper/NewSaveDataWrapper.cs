using System;
using UnityEngine;

[Serializable]
public class NewSaveDataWrapper<T> : BaseSaveDataWrapper where T : BaseSaveData
{
    [SerializeField]
    private T data = default;

    public override BaseSaveData Get()
    {
        return data;
    }

    public override void Set(BaseSaveData data)
    {
        this.data = (T)data;
        base.Set(data);
    }
}

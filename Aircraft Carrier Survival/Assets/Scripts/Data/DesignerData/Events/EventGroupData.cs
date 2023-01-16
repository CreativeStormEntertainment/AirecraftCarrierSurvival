using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

[Serializable]
public class EventGroupData
{
#if UNITY_EDITOR
    static class DataOrder
    {
        public const int Type = 0;
        public const int Modifiers = 1;
    }

    public EventGroupData(string[] data)
    {
        Assert.IsTrue(data.Length >= DataOrder.Modifiers);
        Type = TSVUtils.ParseEnum<EEventGroup>(data[DataOrder.Type]);
        Modifiers = new List<float>();
        for (int i = DataOrder.Modifiers; i < data.Length; i++)
        {
            Modifiers.Add(float.Parse(data[i]));
        }
    }
#endif
    private EventGroupData() { }

    public EEventGroup Type;
    public List<float> Modifiers;

}

//using System;
//using UnityEngine.Assertions;

//[Serializable]
//public class ConstEventData : EventData
//{
//#if UNITY_EDITOR
//    static class DataOrder
//    {
//        public const int EventName = 0;
//        public const int EventTrigger = 1;
//        public const int Effects = 2;
//        public const int EventType = 3;
//        public const int Time = 4;
//        public const int Description = 5;
//        public const int Count = 6;
//    }

//    public ConstEventData(EffectManager effMan, string[] data)
//    {
//        Assert.IsTrue(data.Length == DataOrder.Count);
//        Name = data[DataOrder.EventName];
//        Trigger = TSVUtils.ParseEnum<EEventTrigger>(data[DataOrder.EventTrigger]);
//        //EventType = TSVUtils.ParseEnum<EEventType>(data[DataOrder.EventType]);
//        TickTime = int.Parse(data[DataOrder.Time]);
//        Description = data[DataOrder.Description];

//        EffectIndices = TSVUtils.GetEffectIndices(effMan, data[DataOrder.Effects]);
//    }
//#endif

//    private ConstEventData() { }

//    public EEventTrigger Trigger;
//}

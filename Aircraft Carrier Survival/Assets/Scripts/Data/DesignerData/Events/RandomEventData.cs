//using System;
//using UnityEngine.Assertions;

//[Serializable]
//public class RandomEventData
//{
//#if UNITY_EDITOR
//    static class DataOrder
//    {
//        public const int EventName = 0;
//        public const int EventGroup = 1;
//        public const int Probability = 2;
//        public const int Effects = 3;
//        public const int EventType = 4;
//        public const int Time = 5;
//        public const int Description = 6;
//        public const int Count = 7;
//    }

//    public RandomEventData(EffectManager effMan, string[] data)
//    {
//        Assert.IsTrue(data.Length == DataOrder.Count);
//        Name = data[DataOrder.EventName];
//        Group = TSVUtils.ParseEnum<EEventGroup>(data[DataOrder.EventGroup]);
//        Probability = float.Parse(data[DataOrder.Probability]);
//        //EventType = TSVUtils.ParseEnum<EEventType>(data[DataOrder.EventType]);
//        TickTime = int.Parse(data[DataOrder.Time]);
//        Description = data[DataOrder.Description];

//        EffectIndices = TSVUtils.GetEffectIndices(effMan, data[DataOrder.Effects]);
//    }
//#endif

//    private RandomEventData() { }

//    public EEventGroup Group;
//    public float Probability;
//}

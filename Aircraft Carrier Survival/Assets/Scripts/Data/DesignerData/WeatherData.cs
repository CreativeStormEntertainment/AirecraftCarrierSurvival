using System;
using UnityEngine.Assertions;

[Serializable]
public class WeatherData : EffectsDataWrapper
{
#if UNITY_EDITOR
    static class DataOrder
    {
        public const int Type = 0;
        public const int Effects = 1;
        public const int Count = 2;
    }

    public WeatherData(EffectManager effMan, string[] data)
    {
        Assert.IsTrue(data.Length == DataOrder.Count);
        Type = TSVUtils.ParseEnum<EWeatherType>(data[DataOrder.Type]);
        EffectIndices = TSVUtils.GetEffectIndices(effMan, data[DataOrder.Effects]);
    }
#endif

    private WeatherData() { }

    public EWeatherType Type;
}

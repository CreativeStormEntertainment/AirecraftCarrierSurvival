using UnityEngine;

namespace GambitUtils
{
    public static class TypeConverter
    {
        public static short ConvertFloatToShort(float value, float max)
        {
            value = Mathf.Clamp01(value / max) * short.MaxValue;
            return (short)value;
        }

        public static float ConvertShortToFloat(short value, float max)
        {
            return value * max / short.MaxValue;
        }
    }
}

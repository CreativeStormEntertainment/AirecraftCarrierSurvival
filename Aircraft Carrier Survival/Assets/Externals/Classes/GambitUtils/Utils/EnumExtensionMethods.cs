using System;
using System.Collections.Generic;
using System.Linq;

namespace GambitUtils
{
    public static class EnumExtensionMethods
    {
        public static IEnumerable<string> CastToStringEnumerable(this Enum[] array)
        {
            return array.Select(enumName => enumName.ToString());
        }

        public static List<string> CastToStringList(this Enum[] array)
        {
            return CastToStringEnumerable(array).ToList();
        }
    }
}

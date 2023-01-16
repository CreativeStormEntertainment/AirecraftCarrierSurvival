using UnityEngine;

namespace GambitUtils
{
    public static class ProjectionExtensionMethods
    {
        public static float GetTanHalfFovFromProjection(this Matrix4x4 m)
        {
            // assymetric: m11 = 2n/(t-b)
            // symmetric: m11 = n/t
            return m.m11 >= 0.000001f ? 1f / m.m11 : 0f;
        }

        public static float GetFovFromProjection(this Matrix4x4 m)
        {
            // assymetric: m11 = 2n/(t-b)
            // symmetric: m11 = n/t
            return Mathf.Atan2(1f, m.m11) * 2f * Mathf.Rad2Deg;
        }
    }
}

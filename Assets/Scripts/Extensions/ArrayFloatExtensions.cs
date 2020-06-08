using UnityEngine;

namespace Extensions
{
    public static class ArrayFloatExtensions
    {
        public static void Normalize(this float[] values, float minValue, float maxValue)
        {
            for (var i = 0; i < values.Length; i++)
                values[i] = (values[i] - minValue) / (maxValue - minValue);
        }

        public static void InverseLerp(this float[] values, float minValue, float maxValue)
        {
            for (var i = 0; i < values.Length; i++)
                values[i] = Mathf.InverseLerp(minValue, maxValue, values[i]);
        }
    }
}
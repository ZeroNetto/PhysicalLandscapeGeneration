using System.Linq;
using UnityEngine;

namespace Extensions
{
    public static class MapExtension
    {
        public static float[] Normalize(this float[] map, float minValue, float maxValue)
        {
            return map
                .Select(height => (height - minValue) / (maxValue - minValue))
                .ToArray();
        }

        public static float[] InverseLerp(this float[] map, float minValue, float maxValue)
        {
            return map
                .Select(height => Mathf.InverseLerp(minValue, maxValue, height))
                .ToArray();
        }
    }
}
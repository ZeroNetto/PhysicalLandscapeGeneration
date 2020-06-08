using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Extensions
{
    public static class IEnumerableFloatExtensions
    {
        public static IEnumerable<float> Normalize(this IEnumerable<float> values, float minValue, float maxValue)
        {
            return values.Select(val => (val - minValue) / (maxValue - minValue));
        }

        public static IEnumerable<float> InverseLerp(this IEnumerable<float> values, float minValue, float maxValue)
        {
            return values.Select(val => Mathf.InverseLerp(minValue, maxValue, val));
        }
    }
}
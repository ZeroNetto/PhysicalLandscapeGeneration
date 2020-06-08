using System.Linq;

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
    }
}
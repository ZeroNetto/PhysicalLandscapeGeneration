using UnityEngine;

namespace Components.Map
{
    public struct MapInfoComponent
    {
        public int Seed { get; set; }
        public bool RandomizeSeed { get; set; }
        public int OctaveCount { get; set; }
        public float Persistence { get; set; }
        public float Lacunarity { get; set; }
        public float InitialScale { get; set; }
        public ComputeShader HeightMapComputeShader { get; set; }
        public float[] Map { get; set; }
        public int MapSizeWithBorder { get; set; }
        public Mesh Mesh { get; set; }
    }
}
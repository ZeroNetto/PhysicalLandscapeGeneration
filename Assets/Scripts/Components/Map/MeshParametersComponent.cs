using UnityEngine;

namespace Components.Map
{
    public struct MeshParametersComponent
    {
        public int MapSize { get; set; }
        public float Scale { get; set; }
        public float ElevationScale { get; set; }
        public Material Material { get; set; }
    }
}
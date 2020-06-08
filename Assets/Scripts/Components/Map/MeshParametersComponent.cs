using UnityEngine;

namespace Components.Map
{
    public struct MeshParametersComponent
    {
        public int MapResolution { get; set; }
        public float Scale { get; set; }
        public float ElevationScale { get; set; }
        public Material Material { get; set; }
    }
}
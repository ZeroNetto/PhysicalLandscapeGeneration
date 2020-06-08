using UnityEngine;

namespace Helpers.Structires
{
    public struct BrushShader
    {
        public ComputeBuffer IndexBuffer { get; set; }
        public ComputeBuffer WeightBuffer { get; set; }
    }
}
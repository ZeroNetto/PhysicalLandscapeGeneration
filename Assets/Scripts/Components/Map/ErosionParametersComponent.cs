using UnityEngine;

namespace Components.Map
{
    public struct ErosionParametersComponent
    {
        public int MaxLifetime { get; set; }
        public float Inertia { get; set; }

        public float SedimentCapacityFactor { get; set; }
        public float MinSedimentCapacity { get; set; }
        public float DepositSpeed { get; set; }
        public float ErodeSpeed { get; set; }
        public float EvaporateSpeed { get; set; }
        public float Gravity { get; set; }
        public float StartSpeed { get; set; }
        public float StartWater { get; set; }
        public int ErosionIterationCount { get; set; }
        public int ErosionBrushRadius { get; set; }
        public ComputeShader Erosion { get; set; }
    }
}
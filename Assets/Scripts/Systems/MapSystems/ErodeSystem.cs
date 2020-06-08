using System.Collections.Generic;
using System.Linq;
using Components.Events;
using Components.Map;
using Helpers;
using Helpers.Structires;
using Leopotam.Ecs;
using UnityEngine;

namespace Systems.MapSystems
{
    public class ErodeSystem : IEcsRunSystem
    {
        private EcsFilter<ErodeEvent, ErosionParametersComponent, MeshParametersComponent, MapInfoComponent> erosionMaps;
        private bool PrintTimers;

        public void Run()
        {
            foreach (var erosionId in erosionMaps)
            {
                var erosionEntity = erosionMaps.GetEntity(erosionId);
                var erosionParameters = erosionEntity.Get<ErosionParametersComponent>();
                var meshParameters = erosionEntity.Get<MeshParametersComponent>();
                var mapInfo = erosionEntity.Get<MapInfoComponent>();

                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                
                var brush = ErodeHelper.CreateBrush(erosionParameters, meshParameters.MapSize);
                var brushShader = new BrushShader()
                {
                    IndexBuffer = ErodeHelper.GetBufferFor(brush.IndexOffsets, erosionParameters.Erosion,
                        "brushIndices"),
                    WeightBuffer =
                        ErodeHelper.GetBufferFor(brush.Weights, erosionParameters.Erosion, "brushWeights")
                };
                var randomIndices = ErodeHelper.GenerateIndices(
                    erosionParameters.ErosionIterationCount,
                    erosionParameters.ErosionBrushRadius,
                    meshParameters.MapSize);
                var randomIndexBuffer = ErodeHelper.GetBufferFor(randomIndices, erosionParameters.Erosion, "randomIndices");
                var mapBuffer = ErodeHelper.GetBufferFor(mapInfo.Map, erosionParameters.Erosion, "map");
                SetErosionParameters(erosionParameters, mapInfo, brush);
                
                erosionParameters.Erosion.Dispatch(
                    0, 
                    erosionParameters.ErosionIterationCount / 1024, 
                    1, 
                    1);
                
                ErodeHelper.ReleaseBuffers(new List<ComputeBuffer>()
                {
                    brushShader.IndexBuffer,
                    brushShader.WeightBuffer,
                    mapBuffer,
                    randomIndexBuffer
                });

                var erosionTime = sw.ElapsedMilliseconds;
                sw.Reset();
                
                if (PrintTimers)
                    Debug.Log ($"{erosionParameters.ErosionIterationCount} erosion iterations completed in {erosionTime}ms");

                erosionEntity.Del<ErodeEvent>();
            }
        }

        private void SetErosionParameters(
            ErosionParametersComponent erosionParameters,  MapInfoComponent mapInfo, Brush brush)
        {
            var erosion = erosionParameters.Erosion;
            
            erosion.SetInt("borderSize", erosionParameters.ErosionBrushRadius);
            erosion.SetInt("mapSize", mapInfo.MapSizeWithBorder);
            erosion.SetInt("brushLength", brush.IndexOffsets.Count);
            erosion.SetInt("maxLifetime", erosionParameters.MaxLifetime);
            erosion.SetFloat("inertia", erosionParameters.Inertia);
            erosion.SetFloat("sedimentCapacityFactor", erosionParameters.SedimentCapacityFactor);
            erosion.SetFloat("minSedimentCapacity", erosionParameters.MinSedimentCapacity);
            erosion.SetFloat("depositSpeed", erosionParameters.DepositSpeed);
            erosion.SetFloat("erodeSpeed", erosionParameters.ErodeSpeed);
            erosion.SetFloat("evaporateSpeed", erosionParameters.EvaporateSpeed);
            erosion.SetFloat("gravity", erosionParameters.Gravity);
            erosion.SetFloat("startSpeed", erosionParameters.StartSpeed);
            erosion.SetFloat("startWater", erosionParameters.StartWater);
        }
    }
}
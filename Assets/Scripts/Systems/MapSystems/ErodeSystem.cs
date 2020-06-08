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

        public void Run()
        {
            foreach (var erosionId in erosionMaps)
            {
                var erosionEntity = erosionMaps.GetEntity(erosionId);
                var printTimers = erosionEntity.Get<ErodeEvent>().PrintTimers;
                var erosionParameters = erosionEntity.Get<ErosionParametersComponent>();
                var meshParameters = erosionEntity.Get<MeshParametersComponent>();
                var mapInfo = erosionEntity.Get<MapInfoComponent>();

                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                
                var brush = GeneralHelper.CreateBrush(erosionParameters, meshParameters.MapSize);
                var brushShader = new BrushShader()
                {
                    IndexBuffer = GeneralHelper.GetBufferFor(brush.IndexOffsets, erosionParameters.Erosion,
                        "brushIndices"),
                    WeightBuffer =
                        GeneralHelper.GetBufferFor(brush.Weights, erosionParameters.Erosion, "brushWeights")
                };
                var randomIndices = GeneralHelper.GenerateIndices(
                    erosionParameters.ErosionIterationCount,
                    erosionParameters.ErosionBrushRadius,
                    meshParameters.MapSize);
                var randomIndexBuffer = GeneralHelper.GetBufferFor(randomIndices, erosionParameters.Erosion, "randomIndices");
                var mapBuffer = GeneralHelper.GetBufferFor(mapInfo.Map, erosionParameters.Erosion, "map");
                
                SetErosionParameters(erosionParameters, mapInfo, brush);
                
                erosionParameters.Erosion.Dispatch(
                    0, 
                    erosionParameters.ErosionIterationCount / 1024, 
                    1, 
                    1);
                
                GeneralHelper.ReleaseBuffers(new List<ComputeBuffer>()
                {
                    brushShader.IndexBuffer,
                    brushShader.WeightBuffer,
                    mapBuffer,
                    randomIndexBuffer
                });

                var erosionTime = sw.ElapsedMilliseconds;
                sw.Reset();
                
                if (printTimers)
                    Debug.Log ($"{erosionParameters.ErosionIterationCount} erosion iterations completed in {erosionTime}ms");

                erosionEntity.Del<ErodeEvent>();
                erosionEntity.Get<ConstructMeshEvent>() = new ConstructMeshEvent() {PrintTimers = printTimers};;
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
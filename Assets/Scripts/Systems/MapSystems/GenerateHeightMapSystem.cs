using System;
using System.Linq;
using Components.Map;
using Leopotam.Ecs;
using Components.Events;
using Extensions;
using Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.MapSystems
{
    public class GenerateHeightMapSystem : IEcsRunSystem
    {
        private EcsFilter<GenerateHeightMapEvent, MapInfoComponent> generateMaps;

        public void Run()
        {
            foreach (var mapId in generateMaps)
            {
                var mapEntity = generateMaps.GetEntity(mapId);
                var printTimers = mapEntity.Get<GenerateHeightMapEvent>().PrintTimers;
                var onGPU = mapEntity.Get<GenerateHeightMapEvent>().OnGPU;
                var mapInfo = mapEntity.Get<MapInfoComponent>();
                
                var sw = new System.Diagnostics.Stopwatch ();
                sw.Start();
                if (onGPU)
                    GenerateOnGPU(mapInfo);
                else
                    GenerateOnCPU(mapInfo);
                
                var generateHeightMapTime = sw.ElapsedMilliseconds;
                sw.Reset();

                mapEntity.Del<GenerateHeightMapEvent>();
                mapEntity.Get<ConstructMeshEvent>() = new ConstructMeshEvent() {PrintTimers = printTimers};

                if (printTimers)
                    Debug.Log($"{mapInfo.MapSizeWithBorder}x{mapInfo.MapSizeWithBorder} heightmap generated in {generateHeightMapTime}ms");
            }
        }

        private void GenerateOnGPU(MapInfoComponent mapInfo)
        {
            const int seedRange = 10000;
            var seed = mapInfo.RandomizeSeed ? Random.Range(-seedRange, seedRange) : mapInfo.Seed;
            var numberGenerator = new System.Random(seed);
            const int coordinatesRange = 1000;
            const int floatToIntMultiplier = 1000;
            const int maxThreadCount = 65535;
            
            var octaveCount = mapInfo.OctaveCount;
            var map = mapInfo.Map;
            var heightMapComputeShader = mapInfo.HeightMapComputeShader;
            var minMaxHeight = new[]{ floatToIntMultiplier * octaveCount, 0 };

            var offsets = new Vector2[octaveCount];
            for (var i = 0; i < octaveCount; i++)
                offsets[i] = new Vector2 (
                    numberGenerator.Next(-coordinatesRange, coordinatesRange),
                    numberGenerator.Next (-coordinatesRange, coordinatesRange));
            
            var offsetsBuffer = GeneralHelper.GetBufferFor(offsets, heightMapComputeShader, "offsets", sizeof (float) * 2);
            var mapBuffer = GeneralHelper.GetBufferFor(map, heightMapComputeShader, "heightMap");
            var minMaxBuffer = GeneralHelper.GetBufferFor(minMaxHeight, heightMapComputeShader, "minMax");
            
            FillParameters(heightMapComputeShader, mapInfo);
            heightMapComputeShader.Dispatch (0, Math.Min(map.Length, maxThreadCount), 1, 1);

            mapBuffer.GetData (map);
            minMaxBuffer.GetData (minMaxHeight);
            GeneralHelper.ReleaseBuffers(new []{ mapBuffer, minMaxBuffer, offsetsBuffer });

            var minValue = minMaxHeight[0] / (float) floatToIntMultiplier;
            var maxValue = minMaxHeight[1] / (float) floatToIntMultiplier;

            map.InverseLerp(minValue, maxValue);
        }

        private void GenerateOnCPU(MapInfoComponent mapInfo)
        {
            const int seedRange = 10000;
            var seed = mapInfo.RandomizeSeed ? Random.Range(-seedRange, seedRange) : mapInfo.Seed;
            var numberGenerator = new System.Random(seed);
            const int coordinatesRange = 1000;

            var mapSize = mapInfo.MapSizeWithBorder;
            var octaveCount = mapInfo.OctaveCount;
            var initialScale = mapInfo.InitialScale;
            var persistence = mapInfo.Persistence;
            var lacunarity = mapInfo.Lacunarity;
            var map = mapInfo.Map;

            var offsets = new Vector2[octaveCount];
            for (var i = 0; i < octaveCount; i++)
                offsets[i] = new Vector2(
                    numberGenerator.Next(-coordinatesRange, coordinatesRange),
                    numberGenerator.Next(-coordinatesRange, coordinatesRange));

            var minValue = float.MaxValue;
            var maxValue = float.MinValue;

            for (var y = 0; y < mapSize; y++)
            {
                for (var x = 0; x < mapSize; x++)
                {
                    var noiseValue = 0f;
                    var scale = initialScale;
                    var weight = 1f;

                    for (var i = 0; i < octaveCount; i++)
                    {
                        var p = offsets[i] + new Vector2(x / (float) mapSize, y / (float) mapSize) * scale;
                        noiseValue += Mathf.PerlinNoise(p.x, p.y) * weight;
                        weight *= persistence;
                        scale *= lacunarity;
                    }

                    map[y * mapSize + x] = noiseValue;
                    minValue = Mathf.Min(noiseValue, minValue);
                    maxValue = Mathf.Max(noiseValue, maxValue);
                }
            }
            
            map.Normalize(minValue, maxValue);
        }

        private void FillParameters(
            ComputeShader heightMapComputeShader,
            MapInfoComponent mapInfo,
            int floatToIntMultiplier = 1000)
        {
            heightMapComputeShader.SetInt ("mapSize", mapInfo.MapSizeWithBorder);
            heightMapComputeShader.SetInt ("octaves", mapInfo.OctaveCount);
            heightMapComputeShader.SetFloat ("lacunarity", mapInfo.Lacunarity);
            heightMapComputeShader.SetFloat ("persistence", mapInfo.Persistence);
            heightMapComputeShader.SetFloat ("scaleFactor", mapInfo.InitialScale);
            heightMapComputeShader.SetInt ("floatToIntMultiplier", floatToIntMultiplier);
        }
    }
}
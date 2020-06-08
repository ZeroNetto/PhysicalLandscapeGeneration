using System;
using Components.Map;
using Leopotam.Ecs;
using Components.Events;
using Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.MapSystems
{
    public class GenerateHeightMapSystem : IEcsRunSystem
    {
        private EcsFilter<GenerateHeightMapEvent, MapInfoComponent> generateMaps;
        private bool PrintTimers;
        private bool OnGPU;
        
        public void Run()
        {
            foreach (var mapId in generateMaps)
            {
                var mapEntity = generateMaps.GetEntity(mapId);
                var mapInfo = mapEntity.Get<MapInfoComponent>();
                
                var sw = new System.Diagnostics.Stopwatch ();
                sw.Start();
                if (OnGPU)
                    GenerateOnGPU(mapInfo);
                else
                    GenerateOnCPU(mapInfo);
                
                var generateHeightMapTime = sw.ElapsedMilliseconds;
                sw.Reset();

                mapEntity.Del<GenerateHeightMapEvent>();
                mapEntity.Get<ConstructMeshEvent>();

                if (PrintTimers)
                    Debug.Log($"{mapInfo.MapSizeWithBorder}x{mapInfo.MapSizeWithBorder} heightmap generated in {generateHeightMapTime}ms");
            }
        }

        private void GenerateOnGPU(MapInfoComponent mapInfo)
        {
            //TODO
        }

        private void GenerateOnCPU(MapInfoComponent mapInfo)
        {
            const int seedRange = 10000;
            const int coordinatesRange = 1000;
            const float tolerance = 0.01f;

            var mapSize = mapInfo.MapSizeWithBorder;
            var octaveCount = mapInfo.OctaveCount;
            var initialScale = mapInfo.InitialScale;
            var persistence = mapInfo.Persistence;
            var lacunarity = mapInfo.Lacunarity;
            var map = mapInfo.Map;
            var seed = mapInfo.RandomizeSeed ? Random.Range(-seedRange, seedRange) : mapInfo.Seed;
            var numberGenerator = new System.Random(seed);

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
                        noiseValue = Mathf.PerlinNoise(p.x, p.y) * weight;
                        weight *= persistence;
                        scale *= lacunarity;
                    }

                    map[y * mapSize + x] = noiseValue;
                    minValue = Mathf.Min(noiseValue, minValue);
                    maxValue = Mathf.Max(noiseValue, maxValue);
                }
            }
            
            if (Math.Abs(maxValue - minValue) > tolerance)
                map.Normalize(minValue, maxValue);
        }
    }
}
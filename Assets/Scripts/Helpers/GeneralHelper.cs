using System.Collections.Generic;
using System.Linq;
using Components.Map;
using Helpers.Structires;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Helpers
{
    public class GeneralHelper
    {
        public static ComputeBuffer GetBufferFor<T>(
            List<T> collection,
            ComputeShader shader,
            string bufferName,
            int stride = sizeof(int)) where T : struct
        {
            var collectionBuffer = new ComputeBuffer(collection.Count, stride);
            collectionBuffer.SetData(collection);
            shader.SetBuffer(0, bufferName, collectionBuffer);

            return collectionBuffer;
        }
        
        public static ComputeBuffer GetBufferFor<T>(
            T[] collection,
            ComputeShader shader,
            string bufferName,
            int stride = sizeof(int))
        {
            var collectionBuffer = new ComputeBuffer(collection.Length, stride);
            collectionBuffer.SetData(collection);
            shader.SetBuffer(0, bufferName, collectionBuffer);

            return collectionBuffer;
        }
        
        public static Brush CreateBrush(ErosionParametersComponent erosionParameters, int mapSize)
        {
            var erosionBrushRadius = erosionParameters.ErosionBrushRadius;
            var erosionBrushRadiusSquare = erosionBrushRadius * erosionBrushRadius;

            var brushIndexOffsets = new List<int>();
            var brushWeights = new List<float>();
            var weightSum = 0f;

            for (var brushY = -erosionBrushRadius; brushY <= erosionBrushRadius; brushY++)
            {
                for (var brushX = -erosionBrushRadius; brushX <= erosionBrushRadius; brushX++)
                {
                    var sqrDst = brushX * brushX + brushY * brushY;
                    if (sqrDst < erosionBrushRadiusSquare)
                    {
                        brushIndexOffsets.Add(brushY * mapSize + brushX);
                        var brushWeight = 1 - Mathf.Sqrt(sqrDst) / erosionBrushRadius;
                        weightSum += brushWeight;
                        brushWeights.Add(brushWeight);
                    }
                }
            }

            return new Brush()
            {
                Weights = brushWeights.Select(weight => weight / weightSum).ToList(),
                IndexOffsets = brushIndexOffsets
            };
        }
        
        public static int[] GenerateIndices(int erosionIterationsCount, int erosionBrushRadius, int mapSize)
        {
            var randomIndices = new int[erosionIterationsCount];
            for (var i = 0; i < erosionIterationsCount; i++)
            {
                var randX = Random.Range(erosionBrushRadius, erosionBrushRadius + mapSize);
                var randY = Random.Range(erosionBrushRadius, erosionBrushRadius + mapSize);
                randomIndices[i] = randY * mapSize + randX;
            }

            return randomIndices;
        }
        
        public static void ReleaseBuffers(IEnumerable<ComputeBuffer> buffers)
        {
            foreach (var buffer in buffers)
                buffer.Release();
        }
        
        public static float GetPerlinNoiseValue(
            Vector2[] offsets,
            int x, 
            int y,
            float initialScale,
            int mapSize,
            float persistence,
            float lacunarity)
        {
            var noiseValue = 0f;
            var scale = initialScale;
            var weight = 1f;
            
            foreach (var offset in offsets)
            {
                var p = offset + new Vector2(x / (float) mapSize, y / (float) mapSize) * scale;
                noiseValue += Mathf.PerlinNoise(p.x, p.y) * weight;
                weight *= persistence;
                scale *= lacunarity;
            }

            return noiseValue;
        }
    }
}
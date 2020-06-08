using System;
using System.Linq;
using Components.Events;
using Components.Map;
using Helpers;
using Leopotam.Ecs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.MapSystems
{
    public class ConstructMeshSystem: IEcsRunSystem
    {
        private EcsFilter<ConstructMeshEvent, MapInfoComponent, MeshParametersComponent, ErosionParametersComponent> constructMeshes;

        public void Run()
        {
            foreach (var id in constructMeshes)
            {
                var constructEntity = constructMeshes.GetEntity(id);
                var printTimers = constructEntity.Get<ConstructMeshEvent>().PrintTimers;
                var mapInfo = constructEntity.Get<MapInfoComponent>();
                var meshParameters = constructEntity.Get<MeshParametersComponent>();
                var erosionParameters = constructEntity.Get<ErosionParametersComponent>();

                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                ConstructMesh(mapInfo, meshParameters, erosionParameters);
                var constructMeshTime = sw.ElapsedMilliseconds;
                sw.Reset();
                
                if (printTimers)
                    Debug.Log ($"Mesh constructed in {constructMeshTime}ms");
                
                constructEntity.Del<ConstructMeshEvent>();
            }
        }

        private void ConstructMesh(
            MapInfoComponent mapInfo,
            MeshParametersComponent meshParameters,
            ErosionParametersComponent erosionParameters)
        {
            var mapSize = meshParameters.MapSize;
            var brushRadius = erosionParameters.ErosionBrushRadius;
            var verts = new Vector3[mapSize * mapSize];
            var triangles = new int[(mapSize - 1) * (mapSize - 1) * 6];

            for (var i = 0; i < mapSize * mapSize; i++)
            {
                var x = i % mapSize;
                var y = i / mapSize;
                var borderedMapIndex = (y + brushRadius) * mapInfo.MapSizeWithBorder + x + brushRadius;
                var meshMapIndex = y * mapSize + x;
                
                var percent = new Vector2(x / (mapSize - 1f), y / (mapSize - 1f));
                var pos = new Vector3(percent.x * 2 - 1, 0, percent.y * 2 - 1) * meshParameters.Scale;

                var normalizedHeight = mapInfo.Map[borderedMapIndex];
                pos += Vector3.up * normalizedHeight * meshParameters.ElevationScale;
                verts[meshMapIndex] = pos;
                
                if (ConstructMeshHelper.IsInside(mapSize, x, y))
                    ConstructMeshHelper.ConstructTriangles(x, y, mapSize, meshMapIndex, triangles);
            }
            
            if (mapInfo.Mesh == null)
                mapInfo.Mesh = new Mesh();
            else
                mapInfo.Mesh.Clear();

            mapInfo.Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mapInfo.Mesh.vertices = verts;
            mapInfo.Mesh.triangles = triangles;
            mapInfo.Mesh.RecalculateNormals();
                
            var (meshFilter, meshRenderer) = AssignMeshComponents ();
            meshFilter.sharedMesh = mapInfo.Mesh;
            meshRenderer.sharedMaterial = meshParameters.Material;
        }

        private Tuple<MeshFilter, MeshRenderer> AssignMeshComponents()
        {
            const string meshHolderName = "Mesh Holder";
            var terrainTransform = Object
                .FindObjectsOfType<Transform>()
                .FirstOrDefault(o => o.name == "Terrain");
            var meshHolder = terrainTransform.Find(meshHolderName);
            if (meshHolder == null)
            {
                meshHolder = new GameObject(meshHolderName).transform;
                var transform = meshHolder.transform;
                transform.parent = terrainTransform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }

            var meshFilter = meshHolder.gameObject.GetComponent<MeshFilter>();
            var meshRenderer = meshHolder.GetComponent<MeshRenderer>();
            if (meshFilter == null)
                meshFilter = meshHolder.gameObject.AddComponent<MeshFilter> ();
            if (meshRenderer == null)
                meshRenderer = meshHolder.gameObject.AddComponent<MeshRenderer> ();
            
            return Tuple.Create(meshFilter, meshRenderer);
        }
    }
}
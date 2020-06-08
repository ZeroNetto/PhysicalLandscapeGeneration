using System;
using Components.Events;
using Components.Map;
using Leopotam.Ecs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.MapSystems
{
    public class ConstructMeshSystem: IEcsRunSystem
    {
        private EcsFilter<ConstructMeshEvent, MapInfoComponent, MeshParametersComponent, ErosionParametersComponent> constructMeshes;
        private bool PrintTimers;
        
        public void Run()
        {
            foreach (var id in constructMeshes)
            {
                var constructEntity = constructMeshes.GetEntity(id);
                var mapInfo = constructEntity.Get<MapInfoComponent>();
                var meshParameters = constructEntity.Get<MeshParametersComponent>();
                var erosionParameters = constructEntity.Get<ErosionParametersComponent>();

                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                ConstructMesh(mapInfo, meshParameters, erosionParameters);
                var constructMeshTime = sw.ElapsedMilliseconds;
                sw.Reset();
                
                if (PrintTimers)
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
                
                if (x != mapSize - 1 && y != mapSize - 1)
                    ConstructTriangles( x, y, mapSize, meshMapIndex, triangles);
            }
            
            var mesh = mapInfo.Mesh;
            if (mesh == null)
                mesh = new Mesh();
            else
                mesh.Clear();

            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = verts;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
                
            var (meshFilter, meshRenderer) = AssignMeshComponents ();
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = meshParameters.Material;
            
            meshParameters.Material.SetFloat("_MaxHeight", meshParameters.ElevationScale);
        }

        private Tuple<MeshFilter, MeshRenderer> AssignMeshComponents()
        {
            const string meshHolderName = "MeshHolder";
            var parentTransform = Object.FindObjectOfType<Transform>().parent.GetComponent<Transform>();
            var meshHolder = parentTransform.Find(meshHolderName);
            if (meshHolder == null)
            {
                meshHolder = new GameObject(meshHolderName).transform;
                meshHolder.transform.parent = parentTransform;
                meshHolder.transform.localPosition = Vector3.zero;
                meshHolder.transform.localRotation = Quaternion.identity;
            }

            var meshFilter = meshHolder.gameObject.GetComponent<MeshFilter>();
            var meshRenderer = meshHolder.GetComponent<MeshRenderer>();
            if (meshFilter == null)
                meshFilter = meshHolder.gameObject.AddComponent<MeshFilter> ();
            if (meshRenderer == null)
                meshRenderer = meshHolder.gameObject.AddComponent<MeshRenderer> ();
            
            return Tuple.Create(meshFilter, meshRenderer);
        }

        private void ConstructTriangles(int x, int y, int mapSize, int meshMapIndex, int[] triangles)
        {
            var shift = (y * (mapSize - 1) + x) * 3 * 2;

            triangles[shift + 0] = meshMapIndex + mapSize;
            triangles[shift + 1] = meshMapIndex + mapSize + 1;
            triangles[shift + 2] = meshMapIndex;

            triangles[shift + 3] = meshMapIndex + mapSize + 1;
            triangles[shift + 4] = meshMapIndex + 1;
            triangles[shift + 5] = meshMapIndex;
        }
    }
}
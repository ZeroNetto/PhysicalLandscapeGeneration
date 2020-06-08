using Systems.MapSystems;
using Components.Events;
using Components.Map;
using Leopotam.Ecs;
using UnityEngine;

namespace Systems
{
    public class GameLoader : MonoBehaviour
    {
        //public Levels ChooseLevel;
        public bool OnGPU = false;
        public bool PrintTimers = false;
        
        [Header ("Mesh Parameters")]
        public int MapSize = 255;
        public float Scale = 20;
        public float ElevationScale = 10;
        public Material Material;

        [Header("Erosion Parameters")]
        public ComputeShader Erosion;
        public int ErosionIterationsCount = 50000;
        public int ErosionBrushRadius = 3;

        public int MaxLifetime = 30;
        public float SedimentCapacityFactor = 3;
        public float MinSedimentCapacity = .01f;
        public float DepositSpeed = 0.3f;
        public float ErodeSpeed = 0.3f;

        public float EvaporateSpeed = .01f;
        public float Gravity = 4;
        public float StartSpeed = 1;
        public float StartWater = 1;
        [Range (0, 1)]
        public float Inertia = 0.3f;
        
        [Header ("HeightMapGenerator Parameters")]
        public ComputeShader HeightMapComputeShader;
        public int Seed;
        public bool RandomizeSeed;

        public int OctaveCount = 7;
        public float Persistence = .5f;
        public float Lacunarity = 2;
        public float InitialScale = 2;

        public bool UseComputeShader = true;

        private EcsWorld world;
        private EcsEntity map;
        private EcsSystems systems;
        
        public void Generate()
        {
            map = world.NewEntity();
            FillParameters(map);
            
            map.Get<GenerateHeightMapEvent>();
        }

        public void Erode()
        {
            if (map.IsNull() || !map.IsAlive())
                return;
            
            FillParameters(map);
            map.Get<ErodeEvent>();
        }

        public void Start()
        {
            world = new EcsWorld();

            systems = new EcsSystems(world)
                .Add(new GenerateHeightMapSystem())
                .Add(new ErodeSystem())
                .Add(new ConstructMeshSystem())
                .Inject(OnGPU)
                .Inject(PrintTimers);
            
            //LoadLevel();

            systems.ProcessInjects();
            systems.Init();
        }

        public void Update()
        {
            systems.Run();
        }

        public void OnDestroy()
        {
            systems.Destroy();
            world.Destroy();
        }

        // private void LoadLevel()
        // {
        //     switch (ChooseLevel)
        //     {
        //         case Levels.Level_1:
        //         {
        //             systems.Add(new Map1InitSystem());
        //             break;
        //         }
        //         case Levels.Level_2:
        //         {
        //             //systems.Add(new Map2InitSystem())
        //             break;
        //         }
        //         default:
        //         {
        //             systems.Add(new Map1InitSystem());
        //             break;
        //         }
        //     }
        // }

        private void FillParameters(EcsEntity map)
        {
            FillMeshParameters(map);
            FillErosionParameters(map);
            FillMapParameters(map);
        }
        
        private void FillMeshParameters(EcsEntity map)
        {
            map.Get<MeshParametersComponent>() = new MeshParametersComponent()
            {
                Material = Material,
                Scale = Scale,
                ElevationScale = ElevationScale,
                MapSize = MapSize,
            };
        }

        private void FillErosionParameters(EcsEntity map)
        {
            map.Get<ErosionParametersComponent>() = new ErosionParametersComponent()
            {
                ErosionBrushRadius = ErosionBrushRadius,
                ErosionIterationCount = ErosionIterationsCount,
                Inertia = Inertia,
                Gravity = Gravity,
                EvaporateSpeed = EvaporateSpeed,
                MaxLifetime = MaxLifetime,
                StartSpeed = StartSpeed,
                StartWater = StartWater,
                MinSedimentCapacity = MinSedimentCapacity,
                SedimentCapacityFactor = SedimentCapacityFactor,
                ErodeSpeed = ErodeSpeed,
                Erosion = Erosion,
                DepositSpeed = DepositSpeed
            };
        }

        private void FillMapParameters(EcsEntity map)
        {
            map.Get<MapInfoComponent>() = new MapInfoComponent()
            {
                MapSizeWithBorder = MapSize + ErosionBrushRadius * 2,
                Lacunarity = Lacunarity,
                Persistence = Persistence,
                Seed = Seed,
                OctaveCount = OctaveCount,
                HeightMapComputeShader = HeightMapComputeShader,
                InitialScale = InitialScale,
                RandomizeSeed = RandomizeSeed,
                UseComputerShader = UseComputeShader,
                Map = new float[(MapSize + ErosionBrushRadius * 2) * (MapSize + ErosionBrushRadius * 2)]
            };
        }
    }
}
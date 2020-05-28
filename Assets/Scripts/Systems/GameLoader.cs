using Leopotam.Ecs;
using UnityEngine;

namespace Systems
{
    public class GameLoader : MonoBehaviour
    {
        private EcsWorld world;
        private EcsSystems systems;

        public void Start()
        {
            world = new EcsWorld();
            
            systems = new EcsSystems(world)
                .ProcessInjects();
            
            systems.Init();
        }

        public void Update()
        {
            systems.Run();
            //world.EndFrame();
        }

        public void OnDestroy()
        {
            systems.Destroy();
            world.Destroy();
        }
    }
}
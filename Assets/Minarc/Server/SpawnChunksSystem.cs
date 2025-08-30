using Minarc.Common;
using Unity.Entities;
using UnityEngine;

namespace Minarc.Server
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SpawnChunksSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainPrefabRepo>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var repo = SystemAPI.GetSingleton<MainPrefabRepo>();
            if (Input.GetKeyDown(KeyCode.A))
            {
                state.EntityManager.Instantiate(repo.TerrainChunk);
            }
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
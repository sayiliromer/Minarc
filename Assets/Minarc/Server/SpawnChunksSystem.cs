using Minarc.Common;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Minarc.Server
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct PaintTest : ISystem
    {
        
    }
    
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
            for (int x = 0; x < 30; x++)
            {
                for (int y = 0; y < 30; y++)
                {
                    var chunkIndex = new int2(x, y);
                    float2 chunkPosition = chunkIndex * Constants.ChunkSize;
                    var chunkEntity = state.EntityManager.Instantiate(repo.TerrainChunk);
                    var buffer = state.EntityManager.GetBuffer<TileMapChunkElement>(chunkEntity);
                    for (int xx = 0; xx < Constants.ChunkSize; xx++)
                    {
                        for (int yy = 0; yy < Constants.ChunkSize; yy++)
                        {
                            var cellPos = chunkPosition + new int2(xx, yy);
                            var n = noise.snoise(cellPos * 0.2f);
                            if (n > 0f)
                            {
                                buffer[xx + yy * Constants.ChunkSize] = new TileMapChunkElement()
                                {
                                    MaterialType =TileMaterialType.Dirt
                                };
                            }
                        }
                    }
                    state.EntityManager.SetComponentData(chunkEntity, LocalTransform.FromPosition(chunkPosition.x, chunkPosition.y,0));
                }
            }

            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
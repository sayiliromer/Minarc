using Minarc.Common;
using Unity.Collections;
using UnityEngine;

namespace Minarc.Authoring
{
    public class TerrainChunkAuthoring : AutoBakerMono
    {
        public override void Bake()
        {
            var entity = GetEntity();
            var chunkBuffer = AddBuffer<TileMapChunkElement>(entity);
            chunkBuffer.Resize(Constants.ChunkDataLength, NativeArrayOptions.ClearMemory);
            AddComponent<ChangeVersion>(entity);
            AddComponent(entity, new TileMapChunkMeshVersion()
            {
                CheckedVersion = -1
            });
        }
    }
}
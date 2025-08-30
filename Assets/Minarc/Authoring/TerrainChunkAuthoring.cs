using Minarc.Common;
using UnityEngine;

namespace Minarc.Authoring
{
    public class TerrainChunkAuthoring : AutoBakerMono
    {
        public override void Bake()
        {
            var entity = GetEntity();
            AddBuffer<TerrainChunkElement>(entity);
            AddComponent<ChangeVersion>(entity);
            AddComponent(entity, new TerrainChunkMeshVersion()
            {
                CheckedVersion = -1
            });
        }
    }
}
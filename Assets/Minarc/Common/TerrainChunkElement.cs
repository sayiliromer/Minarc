using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Rendering;

namespace Minarc.Common
{
    [InternalBufferCapacity(100)]
    public struct TerrainChunkElement : IBufferElementData
    {
        [GhostField] public TerrainMaterial Material;
        [GhostField] public byte Rotation;
    }

    public enum TerrainMaterial : short
    {
        Grass,
        Dirt,
        Stone,
        Cobblestone,
    }

    public struct TerrainChunkMeshVersion : IComponentData
    {
        public int CheckedVersion;
    }

    public struct TerrainMeshCleanup : ICleanupComponentData
    {
        public BatchMeshID MeshID;
    }
}

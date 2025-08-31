using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Rendering;

namespace Minarc.Common
{
    [InternalBufferCapacity(100)]
    public struct TileMapChunkElement : IBufferElementData
    {
        [GhostField] public TileMaterialType MaterialType;
        [GhostField] public byte Rotation;
    }

    public enum TileMaterialType : short
    {
        None,
        Grass,
        Dirt,
        Stone,
        Cobblestone,
    }

    public struct TileConfigElement : IBufferElementData
    {
        
    }

    public struct TileMapChunkMeshVersion : IComponentData
    {
        public int CheckedVersion;
    }

    public struct TileMapChunkMeshCleanup : ICleanupComponentData
    {
        public BatchMeshID MeshID;
    }
}

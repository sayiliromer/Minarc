using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Rendering;

namespace Minarc.Common
{
    [InternalBufferCapacity(100)]
    public struct TileMapChunkElement : IBufferElementData
    {
        [GhostField] public TileMaterial Material;
        [GhostField] public byte Rotation;
    }

    public enum TileMaterial : short
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

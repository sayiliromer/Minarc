using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Minarc.Common
{
    public struct SpriteData
    {
        public float PixelsPerUnit;
        public float2 Pivot;
        public int TextureId;

        public float2 Uv0;
        public float2 Uv1;

        public float2 Uv2;
        public float2 Uv3;

        public static SpriteData FromSprite(Sprite sprite)
        {
            SpriteData result = default;
            var uvs = sprite.uv;
            result.Uv0 = uvs[2];
            result.Uv1 = uvs[0];
            result.Uv2 = uvs[1];
            result.Uv3 = uvs[3];
            result.PixelsPerUnit = sprite.pixelsPerUnit;
            result.Pivot = sprite.pivot;

            return result;
        }
    }

    public struct BrushCollectionData
    {
        public BlobArray<RuleBrushData> Brushes;
    }

    public struct RuleBrushData
    {
        public BlobArray<RuleTileData> Rules;
    }

    public struct RuleTileData
    {
        public int NeighborFlags;
        public BlobArray<SpriteData> Sprites;
    }

    public struct RuleTileIndexingData
    {
        public BlobArray<ReducedTileIndex> FlagToIndex;
    }

    public struct BrushCollection : IComponentData
    {
        public BlobAssetReference<BrushCollectionData> Brushes;
        public BlobAssetReference<RuleTileIndexingData> NeighborFlagToTile;
    }

    //Points to sprites for tile sprites and it's cosmetic variations
    public struct TileSpriteSetElement : IBufferElementData
    {
        public short SpriteIndex;
        public byte VariantCount;
    }

    //Represents raw tile sprites, all of them.
    public struct TileSpriteElement : IBufferElementData
    {
        public SpriteData Sprite;
    }
}
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Minarc.Common
{
    public struct SpriteData
    {
        public float2 UvMin;
        public float2 UvMax;
        public float PixelsPerUnit;
        public float2 Pivot;
        public int TextureId;

        public static SpriteData FromSprite(Sprite sprite)
        {
            SpriteData result = default;
            var uvs = sprite.uv;
            for (int i = 0; i < uvs.Length; i++)
            {
                result.UvMin = math.min(result.UvMin, uvs[i]);
                result.UvMax = math.min(result.UvMax, uvs[i]);
            }
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
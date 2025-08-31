using Unity.Entities;
using Unity.Mathematics;

namespace Minarc.Common
{
    public struct SpriteData
    {
        public float2 UvMin;
        public float2 UvMax;
        public float PixelsPerUnit;
        public float2 Pivot;
    }

    public struct RuleTile : IBufferElementData
    {
        public short TileSpriteHeaderIndex;
    }

    //Points to sprites for tile sprites and it's cosmetic variations
    public struct TileSpriteSetElement : IBufferElementData
    {
        public short SpriteIndex;
        public byte VariantCount;
    }

    //Represents raw tile sprites
    public struct TileSpriteElement : IBufferElementData
    {
        public SpriteData Sprite;
    }
}
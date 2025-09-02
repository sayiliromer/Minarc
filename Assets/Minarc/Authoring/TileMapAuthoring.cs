using System.Collections.Generic;
using Minarc.Common;
using Unity.Collections;
using Unity.Entities;

namespace Minarc.Authoring
{
    public class TileMapAuthoring : AutoBakerMono
    {
        public List<TileMaterialAuthoringData> TileMaterials;
        
        public override void Bake()
        {
            var entity = GetEntity();
            var allTileBuffer = AddBuffer<TileSpriteSetElement>(entity);
            var allSpriteBuffer = AddBuffer<TileSpriteElement>(entity);

            var flagIndicesBlob = CreateFlagIndexes(entity);
            CreateBrushes(entity,flagIndicesBlob);
        }

        private BlobAssetReference<RuleTileIndexingData> CreateFlagIndexes(Entity entity)
        {
            var builder = new BlobBuilder(Allocator.Temp);

            ref var root = ref builder.ConstructRoot<RuleTileIndexingData>();
            builder.Construct(ref root.FlagToIndex,TileMapIndexing.BitMapToTileIndex);
            var blobAsset = builder.CreateBlobAssetReference<RuleTileIndexingData>(Allocator.Persistent);
            Baker.AddBlobAsset(ref blobAsset,out _);
            builder.Dispose();
            return blobAsset;
        }

        private void CreateBrushes(Entity entity, BlobAssetReference<RuleTileIndexingData> tileIndexMap)
        {
            // Create a new builder that will use temporary memory to construct the blob asset
            var builder = new BlobBuilder(Allocator.Temp);

            // Construct the root object for the blob asset. Notice the use of `ref`.
            ref var marketData = ref builder.ConstructRoot<BrushCollectionData>();
            var brushes = builder.Allocate(ref marketData.Brushes, TileMaterials.Count);
            
            for (int i = 0; i < TileMaterials.Count; i++)
            {
                var material = TileMaterials[i];
                ref var brush = ref brushes[i];
                var rules = builder.Allocate(ref brush.Rules, material.TileSet.Rules.Length);
                for (int j = 0; j < material.TileSet.Rules.Length; j++)
                {
                    ref var rule = ref rules[j];
                    var authoringRule = material.TileSet.Rules[i];
                    rule.NeighborFlags = authoringRule.NeighborFlags;
                    var sprites = builder.Allocate(ref rule.Sprites, 1);
                    if (authoringRule.BaseSprite)
                    {
                        sprites[0] = SpriteData.FromSprite(authoringRule.BaseSprite);
                    }
                }
            }

            var brushCollectionDataReference = builder.CreateBlobAssetReference<BrushCollectionData>(Allocator.Persistent);
            AddComponent(entity, new BrushCollection()
            {
                NeighborFlagToTile = tileIndexMap,
                Brushes = brushCollectionDataReference
            });
            Baker.AddBlobAsset(ref brushCollectionDataReference, out _);
            builder.Dispose();
        }
    }
}
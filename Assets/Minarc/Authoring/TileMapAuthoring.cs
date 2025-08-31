using System.Collections.Generic;
using Minarc.Common;

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
        }
    }
}
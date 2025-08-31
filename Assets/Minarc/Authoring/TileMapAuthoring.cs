using Minarc.Common;

namespace Minarc.Authoring
{
    public class TileMapAuthoring : AutoBakerMono
    {
        public override void Bake()
        {
            var entity = GetEntity();
            AddBuffer<TileSpriteSetElement>(entity);
            AddBuffer<TileSpriteElement>(entity);
        }
    }
}
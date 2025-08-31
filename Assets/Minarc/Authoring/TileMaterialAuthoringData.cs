using System;
using Minarc.Common;
using UnityEngine;

namespace Minarc.Authoring
{
    [CreateAssetMenu(fileName = "Data/TileMaterial", menuName = "Tile Map Authoring")]
    public class TileMaterialAuthoringData : ScriptableObject
    {
        public TileMaterialType Type;
        public TileSet TileSet;
    }

    [Serializable]
    public class TileSet
    {
        public TileCase[] TileCases;
    }

    [Serializable]
    public class TileCase
    {
        public TileSprite[] Variants;
    }

    [Serializable]
    public struct TileSprite
    {
        public Sprite Sprite;
    }
}
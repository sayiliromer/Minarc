using System;
using System.Collections.Generic;

namespace Minarc.Common
{
    public struct ReducedTileIndex
    {
        public int ReducedBits;
        public int CanonicalTileIndex;
    }
    
    public static class TileMapIndexing
    {
        private static ReducedTileIndex[] _bitmapToTileIndex;
        public static ReducedTileIndex[] BitMapToTileIndex => _bitmapToTileIndex ??= GetBitMapToTileIndexMap();
        private static int[] _canonicalTileBits;
        public static int[] CanonicalTileBits => _canonicalTileBits ??= GetCanonicalTileBits();
        
        static int[] GetCanonicalTileBits()
        {
            int[] result = new int[47];
            Dictionary<int,int> tileIndexes = new Dictionary<int,int>();
            for (int i = 0; i <=  Byte.MaxValue; i++)
            {
                var reducedBit = GetReducedBits(i);
                if (!tileIndexes.TryGetValue(reducedBit, out var tileIndex))
                {
                    tileIndex = tileIndexes.Count;
                    tileIndexes[reducedBit] = tileIndex;
                }

                result[tileIndex] = reducedBit;
            }

            return result;
        }
        
        static ReducedTileIndex[] GetBitMapToTileIndexMap()
        {
            ReducedTileIndex[] result = new ReducedTileIndex[byte.MaxValue + 1];
            Dictionary<int,int> tileIndexes = new Dictionary<int,int>();
            for (int i = 0; i <=  Byte.MaxValue; i++)
            {
                var reducedBit = GetReducedBits(i);
                if (!tileIndexes.TryGetValue(reducedBit, out var tileIndex))
                {
                    tileIndex = tileIndexes.Count;
                    tileIndexes[reducedBit] = tileIndex;
                }

                result[i] = new ReducedTileIndex()
                {
                    ReducedBits = reducedBit,
                    CanonicalTileIndex = tileIndex
                };
            }

            return result;
        }
        
        static int GetReducedBits(int neighborBitFlags)
        {
            var n = neighborBitFlags & 1;
            var ne = neighborBitFlags & 2;
            var e = neighborBitFlags & 4;
            var se = neighborBitFlags & 8;
            var s = neighborBitFlags & 16;
            var sw = neighborBitFlags & 32;
            var w = neighborBitFlags & 64;
            var nw = neighborBitFlags & 128;


            if (n == 0)
            {
                nw = 0;
                ne = 0;
            }

            if (e == 0)
            {
                ne = 0;
                se = 0;
            }

            if (s == 0)
            {
                sw = 0;
                se = 0;
            }

            if (w == 0)
            {
                nw = 0;
                sw = 0;
            }

            return n | ne | e | se | s | sw | w | nw;
        }
    }
}
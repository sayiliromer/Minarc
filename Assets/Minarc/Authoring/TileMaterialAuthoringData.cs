using System;
using Minarc.Common;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Minarc.Authoring
{
    [CreateAssetMenu(fileName = "TileMaterial", menuName = "Tile Map Authoring")]
    public class TileMaterialAuthoringData : ScriptableObject
    {
        public TileMaterialType Type;
        public RuleBrush TileSet;
    }

    [Serializable]
    public class TileSet
    {
        public TileCase[] TileCases;
        
        
    }

    [Serializable]
    public class TileCase
    {
        public int NeighborRule;
        public TileSprite[] Variants;

        private bool[,] _flagMatrix;

        [TableMatrix(DrawElementMethod = "DrawCell")]
        [ShowInInspector]
        private bool[,] FlagsMatrix
        {
            get
            {
                const int size = 3;
                if (_flagMatrix == null)
                    _flagMatrix = new bool[size, size];

                for (int i = 0; i < size * size; i++)
                {
                    int x = i % size;
                    int y = i / size;

                    if (x == 1 && y == 1) // skip center
                        continue;

                    _flagMatrix[x, y] = (NeighborRule & (1 << ToBitIndex(x, y))) != 0;
                }

                return _flagMatrix;
            }
            set
            {
                _flagMatrix = value;
                NeighborRule = 0;

                for (int y = 0; y < value.GetLength(1); y++)
                {
                    for (int x = 0; x < value.GetLength(0); x++)
                    {
                        if (x == 1 && y == 1) // skip center
                            continue;

                        if (value[x, y])
                            NeighborRule |= (1 << ToBitIndex(x, y));
                    }
                }
            }
        }

        private static int ToBitIndex(int x, int y)
        {
            // Map cells to bit positions, skipping center
            // Layout: row-major order but no center
            int[,] map =
            {
                {0, 1, 2},
                {3,-1, 4},
                {5, 6, 7}
            };
            return map[y, x];
        }

#if UNITY_EDITOR
        private static bool DrawCell(Rect rect, bool value)
        {
            // Check if this is the center cell
            int col = (int)(rect.x / rect.width) % 3;
            int row = (int)(rect.y / rect.height) % 3;

            if (col == 1 && row == 1)
            {
                // Draw disabled/placeholder
                UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color(0.3f, 0.3f, 0.3f, 0.8f));
                return false;
            }

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
            }

            UnityEditor.EditorGUI.DrawRect(rect.Padding(1),
                value ? new Color(0.1f, 0.8f, 0.2f) : new Color(0, 0, 0, 0.5f));

            return value;
        }
#endif
    }

    
    [Serializable]
    public struct TileSprite
    {
        public Sprite Sprite;
    }
}
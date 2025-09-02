using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Minarc.Authoring
{
    public class TileRule : ScriptableObject
    {
        [CustomValueDrawer("DrawBits")]
        public int NeighborFlags;
        [PreviewField]
        public Sprite BaseSprite;

#if UNITY_EDITOR
        private int DrawBits(int value, GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 60); // Reserve 60px height
            float cellSize = rect.height / 3f;

            // Local function to draw a cell
            void DrawCell(int gridX, int gridY, bool active, int bit)
            {
                Rect cell = new Rect(
                    rect.x + gridX * cellSize,
                    rect.y + gridY * cellSize,
                    cellSize - 1,
                    cellSize - 1
                );

                EditorGUI.DrawRect(cell, active ? Color.cyan : new Color(0.15f, 0.15f, 0.15f));
            }

            // Extract flags
            var n  = (NeighborFlags & 1)   != 0;
            var ne = (NeighborFlags & 2)   != 0;
            var e  = (NeighborFlags & 4)   != 0;
            var se = (NeighborFlags & 8)   != 0;
            var s  = (NeighborFlags & 16)  != 0;
            var sw = (NeighborFlags & 32)  != 0;
            var w  = (NeighborFlags & 64)  != 0;
            var nw = (NeighborFlags & 128) != 0;

            // Draw neighbors one by one
            DrawCell(0, 0, nw, 128); // NW
            DrawCell(1, 0, n, 1);    // N
            DrawCell(2, 0, ne, 2);   // NE
            DrawCell(2, 1, e, 4);    // E
            DrawCell(2, 2, se, 8);   // SE
            DrawCell(1, 2, s, 16);   // S
            DrawCell(0, 2, sw, 32);  // SW
            DrawCell(0, 1, w, 64);   // W

            // Center (always empty)
            Rect center = new Rect(rect.x + cellSize, rect.y + cellSize, cellSize - 1, cellSize - 1);
            EditorGUI.DrawRect(center, new Color(0.25f, 0.25f, 0.25f));

            return value;
        }
#endif
    }
}
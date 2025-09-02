using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Minarc.Authoring
{
    [CreateAssetMenu(menuName = "Data/Create TileSetSelector", fileName = "TileSetSelector", order = 0)]
    public class TileSetSelector : ScriptableObject
    {
        [PreviewField(100)] public Texture2D TilesetTexture;

        [TableMatrix(DrawElementMethod = "DrawTile", SquareCells = true)] [ShowInInspector] [HideLabel]
        private Sprite[,] _tilesMatrix = new Sprite[11, 5]; // 5 rows, 11 columns = 55 slots

        private Sprite DrawTile(Rect rect, Sprite value, int column, int row)
        {
            if (TilesetTexture == null) return null;

            value = (Sprite)EditorGUI.ObjectField(rect, value, typeof(Sprite), false);
            if (value) return value;
            var index = row * 11 + column; // calculate linear index

            var sliceW = TilesetTexture.width / 11;
            var sliceH = TilesetTexture.height / 5;

            // Calculate tile UV rect
            var uMin = column * sliceW / (float)TilesetTexture.width;
            var vMin = 1f - (row + 1) * sliceH / (float)TilesetTexture.height; // flip Y
            var uMax = (column + 1) * sliceW / (float)TilesetTexture.width;
            var vMax = 1f - row * sliceH / (float)TilesetTexture.height;

            var texCoords = new Rect(uMin, vMin, uMax - uMin, vMax - vMin);

            // Draw tile
            GUI.DrawTextureWithTexCoords(rect, TilesetTexture, texCoords);

            // Draw button overlay
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) Debug.Log($"Clicked tile {index} at ({row},{column})");


            return value;
        }
    }
}
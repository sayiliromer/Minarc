using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using Minarc.Common;
using UnityEditor;
#endif
namespace Minarc.Authoring
{
    [CreateAssetMenu(menuName = "Data/Create RuleBrush", fileName = "RuleBrush", order = 0)]
    public class RuleBrush : ScriptableObject
    {
        [InlineEditor]
        public TileRule[] Rules;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Only auto-generate if Canon is empty and this asset is saved in the project
            if (Rules == null || Rules.Length == 0)
            {
                string path = AssetDatabase.GetAssetPath(this);
                if (!string.IsNullOrEmpty(path))
                {
                    IReadOnlyList<int> tileBits = TileMapIndexing.CanonicalTileBits;
                    Rules = new TileRule[tileBits.Count];
                    for (int i = 0; i < tileBits.Count; i++)
                    {
                        var tileRule = ScriptableObject.CreateInstance<TileRule>();
                        tileRule.name = $"TileRule_{i}";
                        tileRule.NeighborFlags = tileBits[i];
                        AssetDatabase.AddObjectToAsset(tileRule, this);
                        Rules[i] = tileRule;
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
#endif
    }
}
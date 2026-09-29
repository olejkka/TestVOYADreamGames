using System;
using _Project.Scripts.UI;
using _Project.Scripts.UI.Cell;
using UnityEngine;

namespace _Project.Scripts.Configs
{
    
    [CreateAssetMenu(fileName = "CellColorConfig", menuName = "TestVOYADreamGames/Cell Color Config")]
    public sealed class CellColorConfig : ScriptableObject
    {
        public CellColorEntry[] entries;

        
        public Color ColorOf(CellType type)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Type == type)
                    return entries[i].Color;
            }

            throw new InvalidOperationException("No color for cell type " + type + ".");
        }
    }
    
    [Serializable]
    public struct CellColorEntry
    {
        public CellType Type;
        public Color Color;
    }
}

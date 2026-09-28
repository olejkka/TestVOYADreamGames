using _Project.Scripts.Ship;
using UnityEngine;

namespace _Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "ShipLengthConfig", menuName = "TestVOYADreamGames/Ship Length Config")]
    public sealed class ShipLengthConfig : ScriptableObject
    {
        public ShipLengthEntry[] entries;
    }

    [System.Serializable]
    public struct ShipLengthEntry
    {
        public ShipType Type;
        public int Length;
    }
}

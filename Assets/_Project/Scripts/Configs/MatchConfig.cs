using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.Configs
{
    [CreateAssetMenu(fileName = "MatchConfig", menuName = "TestVOYADreamGames/Match Config")]
    public sealed class MatchConfig : ScriptableObject
    {
        [Header("Board")]
        public int width;
        public int height;

        [Header("Fleet")]
        public FleetEntry[] fleet;

        [Header("Network")]
        public int defaultLatencyMs;
        [Range(0f, 1f)]
        public float dropChance;
        public int retransmitMs;
        public int turnTimeoutSec;
        public int timeOut;
        public bool logEnabled;
    }

    [System.Serializable]
    public class FleetEntry
    {
        public ShipType Type;
        public int Count;
    }
}

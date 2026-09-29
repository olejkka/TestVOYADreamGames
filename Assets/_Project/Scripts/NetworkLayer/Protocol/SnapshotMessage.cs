using System;
using _Project.Scripts;
using _Project.Scripts.Ships;

namespace _Project.Scripts.NetworkLayer.Protocol
{
    [Serializable]
    public class ShipPlacement
    {
        public ShipType type;
        public int[] cells;
        public int[] hits;
    }

    [Serializable]
    public class ShotPlacement
    {
        public int cell;
        public ShotResult result;
    }

    [Serializable]
    public class SnapshotMessage
    {
        public int width;
        public int height;
        public int turn;
        public ShipPlacement[] ships;
        public ShotPlacement[] shots;
    }
}

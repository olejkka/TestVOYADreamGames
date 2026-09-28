using System;
using _Project.Scripts.Ships;

namespace _Project.Scripts.NetworkLayer.Protocol
{
    [Serializable]
    public class ShipPlacement
    {
        public ShipType type;
        public int[] cells;
    }

    [Serializable]
    public class SnapshotMessage
    {
        public int width;
        public int height;
        public ShipPlacement[] ships;
    }
}

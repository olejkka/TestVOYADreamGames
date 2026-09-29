using System.Collections.Generic;

namespace _Project.Scripts.Ships
{
    public class Ship
    {
        private readonly List<int> _hitCells = new List<int>();

        public ShipType Type { get; }
        public int[] Cells { get; }
        public int HitCount => _hitCells.Count;
        public bool IsSunk => HitCount == Cells.Length;


        public Ship(ShipType type, int[] cells)
        {
            Type = type;
            Cells = cells;
        }

        internal int[] CopyHits()
        {
            var hits = new int[_hitCells.Count];
            
            for (int i = 0; i < hits.Length; i++)
                hits[i] = _hitCells[i];

            return hits;
        }

        internal void RegisterHit(int cell)
        {
            _hitCells.Add(cell);
        }
    }
}

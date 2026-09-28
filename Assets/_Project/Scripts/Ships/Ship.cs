namespace _Project.Scripts.Ships
{
    public class Ship
    {
        public ShipType Type { get; }
        public int[] Cells { get; }
        public int HitCount { get; private set; }
        
        
        public bool IsSunk => HitCount == Cells.Length;
        
        
        public Ship(ShipType type, int[] cells)
        {
            Type = type;
            Cells = cells;
        }

        internal void RegisterHit()
        {
            HitCount++;
        }
    }
}

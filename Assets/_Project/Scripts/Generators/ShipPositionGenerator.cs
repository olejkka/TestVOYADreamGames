using System;
using System.Collections.Generic;
using _Project.Scripts.Configs;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Generators
{
    public class ShipPositionGenerator
    {
        private const int MaxAttempts = 256;

        private readonly MatchConfig _matchConfig;
        private readonly ShipLengthConfig _shipLengthConfig;

        
        public ShipPositionGenerator(MatchConfig matchConfig, ShipLengthConfig shipLengthConfig)
        {
            _matchConfig = matchConfig;
            _shipLengthConfig = shipLengthConfig;
        }

        public Ship[] Create(Field field, Random random)
        {
            PlannedShip[] planned = Expand();
            
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (TryPlace(field.Width, field.Height, planned, random, out Ship[] ships))
                    return ships;
            }

            throw new InvalidOperationException("Fleet does not fit on the field.");
        }

        private PlannedShip[] Expand()
        {
            ShipLengthEntry[] lengths = _shipLengthConfig.entries;
            ValidateLengths(lengths);

            var planned = new List<PlannedShip>();
            FleetEntry[] fleet = _matchConfig.fleet;
            
            for (int i = 0; i < fleet.Length; i++)
            {
                FleetEntry entry = fleet[i];
                
                if (entry.Type == ShipType.None)
                    throw new InvalidOperationException("Ship type is None.");

                if (entry.Count == 0)
                    continue;

                int length = LengthOf(lengths, entry.Type);
                
                for (int n = 0; n < entry.Count; n++)
                    planned.Add(new PlannedShip { Type = entry.Type, Length = length });
            }

            planned.Sort((a, b) => b.Length.CompareTo(a.Length));
            
            return planned.ToArray();
        }

        private void ValidateLengths(ShipLengthEntry[] lengths)
        {
            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i].Type == ShipType.None)
                    throw new InvalidOperationException("Ship type is None.");

                for (int j = i + 1; j < lengths.Length; j++)
                {
                    if (lengths[j].Type == lengths[i].Type)
                        throw new InvalidOperationException("Duplicate length for ship type " + lengths[i].Type + ".");
                }
            }
        }

        private int LengthOf(ShipLengthEntry[] lengths, ShipType type)
        {
            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i].Type == type)
                    return lengths[i].Length;
            }

            throw new InvalidOperationException("No length for ship type " + type + ".");
        }

        private bool TryPlace(int width, int height, PlannedShip[] planned, Random random, out Ship[] ships)
        {
            var blocked = new bool[width * height];
            var candidates = new List<int[]>();
            
            ships = new Ship[planned.Length];

            for (int s = 0; s < planned.Length; s++)
            {
                candidates.Clear();
                Collect(width, height, planned[s].Length, blocked, candidates);
                
                if (candidates.Count == 0)
                    return false;

                int[] cells = candidates[random.Next(candidates.Count)];
                ships[s] = new Ship(planned[s].Type, cells);
                
                for (int c = 0; c < cells.Length; c++)
                    BlockAround(width, height, blocked, cells[c]);
            }

            return true;
        }

        private void Collect(int width, int height, int length, bool[] blocked, List<int[]> candidates)
        {
            CollectLine(width, height, length, 1, 0, blocked, candidates);
            
            if (length > 1)
                CollectLine(width, height, length, 0, 1, blocked, candidates);
        }

        private void CollectLine(int width, int height, int length, int stepX, int stepY, bool[] blocked, List<int[]> candidates)
        {
            int xCount = stepX == 0 ? width : width - length + 1;
            int yCount = stepY == 0 ? height : height - length + 1;

            for (int y = 0; y < yCount; y++)
            {
                for (int x = 0; x < xCount; x++)
                {
                    var cells = new int[length];
                    bool free = true;
                    
                    for (int i = 0; i < length; i++)
                    {
                        int cell = (y + i * stepY) * width + (x + i * stepX);
                        cells[i] = cell;
                        if (blocked[cell])
                            free = false;
                    }

                    if (free)
                        candidates.Add(cells);
                }
            }
        }

        private void BlockAround(int width, int height, bool[] blocked, int cell)
        {
            int x = cell % width;
            int y = cell / width;
            
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;

                    blocked[ny * width + nx] = true;
                }
            }
        }

        private struct PlannedShip
        {
            public ShipType Type;
            public int Length;
        }
    }
}

using System;
using System.Collections.Generic;
using _Project.Scripts;
using _Project.Scripts.Generators;
using _Project.Scripts.NetworkLayer.Protocol;
using _Project.Scripts.Ships;

namespace _Project.Scripts.NetworkLayer.Server
{
    public class Match
    {
        private readonly FieldGenerator _fieldGenerator;
        private readonly ShipPositionGenerator _positionGenerator;

        private readonly Random _random = new Random();

        private Field[] _fields;
        private Ship[][] _ships;
        private List<ShotRecord>[] _shots;
        private int _turn;
        private bool _placed;


        public Match(FieldGenerator fieldGenerator, ShipPositionGenerator positionGenerator)
        {
            _fieldGenerator = fieldGenerator;
            _positionGenerator = positionGenerator;
        }

        public void Place()
        {
            if (_placed)
                return;

            _fields = new Field[2];
            _ships = new Ship[2][];
            _shots = new List<ShotRecord>[2];

            for (int playerId = 0; playerId < 2; playerId++)
            {
                Field field = _fieldGenerator.Create();
                
                _fields[playerId] = field;
                _ships[playerId] = _positionGenerator.Create(field, _random);
                _shots[playerId] = new List<ShotRecord>();
            }

            _turn = 0;
            _placed = true;
        }

        public bool TryShoot(int shooter, int cell)
        {
            if (!_placed || shooter != _turn)
                return false;

            int size = _fields[shooter].Width * _fields[shooter].Height;
            
            if (cell < 0 || cell >= size)
                throw new ArgumentOutOfRangeException(nameof(cell));

            if (AlreadyShot(shooter, cell))
                return false;

            Ship ship = ShipAt(_ships[1 - shooter], cell);
            ShotResult result = ShotResult.Miss;
            
            if (ship != null)
            {
                ship.RegisterHit(cell);
                result = ship.IsSunk ? ShotResult.Sunk : ShotResult.Hit;
            }

            _shots[shooter].Add(new ShotRecord { Cell = cell, Result = result });
            
            if (result == ShotResult.Sunk)
                MarkSunk(shooter, ship);

            _turn = 1 - shooter;
            
            return true;
        }

        public SnapshotMessage SnapshotFor(int playerId)
        {
            Ship[] ships = _ships[playerId];
            var placements = new ShipPlacement[ships.Length];
            
            for (int i = 0; i < ships.Length; i++)
            {
                placements[i] = new ShipPlacement
                {
                    type = ships[i].Type,
                    cells = ships[i].Cells,
                    hits = ships[i].CopyHits()
                };
            }

            List<ShotRecord> fired = _shots[playerId];
            var shots = new ShotPlacement[fired.Count];
            
            for (int i = 0; i < fired.Count; i++)
            {
                shots[i] = new ShotPlacement
                {
                    cell = fired[i].Cell,
                    result = fired[i].Result
                };
            }

            return new SnapshotMessage
            {
                width = _fields[playerId].Width,
                height = _fields[playerId].Height,
                turn = _turn,
                ships = placements,
                shots = shots
            };
        }

        private static Ship ShipAt(Ship[] ships, int cell)
        {
            for (int s = 0; s < ships.Length; s++)
            {
                int[] cells = ships[s].Cells;
                
                for (int c = 0; c < cells.Length; c++)
                {
                    if (cells[c] == cell)
                        return ships[s];
                }
            }

            return null;
        }

        private bool AlreadyShot(int shooter, int cell)
        {
            List<ShotRecord> shots = _shots[shooter];
            
            for (int i = 0; i < shots.Count; i++)
            {
                if (shots[i].Cell == cell)
                    return true;
            }

            return false;
        }

        private void MarkSunk(int shooter, Ship ship)
        {
            List<ShotRecord> shots = _shots[shooter];
            int[] cells = ship.Cells;
            
            for (int i = 0; i < shots.Count; i++)
            {
                if (!Contains(cells, shots[i].Cell))
                    continue;

                ShotRecord shot = shots[i];
                shot.Result = ShotResult.Sunk;
                shots[i] = shot;
            }
        }

        private static bool Contains(int[] cells, int cell)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == cell)
                    return true;
            }

            return false;
        }

        private struct ShotRecord
        {
            public int Cell;
            public ShotResult Result;
        }
    }
}

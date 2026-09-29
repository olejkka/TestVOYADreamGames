using System;
using System.Collections.Generic;
using _Project.Scripts;
using _Project.Scripts.Configs;
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
        private readonly int _turnTimeoutSec;

        private Field[] _fields;
        private Ship[][] _ships;
        private List<ShotRecord>[] _shots;
        private int _turn;
        private int _winner = SnapshotMessage.NoWinner;
        private long _turnDeadlineMs = SnapshotMessage.NoDeadline;
        private bool _placed;


        public Match(FieldGenerator fieldGenerator, ShipPositionGenerator positionGenerator, MatchConfig matchConfig)
        {
            _fieldGenerator = fieldGenerator;
            _positionGenerator = positionGenerator;
            _turnTimeoutSec = matchConfig.turnTimeoutSec;
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
            _winner = SnapshotMessage.NoWinner;
            _placed = true;
        }

        public bool TryShoot(int shooter, int cell)
        {
            if (!_placed || _winner != SnapshotMessage.NoWinner || shooter != _turn)
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

            if (AllSunk(_ships[1 - shooter]))
                _winner = shooter;
            else
                _turn = 1 - shooter;
            
            return true;
        }

        public void StartTurn(long nowMs)
        {
            if (_winner != SnapshotMessage.NoWinner || _turnTimeoutSec == 0)
            {
                _turnDeadlineMs = SnapshotMessage.NoDeadline;
                return;
            }

            _turnDeadlineMs = nowMs + _turnTimeoutSec * 1000L;
        }

        public bool ExpireTurn(long nowMs)
        {
            if (_turnDeadlineMs < 0 || nowMs < _turnDeadlineMs)
                return false;

            _turn = 1 - _turn;
            StartTurn(nowMs);
            
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
                winner = _winner,
                turnDeadlineMs = _turnDeadlineMs,
                ships = placements,
                shots = shots,
                misses = MissesOn(playerId)
            };
        }

        private int[] MissesOn(int playerId)
        {
            List<ShotRecord> incoming = _shots[1 - playerId];
            int count = 0;

            for (int i = 0; i < incoming.Count; i++)
            {
                if (incoming[i].Result == ShotResult.Miss)
                    count++;
            }

            var misses = new int[count];
            int n = 0;

            for (int i = 0; i < incoming.Count; i++)
            {
                if (incoming[i].Result != ShotResult.Miss)
                    continue;

                misses[n] = incoming[i].Cell;
                n++;
            }

            return misses;
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

        private static bool AllSunk(Ship[] ships)
        {
            for (int i = 0; i < ships.Length; i++)
            {
                if (!ships[i].IsSunk)
                    return false;
            }

            return true;
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

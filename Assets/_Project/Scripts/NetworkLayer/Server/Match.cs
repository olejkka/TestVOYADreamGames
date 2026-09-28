using System;
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
            
            for (int playerId = 0; playerId < 2; playerId++)
            {
                Field field = _fieldGenerator.Create();
                
                _fields[playerId] = field;
                _ships[playerId] = _positionGenerator.Create(field, _random);
            }

            _placed = true;
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
                    cells = ships[i].Cells
                };
            }

            return new SnapshotMessage
            {
                width = _fields[playerId].Width,
                height = _fields[playerId].Height,
                ships = placements
            };
        }
    }
}

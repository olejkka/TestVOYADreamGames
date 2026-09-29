using System;
using _Project.Scripts;
using _Project.Scripts.NetworkLayer.Client;
using _Project.Scripts.NetworkLayer.Protocol;
using _Project.Scripts.UI.Cell;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public class PlayerWindow : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup alliesGrid;
        [SerializeField] private GridLayoutGroup enemyGrid;
        [SerializeField] private GameResultView resultView;

        private ClientSession _session;
        private CellInstantiator _cells;
        private GameResultPresenter _result;
        private CellPresenter[] _allies;
        private CellPresenter[] _enemy;
        private bool _built;

        
        public void Bind(ClientSession session, CellInstantiator cells)
        {
            _session = session;
            _cells = cells;
            _result = new GameResultPresenter(resultView, session);
            _session.SnapshotReceived += OnSnapshot;
            if (_session.Snapshot != null)
                OnSnapshot(_session.Snapshot);
        }

        private void OnSnapshot(SnapshotMessage snapshot)
        {
            CellType[] own = OwnCells(snapshot);
            CellType[] enemy = EnemyCells(snapshot);

            if (!_built)
            {
                _allies = Create(alliesGrid, snapshot.width, own, null);
                _enemy = Create(enemyGrid, snapshot.width, enemy, _session.Shoot);
                _built = true;
                return;
            }

            Show(_allies, own);
            Show(_enemy, enemy);
        }

        private CellPresenter[] Create(GridLayoutGroup grid, int width, CellType[] types, Action<int> onShot)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = width;

            RectTransform field = (RectTransform)grid.transform;
            var presenters = new CellPresenter[types.Length];

            for (int i = 0; i < types.Length; i++)
                presenters[i] = _cells.Create(field, i, types[i], onShot);

            return presenters;
        }

        private void Show(CellPresenter[] presenters, CellType[] types)
        {
            for (int i = 0; i < presenters.Length; i++)
                presenters[i].Show(types[i]);
        }

        private CellType[] OwnCells(SnapshotMessage snapshot)
        {
            int count = snapshot.width * snapshot.height;
            var cells = new CellType[count];
            
            for (int i = 0; i < cells.Length; i++)
                cells[i] = CellType.Water;

            int[] misses = snapshot.misses;

            for (int i = 0; i < misses.Length; i++)
                cells[misses[i]] = CellType.Miss;

            ShipPlacement[] ships = snapshot.ships;
            
            for (int s = 0; s < ships.Length; s++)
            {
                int[] indices = ships[s].cells;
                int[] hits = ships[s].hits;
                
                bool sunk = hits.Length == indices.Length;
                
                for (int c = 0; c < indices.Length; c++)
                    cells[indices[c]] = CellType.Ship;

                for (int h = 0; h < hits.Length; h++)
                    cells[hits[h]] = sunk ? CellType.Sunk : CellType.Hit;
            }

            return cells;
        }

        private CellType[] EnemyCells(SnapshotMessage snapshot)
        {
            int count = snapshot.width * snapshot.height;
            var cells = new CellType[count];
            
            for (int i = 0; i < cells.Length; i++)
                cells[i] = CellType.Unknown;

            ShotPlacement[] shots = snapshot.shots;
            
            for (int i = 0; i < shots.Length; i++)
                cells[shots[i].cell] = TypeOf(shots[i].result);

            return cells;
        }

        private CellType TypeOf(ShotResult result)
        {
            if (result == ShotResult.Miss)
                return CellType.Water;
            
            if (result == ShotResult.Hit)
                return CellType.Hit;
            
            if (result == ShotResult.Sunk)
                return CellType.Sunk;

            throw new InvalidOperationException("No cell type for shot result " + result + ".");
        }
    }
}

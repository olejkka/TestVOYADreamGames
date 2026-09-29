using System;
using _Project.Scripts.Configs;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace _Project.Scripts.UI.Cell
{
    public class CellInstantiator : MonoBehaviour
    {
        [SerializeField] private CellView cellPrefab;

        private CellColorConfig _colors;

        [Inject]
        public void Construct(CellColorConfig colors)
        {
            _colors = colors;
        }

        public CellPresenter Create(RectTransform parent, int index, CellType type, Action<int> onShot)
        {
            CellView view = Object.Instantiate(cellPrefab, parent);

            var presenter = new CellPresenter(view, _colors, index, onShot);
            presenter.Show(type);

            return presenter;
        }
    }
}

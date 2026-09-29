using System;
using _Project.Scripts.Configs;

namespace _Project.Scripts.UI.Cell
{
    public class CellPresenter
    {
        private readonly CellView _view;
        private readonly CellColorConfig _colors;
        private readonly int _index;
        private readonly Func<int, bool> _onShot;
        
        private CellType _type;

        
        public CellPresenter(CellView view, CellColorConfig colors, int index, Func<int, bool> onShot)
        {
            _view = view;
            _colors = colors;
            _index = index;
            _onShot = onShot;
            
            _view.Clicked += OnClick;
        }

        public void Show(CellType type)
        {
            _type = type;
            _view.SetColor(_colors.ColorOf(type));
        }

        private void OnClick()
        {
            if (_onShot == null || _type != CellType.Unknown)
                return;

            if (!_onShot(_index))
                return;

            Show(CellType.Sent);
        }
    }
}

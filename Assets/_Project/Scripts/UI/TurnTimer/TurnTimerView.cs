using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.TurnTimer
{
    public class TurnTimerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Image image;
        [SerializeField] private Color color;

        private TurnTimerPresenter _presenter;
        private Color _source;
        private bool _captured;

        private void Awake()
        {
            Capture();
        }

        public void Attach(TurnTimerPresenter presenter)
        {
            _presenter = presenter;
        }

        public void SetText(string value)
        {
            text.text = value;
        }

        public void Paint()
        {
            Capture();
            image.color = color;
        }

        public void Restore()
        {
            image.color = _source;
        }

        private void Capture()
        {
            if (_captured)
                return;

            _source = image.color;
            _captured = true;
        }

        private void Update()
        {
            if (_presenter == null)
                return;

            long nowMs = (long)(Time.unscaledTime * 1000f);
            _presenter.Tick(nowMs);
        }
    }
}

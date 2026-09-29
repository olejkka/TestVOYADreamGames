using _Project.Scripts.NetworkLayer.Client;
using _Project.Scripts.NetworkLayer.Protocol;
using UnityEngine;

namespace _Project.Scripts.UI.TurnTimer
{
    public class TurnTimerPresenter
    {
        private readonly TurnTimerView _view;
        private readonly ClientSession _session;

        private long _deadline = SnapshotMessage.NoDeadline;
        private int _shown = -1;
        private bool _running;

        
        public TurnTimerPresenter(TurnTimerView view, ClientSession session)
        {
            _view = view;
            _session = session;
            
            _view.Attach(this);
            
            _session.SnapshotReceived += OnSnapshot;

            if (_session.Snapshot != null)
                OnSnapshot(_session.Snapshot);
        }

        public void Tick(long nowMs)
        {
            if (!_running)
                return;

            ShowRemaining(nowMs);
        }

        private void OnSnapshot(SnapshotMessage snapshot)
        {
            if (snapshot.winner != SnapshotMessage.NoWinner || snapshot.turnDeadlineMs < 0)
            {
                if (_running)
                    _view.Restore();

                _running = false;
                _shown = -1;
                _view.SetText("");
                
                return;
            }

            _deadline = snapshot.turnDeadlineMs;

            if (snapshot.turn != _session.PlayerId)
            {
                if (_running)
                    _view.Restore();

                _running = false;
                _view.SetText(_shown < 0 ? "" : _shown.ToString());
                
                return;
            }

            bool wasRunning = _running;
            _running = true;

            if (!wasRunning)
                _view.Paint();

            ShowRemaining((long)(Time.unscaledTime * 1000f));
        }

        private void ShowRemaining(long nowMs)
        {
            long remain = _deadline - nowMs;
            int seconds = remain <= 0 ? 0 : (int)((remain + 999) / 1000);
            
            _shown = seconds;
            _view.SetText(seconds.ToString());
        }

        public void Dispose()
        {
            _session.SnapshotReceived -= OnSnapshot;
        }
    }
}

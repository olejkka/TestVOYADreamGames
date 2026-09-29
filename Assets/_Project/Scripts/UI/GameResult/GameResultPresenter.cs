using _Project.Scripts.NetworkLayer.Client;
using _Project.Scripts.NetworkLayer.Protocol;

namespace _Project.Scripts.UI.GameResult
{
    public class GameResultPresenter
    {
        private readonly GameResultView _view;
        private readonly ClientSession _session;

        
        public GameResultPresenter(GameResultView view, ClientSession session)
        {
            _view = view;
            _session = session;
            
            _session.SnapshotReceived += OnSnapshot;

            if (_session.Snapshot != null)
                OnSnapshot(_session.Snapshot);
        }

        private void OnSnapshot(SnapshotMessage snapshot)
        {
            if (snapshot.winner == SnapshotMessage.NoWinner)
                return;

            string result = snapshot.winner == _session.PlayerId ? "Winner" : "Loser";
            _view.Show(result);
        }
    }
}

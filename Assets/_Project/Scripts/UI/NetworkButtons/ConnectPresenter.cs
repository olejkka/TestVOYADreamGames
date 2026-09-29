using _Project.Scripts.NetworkLayer.Client;

namespace _Project.Scripts.UI.NetworkButtons
{
    public class ConnectPresenter
    {
        private readonly ClientSession _session;

        
        public ConnectPresenter(ConnectView view, ClientSession session)
        {
            _session = session;
            view.Clicked += OnClick;
        }

        private void OnClick()
        {
            _session.Connect();
        }
    }
}

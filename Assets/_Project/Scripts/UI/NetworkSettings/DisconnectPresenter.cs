using _Project.Scripts.NetworkLayer.Client;

namespace _Project.Scripts.UI.NetworkSettings
{
    public class DisconnectPresenter
    {
        private readonly ClientSession _session;

        
        public DisconnectPresenter(DisconnectView view, ClientSession session)
        {
            _session = session;
            view.Clicked += OnClick;
        }

        private void OnClick()
        {
            _session.Disconnect();
        }
    }
}

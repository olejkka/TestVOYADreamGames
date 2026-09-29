using _Project.Scripts.NetworkLayer.Client;

namespace _Project.Scripts.UI.NetworkSettings
{
    public class NetworkSettingsPresenter
    {
        private readonly ClientSession _session;

        
        public NetworkSettingsPresenter(NetworkSettingsView view, ClientSession session)
        {
            _session = session;
            view.Show(session.Latency, session.DropChance, session.RetransmitMs);
            
            view.Changed += OnChanged;
        }

        private void OnChanged(int latencyMs, float dropChance, int retransmitMs)
        {
            _session.SetNetwork(latencyMs, dropChance, retransmitMs);
        }
    }
}

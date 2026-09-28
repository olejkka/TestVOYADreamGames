using _Project.Scripts.NetworkLayer.Client;
using _Project.Scripts.NetworkLayer.Server;
using _Project.Scripts.NetworkLayer.Transport;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts.NetworkLayer
{
    public sealed class NetworkEntryPoint : IStartable, ITickable
    {
        private readonly InProcessHub _hub;
        private readonly GameServer _server;
        private readonly ClientSession _session0;
        private readonly ClientSession _session1;

        
        public NetworkEntryPoint(
            InProcessHub hub,
            GameServer server,
            [Key(0)] ClientSession session0,
            [Key(1)] ClientSession session1)
        {
            _hub = hub;
            _server = server;
            _session0 = session0;
            _session1 = session1;
        }

        public void Start()
        {
            _session0.Start();
            _session1.Start();
        }

        public void Tick()
        {
            long nowMs = (long)(Time.unscaledTime * 1000f);
            _hub.Tick(nowMs);
        }
    }
}

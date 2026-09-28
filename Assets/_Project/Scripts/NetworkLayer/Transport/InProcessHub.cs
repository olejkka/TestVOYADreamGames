using System;
using _Project.Scripts.Configs;

namespace _Project.Scripts.NetworkLayer.Transport
{
    public sealed class InProcessHub : IServerTransport, IDisposable
    {
        private readonly Connection[] _connections;
        
        private long _nowMs;
        internal long NowMs => _nowMs;

        public event Action<int, byte[]> Received;
        public event Action<int> Connected;
        public event Action<int> Disconnected;


        public InProcessHub(MatchConfig matchConfig)
        {
            int latency = matchConfig.defaultLatencyMs;
            
            _connections = new Connection[2];
            _connections[0] = new Connection(this, 0, latency);
            _connections[1] = new Connection(this, 1, latency);
        }

        public IClientTransport Client(int connectionId)
        {
            return _connections[connectionId];
        }

        public void Send(int connectionId, byte[] bytes)
        {
            _connections[connectionId].EnqueueToClient(bytes, _nowMs);
        }

        public void Tick(long nowMs)
        {
            _nowMs = nowMs;
            
            for (int i = 0; i < _connections.Length; i++)
                _connections[i].Tick(nowMs);
        }

        public void Dispose()
        {
            Received = null;
            Connected = null;
            Disconnected = null;
            
            for (int i = 0; i < _connections.Length; i++)
                _connections[i].Dispose();
        }

        internal void RaiseReceived(int connectionId, byte[] bytes)
        {
            Received?.Invoke(connectionId, bytes);
        }

        internal void RaiseConnected(int connectionId)
        {
            Connected?.Invoke(connectionId);
        }

        internal void RaiseDisconnected(int connectionId)
        {
            Disconnected?.Invoke(connectionId);
        }
    }
}

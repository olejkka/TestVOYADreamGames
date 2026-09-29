using System;
using System.Collections.Generic;
using _Project.Scripts.Configs;

namespace _Project.Scripts.NetworkLayer.Transport
{
    public class Connection : IClientTransport
    {
        private readonly InProcessHub _hub;
        private readonly MatchConfig _matchConfig;
        private readonly int _id;
        private readonly Random _random = new Random();
        
        private readonly Queue<Queued> _toServer = new Queue<Queued>();
        private readonly Queue<Queued> _toClient = new Queue<Queued>();
        
        private bool _connected;

        public int Latency { get; set; }

        public event Action<byte[]> Received;
        public event Action Disconnected;

        
        public Connection(InProcessHub hub, int id, int latency, MatchConfig matchConfig)
        {
            _hub = hub;
            _id = id;
            Latency = latency;
            _matchConfig = matchConfig;
        }

        public void Send(byte[] bytes)
        {
            if (!_connected)
                return;

            _toServer.Enqueue(new Queued(bytes, _hub.NowMs + Latency));
        }

        internal void EnqueueToClient(byte[] bytes, long nowMs)
        {
            if (!_connected)
                return;

            _toClient.Enqueue(new Queued(bytes, nowMs + Latency));
        }

        public void Connect()
        {
            if (_connected)
                return;

            _connected = true;
            _hub.RaiseConnected(_id);
        }

        public void Disconnect()
        {
            if (!_connected)
                return;

            _connected = false;
            
            _toServer.Clear();
            _toClient.Clear();
            
            Disconnected?.Invoke();
            
            _hub.RaiseDisconnected(_id);
        }

        internal void Tick(long nowMs)
        {
            if (!_connected)
                return;

            Deliver(_toServer, nowMs, bytes => _hub.RaiseReceived(_id, bytes));
            
            if (!_connected)
                return;

            Deliver(_toClient, nowMs, bytes => Received?.Invoke(bytes));
        }

        internal void Dispose()
        {
            Received = null;
            Disconnected = null;
            
            _toServer.Clear();
            _toClient.Clear();
            
            _connected = false;
        }

        private void Deliver(Queue<Queued> queue, long nowMs, Action<byte[]> deliver)
        {
            while (_connected && queue.Count > 0)
            {
                Queued head = queue.Peek();
                
                if (head.DeliverAtMs > nowMs)
                    return;

                if (_matchConfig.dropChance > 0f && _random.NextDouble() < _matchConfig.dropChance)
                {
                    head.DeliverAtMs = nowMs + _matchConfig.retransmitMs;
                    return;
                }

                queue.Dequeue();
                deliver(head.Bytes);
            }
        }

        private class Queued
        {
            public byte[] Bytes { get; }
            public long DeliverAtMs { get; set; }


            public Queued(byte[] bytes, long deliverAtMs)
            {
                Bytes = bytes;
                DeliverAtMs = deliverAtMs;
            }
        }
    }
}
